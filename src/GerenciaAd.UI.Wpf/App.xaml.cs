using System.Windows;
using GerenciaAd.Application;
using GerenciaAd.Infrastructure;

namespace GerenciaAd.UI.Wpf
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configuração de injeção de dependência
            // Registro de serviços
            var adService = new ActiveDirectoryService();
            var orchestrator = new RenovacaoOrchestrator(adService);

            // Armazenar no Application.Resources para acesso global
            Resources["RenovacaoOrchestrator"] = orchestrator;
            Resources["ActiveDirectoryService"] = adService;
        }
    }
}
