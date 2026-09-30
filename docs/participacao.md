# Registro de Participação da AV1

## Objetivo

Este documento registra as principais contribuições, revisões, responsabilidades e evidências de participação dos integrantes no desenvolvimento da AV1.

A divisão de tarefas foi utilizada para organizar o trabalho. Todos os integrantes devem compreender o fluxo completo e conseguir explicar partes que não tenham desenvolvido individualmente.

## Quadro de participação

| Integrante | Contribuições principais | Revisões e integrações | Parte que consegue demonstrar | Evidências |
|---|---|---|---|---|
| Bernardo Awano Muniz | Abertura e listagem das solicitações | Integração da solicitação com cliente, equipamento e especialidade | Criação e consulta de solicitações | Models, DTOs, Controller, frontend, testes e commits |
| Kelvin Silva Costa | Clientes, endereços, equipamentos e login | Integração dos clientes e equipamentos ao projeto oficial | Cadastro, login, perfil e equipamentos | Models, DTOs, Controllers, frontend, testes e commits |
| Karina Lussac Carneiro | Matching e escolha de técnicos | Integração da busca por especialidade, disponibilidade e localização | Busca e seleção de técnico compatível | Model, DTO, Controller, frontend, testes e commits |
| Luiz Felipe de Oliveira Viana | Técnicos e especialidades | Integração geral dos módulos, resolução de conflitos, padronização do frontend e validações finais | Cadastro, perfil, disponibilidade e especialidades do técnico | Models, DTOs, Controllers, frontend, testes, integrações e commits |
| Maria Eduarda Fidelis | Aceite, recusa e histórico da seleção | Integração da resposta do técnico ao matching e à solicitação | Aceite, recusa e consulta do histórico | Models, DTO, endpoints, frontend, testes e commits |

## Organização do trabalho

O projeto foi dividido em cinco partes principais:

1. Cliente e equipamento.
2. Técnico e especialidade.
3. Abertura da solicitação.
4. Matching e escolha do técnico.
5. Aceite, recusa e histórico.

Cada parte foi desenvolvida separadamente e depois integrada ao projeto oficial.

## Integrações realizadas

As integrações incluíram:

- cliente relacionado aos equipamentos;
- cliente e equipamento relacionados à solicitação;
- solicitação relacionada à especialidade;
- técnico relacionado às especialidades;
- busca de técnicos baseada na especialidade;
- seleção relacionada à solicitação e ao técnico;
- resposta do técnico registrada no histórico;
- frontend integrado aos endpoints;
- banco atualizado por migrations;
- testes executados após as integrações.

## Revisões coletivas

Durante a integração, o grupo realizou ou acompanhou:

- conferência dos Models;
- conferência dos DTOs;
- conferência dos Controllers;
- resolução de conflitos do `AppDbContext`;
- resolução de conflitos do frontend;
- correção das migrations;
- testes dos endpoints;
- testes manuais pelo frontend;
- verificação do banco SQLite;
- compilação do backend;
- execução dos testes automatizados;
- lint e build do frontend;
- organização das branches e Pull Requests.

## Evidências no Git

As evidências de participação estão preservadas por meio de:

- branches de funcionalidades;
- commits;
- Pull Requests;
- arquivos implementados;
- testes;
- correções;
- histórico de integração.

A quantidade de commits não é utilizada isoladamente para definir participação. Também são consideradas as revisões, correções, integrações e a capacidade de demonstrar o funcionamento.

## Responsabilidade coletiva

Todos os integrantes devem conseguir explicar:

- o problema resolvido;
- o fluxo completo da AV1;
- como o frontend chama a API;
- como a API utiliza o Entity Framework;
- como os dados são persistidos no SQLite;
- pelo menos uma regra implementada no backend;
- a mudança de estado da seleção;
- pelo menos um cenário inválido;
- uma parte do sistema desenvolvida por outro integrante.

## Fluxo que todos devem conhecer

1. O cliente cria uma conta.
2. O cliente entra no sistema.
3. O cliente cadastra um equipamento.
4. O cliente abre uma solicitação.
5. O sistema procura técnicos compatíveis.
6. O cliente seleciona um técnico.
7. O técnico recebe a solicitação.
8. O técnico aceita ou recusa.
9. O sistema salva a resposta.
10. O histórico apresenta as mudanças de estado.

## Observação

Caso alguma contribuição ou revisão descrita neste documento não corresponda exatamente ao trabalho realizado, o integrante responsável deverá corrigir sua linha antes da entrega.