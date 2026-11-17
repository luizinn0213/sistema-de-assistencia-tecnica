# Especificação — Diagnóstico e Orçamento

## Objetivo

Implementar o fluxo de diagnóstico técnico e elaboração de orçamento da AV2 do Sistema de Assistência Técnica.

Esta parte começa depois que o técnico aceita uma solicitação e termina quando o cliente aprova ou rejeita o orçamento.

## Responsável

Luiz Felipe de Oliveira Viana.

Branch:

```text
feature/luiz-diagnostico-orcamento
```

## Escopo

Esta implementação contempla:

- registro do diagnóstico técnico;
- descrição do defeito encontrado;
- descrição da solução recomendada;
- conclusão do diagnóstico;
- criação de orçamento para o diagnóstico concluído;
- inclusão de peças e serviços no orçamento;
- cálculo automático dos valores;
- envio do orçamento ao cliente;
- aprovação ou rejeição pelo cliente;
- registro do histórico do orçamento;
- testes das principais regras de negócio;
- telas integradas à API.

Não fazem parte deste módulo:

- autenticação JWT;
- execução do reparo;
- pagamento;
- comissão;
- avaliação do atendimento;
- matching avançado.

## Fluxo principal

1. O cliente abre uma solicitação.
2. Um técnico compatível é selecionado.
3. O técnico aceita a solicitação.
4. O técnico registra o diagnóstico.
5. O diagnóstico informa o defeito e a solução recomendada.
6. O técnico conclui o diagnóstico.
7. Um orçamento é criado para o diagnóstico.
8. Peças e serviços são adicionados ao orçamento.
9. O sistema calcula o valor total.
10. O orçamento é enviado ao cliente.
11. O cliente aprova ou rejeita.
12. A decisão é armazenada no histórico.

## Entidades

### Diagnostico

Representa a análise realizada pelo técnico sobre uma solicitação aceita.

Propriedades principais:

- `Id`;
- `SolicitacaoId`;
- `TecnicoId`;
- `DescricaoDefeito`;
- `SolucaoRecomendada`;
- `Status`;
- `DataCriacao`;
- `DataConclusao`.

Comportamentos:

- registrar ou alterar as informações do diagnóstico;
- concluir o diagnóstico;
- impedir conclusão sem defeito e solução;
- impedir alteração depois da conclusão.

### Orcamento

Representa a proposta de preço gerada a partir de um diagnóstico concluído.

Propriedades principais:

- `Id`;
- `DiagnosticoId`;
- `Status`;
- `DataCriacao`;
- `DataEnvio`;
- `DataResposta`;
- coleção de itens;
- coleção de registros de histórico;
- valor total calculado.

Comportamentos:

- adicionar item;
- remover item;
- calcular o total;
- enviar ao cliente;
- aprovar;
- rejeitar;
- registrar mudanças no histórico.

### ItemOrcamento

Representa uma peça ou um serviço incluído no orçamento.

Propriedades principais:

- `Id`;
- `OrcamentoId`;
- `Descricao`;
- `Tipo`;
- `Quantidade`;
- `ValorUnitario`;
- `ValorTotal`.

O valor total do item deve ser calculado por:

```text
Quantidade × ValorUnitario
```

### HistoricoOrcamento

Registra os eventos relevantes do orçamento.

Propriedades principais:

- `Id`;
- `OrcamentoId`;
- `Status`;
- `Observacao`;
- `DataRegistro`.

## Enumerações

### StatusDiagnostico

```text
EmElaboracao
Concluido
```

### TipoItemOrcamento

```text
Peca
Servico
```

### StatusOrcamento

```text
EmElaboracao
Enviado
Aprovado
Rejeitado
Cancelado
```

## Mudanças de estado

### Diagnóstico

```text
EmElaboracao → Concluido
```

Depois de concluído, o diagnóstico não pode ser editado ou concluído novamente.

### Orçamento

```text
EmElaboracao → Enviado
Enviado → Aprovado
Enviado → Rejeitado
EmElaboracao → Cancelado
Enviado → Cancelado
```

