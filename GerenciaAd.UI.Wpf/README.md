# GerenciaAd.UI.Wpf

Projeto WPF (.NET 8) para interface gráfica de renovação de contas no Active Directory.

## Estrutura

```
GerenciaAd.UI.Wpf/
├── Views/
│   └── MainWindow.xaml
├── ViewModels/
│   ├── MainViewModel.cs
│   └── RelayCommand.cs
├── App.xaml
├── App.xaml.cs
└── GerenciaAd.UI.Wpf.csproj
```

## Dependências

- GerenciaAd.Domain
- GerenciaAd.Application
- GerenciaAd.Infrastructure

## Funcionalidades

- Busca de usuário no Active Directory
- Seleção de domínio (Saude, Betim)
- Seleção de tipo de contrato (Estagiario, Comissionado, Efetivo)
- Renovação de conta com validação
- Exibição de informações do usuário
- Feedback visual de operações (sucesso/erro)

## Padrão MVVM

- **View**: MainWindow.xaml (apenas apresentação)
- **ViewModel**: MainViewModel.cs (lógica de apresentação, sem regras de negócio)
- **Model**: Classes do Domain (UsuarioAd, DominioAD, TipoContrato)

## Notas

- Todas as regras de negócio estão na camada Application
- O ViewModel apenas coordena a UI e chama os serviços da Application
- Commands implementados usando RelayCommand pattern
