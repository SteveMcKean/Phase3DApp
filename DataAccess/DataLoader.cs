using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Phase3D.Core.Contracts;
using Phase3D.Models;

namespace DataAccess
{
    public class DataLoader : IDataLoader
    {
        private CancellationTokenSource cancellationTokenSource = new();
        private readonly Dictionary<string, double> colorIndexes = new ();

        private TimeSpan pushDelay = TimeSpan.FromSeconds(0);
        private decimal pushDelayValue;

        private readonly ISubject<PopulationData> populationsSubject = new Subject<PopulationData>();

        public IObservable<PopulationData> PopulationsSubject => populationsSubject.AsObservable();
        
        public async Task<IEnumerable<PopulationData>> GetPopulationsAsync(string path)
        {
            var populations = new Collection<PopulationData>();
            cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                var reader = new StreamReader(path);
                while (!reader.EndOfStream && !cancellationTokenSource.IsCancellationRequested)
                {
                    var dataLine = await reader.ReadLineAsync(cancellationTokenSource.Token);
                    var serializedValues = dataLine?.Split(',');

                    if (string.IsNullOrWhiteSpace(serializedValues?[0]))
                        continue;

                    var populationInfo = new PopulationData(serializedValues[0],
                        Convert.ToInt32(serializedValues[1], CultureInfo.InvariantCulture),
                        Convert.ToInt32(serializedValues[2], CultureInfo.InvariantCulture), 0);

                    populations.Add(populationInfo);
                    
                }
                
            }
            catch (TaskCanceledException)
            {
                
            }
            catch (Exception e)
            {
                populationsSubject.OnError(e);

            }

            return populations;

        }

        public async void GetPopulations(string path)
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                var reader = new StreamReader(path);

                while (!reader.EndOfStream && !cancellationTokenSource.IsCancellationRequested)
                {
                    var dataLine = await reader.ReadLineAsync(cancellationTokenSource.Token);
                    var serializedValues = dataLine?.Split(',');

                    if (string.IsNullOrWhiteSpace(serializedValues?[0]))
                        continue;

                    var populationInfo = new PopulationData(serializedValues[0],
                        Convert.ToInt32(serializedValues[1], CultureInfo.InvariantCulture),
                        Convert.ToInt32(serializedValues[2], CultureInfo.InvariantCulture),0);

                    populationsSubject.OnNext(populationInfo);

                    if (pushDelayValue > 0)
                        await Task.Delay(pushDelay, cancellationTokenSource.Token);

                }

                populationsSubject.OnCompleted();

            }
            catch (TaskCanceledException)
            {
                
            }    
            catch (Exception e)
            {
                populationsSubject.OnError(e);

            }

        }

        public void SetDelay(decimal delay)
        {
            pushDelayValue = delay;
            pushDelay = TimeSpan.FromSeconds((double)pushDelayValue);
            
        }

        public void CancelLoad()
        {
            cancellationTokenSource?.Cancel();
            
        }
        
    }

}