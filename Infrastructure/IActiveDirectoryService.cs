using System;
using GerenciaAdConsole.Domain;

namespace GerenciaAdConsole.Infrastructure
{
    /// <summary>
    /// Interface para serviços de acesso ao Active Directory.
    /// </summary>
    public interface IActiveDirectoryService
    {
        /// <summary>
        /// Busca um usuário no Active Directory pelo login e domínio.
        /// </summary>
        /// <param name="login">Login (sAMAccountName) do usuário.</param>
        /// <param name="dominio">Domínio onde a busca será realizada.</param>
        /// <returns>Usuário encontrado ou null se não encontrado.</returns>
        UsuarioAd? BuscarUsuario(string login, DominioAD dominio);

        /// <summary>
        /// Atualiza a data de expiração de uma conta no Active Directory.
        /// </summary>
        /// <param name="login">Login (sAMAccountName) do usuário.</param>
        /// <param name="dominio">Domínio onde a atualização será realizada.</param>
        /// <param name="novaData">Nova data de expiração (null para remover expiração).</param>
        void AtualizarExpiracao(string login, DominioAD dominio, DateTime? novaData);
    }
}
