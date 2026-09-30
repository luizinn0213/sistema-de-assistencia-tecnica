# Sistema de Assistência Técnica

Aplicação web criada para conectar clientes que precisam de assistência técnica a profissionais disponíveis e compatíveis com o serviço solicitado.

## Funcionalidades da AV1

- Cadastro e login de clientes.
- Consulta e atualização do perfil do cliente.
- Cadastro, edição e exclusão de equipamentos.
- Cadastro e gerenciamento de técnicos.
- Cadastro e gerenciamento de especialidades.
- Vinculação de especialidades aos técnicos.
- Controle da disponibilidade do técnico.
- Abertura e consulta de solicitações.
- Busca de técnicos compatíveis.
- Seleção de técnico para uma solicitação.
- Aceite ou recusa pelo técnico.
- Histórico das mudanças de estado da seleção.

## Tecnologias

### Backend

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- xUnit

### Frontend

- React
- TypeScript
- Vite
- CSS

### Versionamento

- Git
- GitHub

## Pré-requisitos

Para executar o projeto, é necessário possuir:

- Git;
- .NET SDK 10;
- Node.js;
- npm.

## Como obter o projeto

```powershell
git clone https://github.com/luizinn0213/sistema-de-assistencia-tecnica.git

cd sistema-de-assistencia-tecnica
```

## Preparação do backend

Restaurar as ferramentas e dependências:

```powershell
dotnet tool restore

dotnet restore .\backend\Assistencia.Api\Assistencia.Api.csproj
```

Criar ou atualizar o banco SQLite:

```powershell
dotnet ef database update `
  --project .\backend\Assistencia.Api\Assistencia.Api.csproj `
  --startup-project .\backend\Assistencia.Api\Assistencia.Api.csproj
```

Compilar o backend:

```powershell
dotnet build .\backend\Assistencia.Api\Assistencia.Api.csproj
```

Executar os testes:

```powershell
dotnet test .\backend\Assistencia.Api.Tests\Assistencia.Api.Tests.csproj
```

Iniciar a API:

```powershell
dotnet run --project .\backend\Assistencia.Api\Assistencia.Api.csproj
```

A API utiliza o endereço:

```text
http://localhost:5141
```

O terminal da API deve permanecer aberto durante a utilização do sistema.

## Preparação do frontend

Em outro terminal, instalar as dependências:

```powershell
npm.cmd --prefix .\frontend install
```

Compilar o frontend:

```powershell
npm.cmd --prefix .\frontend run build
```

Iniciar o frontend:

```powershell
npm.cmd --prefix .\frontend run dev
```

A aplicação poderá ser acessada em:

```text
http://localhost:5173
```

O terminal do frontend também deve permanecer aberto.

Em ambientes nos quais o PowerShell não bloqueia o arquivo `npm.ps1`, os comandos podem ser executados utilizando apenas `npm`.

## Fluxo principal da AV1

1. O cliente cria sua conta e entra no sistema.
2. O cliente cadastra um equipamento.
3. O cliente abre uma solicitação de serviço.
4. O sistema busca técnicos disponíveis com a especialidade necessária.
5. O cliente seleciona um técnico.
6. O técnico visualiza a solicitação recebida.
7. O técnico aceita ou recusa a solicitação.
8. A mudança de estado fica registrada no histórico e no banco.

## Regras essenciais

1. Um técnico inativo não pode ficar disponível.
2. Uma solicitação somente pode utilizar um equipamento pertencente ao cliente.
3. Uma seleção já aceita ou recusada não pode receber uma segunda resposta.

Os detalhes e cenários dessas regras estão disponíveis em:

```text
docs/matriz-regras.md
```

## Dados mínimos para demonstração

Para demonstrar o fluxo completo, é necessário possuir:

- um cliente cadastrado;
- um equipamento pertencente ao cliente;
- uma especialidade ativa;
- um técnico ativo e disponível;
- a especialidade vinculada ao técnico;
- uma solicitação aberta.

Os dados podem ser cadastrados por meio da própria aplicação.

## Validações do projeto

Antes da entrega, foram utilizados os seguintes comandos:

```powershell
dotnet build .\backend\Assistencia.Api\Assistencia.Api.csproj

dotnet test .\backend\Assistencia.Api.Tests\Assistencia.Api.Tests.csproj

npm.cmd --prefix .\frontend run lint

npm.cmd --prefix .\frontend run build
```

## Limitações conhecidas da AV1

- Alguns campos de identificação ainda são preenchidos manualmente para demonstração.
- A autenticação ainda não utiliza token JWT.
- O sistema foi preparado para execução local.
- A execução completa do serviço e a avaliação do atendimento ficarão para uma etapa posterior.
- Alguns fluxos utilizam seletores provisórios até a integração completa entre usuário e perfil técnico.

## Estrutura principal

```text
backend/
  Assistencia.Api/
  Assistencia.Api.Tests/

frontend/
  src/

docs/
  proposta.md
  modelo-classes.md
  matriz-regras.md
  verificacao-av1.md
  participacao.md
```

## Documentação

- Proposta do projeto: `docs/proposta.md`
- Modelo de classes: `docs/modelo-classes.md`
- Matriz das regras: `docs/matriz-regras.md`
- Registro de verificação: `docs/verificacao-av1.md`
- Participação dos integrantes: `docs/participacao.md`

## Integrantes

- Bernardo Awano Muniz - 06014676
- Kelvin Silva Costa - 06015489
- Karina Lussac Carneiro - 06006762
- Luiz Felipe de Oliveira Viana - 06914935
- Maria Eduarda Mendes Fidelis - 06013817

## Commit candidato

O commit candidato corresponde à versão mais recente da branch `main` no momento da entrega.

O identificador exato será informado no e-mail de entrega após a integração da documentação.