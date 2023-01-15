using Phase3D.Core.Contracts;
using Phase3D.Models;

namespace Phase3D.Services;

public class GrowthRateCalculator : IGrowthRateCalculator
{
    public Task<IEnumerable<GrowthRate>> CalculateGrowthRateByState(IEnumerable<PopulationData> data)
    {
        var populationGrowth = new List<GrowthRate>();
        var query = data.ToList().GroupBy(stateGroup => stateGroup.State);

        foreach (var item in query)
        {
            var index = 0;
            float firstPopulation  = 0;
		
            foreach (var element in item)
            {
                // skip first year calculation but get the population for processing
                if (index == 0)
                {
                    firstPopulation = element.Population;
                    index++;
				
                    continue;
				
                }
			
                var populationDiff = element.Population - firstPopulation;
						
                var rate = (float)(populationDiff / firstPopulation);
                firstPopulation = element.Population;

                var rateChangedText = RateChangedText(rate);
                
                var results = $"Population for {element.State} for year {element.Year}, {rateChangedText} by {rate:P} from previous year {element.Year - 1} ";
                var stateGrowth = new GrowthRate(element.State, element.Year, rate * 100, element.Year - 1, results);
                populationGrowth.Add(stateGrowth);
                
            }
        }

        return Task.FromResult<IEnumerable<GrowthRate>>(populationGrowth);

    }
    
    private static string RateChangedText(float rate)
    {
        if (rate == 0)
            return "remained unchanged";
        
        var rateChangedText = rate > 0 ? "increased" : "decreased";
        return rateChangedText;
        
    }
}