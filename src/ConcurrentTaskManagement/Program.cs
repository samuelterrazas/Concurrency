using Concurrency.ConcurrentTaskManagement;
using Concurrency.ConcurrentTaskManagement.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddSingleton<IInMemoryOperationRepository, InMemoryOperationRepository>();
    
    builder.Services.AddHostedService<BackgroundTask>();
}

WebApplication app = builder.Build();
{
    app.MapGet("/", () => "Hello World!");

    app.MapGet("/operations", (IInMemoryOperationRepository repository) =>
        repository.GetOperations());
    
    app.MapPost("/operations", (IInMemoryOperationRepository repository) =>
        repository.AddOperation());
    
    app.MapPut("/operations/{id:int}", (IInMemoryOperationRepository repository, int id) =>
        repository.UpdateOperation(id));
    
    app.MapDelete("/operations/{id:int}", (IInMemoryOperationRepository repository, int id) =>
        repository.RemoveOperation(id));

    app.Run();
}