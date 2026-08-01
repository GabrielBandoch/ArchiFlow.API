using System.Net;
using ArchiFlow.API.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArchiFlow.Tests.Middleware;

public class ExceptionMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionMiddleware>> _loggerMock;

    public ExceptionMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_WhenNoError_DeveChamarProximoMiddleware()
    {
        var context = new DefaultHttpContext();
        var chamado = false;
        RequestDelegate next = (ctx) =>
        {
            chamado = true;
            return Task.CompletedTask;
        };

        var middleware = new ExceptionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        chamado.Should().BeTrue();
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task InvokeAsync_QuandoKeyNotFoundExceptionLancada_DeveRetornar404()
    {
        var context = new DefaultHttpContext();
        RequestDelegate next = (ctx) => throw new KeyNotFoundException("Não encontrado.");
        var middleware = new ExceptionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvokeAsync_QuandoArgumentExceptionLancada_DeveRetornar400()
    {
        var context = new DefaultHttpContext();
        RequestDelegate next = (ctx) => throw new ArgumentException("Invalido.");
        var middleware = new ExceptionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_QuandoGenericExceptionLancada_DeveRetornar500()
    {
        var context = new DefaultHttpContext();
        RequestDelegate next = (ctx) => throw new Exception("Falha grave.");
        var middleware = new ExceptionMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }
}
