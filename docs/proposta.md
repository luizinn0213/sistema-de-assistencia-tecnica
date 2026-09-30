# Proposta do Projeto Integrador

## Identificação

- Nome do projeto: Sistema de Assistência Técnica
- Nome apresentado na interface: Os Narggets
- Integrantes:
    - Bernardo Awano Muniz - 06014676
    - Kelvin Silva Costa - 06015489
    - Karina Lussac Carneiro - 06006762
    - Luiz Felipe de Oliveira Viana - 06914935
    - Maria Eduarda Mendes Fidelis - 06013817

## Problema

Pessoas que precisam consertar equipamentos podem encontrar dificuldades para localizar profissionais que atendam ao tipo de equipamento e ao problema apresentado.

Ao mesmo tempo, técnicos precisam divulgar suas especialidades, disponibilidade e experiência para receber solicitações compatíveis com suas capacidades.

O sistema busca organizar esse processo, permitindo que clientes cadastrem seus equipamentos, abram solicitações e encontrem técnicos com especialidades compatíveis.

## Público

O sistema possui dois públicos principais:

- clientes que necessitam de assistência técnica;
- técnicos que realizam manutenção em equipamentos.

## Processo principal

O cliente cria uma conta, cadastra um equipamento e abre uma solicitação informando o problema e a especialidade necessária.

O sistema procura técnicos ativos, disponíveis e vinculados à especialidade da solicitação. O cliente seleciona um profissional e o técnico pode aceitar ou recusar o atendimento.

A resposta do técnico é armazenada e apresentada no histórico da seleção.

## Conceitos do domínio

### Usuário

Representa uma pessoa que possui acesso ao sistema.

### Cliente

Representa o usuário que possui equipamentos e abre solicitações de serviço.

### Equipamento

Representa o item pertencente ao cliente que precisa de manutenção.

### Técnico

Representa o profissional que presta os serviços de assistência técnica.

### Especialidade

Representa uma área de atuação, como manutenção de celulares ou notebooks.

### Solicitação

Representa o pedido de assistência criado por um cliente para determinado equipamento.

### Seleção de técnico

Representa a escolha de um técnico para atender uma solicitação.

### Histórico da seleção

Registra as mudanças de estado ocorridas desde a seleção até sua aceitação ou recusa.

## Relacionamentos principais

- Um usuário pode possuir um perfil de cliente.
- Um cliente pode possuir vários equipamentos.
- Um equipamento pertence a um cliente.
- Um técnico pode possuir várias especialidades.
- Uma especialidade pode estar vinculada a vários técnicos.
- Uma solicitação pertence a um cliente.
- Uma solicitação utiliza um equipamento do cliente.
- Uma solicitação exige uma especialidade.
- Uma seleção relaciona uma solicitação a um técnico.
- Uma seleção pode possuir vários registros de histórico.

## Regras principais

1. Um técnico inativo não pode ficar disponível.
2. Uma especialidade inativa não pode ser adicionada a um técnico.
3. Um técnico não pode receber a mesma especialidade duas vezes.
4. Um equipamento utilizado na solicitação deve pertencer ao cliente.
5. Uma solicitação deve utilizar uma especialidade ativa.
6. Apenas técnicos ativos e disponíveis podem ser selecionados.
7. O técnico selecionado precisa possuir a especialidade exigida.
8. Uma seleção aceita ou recusada não pode receber outra resposta.
9. Uma solicitação não pode possuir duas seleções simultaneamente ativas.

## Mudanças de estado

A principal mudança de estado apresentada na AV1 ocorre na seleção do técnico:

```text
AguardandoResposta → Aceita
AguardandoResposta → Recusada
```

Depois que a seleção for aceita ou recusada, ela não poderá receber uma segunda resposta.