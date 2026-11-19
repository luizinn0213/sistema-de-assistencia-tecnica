# Autenticação JWT — AV2

## Fluxo

```text
Login -> access token + refresh token
Requisição protegida -> Bearer access token
Access expirado -> rotação do refresh token
Logout/troca de senha -> revogação dos refresh tokens
```

## Regras

1. A senha continua armazenada somente como hash BCrypt.
2. O refresh token é armazenado somente como hash SHA-256.
3. Cada refresh token pode ser utilizado uma única vez.
4. O refresh utilizado é revogado e substituído por outro.
5. Logout e troca de senha revogam as sessões renováveis.
6. O token contém identidade, perfil e cliente, quando existir.
7. A autorização será aplicada gradualmente para preservar o fluxo existente.
8. A chave de assinatura não deve ser versionada.
