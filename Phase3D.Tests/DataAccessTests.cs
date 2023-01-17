using DataAccess;
using FluentAssertions;

namespace Phase3D.Tests
{
    public class DataAccessTests
    {
        [Fact]
        public async Task DataLoader_Loaded_CSV_File()
        {
            const string path = "Data/historical.csv";

            var dataLoader = new FileDataProvider();
            var populations = await dataLoader.GetPopulationsAsync(path);

            Assert.NotNull(populations);

            var ak = populations.First();

            ak.State.Should().Be("AK");
            ak.Year.Should().Be(1950);
            ak.Population.Should().Be(135000);

        }
    }
}