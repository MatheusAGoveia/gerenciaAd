using System;
using System.DirectoryServices.AccountManagement;
using System.Text;

namespace GerenciaAdConsole
{
    internal enum DominioAD
    {
        Saude = 1,
        Betim = 2
    }

    internal enum TipoContrato
    {
        Estagiario = 1,
        Comissionado = 2,
        Efetivo = 3
    }

    internal class Program
    {
        // Nomes dos domínios AD suportados.
        private const string NomeDominioSaude = "saude.pmb";
        private const string NomeDominioBetim = "betim.pmb";

        // Opcional: caso queira restringir a uma OU específica:
        // ex.: "OU=Usuarios,DC=meudominio,DC=local"
        private const string? ContainerPadrao = null;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Consulta de Usuário no Active Directory";

            Console.WriteLine("==============================================");
            Console.WriteLine("   Consulta de usuário no Active Directory");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            Console.WriteLine("Selecione o domínio a consultar:");
            Console.WriteLine("1 - saude.pmb");
            Console.WriteLine("2 - betim.pmb");
            Console.Write("Opção: ");

            var dominioSelecionado = LerDominioSelecionado();

            Console.WriteLine();

            Console.Write("Informe o login de rede (sAMAccountName): ");
            var samAccountName = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(samAccountName))
            {
                Console.WriteLine("Nenhum login informado. Encerrando aplicação.");
                PausarAntesDeSair();
                return;
            }

            try
            {
                ConsultarUsuarioNoAd(dominioSelecionado, samAccountName);
            }
            catch (PrincipalServerDownException ex)
            {
                Console.WriteLine();
                Console.WriteLine("Erro ao conectar ao Active Directory.");
                Console.WriteLine("Verifique se o computador está no domínio e se o domínio/configuração estão corretos.");
                Console.WriteLine($"Detalhes técnicos: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Ocorreu um erro inesperado ao consultar o usuário.");
                Console.WriteLine($"Detalhes técnicos: {ex.Message}");
            }

