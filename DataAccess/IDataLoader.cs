using DataAccess.Models;

namespace DataAccess;

public interface IDataLoader
{
    Task<IEnumerable<PopulationInfo>> GetPopulationsAsync(string path);
    IObservable<PopulationInfo> PopulationsSubject { get; }
    void GetPopulations(string path);
    void CancelLoad();
    void SetDelay(decimal delay);

}