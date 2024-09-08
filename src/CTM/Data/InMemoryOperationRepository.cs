namespace CTM.Data;

public interface IInMemoryOperationRepository
{
    List<Operation> GetOperations();

    List<Operation> GetIncompleteOperations();

    bool AddOperation();

    bool UpdateOperation(int id);

    bool RemoveOperation(int id);
}

public sealed class InMemoryOperationRepository : IInMemoryOperationRepository
{
    private readonly List<Operation> _operations = [];


    public List<Operation> GetOperations()
    {
        return _operations.ToList();
    }

    public List<Operation> GetIncompleteOperations()
    {
        return _operations.Where(i => !i.IsCompleted).ToList();
    }

    public bool AddOperation()
    {
        int id = (_operations.MaxBy(i => i.Id)?.Id ?? 0) + 1;

        try
        {
            _operations.Add(
                new Operation
                {
                    Id = id,
                    Name = $"{nameof(Operation)} {id}",
                    IsCompleted = false
                }
            );

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool UpdateOperation(int id)
    {
        Operation? operation = _operations.Find(i => i.Id == id);

        if (operation is null)
        {
            return false;
        }

        operation.IsCompleted = !operation.IsCompleted;

        return true;
    }

    public bool RemoveOperation(int id)
    {
        Operation? operation = _operations.Find(i => i.Id == id);

        return operation is not null && _operations.Remove(operation);
    }
}