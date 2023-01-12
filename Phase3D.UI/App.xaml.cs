using System.Windows;
using DataAccess;
using Phase3D.UI.Views;
using Prism.Ioc;

namespace Phase3D.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();

        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDataLoader, DataLoader>();

        }
       
    }
}
