using Phase3D.Models;

namespace Phase3D.Core.Contracts;

public interface IGrowthRateCalculator
{
    Task<IEnumerable<GrowthRate>> CalculateGrowthRateByState(IEnumerable<PopulationData> data);
}