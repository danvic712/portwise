namespace Portwise.Application.Contracts;

public interface IStockDataSyncScheduler
{
    bool TrySchedule();
}
