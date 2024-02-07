using System.Linq;
using System.Windows;
using MaterialDesignThemes.Wpf;
using ShieldVSExtension.ViewModels;

namespace ShieldVSExtension.UI;

/// <summary>
///   Interaction logic for MainWindowControl.xaml
/// </summary>
public partial class MainWindowControl
{
    // private const string ExtensionConfigurationFile = "ExtensionConfiguration";
    private readonly MainViewModel _vm;

    public MainWindowControl(MainViewModel vm)
    {
        InitializeMaterialDesign();
        InitializeComponent();

        _vm = vm;
        DataContext = vm;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) =>
        ViewModelBase.ProjectChangedHandler.Invoke(_vm.Projects.FirstOrDefault());

    private void InitializeMaterialDesign()
    {
        _ = new Card();
        // var hue = new Hue("Dummy", Colors.Black, Colors.White);
    }
}