using System;

namespace GerenciaAd.Application
{
    /// <summary>
    /// Representa o resultado da execução de uma operação de renovação de conta.
    /// </summary>
    public class ResultadoExecucao
    {
        /// <summary>
        /// Indica se a operação foi concluída com sucesso.
        /// </summary>
        public bool Sucesso { get; set; }

        /// <summary>
        /// Mensagem descritiva do resultado da operação.
        /// </summary>
        public string Mensagem { get; set; } = string.Empty;

        /// <summary>
        /// Nova data de expiração aplicada (ou null se sem expiração).
        /// </summary>
        public DateTime? NovaData { get; set; }
    }
}
