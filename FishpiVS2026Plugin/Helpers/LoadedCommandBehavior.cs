using System;
using System.Windows;
using System.Windows.Input;

namespace FishpiVS2026Plugin
{
    public static class LoadedCommandBehavior
    {
        public static readonly DependencyProperty LoadedCommandProperty =
            DependencyProperty.RegisterAttached(
                "LoadedCommand",
                typeof(ICommand),
                typeof(LoadedCommandBehavior),
                new PropertyMetadata(null, OnLoadedCommandChanged));

        public static void SetLoadedCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(LoadedCommandProperty, value);
        }

        public static ICommand GetLoadedCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(LoadedCommandProperty);
        }

        private static void OnLoadedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.Loaded -= OnElementLoaded;
                if (e.NewValue is ICommand command)
                {
                    element.Loaded += (s, args) => OnElementLoaded(s, args, command);
                }
            }
        }

        private static void OnElementLoaded(object sender, RoutedEventArgs e, ICommand command)
        {
            if (command.CanExecute(sender))
            {
                command.Execute(sender);
            }
        }

        private static void OnElementLoaded(object sender, RoutedEventArgs e) { }
    }
}
