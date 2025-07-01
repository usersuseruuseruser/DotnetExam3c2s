using Google.Protobuf.Collections;

namespace GrpcService2.DataAccess
{
    public interface ITodoRepository
    {
        public Task<List<TodoItem>> GetAllAsync();
        public Task<TodoItem> AddAsync(TodoItem item);
        public Task<TodoItem> UpdateAsync(TodoItem item);
        public Task DeleteAsync(string id);
    }
}