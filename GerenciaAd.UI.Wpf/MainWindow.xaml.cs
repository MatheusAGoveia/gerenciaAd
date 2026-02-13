using System.Windows;
using GerenciaAd.UI.Wpf.ViewModels;

namespace GerenciaAd.UI.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
