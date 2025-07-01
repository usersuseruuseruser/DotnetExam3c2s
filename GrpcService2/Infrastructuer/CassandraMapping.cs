using Cassandra.Mapping;

namespace GrpcService2.Infrastructuer
{
    public static class CassandraMapping
    {
        public static MappingConfiguration Configure()
        {
            var cfg = new MappingConfiguration();
            cfg.Define(
                new Map<TodoItem>()
                    .TableName("todo")
                    .PartitionKey(t => t.Id)      
                    .Column(t => t.Id,          cm => cm.WithName("id"))
                    .Column(t => t.Title,       cm => cm.WithName("title"))
                    .Column(t => t.Description, cm => cm.WithName("description"))
                    .Column(t => t.Done,        cm => cm.WithName("done"))
                    .Column(t => t.CreatedTs,   cm => cm.WithName("created_ts"))
            );
            return cfg;
        }
    }
}