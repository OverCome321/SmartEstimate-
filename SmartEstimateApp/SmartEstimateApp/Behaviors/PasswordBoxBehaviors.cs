using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartEstimateApp.Behaviors
{
    public static class PasswordBoxBehaviors
    {
        public static readonly DependencyProperty ToggleCommandProperty =
            DependencyProperty.RegisterAttached(
                "ToggleCommand",
                typeof(ICommand),
                typeof(PasswordBoxBehaviors),
                new PropertyMetadata(null, OnToggleCommandChanged));

        public static ICommand GetToggleCommand(DependencyObject obj) => (ICommand)obj.GetValue(ToggleCommandProperty);
        public static void SetToggleCommand(DependencyObject obj, ICommand value) => obj.SetValue(ToggleCommandProperty, value);

        public static readonly DependencyProperty ToggleCommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "ToggleCommandParameter",
                typeof(object),
                typeof(PasswordBoxBehaviors),
                new PropertyMetadata(null));

        public static object GetToggleCommandParameter(DependencyObject obj) => obj.GetValue(ToggleCommandParameterProperty);
        public static void SetToggleCommandParameter(DependencyObject obj, object value) => obj.SetValue(ToggleCommandParameterProperty, value);

        private static void OnToggleCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button)
            {
                button.Click += (s, args) =>
                {
                    var command = GetToggleCommand(button);
                    var parameter = GetToggleCommandParameter(button);
                    if (command?.CanExecute(parameter) == true)
                        command.Execute(parameter);
                };
            }
        }
    }

}
