using MaterialDesignThemes.Wpf;
using ShieldVSExtension.ViewModels;

namespace ShieldVSExtension.UI
{
    /// <summary>
    ///   Interaction logic for MainWindowControl.xaml
    /// </summary>
    public partial class MainWindowControl
    {
        // private const string ExtensionConfigurationFile = "ExtensionConfiguration";

        public MainWindowControl(MainViewModel vm)
        {
            InitializeMaterialDesign();
            InitializeComponent();

            DataContext = vm;
        }

        private void InitializeMaterialDesign()
        {
            _ = new Card();
            // var hue = new Hue("Dummy", Colors.Black, Colors.White);
        }
    }
}