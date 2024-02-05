using System.Windows;
using ShieldVSExtension.ViewModels;

namespace ShieldVSExtension.UI.UserControls
{
    public partial class ActionBarControl
    {
        // private readonly SharedViewModel _vm;

        public ActionBarControl()
        {
            InitializeComponent();
            // _vm = new SharedViewModel();
            // DataContext = _vm;
        }

        #region Commands

        public ProjectViewModel Payload
        {
            get => (ProjectViewModel)GetValue(PayloadProperty);
            set
            {
                SetValue(PayloadProperty, value);
                // _vm.Payload = value;
            }
        }

        public static readonly DependencyProperty PayloadProperty = DependencyProperty.Register(
            nameof(Payload),
            typeof(ProjectViewModel),
            typeof(ActionBarControl),
            new PropertyMetadata(null));

        #endregion
    }
}