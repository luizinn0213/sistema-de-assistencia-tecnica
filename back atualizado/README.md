# API Assistência Técnica — backend com autenticação JWT

Reescrita do backend original (`models`, `appdbcontext`, `controlador`, `program`)
agora com **login JWT**, **perfis (papéis)**, **refresh token** e
**usuário identificado automaticamente pelo token**.

Projeto: `C:\Users\RYZEN\Desktop\back\Api` (os arquivos antigos continuam intactos
em `C:\Users\RYZEN\Desktop\back` como backup).

---

## 1. O que mudou

### Autenticação e autorização
| Item | Antes | Agora |
|---|---|---|
| Login | devolvia apenas os dados do usuário | devolve **access token JWT + refresh token + dados do usuário** |
| Sessão | nenhuma | JWT assinado (HMAC-SHA256), expiração curta + refresh com **rotação** |
| Perfis | campo `Tipo` existia, mas não era usado | políticas `SomenteCliente`, `EquipeTecnica`, `SomenteAdministrador` + `[Authorize(Roles = ...)]` |
| Identificação | cliente vinha por `id` na URL | vem das **claims do token** (`/api/clientes/me`, `/api/equipamentos/meus`) |
| Senha | BCrypt | BCrypt mantido + bloqueio de resposta genérica no login |
| Banco | senha em hash | hash + refresh tokens guardados como **SHA-256**, nunca em texto |
| Padrão | endpoints abertos | **tudo exige token** (`FallbackPolicy`); o que é público leva `[AllowAnonymous]` |
| Força bruta | — | rate limit de 15 req/min por IP em login/refresh/cadastro (429 em JSON) |
| Erros | 401/403/500 vazios ou HTML | sempre JSON `{ "erro": "..." }` |

### Otimizações
- **Consultas sem rastreamento** (`AsNoTracking`) e projeção só das colunas usadas.
- **Paginação** em todas as listagens (`pagina`, `tamanho`, `total`, `totalPaginas`).
- **Busca** com `EF.Functions.Like` (aproveita índice e ignora maiúsculas/minúsculas).
- **Um único `SaveChanges`** no cadastro de cliente (usuário + cliente + endereço em
  uma transação) e tratamento de `DbUpdateException` para requisições concorrentes.
- **`ExecuteDeleteAsync`** na exclusão de cliente: apaga em lote, sem carregar
  entidades e sem N+1.
- **Validação automática** com `DataAnnotations` + `[ApiController]` (400 com
  detalhes por campo antes de chegar ao banco).
- **Índices** em e-mail, CPF/CNPJ, número de série, tipo e refresh token.
- **Enum `TipoUsuario` gravado como texto** ("Cliente"/"Tecnico"/"Administrador"),
  legível no banco e imune a mudança de ordem do enum.
- Serviços separados (`TokenService`, `AutenticacaoService`, `UsuarioAtual`), sem
  duplicar a lógica de emissão de token em cada controller.
- CORS restrito às origens do `appsettings` (antes era `AllowAnyOrigin`).
- Limpeza automática de refresh tokens vencidos no startup.

---

## 2. Estrutura

```
Api/
├── Api.csproj
├── Program.cs                  # DI, JWT, políticas, CORS, rate limit, pipeline
├── Models.cs                   # Usuario, Cliente, Endereco, Equipamento, RefreshToken, Perfis
├── DTOs.cs                     # requests/responses com validação
├── AppDbContext.cs             # mapeamento, índices, conversões
├── appsettings.json
├── Properties/launchSettings.json
├── Security/
│   ├── JwtOptions.cs           # configuração + nomes das claims
│   ├── Politicas.cs            # nomes das políticas de autorização
│   ├── TokenService.cs         # gera/valida tokens, hash do refresh
│   ├── AutenticacaoService.cs  # login → tokens, rotação, revogação
│   └── UsuarioAtual.cs         # usuário autenticado a partir das claims
├── Infra/
│   ├── TratamentoDeErrosMiddleware.cs
│   └── DbSeeder.cs             # admin inicial + limpeza de tokens
└── Controllers/
    ├── AuthController.cs       # login, refresh, logout, eu, senha
    ├── ClientesController.cs
    ├── EquipamentosController.cs
    └── UsuariosController.cs   # gestão de perfis (Admin)
```

---

## 3. Como rodar

Requisito: **.NET SDK 8.0** (`dotnet --list-sdks`).

```powershell
cd C:\Users\RYZEN\Desktop\back\Api
dotnet restore
dotnet run
```

A API sobe em **http://localhost:5000** (definido em `Properties/launchSettings.json`,
que também ativa o ambiente `Development`).

### Chave do JWT (obrigatório em produção)

A chave não vai no código. Escolha uma das formas:

```powershell
# 1) Secret local de desenvolvimento
dotnet user-secrets set "Jwt:Chave" "troque-esta-chave-por-uma-com-32-caracteres-ou-mais"

# 2) Variável de ambiente (produção)
$env:Jwt__Chave = "troque-esta-chave-por-uma-com-32-caracteres-ou-mais"
```

