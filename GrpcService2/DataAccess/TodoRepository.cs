using Cassandra;
using Cassandra.Mapping;
using Google.Protobuf.Collections;
using ISession = Cassandra.ISession;

namespace GrpcService2.DataAccess;

public class TodoRepository: ITodoRepository
{
    private readonly ISession _session;
    private readonly IMapper  _mapper; 

    public TodoRepository(ISession session, IMapper mapper)
    {
        _session     = session;
        _mapper = mapper;
    }

    public async Task<List<TodoItem>> GetAllAsync()
    {
        var todos = await _mapper.FetchAsync<TodoItem>();      
        return todos.ToList();
    }

    public async Task<TodoItem> AddAsync(TodoItem item)
    {
        var id        = string.IsNullOrEmpty(item.Id) ? Guid.NewGuid().ToString() : item.Id;
        var createdTs = item.CreatedTs == 0
            ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            : item.CreatedTs;

        item.Id        = id;
        item.CreatedTs = createdTs;

        await _mapper.InsertAsync(item);                         
        return item;
    }

    public async Task<TodoItem> UpdateAsync(TodoItem item)
    {
        await _mapper.UpdateAsync(item);       
        return item;
    }

    public Task DeleteAsync(string id) =>
        _mapper.DeleteAsync<TodoItem>("WHERE id = ?", id); 

    private static TodoItem RowToItem(Row row) => new()
    {
        Id          = row.GetValue<string>("id"),
        Title       = row.GetValue<string>("title"),
        Description = row.GetValue<string>("description"),
        Done        = row.GetValue<bool>("done"),
        CreatedTs   = row.GetValue<long>("created_ts")
    };
}
