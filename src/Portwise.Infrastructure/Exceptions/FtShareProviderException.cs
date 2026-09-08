using Portwise.Application.Contracts;

namespace Portwise.Infrastructure.Exceptions;

public sealed class FtShareProviderException(Exception innerException)
    : Exception(null, innerException), IStockDataProviderFailure;
