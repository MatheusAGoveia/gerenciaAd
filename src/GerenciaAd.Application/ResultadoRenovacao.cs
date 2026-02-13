using System;

namespace GerenciaAd.Application
{
    /// <summary>
    /// Resultado da análise de renovação.
    /// </summary>
    public class ResultadoRenovacao
    {
        public bool PodeRenovar { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public DateTime? NovaDataSugerida { get; set; }
    }
}
