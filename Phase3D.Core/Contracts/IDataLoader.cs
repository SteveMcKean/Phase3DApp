
using Phase3D.Models;

namespace Phase3D.Core.Contracts;

public interface IDataLoader
{
    Task<IEnumerable<PopulationData>> GetPopulationsAsync(string path);
    IObservable<PopulationData> PopulationsSubject { get; }
    void GetPopulations(string path);
    void CancelLoad();
    void SetDelay(decimal delay);

}