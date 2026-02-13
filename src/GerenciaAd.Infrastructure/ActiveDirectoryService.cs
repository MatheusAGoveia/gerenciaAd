using System;
using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;
using GerenciaAd.Application;
using GerenciaAd.Domain;

namespace GerenciaAd.Infrastructure
{
    [SupportedOSPlatform("windows")]
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        // Nomes dos domínios AD suportados.
        private const string NomeDominioSaude = "saude.pmb";
        private const string NomeDominioBetim = "betim.pmb";

        // Opcional: caso queira restringir a uma OU específica:
        // ex.: "OU=Usuarios,DC=meudominio,DC=local"
        private const string? ContainerPadrao = null;

        public UsuarioAd? BuscarUsuario(string login, DominioAD dominio)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Login não pode ser vazio.", nameof(login));

            using var context = CriarContexto(dominio);

            UserPrincipal? principal;
            try
            {
                principal = UserPrincipal.FindByIdentity(
                    context,
                    IdentityType.SamAccountName,
                    login
                );
            }
            catch
            {
                // Deixa a exceção subir para a camada de apresentação tratar.
                throw;
            }

            if (principal == null)
                return null;

            return new UsuarioAd
            {
                SamAccountName = principal.SamAccountName ?? string.Empty,
                DisplayName = principal.DisplayName ?? string.Empty,
                UserPrincipalName = principal.UserPrincipalName ?? string.Empty,
                AccountExpirationDate = principal.AccountExpirationDate,
                Enabled = principal.Enabled ?? true
            };
        }

        public void AtualizarExpiracao(string login, DominioAD dominio, DateTime? novaData)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Login não pode ser vazio.", nameof(login));

            using var context = CriarContexto(dominio);

            var principal = UserPrincipal.FindByIdentity(
                context,
                IdentityType.SamAccountName,
                login
            );

            if (principal == null)
            {
                var nomeDominio = ObterNomeDominio(dominio);
                throw new InvalidOperationException(
                    $"Usuário '{login}' não encontrado no domínio '{nomeDominio}'."
                );
            }

            principal.AccountExpirationDate = novaData;
            principal.Save();
        }

        private PrincipalContext CriarContexto(DominioAD dominio)
        {
            string nomeDominio = ObterNomeDominio(dominio);

            // Se necessário, ajustar para usar credenciais de serviço específicas.
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

        private static string ObterNomeDominio(DominioAD dominio)
        {
            return dominio switch
            {
                DominioAD.Saude => NomeDominioSaude,
                DominioAD.Betim => NomeDominioBetim,
                _ => throw new ArgumentOutOfRangeException(nameof(dominio), "Domínio não suportado.")
            };
        }
    }
}
