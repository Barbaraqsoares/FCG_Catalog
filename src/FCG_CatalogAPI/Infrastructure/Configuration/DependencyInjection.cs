using FCG_CatalogAPI.Adapters.Outbound;
using FCG_CatalogAPI.Application.UseCases;
using FCG_CatalogAPI.Domain.Ports;
using FCG_CatalogAPI.Infrastructure.SqlServer;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace FCG_CatalogAPI.Infrastructure.Configuration;

/// <summary>
/// Registra os serviços da camada de API (controllers, autenticação e Swagger).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra serviços de infraestrutura (DbContext, repositories, etc.).
    /// Mantém Program.cs enxuto delegando a configuração aqui.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Tenta obter connection string nomeada "SqlConnection" ou fallback para "DefaultConnection"
        var connectionString = configuration.GetConnectionString("SqlConnection")
            ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<CatalogDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure();
                }
            );
        });

        services.AddScoped<IGameUseCase, GameUseCase>();
        services.AddScoped<IPromotionUseCase, PromotionUseCase>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();

        return services;
    }

    /// <summary>
    /// Escreve a resposta de erro no mesmo formato ProblemDetails (RFC 7807) usado
    /// pelo ExceptionHandlingMiddleware, para que quem consome a API trate todos os
    /// erros da mesma maneira — inclusive os gerados pelo pipeline de autorização.
    /// </summary>
    private static async Task WriteProblemDetails(HttpContext context, int statusCode, string title, string detail)
    {
        // Se a resposta já começou a ser enviada, não é mais possível alterá-la.
        if (context.Response.HasStarted)
        {
            return;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json");
    }

    /// <summary>
    /// Documentação da API no Swagger, com suporte a autenticação por token.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(swagger =>
        {
            swagger.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Catalog API",
                Version = "v1",
                Description = "API responsável pelo gerenciamento de jogos e promoções.",
                Contact = new OpenApiContact
                {
                    Name = "Equipe Desafio",
                    Email = "contato@desafio.com"
                }
            });

            // Campo para informar o token nas requisições feitas pelo Swagger.
            swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description =
                    "Informe o token no formato: Bearer {seu token}\n\n" +
                    "Atenção: chamar /auth/login NÃO troca o token daqui. " +
                    "Ao trocar de usuário, volte neste botão e cole o token novo — " +
                    "caso contrário as requisições continuarão sendo feitas como o usuário anterior.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Traz para o Swagger os comentários <summary> dos controllers.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                swagger.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static async Task ConfigMasstransit(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });
        });
        // Se você quiser usar o MassTransit Hosted Service para iniciar e parar o bus automaticamente
        services.AddMassTransitHostedService();
        return services;
    }
}
