using System.ComponentModel.DataAnnotations;

namespace Api;

// ============================================================
// AUTENTICAÇÃO
// ============================================================
public sealed record LoginDto(
    [property: Required(ErrorMessage = "Informe o e-mail."),
              EmailAddress(ErrorMessage = "E-mail inválido."),
              StringLength(160)]
    string Email,

    [property: Required(ErrorMessage = "Informe a senha."),
              StringLength(64, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 64 caracteres.")]
    string Senha);

public sealed record RefreshDto(
    [property: Required(ErrorMessage = "Informe o refresh token.")]
    string RefreshToken);

public sealed record TrocaSenhaDto(
    [property: Required(ErrorMessage = "Informe a senha atual.")]
    string SenhaAtual,

    [property: Required(ErrorMessage = "Informe a nova senha."),
              StringLength(64, MinimumLength = 6, ErrorMessage = "A nova senha deve ter entre 6 e 64 caracteres.")]
    string NovaSenha);

// ============================================================
// CLIENTE
// ============================================================
public sealed record EnderecoDto(
    [property: Required(ErrorMessage = "Informe o CEP."), StringLength(9)]
    string Cep,

    [property: Required(ErrorMessage = "Informe o logradouro."), StringLength(120, MinimumLength = 3)]
    string Logradouro,

    [property: Required(ErrorMessage = "Informe o número."), StringLength(10)]
    string Numero,

    [property: Required(ErrorMessage = "Informe o bairro."), StringLength(80, MinimumLength = 2)]
    string Bairro,

    [property: Required(ErrorMessage = "Informe a cidade."), StringLength(80, MinimumLength = 2)]
    string Cidade,

    [property: Required(ErrorMessage = "Informe o estado (UF)."), StringLength(2, MinimumLength = 2)]
    string Estado);

public sealed record RegistroClienteDto(
    [property: Required(ErrorMessage = "Informe o nome."), StringLength(120, MinimumLength = 3)]
    string Nome,

    [property: Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "E-mail inválido."), StringLength(160)]
    string Email,

    [property: Required(ErrorMessage = "Informe a senha."), StringLength(64, MinimumLength = 6)]
    string Senha,

    [property: Phone(ErrorMessage = "Telefone inválido."), StringLength(20)]
    string? Telefone,

    [property: Required(ErrorMessage = "Informe o CPF ou CNPJ."), StringLength(18, MinimumLength = 11)]
    string CpfCnpj,

    EnderecoDto? Endereco);

public sealed record ClienteUpdateDto(
    [property: Required(ErrorMessage = "Informe o nome."), StringLength(120, MinimumLength = 3)]
    string Nome,

    [property: Phone(ErrorMessage = "Telefone inválido."), StringLength(20)]
    string? Telefone,

    EnderecoDto? Endereco);

// ============================================================
// EQUIPAMENTO
// ============================================================
public sealed record EquipamentoDto(
    [property: Required(ErrorMessage = "Informe o tipo."), StringLength(40, MinimumLength = 2)]
    string Tipo,

    [property: Required(ErrorMessage = "Informe a marca."), StringLength(60, MinimumLength = 2)]
    string Marca,

    [property: Required(ErrorMessage = "Informe o modelo."), StringLength(60, MinimumLength = 1)]
    string Modelo,

    [property: Required(ErrorMessage = "Informe o número de série."), StringLength(60, MinimumLength = 4)]
    string NumeroSerie);

// ============================================================
// USUÁRIOS (gestão de perfis — Admin)
// ============================================================
public sealed record NovoUsuarioEquipeDto(
    [property: Required(ErrorMessage = "Informe o nome."), StringLength(120, MinimumLength = 3)]
    string Nome,

    [property: Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "E-mail inválido."), StringLength(160)]
    string Email,

    [property: Required(ErrorMessage = "Informe a senha."), StringLength(64, MinimumLength = 6)]
    string Senha,

    [property: Phone(ErrorMessage = "Telefone inválido."), StringLength(20)]
    string? Telefone,

    [property: Required(ErrorMessage = "Informe o perfil."), EnumDataType(typeof(TipoUsuario), ErrorMessage = "Perfil inválido.")]
    TipoUsuario Tipo);

public sealed record AlterarPerfilDto(
    [property: Required(ErrorMessage = "Informe o perfil."), EnumDataType(typeof(TipoUsuario), ErrorMessage = "Perfil inválido.")]
    TipoUsuario Tipo);

public sealed record AlterarStatusDto(
    [property: Required(ErrorMessage = "Informe se o usuário está ativo.")]
    bool? Ativo);

// ============================================================
// RESPOSTAS
// ============================================================
public sealed record UsuarioRespostaDto(
    int Id,
    string Nome,
    string Email,
    string? Telefone,
    string Perfil,
    int? ClienteId);

public sealed record TokenRespostaDto(
    string AccessToken,
    string TokenType,
    int ExpiraEmSegundos,
    string RefreshToken,
    DateTime RefreshExpiraEm,
    UsuarioRespostaDto Usuario);

public sealed record RegistroClienteRespostaDto(
    int ClienteId,
    string Mensagem,
    TokenRespostaDto Tokens);

public sealed record EnderecoRespostaDto(
    string Cep,
    string Logradouro,
    string Numero,
    string Bairro,
    string Cidade,
    string Estado);

public sealed record ClienteResumoDto(
    int Id,
    string Nome,
    string Email,
    string? Telefone,
    string CpfCnpj,
    bool Ativo);

public sealed record ClienteDetalheDto(
    int Id,
    string Nome,
    string Email,
    string? Telefone,
    string CpfCnpj,
    bool Ativo,
    DateTime CriadoEm,
    EnderecoRespostaDto? Endereco);

public sealed record EquipamentoRespostaDto(
    int Id,
    int ClienteId,
    string Tipo,
    string Marca,
    string Modelo,
    string NumeroSerie,
    DateTime CriadoEm);

public sealed record UsuarioListaDto(
    int Id,
    string Nome,
    string Email,
    string? Telefone,
    string Perfil,
    bool Ativo,
    DateTime CriadoEm);

/// <summary>Envelope padrão de listagens paginadas.</summary>
public sealed record PaginaDto<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalPaginas,
    int Total);
