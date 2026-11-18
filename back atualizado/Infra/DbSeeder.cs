using Microsoft.EntityFrameworkCore;

namespace Api.Infra;

/// <summary>
/// Dados iniciais: cria o administrador padrão (para o primeiro login) e
/// limpa refresh tokens antigos. O e-mail/senha vêm da seção "Seed".
/// </summary>
public static class DbSeeder
{
    public static async Task ExecutarAsync(AppDbContext db, IConfiguration config, ILogger log)
    {
        // Higiene: remove refresh tokens vencidos há mais de 30 dias.
        try
        {
            var limite = DateTime.UtcNow.AddDays(-30);
            var removidos = await db.RefreshTokens
                .Where(t => t.ExpiraEm < limite)
                .ExecuteDeleteAsync();

            if (removidos > 0)
                log.LogInformation("{Quantidade} refresh token(s) expirado(s) removido(s).", removidos);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Não foi possível limpar os refresh tokens expirados.");
        }

        if (!config.GetValue("Seed:Criar", true))
            return;

        var email = (config["Seed:AdminEmail"] ?? "admin@assistencia.local").Trim().ToLowerInvariant();
        var senha = config["Seed:AdminSenha"] ?? "Admin@123456";

        if (await db.Usuarios.AnyAsync(u => u.Tipo == TipoUsuario.Administrador))
            return;

        db.Usuarios.Add(new Usuario
        {
            Nome = "Administrador",
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
            Tipo = TipoUsuario.Administrador,
            Ativo = true
        });

        await db.SaveChangesAsync();
        log.LogInformation("Administrador inicial criado: {Email}", email);
    }
}
