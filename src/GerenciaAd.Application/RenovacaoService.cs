using System;
using GerenciaAd.Domain;

namespace GerenciaAd.Application
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
        /// Analisa se uma renovação pode ser realizada para um usuário e tipo de contrato específicos.
        /// Retorna um resultado com a decisão, motivo e data sugerida.
        /// </summary>
        /// <param name="usuario">Usuário do Active Directory a ser analisado.</param>
        /// <param name="tipo">Tipo de contrato para renovação.</param>
        /// <returns>Resultado da análise com indicação se pode renovar, motivo e data sugerida.</returns>
        public ResultadoRenovacao AnalisarRenovacao(UsuarioAd usuario, TipoContrato tipo)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            var resultado = new ResultadoRenovacao
            {
                NovaDataSugerida = CalcularNovaData(tipo)
            };

            // Regra 1: TipoContrato.Efetivo → sempre PodeRenovar = true
            if (tipo == TipoContrato.Efetivo)
            {
                resultado.PodeRenovar = true;
                resultado.Motivo = "Contrato efetivo permite renovação sem restrições.";
                return resultado;
            }

            // Regra 2: Se AccountExpirationDate for null → PodeRenovar = true
            if (!usuario.AccountExpirationDate.HasValue)
            {
                resultado.PodeRenovar = true;
                resultado.Motivo = "Conta sem data de expiração definida. Renovação permitida.";
                return resultado;
            }

            var dataExpiracao = usuario.AccountExpirationDate.Value;
            var agora = DateTime.Now;
            var diasRestantes = (dataExpiracao.Date - agora.Date).TotalDays;

            // Regra 3: Se já expirou → PodeRenovar = true
            if (diasRestantes < 0)
            {
                resultado.PodeRenovar = true;
                resultado.Motivo = "Conta já expirada. Renovação necessária e permitida.";
                return resultado;
            }

            // Regra 4: Se faltam mais de 30 dias → PodeRenovar = false
            if (diasRestantes > 30)
            {
                resultado.PodeRenovar = false;
                resultado.Motivo = $"Conta ainda possui {Math.Ceiling(diasRestantes)} dias de validade. Renovação não recomendada.";
                return resultado;
            }

            // Regra 5: Se faltam 30 dias ou menos → PodeRenovar = true
            resultado.PodeRenovar = true;
            resultado.Motivo = $"Conta expira em {Math.Ceiling(diasRestantes)} dias. Renovação permitida.";
            return resultado;
        }

        /// <summary>
        /// Calcula a nova data de expiração com base no tipo de contrato.
        /// Sempre usa DateTime.Now como referência.
        /// </summary>
        internal DateTime? CalcularNovaData(TipoContrato tipo)
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
