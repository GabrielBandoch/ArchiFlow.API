using System;
using System.IO;
using System.Text;
using ArchiFlow.Application.Security;
using FluentAssertions;
using Xunit;

namespace ArchiFlow.Tests.Security;

public class FileSecurityValidatorTests
{
    [Fact]
    public void ValidarImagem_QuandoArquivoVazio_LancaArgumentException()
    {
        using var stream = new MemoryStream();

        var act = () => FileSecurityValidator.ValidarImagem(stream, "teste.png", "image/png");

        act.Should().Throw<ArgumentException>().WithMessage("*Arquivo vazio ou inválido*");
    }

    [Fact]
    public void ValidarImagem_QuandoTamanhoExcedeLimite_LancaArgumentException()
    {
        var buffer = new byte[1024];
        using var stream = new MemoryStream(buffer);

        var act = () => FileSecurityValidator.ValidarImagem(stream, "teste.png", "image/png", maxSizeBytes: 512);

        act.Should().Throw<ArgumentException>().WithMessage("*excede o limite máximo permitido*");
    }

    [Fact]
    public void ValidarImagem_QuandoExtensaoNaoPermitida_LancaArgumentException()
    {
        using var stream = new MemoryStream(new byte[] { 0x01, 0x02 });

        var act = () => FileSecurityValidator.ValidarImagem(stream, "executavel.exe", "application/octet-stream");

        act.Should().Throw<ArgumentException>().WithMessage("*Formato de imagem não suportado*");
    }

    [Fact]
    public void ValidarImagem_QuandoPngValido_NaoLancaExcecao()
    {
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };
        using var stream = new MemoryStream(pngHeader);

        var act = () => FileSecurityValidator.ValidarImagem(stream, "logo.png", "image/png");

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidarImagem_QuandoJpegValido_NaoLancaExcecao()
    {
        var jpegHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 };
        using var stream = new MemoryStream(jpegHeader);

        var act = () => FileSecurityValidator.ValidarImagem(stream, "foto.jpg", "image/jpeg");

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidarImagem_QuandoWebpValido_NaoLancaExcecao()
    {
        var webpHeader = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 };
        using var stream = new MemoryStream(webpHeader);

        var act = () => FileSecurityValidator.ValidarImagem(stream, "imagem.webp", "image/webp");

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidarImagem_QuandoExtensaoPngMasCabecalhoInvalido_LancaArgumentException()
    {
        var fakeBytes = Encoding.UTF8.GetBytes("Este é um arquivo de texto fingindo ser PNG");
        using var stream = new MemoryStream(fakeBytes);

        var act = () => FileSecurityValidator.ValidarImagem(stream, "falso.png", "image/png");

        act.Should().Throw<ArgumentException>().WithMessage("*não corresponde à sua extensão*");
    }

    [Fact]
    public void ValidarImagem_QuandoSvgSeguro_NaoLancaExcecao()
    {
        var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"100\" height=\"100\"><circle cx=\"50\" cy=\"50\" r=\"40\" fill=\"red\" /></svg>";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));

        var act = () => FileSecurityValidator.ValidarImagem(stream, "icone.svg", "image/svg+xml");

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("<svg><script>alert(1)</script></svg>")]
    [InlineData("<svg><a href=\"javascript:alert(1)\"><text>Click</text></a></svg>")]
    [InlineData("<svg onload=\"alert(1)\"></svg>")]
    [InlineData("<svg><foreignObject><iframe src=\"http://malicious.com\"></iframe></foreignObject></svg>")]
    [InlineData("<!DOCTYPE svg [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><svg>&xxe;</svg>")]
    public void ValidarImagem_QuandoSvgInseguroComPayloadXSS_LancaInvalidOperationException(string maliciousSvg)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(maliciousSvg));

        var act = () => FileSecurityValidator.ValidarImagem(stream, "malicioso.svg", "image/svg+xml");

        act.Should().Throw<InvalidOperationException>().WithMessage("*inseguro detectado*");
    }
}
