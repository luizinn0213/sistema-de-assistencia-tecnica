namespace Assistencia.Api.Security;

public class JwtOptions
{
    public const string Secao = "Jwt";

    public string Issuer { get; set; } = "Assistencia.Api";
    public string Audience { get; set; } = "Assistencia.Frontend";
    public string Chave { get; set; } = string.Empty;
    public int ExpiraMinutos { get; set; } = 30;
    public int RefreshDias { get; set; } = 7;
}
