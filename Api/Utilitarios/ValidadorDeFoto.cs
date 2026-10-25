namespace Api.Utilitarios;

public static class ValidadorDeFoto
{
    private const long TamanhoMaximoEmBytes = 5 * 1024 * 1024;
    private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

    public static string? Validar(IFormFile? foto)
    {
        if (foto == null || foto.Length == 0) return null;

        if (foto.Length > TamanhoMaximoEmBytes)
            return "A foto deve ter no máximo 5 MB.";

        var extensao = Path.GetExtension(foto.FileName).ToLowerInvariant();
        if (!ExtensoesPermitidas.Contains(extensao) || !foto.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return "Envie a foto em JPG, PNG ou WEBP.";

        return null;
    }

    public static string NomeSeguro(IFormFile foto)
    {
        return $"{Guid.NewGuid()}{Path.GetExtension(foto.FileName).ToLowerInvariant()}";
    }
}