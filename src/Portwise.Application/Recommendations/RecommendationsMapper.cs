using Portwise.Application.Recommendations.Dtos;
using Portwise.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Portwise.Application.Recommendations;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class RecommendationsMapper
{
    [MapProperty(nameof(ModelParameterSet.Id), nameof(StockModelParameterSet.ModelParameterSetId))]
    public static partial StockModelParameterSet ToStockModelParameterSet(
        ModelParameterSet parameters,
        string securityCode,
        string exchangeCode);
}
