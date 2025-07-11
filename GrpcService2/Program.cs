using Cassandra;
using Cassandra.Mapping;
using GrpcService2.DataAccess;
using GrpcService2.Infrastructuer;
using GrpcService2.Services;
using ISession = Cassandra.ISession;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddGrpc();

builder.Services.AddSingleton<MappingConfiguration>(_ => 
    CassandraMapping.Configure());

builder.Services.AddSingleton<IMapper>(sp =>         
{
    var session = sp.GetRequiredService<ISession>();
    var config  = sp.GetRequiredService<MappingConfiguration>();
    return new Mapper(session, config);
});

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

app.Run();