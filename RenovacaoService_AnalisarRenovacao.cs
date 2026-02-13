using System;

namespace GerenciaAdConsole
{
    // Trecho a ser adicionado na classe RenovacaoService existente
    public partial class RenovacaoService
    {
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
    }
}
