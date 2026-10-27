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

        using var fluxo = foto.OpenReadStream();
        if (!ConteudoEhImagemPermitida(fluxo))
            return "O arquivo enviado não é uma imagem válida (JPG, PNG ou WEBP).";

        return null;
    }

    public static bool ConteudoEhImagemPermitida(Stream fluxo)
    {
        var cabecalho = new byte[12];
        var bytesLidos = 0;
        while (bytesLidos < cabecalho.Length)
        {
            var lidosAgora = fluxo.Read(cabecalho, bytesLidos, cabecalho.Length - bytesLidos);
            if (lidosAgora == 0) break;
            bytesLidos += lidosAgora;
        }

        var ehJpeg = bytesLidos >= 3
            && cabecalho[0] == 0xFF && cabecalho[1] == 0xD8 && cabecalho[2] == 0xFF;

        var ehPng = bytesLidos >= 8
            && cabecalho[0] == 0x89 && cabecalho[1] == 0x50 && cabecalho[2] == 0x4E && cabecalho[3] == 0x47
            && cabecalho[4] == 0x0D && cabecalho[5] == 0x0A && cabecalho[6] == 0x1A && cabecalho[7] == 0x0A;

        var ehWebp = bytesLidos >= 12
            && cabecalho[0] == 0x52 && cabecalho[1] == 0x49 && cabecalho[2] == 0x46 && cabecalho[3] == 0x46
            && cabecalho[8] == 0x57 && cabecalho[9] == 0x45 && cabecalho[10] == 0x42 && cabecalho[11] == 0x50;

        return ehJpeg || ehPng || ehWebp;
    }

    public static string NomeSeguro(IFormFile foto)
    {
        return $"{Guid.NewGuid()}{Path.GetExtension(foto.FileName).ToLowerInvariant()}";
    }
}