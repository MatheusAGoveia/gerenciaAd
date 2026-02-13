using System;

namespace GerenciaAd.UI.Wpf.Domain
{
    /// <summary>
    /// Representa um usuário do Active Directory.
    /// </summary>
    public class UsuarioAd
    {
        /// <summary>
        /// Login do usuário (sAMAccountName).
        /// </summary>
        public string SamAccountName { get; set; } = string.Empty;

        /// <summary>
        /// Nome de exibição do usuário.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// User Principal Name (UPN) do usuário.
        /// </summary>
        public string UserPrincipalName { get; set; } = string.Empty;

        /// <summary>
        /// Data de expiração da conta (null se sem expiração).
        /// </summary>
        public DateTime? AccountExpirationDate { get; set; }

        /// <summary>
        /// Indica se a conta está habilitada.
        /// </summary>
        public bool Enabled { get; set; }
    }
}
