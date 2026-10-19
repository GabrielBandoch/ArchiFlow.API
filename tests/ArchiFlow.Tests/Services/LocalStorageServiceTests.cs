using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ArchiFlow.Domain.Shared;
using ArchiFlow.Infrastructure.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Services;

public class LocalStorageServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<ITenantContext> _tenantContextMock = new();

    public LocalStorageServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "archiflow_storage_tests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
        _tenantContextMock.Setup(t => t.TenantId).Returns("tenant-alfa");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task UploadAsync_SalvaArquivoNoDiretorioDoTenantCorreto()
    {
        var sut = new LocalStorageService(_tenantContextMock.Object, _tempDir);
        var content = "conteudo do arquivo de teste";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        var url = await sut.UploadAsync(stream, "documento.pdf", "application/pdf", "projetos", "p1");

        url.Should().StartWith("/uploads/tenants/tenant-alfa/projetos/p1/");
        var localPath = Path.Combine(_tempDir, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        File.Exists(localPath).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_QuandoTentativaDeTravessiaDeDiretorio_LancaInvalidOperationException()
    {
        var sut = new LocalStorageService(_tenantContextMock.Object, _tempDir);

        var act = () => sut.DeleteAsync("/uploads/../../etc/passwd");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*travessia de diretório*");
    }

    [Fact]
    public async Task DeleteAsync_QuandoArquivoPertenceAOutroTenant_LancaUnauthorizedAccessException()
    {
        var sut = new LocalStorageService(_tenantContextMock.Object, _tempDir);

        var act = () => sut.DeleteAsync("/uploads/tenants/tenant-beta/documentos/arquivo.pdf");

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*pertence a outro tenant*");
    }

    [Fact]
    public async Task DownloadAsync_QuandoArquivoPertenceAOutroTenant_LancaUnauthorizedAccessException()
    {
        var sut = new LocalStorageService(_tenantContextMock.Object, _tempDir);

        var act = () => sut.DownloadAsync("/uploads/tenants/tenant-beta/documentos/arquivo.pdf");

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*pertence a outro tenant*");
    }

    [Fact]
    public async Task DownloadAsync_QuandoArquivoDoMesmoTenantExiste_RetornaStream()
    {
        var sut = new LocalStorageService(_tenantContextMock.Object, _tempDir);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("dados"));
        var url = await sut.UploadAsync(stream, "dados.txt", "text/plain", "geral");

        using var downloadStream = await sut.DownloadAsync(url);

        downloadStream.Should().NotBeNull();
    }
}
