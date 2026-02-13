using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using GerenciaAd.UI.Wpf.Application;
using GerenciaAd.UI.Wpf.Domain;
using GerenciaAd.UI.Wpf.Infrastructure;

namespace GerenciaAd.UI.Wpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly RenovacaoOrchestrator _orchestrator;
        private string _login = string.Empty;
        private DominioAD? _dominioSelecionado;
        private TipoContrato? _tipoContratoSelecionado;
        private UsuarioAd? _usuarioAtual;
        private string _mensagem = string.Empty;
        private DateTime? _novaData;
        private bool _mostrarNovaData;
        private string _statusBarMensagem = "Pronto";

        public MainViewModel()
        {
            var adService = new ActiveDirectoryService();
            _orchestrator = new RenovacaoOrchestrator(adService);

            BuscarCommand = new RelayCommand(ExecuteBuscar, CanExecuteBuscar);
            RenovarCommand = new RelayCommand(ExecuteRenovar, CanExecuteRenovar);

            // Inicializar listas disponíveis
            DominiosDisponiveis = new ObservableCollection<DominioAD>
            {
                DominioAD.Saude,
                DominioAD.Betim
            };

            TiposContratoDisponiveis = new ObservableCollection<TipoContrato>
            {
                TipoContrato.Estagiario,
                TipoContrato.Comissionado,
                TipoContrato.Efetivo
            };

            // Selecionar valores padrão
            DominioSelecionado = DominioAD.Saude;
            TipoContratoSelecionado = TipoContrato.Estagiario;
        }

        public ObservableCollection<DominioAD> DominiosDisponiveis { get; }
        public ObservableCollection<TipoContrato> TiposContratoDisponiveis { get; }

        private RelayCommand BuscarCommandRelay => (RelayCommand)BuscarCommand;
        private RelayCommand RenovarCommandRelay => (RelayCommand)RenovarCommand;

        public string Login
        {
            get => _login;
            set
            {
                if (_login != value)
                {
                    _login = value;
                    OnPropertyChanged();
                    BuscarCommandRelay.RaiseCanExecuteChanged();
                    RenovarCommandRelay.RaiseCanExecuteChanged();
                }
            }
        }

        public DominioAD? DominioSelecionado
        {
            get => _dominioSelecionado;
            set
            {
                if (_dominioSelecionado != value)
                {
                    _dominioSelecionado = value;
                    OnPropertyChanged();
                    BuscarCommandRelay.RaiseCanExecuteChanged();
                    RenovarCommandRelay.RaiseCanExecuteChanged();
                }
            }
        }

        public TipoContrato? TipoContratoSelecionado
        {
            get => _tipoContratoSelecionado;
            set
            {
                if (_tipoContratoSelecionado != value)
                {
                    _tipoContratoSelecionado = value;
                    OnPropertyChanged();
                    RenovarCommandRelay.RaiseCanExecuteChanged();
                }
            }
        }

        public UsuarioAd? UsuarioAtual
        {
            get => _usuarioAtual;
            set
            {
                if (_usuarioAtual != value)
                {
                    _usuarioAtual = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DataExpiracaoFormatada));
                    OnPropertyChanged(nameof(StatusFormatado));
                    RenovarCommandRelay.RaiseCanExecuteChanged();
                }
            }
        }

        public string Mensagem
        {
            get => _mensagem;
            set
            {
                if (_mensagem != value)
                {
                    _mensagem = value;
                    OnPropertyChanged();
                    AtualizarCorMensagem();
                }
            }
        }

        private void AtualizarCorMensagem()
        {
            if (string.IsNullOrEmpty(Mensagem))
            {
                CorMensagem = Brushes.Black;
                return;
            }

            var mensagemUpper = Mensagem.ToUpperInvariant();
            if (mensagemUpper.Contains("SUCESSO"))
            {
                CorMensagem = Brushes.Green;
            }
            else if (mensagemUpper.Contains("ERRO") || mensagemUpper.Contains("FALHA"))
            {
                CorMensagem = Brushes.Red;
            }
            else
            {
                CorMensagem = Brushes.Black;
            }
        }

        public DateTime? NovaData
        {
            get => _novaData;
            set
            {
                if (_novaData != value)
                {
                    _novaData = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NovaDataFormatada));
                    MostrarNovaData = value.HasValue;
                }
            }
        }

        public bool MostrarNovaData
        {
            get => _mostrarNovaData;
            set
            {
                if (_mostrarNovaData != value)
                {
                    _mostrarNovaData = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DataExpiracaoFormatada
        {
            get
            {
                if (UsuarioAtual?.AccountExpirationDate == null)
                    return "Sem expiração definida";

                return UsuarioAtual.AccountExpirationDate.Value.ToString("dd/MM/yyyy HH:mm:ss");
            }
        }

        public string StatusFormatado
        {
            get
            {
                if (UsuarioAtual == null)
                    return "Não carregado";

                return UsuarioAtual.Enabled ? "Ativo / Habilitado" : "Desabilitado";
            }
        }

        public string NovaDataFormatada
        {
            get
            {
                if (!NovaData.HasValue)
                    return "Sem expiração definida";

                return NovaData.Value.ToString("dd/MM/yyyy HH:mm:ss");
            }
        }

        private Brush _corMensagem = Brushes.Black;

        public Brush CorMensagem
        {
            get => _corMensagem;
            set
            {
                if (_corMensagem != value)
                {
                    _corMensagem = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusBarMensagem
        {
            get => _statusBarMensagem;
            set
            {
                if (_statusBarMensagem != value)
                {
                    _statusBarMensagem = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand BuscarCommand { get; }
        public ICommand RenovarCommand { get; }

        private bool CanExecuteBuscar()
        {
            return !string.IsNullOrWhiteSpace(Login) && DominioSelecionado.HasValue;
        }

        private void ExecuteBuscar()
        {
            try
            {
                StatusBarMensagem = "Buscando usuário...";
                Mensagem = string.Empty;
                NovaData = null;
                MostrarNovaData = false;

                var adService = new ActiveDirectoryService();
                var usuario = adService.BuscarUsuario(Login, DominioSelecionado!.Value);

                if (usuario == null)
                {
                    UsuarioAtual = null;
                    Mensagem = $"Usuário '{Login}' não encontrado no domínio selecionado.";
                    StatusBarMensagem = "Usuário não encontrado";
                    return;
                }

                UsuarioAtual = usuario;
                Mensagem = $"Usuário encontrado: {usuario.DisplayName}";
                StatusBarMensagem = "Usuário encontrado";
            }
            catch (Exception ex)
            {
                UsuarioAtual = null;
                Mensagem = $"Erro ao buscar usuário: {ex.Message}";
                StatusBarMensagem = "Erro na busca";
            }
        }

        private bool CanExecuteRenovar()
        {
            return !string.IsNullOrWhiteSpace(Login) &&
                   DominioSelecionado.HasValue &&
                   TipoContratoSelecionado.HasValue &&
                   UsuarioAtual != null;
        }

        private void ExecuteRenovar()
        {
            try
            {
                StatusBarMensagem = "Processando renovação...";
                Mensagem = string.Empty;
                NovaData = null;
                MostrarNovaData = false;

                var resultado = _orchestrator.Executar(
                    Login,
                    DominioSelecionado!.Value,
                    TipoContratoSelecionado!.Value
                );

                if (resultado.Sucesso)
                {
                    Mensagem = $"SUCESSO: {resultado.Mensagem}";
                    NovaData = resultado.NovaData;
                    MostrarNovaData = true;
                    StatusBarMensagem = "Renovação realizada com sucesso";

                    // Atualizar usuário atual com nova data
                    if (UsuarioAtual != null && resultado.NovaData.HasValue)
                    {
                        UsuarioAtual.AccountExpirationDate = resultado.NovaData;
                        OnPropertyChanged(nameof(DataExpiracaoFormatada));
                    }
                }
                else
                {
                    Mensagem = $"FALHA: {resultado.Mensagem}";
                    StatusBarMensagem = "Renovação não realizada";
                }
            }
            catch (Exception ex)
            {
                Mensagem = $"ERRO: {ex.Message}";
                StatusBarMensagem = "Erro na renovação";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
