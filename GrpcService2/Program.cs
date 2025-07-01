using Cassandra;
using GrpcService2.DataAccess;
using GrpcService2.Services;
using ISession = Cassandra.ISession;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddSingleton<ICluster>(_ =>
    Cluster.Builder()
        .AddContactPoint(builder.Configuration["CASSANDRA_CONTACT_POINTS"] ?? throw new Exception("Не задан адрес Cassandra"))
        .Build());

builder.Services.AddSingleton<ISession>(sp =>
{
    var cluster = sp.GetRequiredService<ICluster>();
    return cluster.Connect(builder.Configuration["CASSANDRA_KEYSPACE"] ?? throw new Exception("Не задан ключевое пространство Cassandra"));     
});

builder.Services.AddScoped<ITodoRepository, TodoRepository>();

var app = builder.Build();

app.MapGrpcService<TodoService>();   

// Configure the HTTP request pipeline.
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();