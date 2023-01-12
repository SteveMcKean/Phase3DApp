using System;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using DataAccess;
using DataAccess.Models;
using DevExpress.Mvvm;
using DevExpress.Xpf.Charts;
using ReactiveUI;
using Unit = System.Reactive.Unit;

namespace Phase3D.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly Func<IDataLoader> dataLoader;
        private readonly SynchronizationContext context;
        private IDisposable subscription;
        private IDataLoader loader;
        
        public INotificationService NotificationService => GetService<INotificationService>();
        public string Title => "Phase3D Demo Application";
        public ReactiveCommand<Unit, Unit> LoadDataCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> CancelLoadCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadHistoryCommand { get; private set; }
        public decimal Delay { get; set; } = 1;
        public bool IsBusy { get; set; }
        public ObservableCollection<PopulationInfo> Populations { get; set; }

        public MainWindowViewModel(Func<IDataLoader> dataLoader)
        {
            this.dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
            context = SynchronizationContext.Current;

            var canExecuteObservable = this.WhenAnyValue(x => x.IsBusy)
                .Select(x => !x);

            var cancelCanExecuteObservable = this.WhenAnyValue(x => x.IsBusy)
                .Select(x => x);

            LoadDataCommand = ReactiveCommand.Create(StreamDataLoad, canExecuteObservable, Scheduler.CurrentThread);
            CancelLoadCommand = ReactiveCommand.Create(CancelLoad, cancelCanExecuteObservable, Scheduler.CurrentThread);
            LoadHistoryCommand = ReactiveCommand.Create(LoadAsHistorical, canExecuteObservable, Scheduler.CurrentThread);
            
            Populations = new ObservableCollection<PopulationInfo>();
            
        }
        
        private void LoaderSubscribe()
        {
            subscription?.Dispose();
            subscription = null;

            loader = null;
            loader = dataLoader();
            
            subscription = loader.PopulationsSubject
                .SubscribeOn(Scheduler.Default)
                .ObserveOn(Scheduler.Default)
                .Subscribe(HandlePopulationInfo, ex =>
                    {
                        // show error
                        context.Post(async _ =>
                            {
                                IsBusy = false;
                                
                                var notification = NotificationService.CreatePredefinedNotification( "Phase3D Demo App", $"An error occurred: {ex.Message}", "");
                                await notification.ShowAsync();

                            }, null);
                        
                    }, () =>
                    {
                        context.Post(async _ =>
                            {
                                IsBusy = false;
                                
                                var notification = NotificationService.CreatePredefinedNotification("Phase3D Demo App", "Simulation completed","");
                                await notification.ShowAsync();

                            }, null);
                        
                    });

        }

        private async void LoadAsHistorical()
        {
            IsBusy = true;
            
            Populations.Clear();
            LoaderSubscribe();
            
            Populations.AddRange(await loader?.GetPopulationsAsync("Data/historical.csv")!);
            IsBusy = false;
           
            var notification = NotificationService.CreatePredefinedNotification("Phase3D Demo App", "Simulation completed", "", null, "Space3D");
            await notification.ShowAsync();

        }

        private async void CancelLoad()
        {
            loader?.CancelLoad();
            IsBusy = false;

            var notification = NotificationService.CreatePredefinedNotification("Phase3D Demo App", "Simulation cancelled", "", null, "Space3D");
            await notification.ShowAsync();
            
        }

        private void HandlePopulationInfo(PopulationInfo args)
        {
            context?.Post((_) =>
                {
                    Populations.Add(args);
                    
                }, null);

        }

        private void StreamDataLoad()
        {
            IsBusy = true;

            if (Delay == 0)
                Delay = 1;

            LoaderSubscribe();
            Populations.Clear();

            loader?.SetDelay(Delay);
            loader?.GetPopulations("Data/historical.csv");
           
        }

        private void OnDelayChanged()
        {
            loader?.SetDelay(Delay);

        }
    }
}
