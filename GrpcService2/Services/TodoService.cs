using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService2.DataAccess;

namespace GrpcService2.Services;

public class TodoService : GrpcService2.TodoService.TodoServiceBase
{
    private readonly ITodoRepository _repo;

    public TodoService(ITodoRepository repo) => _repo = repo;

    public override async Task<TodoList> List(Empty request, ServerCallContext context)
    {
        var result = new TodoList();
        result.Items.AddRange(await _repo.GetAllAsync());
        return result;
    }

    public override Task<TodoItem> Add(TodoItem request, ServerCallContext context) =>
        _repo.AddAsync(request);

    public override Task<TodoItem> Update(TodoItem request, ServerCallContext context) =>
        _repo.UpdateAsync(request);

    public override async Task<Empty> Delete(ItemId request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new Empty();
    }
}