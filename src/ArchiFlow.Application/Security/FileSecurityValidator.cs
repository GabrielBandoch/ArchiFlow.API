using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ArchiFlow.Application.Security;

public static class FileSecurityValidator
{
    private static readonly string[] ImagensPermitidas = [".jpg", ".jpeg", ".png", ".webp", ".svg", ".gif"];
    private static readonly Regex PadroesInsegurosSvg = new(
        @"(<script|javascript:|vbscript:|data:text\/html|<foreignobject|<iframe|<object|<embed|<!entity|\bon\w+\s*=)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(2));

    public static void ValidarImagem(Stream stream, string fileName, string contentType, long maxSizeBytes = 5 * 1024 * 1024)
    {
        if (stream == null || stream.Length == 0)
        {
            throw new ArgumentException("Arquivo vazio ou inválido.");
        }

        if (stream.Length > maxSizeBytes)
        {
            throw new ArgumentException($"O tamanho do arquivo excede o limite máximo permitido de {maxSizeBytes / (1024 * 1024)}MB.");
        }

        var ext = Path.GetExtension(fileName);
        if (!ImagensPermitidas.Any(p => string.Equals(p, ext, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Formato de imagem não suportado. Formatos permitidos: JPG, PNG, WEBP e SVG.");
        }

        if (string.Equals(ext, ".svg", StringComparison.OrdinalIgnoreCase))
        {
            ValidarSvg(stream);
        }
        else
        {
            ValidarAssinaturaBinaria(stream, ext);
        }
    }

    private static void ValidarSvg(Stream stream)
    {
        var startPos = stream.CanSeek ? stream.Position : 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var content = reader.ReadToEnd();

        if (stream.CanSeek)
        {
            stream.Position = startPos;
        }

        if (!content.Contains("<svg", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Arquivo SVG inválido.");
        }

        if (PadroesInsegurosSvg.IsMatch(content))
        {
            throw new InvalidOperationException("Conteúdo SVG potencialmente inseguro detectado.");
        }
    }

    private static void ValidarAssinaturaBinaria(Stream stream, string extension)
    {
        var startPos = stream.CanSeek ? stream.Position : 0;
        var header = new byte[12];
        var bytesRead = stream.Read(header, 0, header.Length);

        if (stream.CanSeek)
        {
            stream.Position = startPos;
        }

        if (bytesRead < 4)
        {
            throw new ArgumentException("Arquivo corrompido ou formato não reconhecido.");
        }

        var valido = extension switch
        {
            ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
            ".gif" => header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46,
            ".webp" => bytesRead >= 12 &&
                       header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                       header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50,
            _ => true
        };

        if (!valido)
        {
            throw new ArgumentException("O conteúdo do arquivo não corresponde à sua extensão.");
        }
    }
}
