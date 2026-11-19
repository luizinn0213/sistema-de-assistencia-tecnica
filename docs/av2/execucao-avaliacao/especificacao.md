# Execução e Avaliação — AV2

## Fluxo

```text
Orçamento aprovado
        ↓
Execução aguardando início
        ↓
Execução em andamento
        ↓
Execução concluída
        ↓
Avaliação registrada
```

## Regras principais

1. Somente um orçamento aprovado pode gerar uma execução.
2. Um orçamento pode possuir somente uma execução.
3. Somente uma execução aguardando início pode ser iniciada.
4. Somente uma execução em andamento pode ser concluída.
5. Início e conclusão são registrados no histórico.
6. Somente uma execução concluída pode ser avaliada.
7. Somente o cliente da solicitação pode avaliar.
8. Uma execução pode possuir somente uma avaliação.
9. A nota deve estar entre 1 e 5.
10. O valor do orçamento não é duplicado em outras entidades.
