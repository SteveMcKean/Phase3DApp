using DataAccess;
using Phase3D.Models;
using Xunit.Abstractions;

namespace Phase3D.Tests;

public class GrowthCalculatorTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public GrowthCalculatorTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Calculate_returns_correct_growth_per_year_per_state()
    {
        const string path = "Data/historical.csv";
        var populationGrowth = new List<GrowthRate>();
        
        var dataLoader = new FileDataProvider();
        var populations = await dataLoader.GetPopulationsAsync(path);

        var query = populations.ToList()
            .GroupBy(stateGroup => stateGroup.State);

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
                var stateGrowth = new GrowthRate(element.State, element.Year, rate, element.Year - 1, results);
                
                populationGrowth.Add(stateGrowth);
                testOutputHelper.WriteLine(results);
			
            }
        }
        
    }

    private static string RateChangedText(float rate)
    {
        if (rate == 0)
            return "remained unchanged";
        
        var rateChangedText = rate > 0 ? "increased" : "decreased";
        return rateChangedText;
        
    }
}