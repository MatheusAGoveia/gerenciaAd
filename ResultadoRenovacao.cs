namespace GerenciaAdConsole
{
    public class ResultadoRenovacao
    {
        public bool PodeRenovar { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public DateTime? NovaDataSugerida { get; set; }
    }
}