Um orçamento aprovado, rejeitado ou cancelado não pode receber outra resposta.

## Regras de negócio

1. Somente uma solicitação com seleção aceita pode receber diagnóstico.
2. O técnico do diagnóstico deve ser o mesmo que aceitou a solicitação.
3. Uma solicitação não pode possuir dois diagnósticos ativos.
4. A descrição do defeito é obrigatória.
5. A solução recomendada é obrigatória para concluir o diagnóstico.
6. Um diagnóstico concluído não pode ser alterado.
7. Somente um diagnóstico concluído pode gerar orçamento.
8. O orçamento precisa possuir pelo menos um item para ser enviado.
9. A descrição do item é obrigatória.
10. A quantidade do item deve ser maior que zero.
11. O valor unitário não pode ser negativo.
12. O valor total deve ser calculado pelo domínio.
13. Somente orçamento enviado pode ser aprovado ou rejeitado.
14. Um orçamento respondido não pode receber uma segunda resposta.
15. Cada mudança relevante deve gerar um registro no histórico.

## Organização arquitetural

Os controllers devem cuidar somente das responsabilidades HTTP:

- receber os dados;
- chamar o serviço correspondente;
- converter o resultado em resposta HTTP.

As regras de aplicação devem ficar nos serviços:

- `IDiagnosticoService`;
- `IOrcamentoService`;
- `DiagnosticoService`;
- `OrcamentoService`.

O acesso aos dados deve utilizar contratos:

- `IDiagnosticoRepository`;
- `IOrcamentoRepository`;
- `ISolicitacaoRepository`;
- `ISelecaoTecnicoRepository`.

As implementações dos repositórios usarão o Entity Framework Core e o `AppDbContext`.

As invariantes internas permanecerão protegidas pelas próprias entidades.

## Endpoints planejados

### Diagnósticos

```text
POST   /api/diagnosticos
GET    /api/diagnosticos/{id}
GET    /api/diagnosticos/solicitacao/{solicitacaoId}
PUT    /api/diagnosticos/{id}
POST   /api/diagnosticos/{id}/concluir
```

### Orçamentos

```text
POST   /api/orcamentos
GET    /api/orcamentos/{id}
GET    /api/orcamentos/diagnostico/{diagnosticoId}
POST   /api/orcamentos/{id}/itens
DELETE /api/orcamentos/{id}/itens/{itemId}
POST   /api/orcamentos/{id}/enviar
POST   /api/orcamentos/{id}/aprovar
POST   /api/orcamentos/{id}/rejeitar
GET    /api/orcamentos/{id}/historico
```

## Respostas HTTP esperadas

- `200 OK`: consulta ou alteração realizada;
- `201 Created`: recurso criado;
- `400 Bad Request`: regra ou dados inválidos;
- `404 Not Found`: recurso não encontrado;
- `409 Conflict`: estado atual impede a operação.

## Testes necessários

### Diagnóstico

- deve criar diagnóstico para seleção aceita;
- não deve criar diagnóstico sem seleção aceita;
- não deve aceitar técnico diferente;
- deve concluir diagnóstico válido;
- não deve concluir sem solução;
- não deve alterar diagnóstico concluído;
- não deve criar diagnóstico duplicado.

### Orçamento

- deve criar orçamento para diagnóstico concluído;
- não deve criar para diagnóstico em elaboração;
- deve adicionar peça;
- deve adicionar serviço;
- deve calcular o total corretamente;
- não deve aceitar quantidade inválida;
- não deve aceitar valor negativo;
- não deve enviar orçamento vazio;
- deve aprovar orçamento enviado;
- deve rejeitar orçamento enviado;
- não deve responder duas vezes;
- deve registrar o histórico.

## Critérios de conclusão

A funcionalidade será considerada concluída quando:

- o backend compilar;
- todas as migrations forem aplicadas;
- todos os testes passarem;
- o lint do frontend passar;
- o frontend compilar;
- o fluxo funcionar pela interface;
- as regras inválidas forem rejeitadas;
- os controllers não concentrarem as regras de negócio;
- serviços e repositórios forem utilizados por interfaces.