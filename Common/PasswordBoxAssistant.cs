using System.Windows;
using System.Windows.Controls;

namespace ShieldVSExtension.Common;

internal static class PasswordBoxAssistant
{
    public static readonly DependencyProperty BoundPassword =
        DependencyProperty.RegisterAttached(nameof(BoundPassword), typeof(string), typeof(PasswordBoxAssistant),
            new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

    public static readonly DependencyProperty BindPassword =
        DependencyProperty.RegisterAttached(nameof(BindPassword), typeof(bool), typeof(PasswordBoxAssistant),
            new PropertyMetadata(false, OnBindPasswordChanged));

    private static readonly DependencyProperty UpdatingPassword =
        DependencyProperty.RegisterAttached(nameof(UpdatingPassword), typeof(bool), typeof(PasswordBoxAssistant),
            new PropertyMetadata(false));

    private static void OnBoundPasswordChanged(DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
        // only handle this event when the property is attached to a PasswordBox
        // and when the BindPassword attached property has been set to true
        if (dependencyObject is not PasswordBox control || !GetBindPassword(dependencyObject)) return;

        // avoid recursive updating by ignoring the box's changed event
        control.PasswordChanged -= HandlePasswordChanged;

        var newPassword = (string)e.NewValue;

        if (!GetUpdatingPassword(control)) control.Password = newPassword;

        control.PasswordChanged += HandlePasswordChanged;
    }

    private static void OnBindPasswordChanged(DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox control) return;

        var wasBound = (bool)e.OldValue;
        var needToBind = (bool)e.NewValue;

        if (wasBound) control.PasswordChanged -= HandlePasswordChanged;

        if (needToBind) control.PasswordChanged += HandlePasswordChanged;
    }

    private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox control) return;

        // set a flag to indicate that we're updating the password
        SetUpdatingPassword(control, true);
        // push the new password into the BoundPassword property
        SetBoundPassword(control, control.Password);
        SetUpdatingPassword(control, false);
    }

    public static void SetBindPassword(DependencyObject dependencyObject, bool value) =>
        dependencyObject.SetValue(BindPassword, value);

    public static bool GetBindPassword(DependencyObject dependencyObject) =>
        (bool)dependencyObject.GetValue(BindPassword);

    private static bool GetUpdatingPassword(DependencyObject dependencyObject) =>
        (bool)dependencyObject.GetValue(UpdatingPassword);

    public static void SetBoundPassword(DependencyObject dependencyObject, string value) =>
        dependencyObject.SetValue(BoundPassword, value);

    private static void SetUpdatingPassword(DependencyObject dependencyObject, bool value) =>
        dependencyObject.SetValue(UpdatingPassword, value);
}