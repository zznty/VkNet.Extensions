using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WpfApp.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        Debug.WriteLine($"PropertyChanged {this}.{propertyName}");
        PropertyChanged?.Invoke(this, new(propertyName));
    }
}