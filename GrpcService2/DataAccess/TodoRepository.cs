using Cassandra;
using Google.Protobuf.Collections;
using ISession = Cassandra.ISession;

namespace GrpcService2.DataAccess;

public class TodoRepository: ITodoRepository
{
    private readonly ISession _session;
    private readonly PreparedStatement _psSelectAll;
    private readonly PreparedStatement _psInsert;
    private readonly PreparedStatement _psUpdate;
    private readonly PreparedStatement _psDelete;

    public TodoRepository(ISession session)
    {
        _session     = session;

        _psSelectAll = _session.Prepare(
            "SELECT id, title, description, done, created_ts FROM todo");

        _psInsert    = _session.Prepare(
            "INSERT INTO todo (id, title, description, done, created_ts) VALUES (?, ?, ?, ?, ?)");

        _psUpdate    = _session.Prepare(
            "UPDATE todo SET title = ?, description = ?, done = ? WHERE id = ?");

        _psDelete    = _session.Prepare(
            "DELETE FROM todo WHERE id = ?");
    }

    public async Task<List<TodoItem>> GetAllAsync()
    {
        var rs   = await _session.ExecuteAsync(_psSelectAll.Bind());
        var list = new List<TodoItem>();

        foreach (var row in rs)
        {
            list.Add(RowToItem(row));
        }
        return list;
    }

    public async Task<TodoItem> AddAsync(TodoItem item)
    {
        // генерируем id если не передан, к примеру если клиент хочет сохранить под своим
        var id        = string.IsNullOrEmpty(item.Id) ? Guid.NewGuid().ToString() : item.Id;
        
        // когда был создан тоже
        var createdTs = item.CreatedTs == 0
                        ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        : item.CreatedTs;

        await _session.ExecuteAsync(
            _psInsert.Bind(id, item.Title, item.Description, item.Done, createdTs));

        item.Id         = id;
        item.CreatedTs  = createdTs;
        return item;
    }

    public async Task<TodoItem> UpdateAsync(TodoItem item)
    {
        await _session.ExecuteAsync(
            _psUpdate.Bind(item.Title, item.Description, item.Done, item.Id));

        return item;
    }

    public Task DeleteAsync(string id) =>
        _session.ExecuteAsync(_psDelete.Bind(id));

    private static TodoItem RowToItem(Row row) => new()
    {
        Id          = row.GetValue<string>("id"),
        Title       = row.GetValue<string>("title"),
        Description = row.GetValue<string>("description"),
        Done        = row.GetValue<bool>("done"),
        CreatedTs   = row.GetValue<long>("created_ts")
    };
}