            PausarAntesDeSair();
        }

        /// <summary>
        /// Consulta um usuário no Active Directory pelo sAMAccountName
        /// usando System.DirectoryServices.AccountManagement.
        /// </summary>
        /// <param name="dominio">Domínio onde a consulta será realizada.</param>
        /// <param name="samAccountName">Login de rede do usuário.</param>
        private static void ConsultarUsuarioNoAd(DominioAD dominio, string samAccountName)
        {
            using var context = CriarContexto(dominio);

            string nomeDominio = ObterNomeDominio(dominio);

            Console.WriteLine();
            Console.WriteLine($"Consultando usuário '{samAccountName}' no domínio '{nomeDominio}'...");

            UserPrincipal? usuario = null;

            try
            {
                usuario = UserPrincipal.FindByIdentity(
                    context,
                    IdentityType.SamAccountName,
                    samAccountName
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("Falha ao buscar o usuário no AD.");
                Console.WriteLine($"Detalhes técnicos: {ex.Message}");
                return;
            }

            if (usuario == null)
            {
                Console.WriteLine();
                Console.WriteLine($"Usuário '{samAccountName}' não encontrado no domínio.");
                return;
            }

            // Usuário encontrado – exibe informações básicas
            Console.WriteLine();
            Console.WriteLine("Usuário encontrado no Active Directory:");
            Console.WriteLine("----------------------------------------------");

            Console.WriteLine($"Nome exibido..............: {usuario.DisplayName}");
            Console.WriteLine($"Nome de login (sAMAccountName): {usuario.SamAccountName}");
            Console.WriteLine($"UPN (UserPrincipalName)...: {usuario.UserPrincipalName}");

            // Data de expiração
            if (usuario.AccountExpirationDate.HasValue)
            {
                var exp = usuario.AccountExpirationDate.Value;
                Console.WriteLine($"Data de expiração da conta: {exp:dd/MM/yyyy HH:mm:ss}");
            }
            else
            {
                Console.WriteLine("Data de expiração da conta: Sem expiração definida (possivelmente nunca expira).");
            }

            // Status habilitado/desabilitado
            bool habilitado = usuario.Enabled ?? true; // se null, assume habilitado
            string status = habilitado ? "Ativo / Habilitado" : "Desabilitado";
            Console.WriteLine($"Status da conta...........: {status}");

            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("Consulta concluída.");
            Console.WriteLine();

            // Exibe status de expiração (informativo, não bloqueia renovação)
            ExibirStatusExpiracao(usuario);

            // Menu de renovação de conta
            ExibirMenuRenovacao(usuario);
        }

        /// <summary>
        /// Exibe, de forma informativa, o status de expiração da conta,
        /// com base em DateTime.Now e na AccountExpirationDate do usuário.
        /// </summary>
        /// <param name="usuario">Usuário cujo status de expiração será exibido.</param>
        private static void ExibirStatusExpiracao(UserPrincipal usuario)
        {
            Console.WriteLine("Status de expiração da conta:");
            Console.WriteLine("----------------------------------------------");

            DateTime agora = DateTime.Now;

            if (!usuario.AccountExpirationDate.HasValue)
            {
                Console.WriteLine("Conta sem expiração.");
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine();
                return;
            }

            DateTime dataExpiracao = usuario.AccountExpirationDate.Value;

            // Considera apenas a parte da data para cálculo em dias
            double diasRestantesDouble = (dataExpiracao.Date - agora.Date).TotalDays;

            if (diasRestantesDouble < 0)
            {
                Console.WriteLine("Conta já expirada");
            }
            else
            {
                int diasRestantes = (int)Math.Ceiling(diasRestantesDouble);
                Console.WriteLine($"Conta expira em {diasRestantes} dias");

                if (diasRestantes > 30)
                {
                    Console.WriteLine("ATENÇÃO: Esta conta ainda possui mais de 30 dias de validade.");
                }
                else if (diasRestantes <= 5)
                {
                    Console.WriteLine("Conta próxima de expirar.");
                }
            }

            Console.WriteLine("----------------------------------------------");
            Console.WriteLine();
        }

        /// <summary>
        /// Lê do console a opção de domínio escolhida pelo operador.
        /// Sempre retorna um domínio válido (Saude ou Betim).
        /// </summary>
        private static DominioAD LerDominioSelecionado()
        {
            while (true)
            {
                var entrada = Console.ReadLine()?.Trim();

                if (int.TryParse(entrada, out var opcao))
                {
                    if (opcao == 1)
                    {
                        return DominioAD.Saude;
                    }

                    if (opcao == 2)
                    {
                        return DominioAD.Betim;
                    }
                }

                Console.WriteLine("Opção inválida. Informe 1 para saude.pmb ou 2 para betim.pmb.");
                Console.Write("Opção: ");
            }
        }

        /// <summary>
        /// Cria um PrincipalContext para o domínio informado.
        /// Responsável por centralizar a configuração de conexão com o AD.
        /// </summary>
        /// <param name="dominio">Domínio alvo.</param>
        private static PrincipalContext CriarContexto(DominioAD dominio)
        {
            string nomeDominio = ObterNomeDominio(dominio);

            // Se necessário, você pode trocar para o construtor com credenciais específicas.
            // Exemplo:
            // return new PrincipalContext(
            //     ContextType.Domain,
            //     nomeDominio,
            //     ContainerPadrao,
            //     "USUARIO_DE_SERVICO",
            //     "SENHA_DE_SERVICO"
            // );

            return new PrincipalContext(
                ContextType.Domain,
                nomeDominio,
                ContainerPadrao
            );
        }

        /// <summary>
        /// Obtém o nome de domínio (string) correspondente ao enum DominioAD.
        /// </summary>
        private static string ObterNomeDominio(DominioAD dominio)
        {
            return dominio switch
            {
                DominioAD.Saude => NomeDominioSaude,
                DominioAD.Betim => NomeDominioBetim,
                _ => throw new ArgumentOutOfRangeException(nameof(dominio), "Domínio não suportado.")
            };
        }

        private static void ExibirMenuRenovacao(UserPrincipal usuario)
        {
            while (true)
            {
                Console.WriteLine("Deseja renovar a conta deste usuário?");
                Console.WriteLine("1 - Estagiário   (renovar para hoje + 6 meses)");
                Console.WriteLine("2 - Comissionado (renovar para hoje + 1 ano)");
                Console.WriteLine("3 - Efetivo      (remover expiração - conta sem data de expiração)");
                Console.WriteLine("0 - Não renovar / Cancelar");
                Console.Write("Informe a opção desejada: ");

                var entrada = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(entrada) || entrada == "0")
                {
                    Console.WriteLine("Renovação não realizada. Operação cancelada pelo operador.");
                    return;
                }

                if (int.TryParse(entrada, out var opcao) &&
                    opcao >= 1 && opcao <= 3)
                {
                    var tipo = (TipoContrato)opcao;
                    RenovarConta(usuario, tipo);
                    return;
                }

                Console.WriteLine("Opção inválida. Tente novamente.");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Renova a conta de um usuário no AD de acordo com o tipo de contrato informado.
        /// A nova data é sempre baseada em DateTime.Now.
        /// </summary>
        /// <param name="usuario">Usuário a ser renovado.</param>
        /// <param name="tipo">Tipo de contrato escolhido.</param>
        private static void RenovarConta(UserPrincipal usuario, TipoContrato tipo)
        {
            DateTime? dataAtualExpiracao = usuario.AccountExpirationDate;
            DateTime agora = DateTime.Now;
            DateTime? novaDataExpiracao;

            switch (tipo)
            {
                case TipoContrato.Estagiario:
                    novaDataExpiracao = agora.AddMonths(6);
                    break;
                case TipoContrato.Comissionado:
                    novaDataExpiracao = agora.AddYears(1);
                    break;
                case TipoContrato.Efetivo:
                    novaDataExpiracao = null; // sem expiração
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo), "Tipo de contrato inválido.");
            }

            Console.WriteLine();
            Console.WriteLine("Resumo da operação de renovação (pré-visualização):");
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine($"Usuário.....................: {usuario.SamAccountName} - {usuario.DisplayName}");
            Console.WriteLine($"Tipo de vínculo escolhido...: {tipo}");
            Console.WriteLine($"Data de expiração atual.....: {FormatarData(dataAtualExpiracao)}");
            Console.WriteLine($"Nova data de expiração......: {FormatarData(novaDataExpiracao)}");
            Console.WriteLine($"Data/hora da solicitação....: {agora:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine("---------------------------------------------------");
            Console.Write("Confirma a aplicação desta alteração? (S/N): ");

            var confirmacao = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(confirmacao) ||
                !confirmacao.Trim().StartsWith("S", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Operação cancelada. Nenhuma alteração foi salva no Active Directory.");
                return;
            }

            bool sucesso = false;
            string? mensagemErro = null;
            DateTime dataOperacao = DateTime.Now;

            try
            {
                usuario.AccountExpirationDate = novaDataExpiracao;
                usuario.Save();
                sucesso = true;
            }
            catch (Exception ex)
            {
                mensagemErro = ex.Message;
            }

            Console.WriteLine();
            Console.WriteLine("Resultado da operação de renovação:");
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine($"Data de expiração anterior..: {FormatarData(dataAtualExpiracao)}");
            Console.WriteLine($"Nova data de expiração......: {FormatarData(novaDataExpiracao)}");
            Console.WriteLine($"Tipo de vínculo aplicado....: {tipo}");
            Console.WriteLine($"Data/hora da operação.......: {dataOperacao:dd/MM/yyyy HH:mm:ss}");

            if (sucesso)
            {
                Console.WriteLine("Status final.................: SUCESSO - Conta renovada no Active Directory.");
            }
            else
            {
                Console.WriteLine("Status final.................: ERRO ao salvar a alteração no Active Directory.");
                if (!string.IsNullOrEmpty(mensagemErro))
                {
                    Console.WriteLine($"Detalhes técnicos............: {mensagemErro}");
                }
            }

            Console.WriteLine("---------------------------------------------------");
        }

        private static string FormatarData(DateTime? data)
        {
            if (!data.HasValue)
            {
                return "Sem expiração definida";
            }

            return data.Value.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private static void PausarAntesDeSair()
        {
            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey(intercept: true);
        }
    }
}