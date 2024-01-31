using MaterialDesignThemes.Wpf;
using ShieldVSExtension.ViewModels;

namespace ShieldVSExtension.UI
{
    /// <summary>
    ///   Interaction logic for MainWindowControl.xaml
    /// </summary>
    public partial class MainWindowControl
    {
        private readonly MainViewModel _vm;
        private const string ExtensionConfigurationFile = "ExtensionConfiguration";

        public MainWindowControl(MainViewModel vm)
        {
            InitializeMaterialDesign();
            InitializeComponent();

            _vm = vm;
            DataContext = vm;
        }

        private void InitializeMaterialDesign()
        {
            _ = new Card();
            // var hue = new Hue("Dummy", Colors.Black, Colors.White);
        }
    }
}