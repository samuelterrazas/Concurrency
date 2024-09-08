using System.Collections.Concurrent;
using Concurrency.ConcurrentTaskManagement.Data;

namespace Concurrency.ConcurrentTaskManagement;

public sealed class BackgroundTask(
    ILogger<BackgroundTask> logger,
    IInMemoryOperationRepository repository
) : BackgroundService
{
    private readonly ConcurrentDictionary<int, Task> _concurrentDictionary = new();
    private readonly SemaphoreSlim _semaphoreSlim = new(5, 5);


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{backgroundService} running...", nameof(BackgroundTask));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConcurrentTaskManagement(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "{exMessage}", ex.Message);
                throw;
            }

            await Task.Delay(15_000, stoppingToken);
        }

        logger.LogInformation("{backgroundService} stopped", nameof(BackgroundTask));
    }

    private async Task ConcurrentTaskManagement(CancellationToken stoppingToken)
    {
        List<Operation> operations = repository.GetIncompleteOperations();

        if (operations.Count == 0)
        {
            logger.LogInformation("No pending operations found");
            return;
        }

        foreach (Operation operation in operations)
        {
            int operationId = operation.Id;

            if (_concurrentDictionary.ContainsKey(operationId))
            {
                logger.LogWarning("Operation ID {id} is already in progress", operationId);
                continue;
            }

            await _semaphoreSlim.WaitAsync(stoppingToken);

            Task processTask = ProcessTaskAsync(operation, stoppingToken);

            _concurrentDictionary.TryAdd(operationId, processTask);
        }
    }

    private async Task ProcessTaskAsync(Operation operation, CancellationToken stoppingToken)
    {
        try
        {
            await RunTask(operation, stoppingToken);
        }
        finally
        {
            _concurrentDictionary.TryRemove(operation.Id, out Task? _);
            _semaphoreSlim.Release();
        }
    }

    private async Task RunTask(Operation operation, CancellationToken stoppingToken)
    {
        int operationId = operation.Id;

        logger.LogWarning("Operation ID {id} started...", operationId);

        var random = new Random();
        int time = random.Next(minValue: 15_000, maxValue: 60_000);

        await Task.Delay(time, stoppingToken);

        repository.UpdateOperation(operationId);

        logger.LogWarning("Operation ID {id} completed", operationId);
    }
}