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
    builder.Services.ConfigMassTransit(builder.Configuration);
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
    /*Console.Error.WriteLine(exception.ToString());
    Log.Fatal(exception, "A aplicação não pôde ser iniciada");
    Log.CloseAndFlush();
    return 1;*/
    Console.Error.WriteLine("Startup exception (full):");
    Console.Error.WriteLine(exception.ToString());

    // Se for AggregateException, imprime cada inner
    if (exception is AggregateException agg)
    {
        var flat = agg.Flatten();
        Console.Error.WriteLine("AggregateException flattened. Inner exceptions:");
        foreach (var inner in flat.InnerExceptions)
        {
            Console.Error.WriteLine("---- Inner exception ----");
            Console.Error.WriteLine(inner.ToString());
        }
    }
    else
    {
        // Percorre a cadeia de inner exceptions
        var current = exception.InnerException;
        while (current != null)
        {
            Console.Error.WriteLine("---- Inner exception ----");
            Console.Error.WriteLine(current.ToString());
            current = current.InnerException;
        }
    }
}
finally
{
    Log.CloseAndFlush();
}

return 0;