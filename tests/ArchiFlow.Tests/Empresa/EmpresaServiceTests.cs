using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ArchiFlow.Application.Empresa.Commands;
using ArchiFlow.Application.Empresa.Services;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Empresa;
using ArchiFlow.Domain.Shared;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Empresa;

public class EmpresaServiceTests
{
    private readonly Mock<IDadosEmpresaRepository> _repoMock = new();
    private readonly Mock<IStorageService> _storageMock = new();
    private readonly Mock<ILogger<EmpresaService>> _loggerMock = new();
    private readonly EmpresaService _service;

    public EmpresaServiceTests()
    {
        _service = new EmpresaService(
            _repoMock.Object,
            _storageMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ObterDadosEmpresaAsync_WhenNoRecord_ReturnsDefaultDto()
    {
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync((DadosEmpresa?)null);

        var dto = await _service.ObterDadosEmpresaAsync();

        dto.Should().NotBeNull();
        dto.NomeEmpresa.Should().Be("COTU Studio");
        dto.CorPrimaria.Should().Be("#765538");
    }

    [Fact]
    public async Task SalvarDadosEmpresaAsync_WhenValidCommand_UpdatesAndSaves()
    {
        var existing = new DadosEmpresa
        {
            Id = Guid.NewGuid(),
            NomeEmpresa = "Antiga",
            CorPrimaria = "#000000"
        };
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(existing);

        var command = new SalvarDadosEmpresaCommand(
            NomeEmpresa: "COTU Studio",
            RazaoSocial: "COTU Studio Arquitetura LTDA",
            Cnpj: "12.345.678/0001-90",
            RegistroProfissional: "CAU A12345-6",
            Email: "contato@cotu.com.br",
            Telefone: "(11) 98765-4321",
            Endereco: "Av. Paulista, 1000",
            Slogan: "Arquitetura com propósito",
            CorPrimaria: "#223344"
        );

        var result = await _service.SalvarDadosEmpresaAsync(command);

        result.NomeEmpresa.Should().Be("COTU Studio");
        result.CorPrimaria.Should().Be("#223344");
        _repoMock.Verify(r => r.SalvarAsync(existing), Times.Once);
    }

    [Fact]
    public async Task AtualizarLogotipoAsync_WhenValidImage_UploadsAndUpdates()
    {
        var existing = new DadosEmpresa { Id = Guid.NewGuid(), NomeEmpresa = "COTU" };
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(existing);
        _storageMock.Setup(s => s.UploadAsync(It.IsAny<Stream>(), "logo.png", "image/png", "logos", null))
            .ReturnsAsync("https://storage/logo.png");

        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 };
        using var stream = new MemoryStream(pngBytes);
        var url = await _service.AtualizarLogotipoAsync(stream, "logo.png", "image/png");

        url.Should().Be("https://storage/logo.png");
        existing.LogoUrl.Should().Be("https://storage/logo.png");
        _repoMock.Verify(r => r.SalvarAsync(existing), Times.Once);
    }

    [Fact]
    public async Task AtualizarLogotipoAsync_WhenInvalidExtension_ThrowsArgumentException()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake"));
        var act = async () => await _service.AtualizarLogotipoAsync(stream, "script.exe", "application/octet-stream");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Formatos permitidos*");
    }

    [Fact]
    public async Task AtualizarCorPrimariaAsync_WhenValidColor_UpdatesAndReturnsDto()
    {
        var existing = new DadosEmpresa { Id = Guid.NewGuid(), NomeEmpresa = "COTU" };
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(existing);

        var result = await _service.AtualizarCorPrimariaAsync("#112233");

        result.CorPrimaria.Should().Be("#112233");
        existing.CorPrimaria.Should().Be("#112233");
        _repoMock.Verify(r => r.SalvarAsync(existing), Times.Once);
    }

    [Fact]
    public async Task AtualizarLogotipoAsync_WhenPreviousLogoExists_DeletesPreviousLogo()
    {
        var existing = new DadosEmpresa { Id = Guid.NewGuid(), NomeEmpresa = "COTU", LogoUrl = "https://storage/old.png" };
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(existing);
        _storageMock.Setup(s => s.UploadAsync(It.IsAny<Stream>(), "new.png", "image/png", "logos", null))
            .ReturnsAsync("https://storage/new.png");

        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 };
        using var stream = new MemoryStream(pngBytes);
        await _service.AtualizarLogotipoAsync(stream, "new.png", "image/png");

        _storageMock.Verify(s => s.DeleteAsync("https://storage/old.png"), Times.Once);
    }

    [Fact]
    public async Task RemoverLogotipoAsync_WhenLogoExists_DeletesFromStorageAndClearsLogo()
    {
        var existing = new DadosEmpresa { Id = Guid.NewGuid(), NomeEmpresa = "COTU", LogoUrl = "https://storage/logo.png" };
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(existing);

        await _service.RemoverLogotipoAsync();

        existing.LogoUrl.Should().BeNull();
        _storageMock.Verify(s => s.DeleteAsync("https://storage/logo.png"), Times.Once);
        _repoMock.Verify(r => r.SalvarAsync(existing), Times.Once);
    }
}

