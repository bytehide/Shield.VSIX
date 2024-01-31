using MaterialDesignThemes.Wpf;

namespace ShieldVSExtension.UI
{
    /// <summary>
    ///   Interaction logic for WelcomeWindowControl.xaml
    /// </summary>
    public partial class WelcomeWindowControl
    {
        public WelcomeWindowControl()
        {
            InitializeMaterialDesign();
            InitializeComponent();
        }

        private void InitializeMaterialDesign()
        {
            _ = new Card();
            // var hue = new Hue("Dummy", Colors.Black, Colors.White);
        }
    }
}