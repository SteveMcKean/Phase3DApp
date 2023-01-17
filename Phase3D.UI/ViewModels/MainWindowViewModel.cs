using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using DevExpress.Mvvm;
using Phase3D.Core.Contracts;
using Phase3D.Models;
using ReactiveUI;
using Unit = System.Reactive.Unit;

namespace Phase3D.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly Func<IDataProvider> dataLoader;
        private readonly Func<PopulationGrowthViewModel> vmCreator;
        private readonly SynchronizationContext context;
        private IDisposable subscription;
        private IDataProvider provider;

        public bool IsWaitIndicatorBusy { get; set; }
        public INotificationService NotificationService => GetService<INotificationService>();
        public IDocumentManagerService DocumentManagerService => GetService<IDocumentManagerService>();
        public string Title => "Phase3D Demo Application";
        public ReactiveCommand<Unit, Unit> LoadDataCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> CancelLoadCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadHistoryCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadGrowthCommand { get; private set; }
        public decimal Delay { get; set; } = 1;
        public bool IsBusy { get; set; }
        public ObservableCollection<PopulationData> Populations { get; set; }

        public MainWindowViewModel(Func<IDataProvider> dataLoader, Func<PopulationGrowthViewModel> vmCreator)
        {
            this.dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
            this.vmCreator = vmCreator ?? throw new ArgumentException(nameof(vmCreator));
            
            context = SynchronizationContext.Current;

            var canExecuteObservable = this.WhenAnyValue(x => x.IsBusy)
                .Select(x => !x);

            var cancelCanExecuteObservable = this.WhenAnyValue(x => x.IsBusy)
                .Select(x => x);

            var growthRateCanExecute =
                this.WhenAnyValue(x => x.Populations, x => x.IsBusy)
                    .Select(x => x.Item1 != null && x.Item1.Any() && !x.Item2);

            LoadDataCommand = ReactiveCommand.Create(StreamDataLoad, canExecuteObservable, Scheduler.CurrentThread);
            CancelLoadCommand = ReactiveCommand.Create(CancelLoad, cancelCanExecuteObservable, Scheduler.CurrentThread);
            LoadHistoryCommand = ReactiveCommand.Create(LoadAsHistorical, canExecuteObservable, Scheduler.CurrentThread);
            LoadGrowthCommand = ReactiveCommand.Create(LoadGrowth, growthRateCanExecute, Scheduler.CurrentThread);
            
            Populations = new ObservableCollection<PopulationData>();
            
        }

        private void LoadGrowth()
        {
            IsWaitIndicatorBusy = true;
            IsBusy = true;
            
            var viewModel = vmCreator();
            viewModel.PopulationData = Populations;
            
            var doc = DocumentManagerService.CreateDocument("PopulationGrowthView", viewModel);

            IsWaitIndicatorBusy = false;
            IsBusy = false;
            
            doc.Show();
            
        }

        private void LoaderSubscribe()
        {
            subscription?.Dispose();
            subscription = null;

            provider = null;
            provider = dataLoader();
            
            subscription = provider.PopulationsSubject
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
            IsWaitIndicatorBusy = true;
            
            Populations.Clear();
            LoaderSubscribe();
           
            Populations.AddRange(await provider?.GetPopulationsAsync("Data/historical.csv")!);
            IsBusy = false;
            IsWaitIndicatorBusy = false;
           
            var notification = NotificationService.CreatePredefinedNotification("Phase3D Demo App", "Simulation completed", "", null, "Space3D");
            await notification.ShowAsync();

        }

        private async void CancelLoad()
        {
            provider?.CancelLoad();
            IsBusy = false;
            IsWaitIndicatorBusy = false;

            var notification = NotificationService.CreatePredefinedNotification("Phase3D Demo App", "Simulation cancelled", "", null, "Space3D");
            await notification.ShowAsync();
            
        }

        private void HandlePopulationInfo(PopulationData args)
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

            provider?.SetDelay(Delay);
            provider?.GetPopulations("Data/historical.csv");
           
        }

        private void OnDelayChanged()
        {
            provider?.SetDelay(Delay);

        }
    }
}
