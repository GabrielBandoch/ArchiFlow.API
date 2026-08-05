using ArchiFlow.API;
using FluentAssertions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ArchiFlow.Tests.Common;

public class EnvLoaderTests
{
    [Fact]
    public async Task LoadAsync_QuandoArquivoEnvExiste_DeveCarregarVariaveis()
    {
        var tempEnvPath = Path.Combine(AppContext.BaseDirectory, ".env");
        // Adicionada uma linha sem '=' (TEST_VAR_INVALID) para cobrir o branch parts.Length != 2
        await File.WriteAllTextAsync(tempEnvPath, "TEST_VAR_1=valor1\nTEST_VAR_2=valor2\n# Comentario\n\nTEST_VAR_INVALID\nTEST_VAR_3=valor=com=igual");

        try
        {
            await EnvLoader.LoadAsync();

            Environment.GetEnvironmentVariable("TEST_VAR_1").Should().Be("valor1");
            Environment.GetEnvironmentVariable("TEST_VAR_2").Should().Be("valor2");
            Environment.GetEnvironmentVariable("TEST_VAR_3").Should().Be("valor=com=igual");
            Environment.GetEnvironmentVariable("TEST_VAR_INVALID").Should().BeNull();
        }
        finally
        {
            if (File.Exists(tempEnvPath))
            {
                File.Delete(tempEnvPath);
            }
            Environment.SetEnvironmentVariable("TEST_VAR_1", null);
            Environment.SetEnvironmentVariable("TEST_VAR_2", null);
            Environment.SetEnvironmentVariable("TEST_VAR_3", null);
        }
    }

    [Fact]
    public async Task LoadAsync_QuandoArquivoEnvExisteNoDiretorioPai_DeveCarregarVariaveis()
    {
        var baseDir = new DirectoryInfo(AppContext.BaseDirectory);
        var parentDir = baseDir.Parent;
        if (parentDir != null)
        {
            var tempEnvPath = Path.Combine(parentDir.FullName, ".env");
            await File.WriteAllTextAsync(tempEnvPath, "PARENT_VAR=valor_pai");

            try
            {
                await EnvLoader.LoadAsync();
                Environment.GetEnvironmentVariable("PARENT_VAR").Should().Be("valor_pai");
            }
            finally
            {
                if (File.Exists(tempEnvPath))
                {
                    File.Delete(tempEnvPath);
                }
                Environment.SetEnvironmentVariable("PARENT_VAR", null);
            }
        }
    }

    [Fact]
    public async Task LoadAsync_QuandoArquivoEnvNaoExiste_NaoDeveFalhar()
    {
        // Garante que o arquivo .env temporário local e em níveis acima não exista neste escopo de teste isolado
        var tempEnvPath = Path.Combine(AppContext.BaseDirectory, ".env");
        if (File.Exists(tempEnvPath))
        {
            File.Delete(tempEnvPath);
        }

        var act = async () => await EnvLoader.LoadAsync();

        await act.Should().NotThrowAsync();
    }
}
