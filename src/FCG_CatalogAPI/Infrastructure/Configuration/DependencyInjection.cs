using FCG_CatalogAPI.Adapters.Outbound;
using FCG_CatalogAPI.Application.UseCases;
using FCG_CatalogAPI.Domain.Ports;
using FCG_CatalogAPI.Infrastructure.SqlServer;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        services.AddScoped<IUserGameUseCase, UserGameUseCase>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<IUserGameRepository, UserGameRepository>();

        ConfigMasstransit(services, configuration);

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
            Instance = context.Request.Path.HasValue ? context.Request.Path.Value : context.Request.Path.ToString()
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
            // Tenta criar dinamicamente os tipos do Microsoft.OpenApi se o pacote estiver disponível.
            // Isso evita erros de compilação quando há alguma incompatibilidade transitiva de pacotes.
            var openApiInfoType = Type.GetType("Microsoft.OpenApi.Models.OpenApiInfo, Microsoft.OpenApi");
            if (openApiInfoType != null)
            {
                var info = Activator.CreateInstance(openApiInfoType);
                openApiInfoType.GetProperty("Title")?.SetValue(info, "Catalog API");
                openApiInfoType.GetProperty("Version")?.SetValue(info, "v1");
                openApiInfoType.GetProperty("Description")?.SetValue(info, "API responsável pelo gerenciamento de jogos e promoções.");

                var contactType = Type.GetType("Microsoft.OpenApi.Models.OpenApiContact, Microsoft.OpenApi");
                if (contactType != null)
                {
                    var contact = Activator.CreateInstance(contactType);
                    contactType.GetProperty("Name")?.SetValue(contact, "Equipe Desafio");
                    contactType.GetProperty("Email")?.SetValue(contact, "contato@desafio.com");
                    openApiInfoType.GetProperty("Contact")?.SetValue(info, contact);
                }

                // Invoca SwaggerDoc("v1", info) por reflexão
                var swaggerDocMethod = swagger.GetType().GetMethod("SwaggerDoc", new[] { typeof(string), openApiInfoType });
                if (swaggerDocMethod != null)
                {
                    swaggerDocMethod.Invoke(swagger, new object[] { "v1", info });
                }
            }

            // Configura definição de segurança (Bearer) por reflexão se disponível
            var openApiSecuritySchemeType = Type.GetType("Microsoft.OpenApi.Models.OpenApiSecurityScheme, Microsoft.OpenApi");
            var openApiReferenceType = Type.GetType("Microsoft.OpenApi.Models.OpenApiReference, Microsoft.OpenApi");
            var referenceTypeEnum = Type.GetType("Microsoft.OpenApi.Models.ReferenceType, Microsoft.OpenApi");
            var parameterLocationType = Type.GetType("Microsoft.OpenApi.Models.ParameterLocation, Microsoft.OpenApi");
            var securitySchemeTypeEnum = Type.GetType("Microsoft.OpenApi.Models.SecuritySchemeType, Microsoft.OpenApi");

            if (openApiSecuritySchemeType != null && openApiReferenceType != null && referenceTypeEnum != null && parameterLocationType != null && securitySchemeTypeEnum != null)
            {
                var scheme = Activator.CreateInstance(openApiSecuritySchemeType);
                openApiSecuritySchemeType.GetProperty("Description")?.SetValue(scheme,
                    "Informe o token no formato: Bearer {seu token}\n\n" +
                    "Atenção: chamar /auth/login NÃO troca o token daqui. Ao trocar de usuário, volte neste botão e cole o token novo — caso contrário as requisições continuarão sendo feitas como o usuário anterior.");
                openApiSecuritySchemeType.GetProperty("Name")?.SetValue(scheme, "Authorization");
                openApiSecuritySchemeType.GetProperty("Scheme")?.SetValue(scheme, "Bearer");
                openApiSecuritySchemeType.GetProperty("In")?.SetValue(scheme, Enum.Parse(parameterLocationType, "Header"));
                openApiSecuritySchemeType.GetProperty("Type")?.SetValue(scheme, Enum.Parse(securitySchemeTypeEnum, "ApiKey"));

                var addSecDefMethod = swagger.GetType().GetMethod("AddSecurityDefinition", new[] { typeof(string), openApiSecuritySchemeType });
                addSecDefMethod?.Invoke(swagger, new object[] { "Bearer", scheme });

                // Monta OpenApiSecurityRequirement e chama AddSecurityRequirement
                var openApiSecurityRequirementType = Type.GetType("Microsoft.OpenApi.Models.OpenApiSecurityRequirement, Microsoft.OpenApi");
                if (openApiSecurityRequirementType != null)
                {
                    var requirement = Activator.CreateInstance(openApiSecurityRequirementType);

                    // Cria chave: OpenApiSecurityScheme com Reference apontando para SecurityScheme "Bearer"
                    var keyScheme = Activator.CreateInstance(openApiSecuritySchemeType);
                    var reference = Activator.CreateInstance(openApiReferenceType);
                    openApiReferenceType.GetProperty("Type")?.SetValue(reference, Enum.Parse(referenceTypeEnum, "SecurityScheme"));
                    openApiReferenceType.GetProperty("Id")?.SetValue(reference, "Bearer");
                    openApiSecuritySchemeType.GetProperty("Reference")?.SetValue(keyScheme, reference);

                    // requirement.Add(keyScheme, new List<string>()) via reflexão
                    var addMethod = openApiSecurityRequirementType.GetMethod("Add", new[] { openApiSecuritySchemeType, typeof(System.Collections.Generic.IList<string>) });
                    addMethod?.Invoke(requirement, new object[] { keyScheme, new System.Collections.Generic.List<string>() });

                    var addSecReqMethod = swagger.GetType().GetMethod("AddSecurityRequirement", new[] { openApiSecurityRequirementType });
                    addSecReqMethod?.Invoke(swagger, new object[] { requirement });
                }
            }

            // Traz para o Swagger os comentários <summary> dos controllers.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                var includeXmlMethod = swagger.GetType().GetMethod("IncludeXmlComments", new[] { typeof(string) });
                includeXmlMethod?.Invoke(swagger, new object[] { xmlPath });
            }
        });

        return services;
    }

    public static IServiceCollection ConfigMasstransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                // Permite configurar o host via configuração se disponível, com fallback para localhost
                var host = configuration["RabbitMq:Host"] ?? "localhost";
                var virtualHost = configuration["RabbitMq:VirtualHost"] ?? "/";
                var username = configuration["RabbitMq:Username"] ?? "guest";
                var password = configuration["RabbitMq:Password"] ?? "guest";

                cfg.Host(host, virtualHost, h =>
                {
                    h.Username(username); //guest
                    h.Password(password); //guest
                });
            });
        });
        // Se você quiser usar o MassTransit Hosted Service para iniciar e parar o bus automaticamente
        services.AddMassTransitHostedService();
        return services;
    }
}