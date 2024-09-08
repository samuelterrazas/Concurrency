namespace Concurrency.ConcurrentTaskManagement.Data;

public sealed class Operation
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public bool IsCompleted { get; set; }
}