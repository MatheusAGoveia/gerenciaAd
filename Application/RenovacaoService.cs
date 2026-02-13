using System;
using GerenciaAdConsole.Domain;
using GerenciaAdConsole.Infrastructure;

namespace GerenciaAdConsole.Application
{
    public class RenovacaoService
    {
        private readonly IActiveDirectoryService _adService;

        public RenovacaoService(IActiveDirectoryService adService)
        {
            _adService = adService ?? throw new ArgumentNullException(nameof(adService));
        }

        /// <summary>
        /// Renova a conta de um usuário no AD de acordo com o tipo de contrato informado.
        /// Calcula internamente a nova data e a aplica no Active Directory.
        /// Retorna a nova data de expiração aplicada (ou null para Efetivo).
        /// </summary>
        /// <param name="login">Login (sAMAccountName) do usuário.</param>
        /// <param name="dominio">Domínio em que o usuário será renovado.</param>
        /// <param name="tipo">Tipo de contrato escolhido.</param>
        /// <returns>Nova data de expiração aplicada (ou null se sem expiração).</returns>
        public DateTime? RenovarUsuario(string login, DominioAD dominio, TipoContrato tipo)
        {
            var novaData = CalcularNovaData(tipo);
            _adService.AtualizarExpiracao(login, dominio, novaData);
            return novaData;
        }

        /// <summary>
        /// Indica se a conta deve ser renovada com base na data de expiração atual
        /// e no tipo de contrato.
        /// </summary>
        /// <param name="dataExpiracao">Data de expiração atual da conta.</param>
        /// <param name="tipo">Tipo de contrato considerado.</param>
        /// <returns>true se deve renovar, false caso contrário.</returns>
        public bool DeveRenovar(DateTime? dataExpiracao, TipoContrato tipo)
        {
            // Efetivo sempre pode renovar
            if (tipo == TipoContrato.Efetivo)
            {
                return true;
            }

            // Se data for null e não for efetivo, permitir renovar
            if (!dataExpiracao.HasValue)
            {
                return true;
            }

            var hoje = DateTime.Now.Date;
            var expiracao = dataExpiracao.Value.Date;
            var diasRestantes = (expiracao - hoje).TotalDays;

            // Se faltar mais de 30 dias, retornar false
            if (diasRestantes > 30)
            {
                return false;
            }

            // Se faltar 30 dias ou menos, ou já expirou, retornar true
            return true;
        }

        /// <summary>
        /// Calcula a nova data de expiração com base no tipo de contrato.
        /// Sempre usa DateTime.Now como referência.
        /// </summary>
        private DateTime? CalcularNovaData(TipoContrato tipo)
        {
            var agora = DateTime.Now;

            return tipo switch
            {
                TipoContrato.Estagiario   => agora.AddMonths(6),
                TipoContrato.Comissionado => agora.AddYears(1),
                TipoContrato.Efetivo      => (DateTime?)null,
                _ => throw new ArgumentOutOfRangeException(nameof(tipo), "Tipo de contrato inválido.")
            };
        }
    }
}
