using System;
using System.Text;
using GerenciaAdConsole.Domain;
using GerenciaAdConsole.Application;
using GerenciaAdConsole.Infrastructure;

namespace GerenciaAdConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Renovação de Conta no Active Directory";

            Console.WriteLine("==============================================");
            Console.WriteLine("   Renovação de usuário no Active Directory");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            try
            {
                ExecutarFluxoRenovacao();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Ocorreu um erro inesperado na aplicação.");
                Console.WriteLine($"Detalhes técnicos: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey(intercept: true);
        }

        private static void ExecutarFluxoRenovacao()
        {
            // Coleta de dados do usuário
            var dominio = LerDominio();
            Console.WriteLine();

            Console.Write("Informe o login de rede (sAMAccountName): ");
            var login = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(login))
            {
                Console.WriteLine("Nenhum login informado. Encerrando operação.");
                return;
            }

            Console.WriteLine();
            var tipoContrato = LerTipoContrato();

            // Instanciar serviços
            var adService = new ActiveDirectoryService();
            var orchestrator = new RenovacaoOrchestrator(adService);

            // Executar renovação
            Console.WriteLine();
            Console.WriteLine("Processando renovação...");
            var resultado = orchestrator.Executar(login, dominio, tipoContrato);

            // Exibir resultado
            ExibirResultado(resultado);
        }

        private static DominioAD LerDominio()
        {
            Console.WriteLine("Selecione o domínio a consultar:");
            Console.WriteLine("1 - saude.pmb");
            Console.WriteLine("2 - betim.pmb");
            Console.Write("Opção: ");

            while (true)
            {
                var entrada = Console.ReadLine()?.Trim();

                if (int.TryParse(entrada, out var opcao))
                {
                    if (opcao == 1)
                        return DominioAD.Saude;

                    if (opcao == 2)
                        return DominioAD.Betim;
                }

                Console.WriteLine("Opção inválida. Informe 1 para saude.pmb ou 2 para betim.pmb.");
                Console.Write("Opção: ");
            }
        }

        private static TipoContrato LerTipoContrato()
        {
            Console.WriteLine("Selecione o tipo de vínculo para renovação:");
            Console.WriteLine("1 - Estagiário   (renovar para hoje + 6 meses)");
            Console.WriteLine("2 - Comissionado (renovar para hoje + 1 ano)");
            Console.WriteLine("3 - Efetivo      (remover expiração - conta sem data de expiração)");
            Console.Write("Opção: ");

            while (true)
            {
                var entrada = Console.ReadLine()?.Trim();

                if (int.TryParse(entrada, out var opcao))
                {
                    if (opcao == 1) return TipoContrato.Estagiario;
                    if (opcao == 2) return TipoContrato.Comissionado;
                    if (opcao == 3) return TipoContrato.Efetivo;
                }

                Console.WriteLine("Opção inválida. Informe 1, 2 ou 3.");
                Console.Write("Opção: ");
            }
        }

        private static void ExibirResultado(ResultadoExecucao resultado)
        {
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine("   Resultado da Operação");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            if (resultado.Sucesso)
            {
                Console.WriteLine("Status: SUCESSO");
                Console.WriteLine($"Mensagem: {resultado.Mensagem}");
                
                if (resultado.NovaData.HasValue)
                {
                    Console.WriteLine($"Nova data de expiração: {FormatarData(resultado.NovaData)}");
                }
                else
                {
                    Console.WriteLine("Nova data de expiração: Sem expiração definida");
                }
            }
            else
            {
                Console.WriteLine("Status: FALHA");
                Console.WriteLine($"Mensagem: {resultado.Mensagem}");
            }

            Console.WriteLine();
            Console.WriteLine("==============================================");
        }

        private static string FormatarData(DateTime? data)
        {
            if (!data.HasValue)
                return "Sem expiração definida";

            return data.Value.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }
}
