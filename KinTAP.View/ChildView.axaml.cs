///
/// Das Fenster, in dem das Kind seine Aufgaben anzeigen, als erledigt markieren, verdiente Punkte für Belohnungen ausgeben und sein Fenster personalisieren kann.
/// 

using Avalonia.Controls;
using Avalonia.Interactivity;
using KinTAP.Controller;
using System.Diagnostics;

namespace KinTAP.View;
public partial class ChildView : UserControl
{
    public ChildView() 
    { 
        InitializeComponent();
        this.Loaded += ChildView_Loaded;
    }

    private async void ChildView_Loaded(object? sender, RoutedEventArgs e)
    {
        this.Loaded -= ChildView_Loaded;
        if(this.DataContext is ChildController controller)
        {
            await controller.InitializeAsync();
        }
        else
        {
            Debug.WriteLine("Fehler: DataContext in ChildView ist kein ChildController beim Laden.");
        }
    }
}