Sem chave configurada a API **sobe mesmo assim**, gera uma chave efêmera e registra
um `WARNING`: os tokens deixam de valer quando a API reinicia.

### Primeiro acesso (seed)

```
E-mail: admin@assistencia.local
Senha:  Admin@123456
```

Está em `appsettings.json` → `Seed`. Troque a senha depois do primeiro login
(`PUT /api/auth/senha`) e use `Seed:Criar = false` em produção.

### Banco de dados

`EnsureCreated()` cria `assistencia.db` automaticamente. Como o schema mudou
(`Tipo` agora é texto e existe a tabela `RefreshTokens`), **apague o `assistencia.db`
antigo** antes de rodar. Para evoluir o schema daqui pra frente, use migrations:

```powershell
dotnet ef migrations add Inicial
dotnet ef database update
```

---

## 4. Fluxo de autenticação

```
POST /api/auth/login        { "email": "...", "senha": "..." }
  → 200 { accessToken, tokenType: "Bearer", expiraEmSegundos,
          refreshToken, refreshExpiraEm, usuario: { id, nome, email, perfil, clienteId } }

GET  /api/...               Authorization: Bearer <accessToken>

POST /api/auth/refresh      { "refreshToken": "..." }   → novo par de tokens (o antigo é revogado)
POST /api/auth/logout       Authorization: Bearer ...   → revoga todos os refresh tokens
GET  /api/auth/eu           → dados do usuário do token
PUT  /api/auth/senha        → troca a senha e derruba as sessões antigas
```

Claims do token: `sub` (id), `nome`, `email`, `role` (perfil), `tipo`,
`clienteId` (quando o perfil é Cliente) e `jti`.

---

## 5. Perfis e permissões

| Endpoint | Anônimo | Cliente | Técnico | Administrador |
|---|---|---|---|---|
| `POST /api/auth/login` | ✅ | ✅ | ✅ | ✅ |
| `POST /api/auth/refresh` | ✅ | ✅ | ✅ | ✅ |
| `POST /api/clientes` (cadastro) | ✅ | ✅ | ✅ | ✅ |
| `GET /api/auth/eu`, `POST /api/auth/logout`, `PUT /api/auth/senha` | — | ✅ | ✅ | ✅ |
| `GET/PUT /api/clientes/me` | — | ✅ | — | — |
| `GET /api/clientes`, `GET/PUT /api/clientes/{id}` | — | — | ✅ | ✅ |
| `DELETE /api/clientes/{id}` | — | — | — | ✅ |
| `GET /api/equipamentos/meus`, `POST /api/equipamentos` | — | ✅ | — | — |
| `POST /api/equipamentos/cliente/{id}` | — | — | ✅ | ✅ |
| `GET /api/equipamentos`, `GET/PUT/DELETE /api/equipamentos/{id}` | — | dono | ✅ | ✅ |
| `GET /api/usuarios`, `POST /api/usuarios`, `PUT /{id}/perfil`, `PUT /{id}/status`, `DELETE /{id}` | — | — | — | ✅ |

Regra de ouro: **nenhum endpoint de cliente confia em `id` vindo da URL/body** —
o vínculo usuário → cliente vem da claim `clienteId` (ou do banco, se a claim não
existir). Um cliente nunca consegue ler ou alterar dados de outro cliente.

---

## 6. Exemplos rápidos (curl)

```bash
# login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@assistencia.local","senha":"Admin@123456"}'

# usar o token
curl http://localhost:5000/api/usuarios \
  -H "Authorization: Bearer <ACCESS_TOKEN>"

# cadastro de cliente (público)
curl -X POST http://localhost:5000/api/clientes \
  -H "Content-Type: application/json" \
  -d '{"nome":"Maria","email":"maria@exemplo.com","senha":"Senha@123",
       "cpfCnpj":"12345678909","telefone":"11999990000"}'
```

Há um arquivo `Api.http` com todas as chamadas prontas (VS Code REST Client ou
Visual Studio).

---

## 7. Observações de segurança

- **Senhas**: BCrypt com salt por usuário; nenhuma resposta devolve `SenhaHash`.
- **Refresh token**: 32 bytes aleatórios, guardado como SHA-256, rotação a cada uso
  (reutilização de token antigo falha) e revogação em logout, troca de senha,
  desativação e mudança de perfil.
- **Login**: mensagem genérica ("E-mail ou senha inválidos"), sem revelar se o
  e-mail existe.
- **Rate limit**: 15 requisições/minuto por IP em `login`, `refresh` e cadastro.
- **Limitação conhecida**: o perfil e o status ativo viajam no token. Se um admin
  desativar um usuário, o *access token* atual continua válido até expirar
  (60 min por padrão); a renovação já é bloqueada. Para efeito imediato, reduza
  `Jwt:ExpiraMinutos` ou valide o usuário no banco a cada requisição (custo de 1
  consulta por request).
