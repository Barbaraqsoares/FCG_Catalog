using System.Text.Json;
using FCG_CatalogAPI.Adapters.Middlewares;
using FCG_CatalogAPI.Adapters.Outbound;
using FCG_CatalogAPI.Domain.Events;
using FCG_CatalogAPI.Domain.Exceptions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FCG_CatalogAPI.Tests;

public class PublisherAndMiddlewareTests
{
    [Fact]
    public async Task RabbitMqEventPublisher_DelegatesMessageToMassTransit()
    {
        var endpoint = new Mock<IPublishEndpoint>();
        var logger = new Mock<ILogger<OrderPlacedPublisher>>();
        var publisher = new OrderPlacedPublisher(endpoint.Object, logger.Object);
        using var cancellation = new CancellationTokenSource();
        var message = new OrderPlacedCommand(Guid.NewGuid(), 1, 2, 3m);

        await publisher.Publish(message, cancellation.Token);

        endpoint.Verify(x => x.Publish(message, cancellation.Token), Times.Once);
    }

    [Fact]
    public async Task ExceptionMiddleware_ContinuesWhenNoExceptionIsThrown()
    {
        var nextCalled = false;
        var middleware = new ExceptionHandlingMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(new DefaultHttpContext());

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task ExceptionMiddleware_DoesNotRewriteResponseAfterItHasStarted()
    {
        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(new StartedResponseFeature());
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("internal"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.True(context.Response.HasStarted);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("not-found", 404, "Recurso não encontrado")]
    [InlineData("conflict", 409, "Conflito com o estado atual do recurso")]
    [InlineData("forbidden", 403, "Acesso restrito")]
    [InlineData("domain", 400, "Requisição inválida")]
    [InlineData("unauthorized", 401, "Não autorizado")]
    [InlineData("unexpected", 500, "Erro interno do servidor")]
    public async Task ExceptionMiddleware_MapsExceptionsToProblemDetails(string kind, int status, string title)
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw ExceptionFor(kind),
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Request.Path = "/test";
        context.Request.Method = "POST";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(status, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        context.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(title, body.RootElement.GetProperty("title").GetString());
        Assert.Equal("/test", body.RootElement.GetProperty("instance").GetString());
        Assert.True(body.RootElement.TryGetProperty("traceId", out _));
        Assert.Equal(
            status == 500 ? "Ocorreu um erro inesperado. Informe o traceId ao suporte." : "details",
            body.RootElement.GetProperty("detail").GetString());
    }

    private static Exception ExceptionFor(string kind) => kind switch
    {
        "not-found" => new NotFoundException("details"),
        "conflict" => new ConflictException("details"),
        "forbidden" => new ForbiddenException("details"),
        "domain" => new DomainException("details"),
        "unauthorized" => new UnauthorizedAccessException("details"),
        _ => new InvalidOperationException("internal details")
    };

    private sealed class StartedResponseFeature : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public Stream Body { get; set; } = Stream.Null;
        public bool HasStarted => true;
        public void OnStarting(Func<object, Task> callback, object state) { }
        public void OnCompleted(Func<object, Task> callback, object state) { }
    }
}
