using System.Windows;

namespace WpfApp.Abstractions;

public interface IWindow
{
    event RoutedEventHandler Loaded;

    void Show();
}