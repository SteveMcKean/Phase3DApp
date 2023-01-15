using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Phase3D.Core.Contracts;
using Phase3D.Models;

namespace Phase3D.UI.ViewModels;

public class PopulationGrowthViewModel: ViewModelBase
{
    private readonly IGrowthRateCalculator growthRateCalculator;
    public ICurrentWindowService CurrentWindowService => GetService<ICurrentWindowService>();
    public ObservableCollection<GrowthRate> GrowthRates { get; set; }
    
    public string Title => "Population Growth by Year";
    public bool IsWaitIndicatorBusy { get; set; }
    public IEnumerable<PopulationData> PopulationData { get; set; }
    public PopulationGrowthViewModel(IGrowthRateCalculator growthRateCalculator)
    {
        GrowthRates = new ObservableCollection<GrowthRate>();
        this.growthRateCalculator = growthRateCalculator ?? throw new ArgumentNullException(nameof(growthRateCalculator));
        
    }

    [Command]
    public async void OnLoaded()
    {
        IsWaitIndicatorBusy = true;
        if (PopulationData == null)
        {
            IsWaitIndicatorBusy = false;
            return;
            
        }

        var items = await growthRateCalculator.CalculateGrowthRateByState(PopulationData).ConfigureAwait(true);
        GrowthRates.AddRange(items.ToList());

        IsWaitIndicatorBusy = false;

    }
    
    [Command]
    public void Close()
    {
        CurrentWindowService?.Close();
        
    }
}