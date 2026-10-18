using FCG_CatalogAPI.Infrastructure.Configuration;

namespace FCG_CatalogAPI.Adapters.Middlewares;

public static class MiddlewareExtensions
{
    public static WebApplication UseExceptionHandling(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }

    public static WebApplication UseInfrastructurePipeline(this WebApplication app)
    {
        // O log de requisições vem primeiro para envolver tudo, inclusive o
        // tratamento de exceções. Invertido, ele veria a exceção antes de virar
        // 404/400/409 e registraria toda falha de negócio como erro 500 —
        // contradizendo o status que o cliente de fato recebeu.
        app.UseRequestLogging();
        app.UseExceptionHandling();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechChallenge API v1");
            c.RoutePrefix = string.Empty;
        });

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
