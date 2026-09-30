# Matriz de Regras da AV1

## Objetivo

Este documento registra três regras essenciais do fluxo principal da AV1, indicando os objetos responsáveis, os estados inválidos impedidos e as evidências existentes no projeto.

## Matriz

| Regra | Responsável | Estado inválido impedido | Cenário válido | Cenário inválido | Evidência |
|---|---|---|---|---|---|
| Um técnico inativo não pode ficar disponível | `Tecnico` | Técnico com `Ativo = false` e `Disponivel = true` | Alterar a disponibilidade de um técnico ativo | Tentar disponibilizar um técnico inativo | Método `AlterarDisponibilidade` e testes de técnico |
| O equipamento da solicitação deve pertencer ao cliente | `Solicitacao` | Solicitação aberta por um cliente usando equipamento de outro cliente | Abrir solicitação com equipamento pertencente ao cliente | Abrir solicitação utilizando equipamento de outro cliente | Construtor de `Solicitacao`, `SolicitacoesController` e testes de solicitação |
| Uma seleção só pode receber uma resposta | `SelecaoTecnico` | Seleção aceita ou recusada recebendo uma segunda resposta | Aceitar ou recusar uma seleção em `AguardandoResposta` | Tentar aceitar ou recusar novamente uma seleção já respondida | Métodos `Aceitar`, `Recusar`, validação de estado e `MatchingTests` |

## Regra 1 — Técnico inativo não pode ficar disponível

### Descrição

A disponibilidade representa se o técnico pode receber novas solicitações. Um técnico inativo não pode ser apresentado como disponível.

### Objeto responsável

```text
Tecnico
```

### Cenário válido

1. O técnico está ativo.
2. A disponibilidade é alterada para `true`.
3. O sistema salva o novo estado.
4. O técnico passa a aparecer nas buscas compatíveis.

### Cenário inválido

1. O técnico é inativado.
2. É realizada uma tentativa de alterar sua disponibilidade para `true`.
3. O modelo rejeita a operação.
4. O estado inválido não é salvo.

### Regra no backend

A regra é protegida pelo método:

```csharp
AlterarDisponibilidade(bool disponivel)
```

O método lança uma exceção quando um técnico inativo tenta ficar disponível.

### Evidência

- Classe `Tecnico`.
- Endpoint de alteração de disponibilidade.
- Testes automatizados das regras de técnico.
- Teste manual pelo frontend.

## Regra 2 — Equipamento precisa pertencer ao cliente

### Descrição

Um cliente somente pode abrir uma solicitação utilizando um equipamento cadastrado em seu próprio perfil.

### Objeto responsável

```text
Solicitacao
```

### Cenário válido

1. O cliente possui o equipamento.
2. O cliente seleciona esse equipamento.
3. A solicitação é criada.
4. A solicitação fica armazenada no banco.

### Cenário inválido

1. O cliente informa um equipamento pertencente a outro cliente.
2. A solicitação tenta ser criada.
3. O domínio rejeita a operação.
4. Nenhuma solicitação inválida é persistida.

### Regra no backend

O construtor de `Solicitacao` compara:

```text
Equipamento.ClienteId
```

com:

```text
ClienteId da solicitação
```

A solicitação somente é criada quando os identificadores correspondem.

### Evidência

- Classe `Solicitacao`.
- `SolicitacoesController`.
- Testes automatizados de solicitações.
- Resposta de erro da API no cenário inválido.

## Regra 3 — Seleção recebe somente uma resposta

### Descrição

Uma seleção começa aguardando a resposta do técnico. Depois de aceita ou recusada, não pode receber uma nova resposta.

### Objeto responsável

```text
SelecaoTecnico
```

### Estados envolvidos

```text
AguardandoResposta
Aceita
Recusada
```

### Transições válidas

```text
AguardandoResposta → Aceita
AguardandoResposta → Recusada
```

### Transições inválidas

```text
Aceita → Recusada
Aceita → Aceita
Recusada → Aceita
Recusada → Recusada
```

### Cenário válido

1. A seleção está em `AguardandoResposta`.
2. O técnico aceita ou recusa.
3. O estado é atualizado.
4. A data da resposta é registrada.
5. O histórico recebe um novo registro.

### Cenário inválido

1. A seleção já foi aceita ou recusada.
2. É realizada uma nova tentativa de resposta.
3. O modelo rejeita a operação.
4. O estado e o histórico permanecem consistentes.

### Regra no backend

Os métodos:

```csharp
Aceitar()
Recusar()
```

verificam se o estado atual ainda é `AguardandoResposta`.

### Evidência

- Classe `SelecaoTecnico`.
- `MatchingController`.
- Classe `HistoricoSelecaoTecnico`.
- Testes de matching.
- Cenário inválido executado pela API.

## Outras regras implementadas

Além das três regras selecionadas para a matriz, o projeto possui:

- especialidade inativa não pode ser vinculada;
- técnico não pode receber especialidade repetida;
- anos de experiência não podem ser negativos;
- somente técnico ativo e disponível pode ser selecionado;
- técnico selecionado deve possuir a especialidade;
- uma solicitação não pode possuir duas seleções simultaneamente ativas;
- especialidade da seleção deve corresponder à solicitação.