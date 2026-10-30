///
/// Das Hauptfenster dient als Einstiegspunkt und zur Auswahl des Benutzermodus.
/// 

using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using System.Linq;
using KinTAP.Controller;
using KinTAP.Persistence;
using KinTAP.Model;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using KinTAP.Services;
using System.Diagnostics;
using Avalonia.Platform.Storage;
using System;
using System.IO;

namespace KinTAP.View;

public partial class MainWindow : Window
{
    private readonly DataService _dataService;
    private readonly IPasswordService _passwordService;

    private Child? _selectedChildForLogin;

    public MainWindow()
    {        
        InitializeComponent();
        this.WindowState = WindowState.FullScreen;

        string desktopDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if(this.FindControl<Panel>("MainContentArea") == null || 
            this.FindControl<StackPanel>("SelectionPanel") == null ||
            this.FindControl<Button>("ParentModeButton") == null ||
            this.FindControl<Button>("ChildModeButton") == null ||
            this.FindControl<ListBox>("ChildSelectionList") == null ||
            this.FindControl<Button>("LoginAsChildButton") == null)
        {
            Debug.WriteLine($"Fehler! Nicht alle Elemente in MainWindow wurden gefunden");
        }

        IDataPersistence persistenceService = new JsonPersistenceService(PlatformSpecificData.BaseDataPath);
        _dataService = new DataService(persistenceService);
        _passwordService = new PasswordService(PlatformSpecificData.BaseDataPath);
        _passwordService.EnsurePasswordFileExists();

        ChildSelectionList.ItemsSource = null;
        SelectionPanel.IsVisible = true;
        ChildSelectionList.IsVisible = false;
        LoginAsChildButton.IsVisible = false;

        ParentModeButton.Click += ParentModeButton_Click;
        ChildModeButton.Click += ChildModeButton_Click;
        ChildSelectionList.SelectionChanged += ChildSelectionList_SelectionChanged;
        LoginAsChildButton.Click += LoginAsChildButton_Click;

        this.Opened += MainWindow_Opened;
    }

    private async Task HandleConfirmationRequestAsync(object? sender, ConfirmEventArgs args)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard(
            args.Title,
            args.Message,
            MsBox.Avalonia.Enums.ButtonEnum.YesNo,
            MsBox.Avalonia.Enums.Icon.Warning);

        var result = await messageBox.ShowWindowDialogAsync(this);
        args.Confirmed = (result == ButtonResult.Yes);
    }

    private async Task<string?> HandlePhotoSelectionRequestAsync(object? sender)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if(topLevel == null)
        {
            Debug.WriteLine("TopLevel holen nicht gelungen");
            return null;
        }

        var storageProvider = topLevel.StorageProvider;
        if(!storageProvider.CanOpen)
        {
            Debug.WriteLine("Daten lesen ist nicht möglich");
            return null;
        }

        var filePickerOpenOptions = new FilePickerOpenOptions
        {
            Title = "Foto des Kindes auswählen",
            AllowMultiple = false,
            FileTypeFilter = new[] {FilePickerFileTypes.ImageAll}
        };

        var result = await storageProvider.OpenFilePickerAsync(filePickerOpenOptions);

        if(result?.Count >= 1)
        {
            string? fileUri = result[0].TryGetLocalPath();
            if(fileUri == null)
            {
                Debug.WriteLine($"Fehler beim Pfad suche für File: {result[0].Name}");
                return null;
            }
            return fileUri;
        }
        else
        {
            Debug.WriteLine("File wurde nicht ausgewählt");
            return null;
        }
    }

    private async void MainWindow_Opened(object? sender, EventArgs e)
    {
        this.Opened -= MainWindow_Opened;

        Debug.WriteLine("Program gestartet... die Daten müssen geladen werden...");
        try
        {
            await _dataService.LoadAllDataAsync();
            Debug.WriteLine("Die Daten werden geladen...");
            ChildSelectionList.ItemsSource = _dataService.Children;
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Fehler beim Datenladen: {ex.Message}");
            var errorBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Kritishe Fehler", $"{ex.Message}", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Stop);
            await errorBox.ShowWindowDialogAsync(this);
        }
    }

    private async void ParentModeButton_Click(object? sender, RoutedEventArgs e)
    {
        var promptWindow = new PasswordPromptWindow(_passwordService);

        var result = await promptWindow.ShowDialog<PasswordPromptResult?>(this);

        if (result != null && result.LoginSuccessful)
        {
            var parentController = new ParentController(_dataService);
            parentController.RequestConfirmationAsync += HandleConfirmationRequestAsync;
            parentController.RequestPhotoSelectionAsync += HandlePhotoSelectionRequestAsync;
            var parentView = new ParentView
            {
                DataContext = parentController
            };
            var logoutButton = parentView.FindControl<Button>("ParentLogoutButton");
            if (logoutButton != null)
            {
                logoutButton.Click += GoBackToSelection;
            }
            else
            {
                Debug.WriteLine("Fehler! Button [ParentLogoutButton] wurde nicht in ParentView gefunden!");
            }

            MainContentArea.Children.Clear();
            MainContentArea.Children.Add(parentView);
            SelectionPanel.IsVisible = false;
        }
        else if (result != null && result.LoginCanceled)
        {
            return;
        }
        else if (result != null && !result.LoginSuccessful)
        {
            var errorBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Fehler beim Anmelden", "Falsche Passwort!", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await errorBox.ShowWindowDialogAsync(this);
        }        
    }

    private void ChildModeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ChildSelectionList != null && LoginAsChildButton != null && _dataService.Children.Any())
        {
            ChildSelectionList.IsVisible = !ChildSelectionList.IsVisible;
            LoginAsChildButton.IsVisible = !LoginAsChildButton.IsVisible;
            LoginAsChildButton.IsEnabled = !LoginAsChildButton.IsEnabled;
        }
    }

    private void ChildSelectionList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _selectedChildForLogin = ChildSelectionList.SelectedItem as Child;

        if(_selectedChildForLogin != null)
        {
            LoginAsChildButton.IsEnabled = true;
            LoginAsChildButton.Content = $"Start als {_selectedChildForLogin.Name}";
        }
        else
        {
            LoginAsChildButton.IsEnabled = false;
            LoginAsChildButton.Content = $"Start";
        }
    }

    private void LoginAsChildButton_Click(object? sender, RoutedEventArgs e)
    {
        if(_selectedChildForLogin == null)
        {
            return;
        }

        var childController = new ChildController(_dataService, _selectedChildForLogin.Id);

        var childView = new ChildView
        {
            DataContext = childController
        };

        var logoutButton = childView.FindControl<Button>("ChildLogoutButton");
        if(logoutButton != null)
        {
            logoutButton.Click += GoBackToSelection;
        }
        else
        {
            Debug.WriteLine("Fehler! Button [ChildLogoutButton] wurde nicht in ChildView gefunden!");
        }

        MainContentArea.Children.Clear();
        MainContentArea.Children.Add(childView);
        SelectionPanel.IsVisible = false;
    }

    private void GoBackToSelection(object? sender, RoutedEventArgs e)
    {
        MainContentArea.Children.Clear();

        if(!MainContentArea.Children.Contains(SelectionPanel))
        {
            MainContentArea.Children.Add(SelectionPanel);
        }

        SelectionPanel.IsVisible = true;
        ChildSelectionList.SelectedItem = null;
        _selectedChildForLogin = null;
        ChildSelectionList.IsVisible = false;
        LoginAsChildButton.IsVisible = false;
        LoginAsChildButton.IsEnabled = false;
        LoginAsChildButton.Content = "Start";
    }
}