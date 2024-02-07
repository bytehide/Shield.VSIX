using ShieldVSExtension.Common.Models;
using System.Windows;

namespace ShieldVSExtension.UI.UserControls;

/// <summary>
/// Interaction logic for ProtectionCheckControl.xaml
/// </summary>
public partial class ProtectionCheckControl
{
    public ProtectionCheckControl()
    {
        InitializeComponent();
    }

    #region Commands

    public ProtectionModel Protection
    {
        get => (ProtectionModel)GetValue(ProtectionProperty);
        set => SetValue(ProtectionProperty, value);
    }

    public static readonly DependencyProperty ProtectionProperty = DependencyProperty.Register(
        nameof(Protection),
        typeof(ProtectionModel),
        typeof(ProtectionCheckControl),
        new PropertyMetadata(null));

    #endregion
}