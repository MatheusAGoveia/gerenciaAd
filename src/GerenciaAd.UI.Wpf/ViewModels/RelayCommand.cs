using System;
using System.Windows.Input;

namespace GerenciaAd.UI.Wpf.ViewModels
{
    /// <summary>
    /// Implementação padrão de ICommand para WPF usando padrão RelayCommand.
    /// Suporta execução com ou sem parâmetro e validação de CanExecute.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<object?, bool>? _canExecute;
        private readonly Action<object?> _execute;

        /// <summary>
        /// Cria uma nova instância de RelayCommand.
        /// </summary>
        /// <param name="execute">Ação a ser executada quando o comando for invocado.</param>
        /// <param name="canExecute">Função que determina se o comando pode ser executado. Opcional.</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(execute != null ? (Action<object?>)(_ => execute()) : throw new ArgumentNullException(nameof(execute)),
                   canExecute != null ? (Func<object?, bool>)(_ => canExecute()) : null)
        {
        }

        /// <summary>
        /// Cria uma nova instância de RelayCommand com suporte a parâmetro.
        /// </summary>
        /// <param name="execute">Ação a ser executada quando o comando for invocado (recebe parâmetro).</param>
        /// <param name="canExecute">Função que determina se o comando pode ser executado (recebe parâmetro). Opcional.</param>
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Evento disparado quando a capacidade de execução do comando muda.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Determina se o comando pode ser executado no estado atual.
        /// </summary>
        /// <param name="parameter">Parâmetro opcional do comando.</param>
        /// <returns>true se o comando pode ser executado; caso contrário, false.</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Executa o comando.
        /// </summary>
        /// <param name="parameter">Parâmetro opcional do comando.</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Notifica que a capacidade de execução do comando mudou.
        /// Deve ser chamado quando as condições de CanExecute mudarem.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Implementação genérica de ICommand para WPF usando padrão RelayCommand.
    /// Suporta parâmetros tipados.
    /// </summary>
    /// <typeparam name="T">Tipo do parâmetro do comando.</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T?, bool>? _canExecute;
        private readonly Action<T?> _execute;

        /// <summary>
        /// Cria uma nova instância de RelayCommand genérico.
        /// </summary>
        /// <param name="execute">Ação a ser executada quando o comando for invocado.</param>
        /// <param name="canExecute">Função que determina se o comando pode ser executado. Opcional.</param>
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Evento disparado quando a capacidade de execução do comando muda.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Determina se o comando pode ser executado no estado atual.
        /// </summary>
        /// <param name="parameter">Parâmetro do comando.</param>
        /// <returns>true se o comando pode ser executado; caso contrário, false.</returns>
        public bool CanExecute(object? parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
                return _canExecute?.Invoke(default) ?? true;

            if (parameter is T typedParameter)
                return _canExecute?.Invoke(typedParameter) ?? true;

            return false;
        }

        /// <summary>
        /// Executa o comando.
        /// </summary>
        /// <param name="parameter">Parâmetro do comando.</param>
        public void Execute(object? parameter)
        {
            if (parameter is T typedParameter)
            {
                _execute(typedParameter);
            }
            else if (parameter == null && !typeof(T).IsValueType)
            {
                _execute(default);
            }
        }

        /// <summary>
        /// Notifica que a capacidade de execução do comando mudou.
        /// Deve ser chamado quando as condições de CanExecute mudarem.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
