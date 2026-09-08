namespace Portwise.Application.Stocks.Contracts;

public interface IStockDataSyncScheduler
{
    bool TrySchedule();
}
