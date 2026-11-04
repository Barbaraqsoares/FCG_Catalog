using FCG_CatalogAPI.Adapters.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FCG_CatalogAPI.Tests;

public class InfrastructurePipelineTests
{
    [Fact]
    public async Task UseInfrastructurePipeline_ConfiguresApplicationPipeline()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        await using var app = builder.Build();

        var result = app.UseInfrastructurePipeline();

        Assert.Same(app, result);
    }
}