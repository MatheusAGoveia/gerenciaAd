using System;
using GerenciaAd.UI.Wpf.Domain;
using GerenciaAd.UI.Wpf.Infrastructure;

namespace GerenciaAd.UI.Wpf.Application
{
    /// <summary>
    /// Orquestrador responsável por coordenar o fluxo completo de renovação de contas no Active Directory.
    /// </summary>
    public class RenovacaoOrchestrator
    {
        private readonly IActiveDirectoryService _adService;
        private readonly RenovacaoService _renovacaoService;

        public RenovacaoOrchestrator(IActiveDirectoryService adService)
        {
            _adService = adService ?? throw new ArgumentNullException(nameof(adService));
            _renovacaoService = new RenovacaoService(adService);
        }

        /// <summary>
        /// Executa o fluxo completo de renovação de conta:
        /// 1. Busca o usuário no Active Directory
        /// 2. Analisa se a renovação pode ser realizada
        /// 3. Se não pode renovar, retorna resultado com mensagem
        /// 4. Se pode renovar, executa a renovação e retorna sucesso
        /// </summary>
        /// <param name="login">Login (sAMAccountName) do usuário.</param>
        /// <param name="dominio">Domínio onde a operação será realizada.</param>
        /// <param name="tipo">Tipo de contrato para renovação.</param>
        /// <returns>Resultado da execução contendo sucesso, mensagem e nova data aplicada.</returns>
        public ResultadoExecucao Executar(string login, DominioAD dominio, TipoContrato tipo)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                return new ResultadoExecucao
                {
                    Sucesso = false,
                    Mensagem = "Login não pode ser vazio."
                };
            }

            // 1. Buscar usuário
            UsuarioAd? usuario;
            try
            {
                usuario = _adService.BuscarUsuario(login, dominio);
            }
            catch (Exception ex)
            {
                return new ResultadoExecucao
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao buscar usuário no Active Directory: {ex.Message}"
                };
            }

            if (usuario == null)
            {
                var nomeDominio = ObterNomeDominio(dominio);
                return new ResultadoExecucao
                {
                    Sucesso = false,
                    Mensagem = $"Usuário '{login}' não encontrado no domínio '{nomeDominio}'."
                };
            }

            // 2. Analisar se pode renovar
            var analise = _renovacaoService.AnalisarRenovacao(usuario, tipo);

            if (!analise.PodeRenovar)
            {
                return new ResultadoExecucao
                {
                    Sucesso = false,
                    Mensagem = analise.Motivo
                };
            }

            // 3. Renovar conta
            try
            {
                var novaData = _renovacaoService.RenovarUsuario(login, dominio, tipo);

                return new ResultadoExecucao
                {
                    Sucesso = true,
                    Mensagem = "Renovação realizada com sucesso.",
                    NovaData = novaData
                };
            }
            catch (Exception ex)
            {
                return new ResultadoExecucao
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao renovar conta no Active Directory: {ex.Message}"
                };
            }
        }

        private static string ObterNomeDominio(DominioAD dominio)
        {
            return dominio switch
            {
                DominioAD.Saude => "saude.pmb",
                DominioAD.Betim => "betim.pmb",
                _ => "desconhecido"
            };
        }
    }
}
