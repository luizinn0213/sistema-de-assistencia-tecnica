# Registro de Verificação da AV1

## Objetivo

Este documento registra os comandos e os procedimentos utilizados para verificar a versão candidata da AV1.

## Ambiente utilizado

- Sistema operacional: Windows
- Backend: .NET 10
- Frontend: React, TypeScript e Vite
- Banco de dados: SQLite
- Persistência: Entity Framework Core
- Testes: xUnit
- Versionamento: Git

## Verificação do backend

### Restauração das ferramentas

Comando:

```powershell
dotnet tool restore
```

Resultado:

```text
dotnet-ef 10.0.12 restaurado com sucesso.
```

### Compilação da API

Comando:

```powershell
dotnet build .\backend\Assistencia.Api\Assistencia.Api.csproj
```

Resultado:

```text
Compilação concluída com sucesso.
```

### Execução dos testes

Comando:

```powershell
dotnet test .\backend\Assistencia.Api.Tests\Assistencia.Api.Tests.csproj
```

Resultado:

```text
Total: 29
Bem-sucedidos: 29
Falharam: 0
Ignorados: 0
```

### Preparação do banco

Comando:

```powershell
dotnet ef database update `
  --project .\backend\Assistencia.Api\Assistencia.Api.csproj `
  --startup-project .\backend\Assistencia.Api\Assistencia.Api.csproj
```

Resultado:

```text
Build concluído.
Banco de dados atualizado.
Nenhuma migration pendente.
```

### Inicialização da API

Comando:

```powershell
dotnet run --project .\backend\Assistencia.Api\Assistencia.Api.csproj
```

Endereço utilizado:

```text
http://localhost:5141
```

Resultado:

```text
API iniciada e utilizada com sucesso durante os testes manuais.
```

## Verificação do frontend

### Instalação das dependências

Comando:

```powershell
npm.cmd --prefix .\frontend install
```

Resultado:

```text
Dependências atualizadas.
161 pacotes verificados.
0 vulnerabilidades encontradas.
```

### Verificação do código

Comando:

```powershell
npm.cmd --prefix .\frontend run lint
```

Resultado:

```text
Lint concluído sem erros.
```

### Compilação do frontend

Comando:

```powershell
npm.cmd --prefix .\frontend run build
```

Resultado:

```text
TypeScript compilado.
40 módulos transformados.
Build do Vite concluído com sucesso.
```

### Inicialização do frontend

Comando:

```powershell
npm.cmd --prefix .\frontend run dev
```

Endereço utilizado:

```text
http://localhost:5173
```

Resultado:

```text
Frontend iniciado e utilizado com sucesso durante os testes manuais.
```

## Fluxo de sucesso

O fluxo principal utilizado na verificação foi:

1. Criar uma conta de cliente.
2. Entrar no sistema.
3. Cadastrar um equipamento.
4. Criar uma especialidade.
5. Cadastrar um técnico.
6. Vincular a especialidade ao técnico.
7. Tornar o técnico disponível.
8. Abrir uma solicitação utilizando o equipamento do cliente.
9. Buscar técnicos compatíveis.
10. Selecionar o técnico.
11. Consultar a solicitação recebida pelo técnico.
12. Aceitar a solicitação.
13. Consultar o histórico.
14. Confirmar que o estado ficou persistido.

Resultado:

```text
Fluxo concluído com sucesso pelo frontend, passando pela API e pelo banco SQLite.
```

## Cenário inválido

O cenário inválido utilizado foi tentar responder novamente uma seleção que já havia sido aceita.

Comportamento esperado:

- a API rejeita a operação;
- o sistema apresenta uma mensagem compreensível;
- o estado anterior permanece;
- nenhum histórico inválido é criado.

Resultado:

```text
A segunda resposta foi rejeitada.
O estado permaneceu consistente.
O comportamento também está protegido por teste automatizado.
```

## Persistência

A persistência foi confirmada por meio de:

1. conclusão do fluxo principal;
2. realização de nova consulta;
3. conferência do estado da seleção;
4. consulta do histórico registrado;
5. confirmação dos dados armazenados no SQLite.

Resultado:

```text
Os dados foram recuperados corretamente em novas consultas.
```

## Resumo da verificação

| Verificação | Resultado |
|---|---|
| Restauração das ferramentas | Concluída |
| Compilação do backend | Concluída |
| Testes automatizados | 29 aprovados e 0 falhas |
| Atualização do banco | Concluída, sem migrations pendentes |
| Inicialização da API | Concluída |
| Instalação das dependências do frontend | Concluída, 0 vulnerabilidades |
| Lint do frontend | Concluído sem erros |
| Compilação do frontend | Concluída |
| Inicialização do frontend | Concluída |
| Fluxo de sucesso | Concluído |
| Cenário inválido | Rejeitado corretamente |
| Persistência após nova consulta | Confirmada |

## Conclusão

A versão verificada compila, executa os testes automatizados, prepara o banco SQLite e gera o build do frontend sem erros.

O fluxo principal da AV1 foi testado entre frontend, API e banco de dados. O cenário inválido selecionado também foi rejeitado corretamente.