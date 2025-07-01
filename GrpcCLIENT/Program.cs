using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcCLIENT;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Можно было IOptions, но зачем усложнять
builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddGrpcClient<TodoService.TodoServiceClient>(
        o => o.Address = new Uri(builder.Configuration["GRPC_SERVER_ADDRESS"]
                                 ?? throw new Exception("Не задан адрес сервера gRPC")));

builder.WebHost.ConfigureKestrel(opts =>
{
    // потому что template устанавливает http 2 и поэтому я не могу в сваггеру обращатсья
    opts.ListenAnyIP(8080, o => o.Protocols = HttpProtocols.Http1AndHttp2);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpc();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/todos", async (
    TodoService.TodoServiceClient grpc) =>
{
    var reply = await grpc.ListAsync(new Empty());
    return Results.Ok(reply.Items);   
});

app.MapPost("/todos", async (
    [FromBody] TodoItem item,
    TodoService.TodoServiceClient grpc) =>
{
    var created = await grpc.AddAsync(item);
    return Results.Created($"/todos/{created.Id}", created);
});

app.MapPut("/todos/{id}", async (
    string id,
    [FromBody] TodoItem item,
    TodoService.TodoServiceClient grpc) =>
{
    item.Id = id;                       
    var updated = await grpc.UpdateAsync(item);
    return Results.Ok(updated);
});

app.MapDelete("/todos/{id}", async (
    string id,
    TodoService.TodoServiceClient grpc) =>
{
    await grpc.DeleteAsync(new ItemId { Id = id });
    return Results.NoContent();
});

app.Run();