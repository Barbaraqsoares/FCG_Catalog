using FCG_CatalogAPI.Adapters.Middlewares;
using FCG_CatalogAPI.Infrastructure.Configuration;
using FCG_CatalogAPI.Infrastructure.SqlServer;
using Serilog;

// Cria um logger mínimo no bootstrap para garantir mensagens no console em caso de falha na inicialização
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilogLogging();

    // Registrar serviços e documentação usando métodos centralizados
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddSwaggerDocumentation();

    // Serviços da aplicação
    builder.Services.AddControllers();

    var app = builder.Build();

    // Migração e seed do banco de dados
    await app.MigrateAndSeedAsync();

    // Pipeline de middlewares e roteamento
    app.UseInfrastructurePipeline();

    app.Run();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    // Garante que a exceção de inicialização seja visível no console e no logger
    Console.Error.WriteLine(exception.ToString());
    Log.Fatal(exception, "A aplicação não pôde ser iniciada");
    Log.CloseAndFlush();
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;
