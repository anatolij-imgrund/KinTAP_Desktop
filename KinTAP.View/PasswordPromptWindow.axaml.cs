///
/// Das Passwort-Eingabefenster für den Zugang zum Elternpanel ist in View implementiert.
/// 

using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using Avalonia.Media;
using KinTAP.Services;

namespace KinTAP.View
{
    public partial class PasswordPromptWindow : Window
    {
        private readonly IPasswordService _passwordService;

        public PasswordPromptWindow()
        {
            InitializeComponent();
            _passwordService = new PasswordService(PlatformSpecificData.BaseDataPath);
            _passwordService.EnsurePasswordFileExists();
        }

        public PasswordPromptWindow(IPasswordService passwordService)
        {
            InitializeComponent();
            _passwordService = passwordService;

            PasswordInput = this.FindControl<TextBox>("PasswordInput");
            OkButton = this.FindControl<Button>("OkButton");
            CancelButton = this.FindControl<Button>("CancelButton");
            PasswordInfo = this.FindControl<TextBlock>("PasswordInfo");
            NewPasswordInput = this.FindControl<TextBox>("NewPasswordInput");
            NewPasswordConfirmationInput = this.FindControl<TextBox>("NewPasswordConfirmationInput");
            SaveButton = this.FindControl<Button>("SaveButton");

            if (PasswordInput == null || OkButton == null || CancelButton == null || SaveButton == null)
            {
                Debug.WriteLine("In PasswordPromptWindow fehlen die Elemente");
                return;
            }

            OkButton.Click += OkButton_Click;
            CancelButton.Click += CancelButton_Click;
            SaveButton.Click += SaveButton_Click;
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_passwordService.VerifyPassword(PasswordInput.Text ?? string.Empty))
            {
                Close(new PasswordPromptResult { LoginSuccessful = true });
            }
            else
            {
                Close(new PasswordPromptResult { LoginSuccessful = false });
                System.Console.WriteLine("Falsche Passwort.");
            }
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(new PasswordPromptResult { LoginCanceled = true });
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            string enteredPassword = PasswordInput.Text ?? "";
            string newPassword = NewPasswordInput.Text ?? "";
            string newPasswordConfirmationInput = NewPasswordConfirmationInput.Text ?? "";

            if (string.IsNullOrWhiteSpace(enteredPassword))
            {
                PasswordInfo.Foreground = Brushes.Red;
                PasswordInfo.Text = "Nicht geändert!";
                return;
            }

            if (newPassword != newPasswordConfirmationInput)
            {
                PasswordInfo.Foreground = Brushes.Red;
                PasswordInfo.Text = "Nicht geändert!";
                return;
            }

            if (_passwordService.ChangePassword(enteredPassword, newPassword))
            {
                PasswordInfo.Foreground = Brushes.Green;
                PasswordInfo.Text = "Geändert!";
                PasswordInput.Text = "";
                NewPasswordInput.Text = "";
                NewPasswordConfirmationInput.Text = "";
            }
        }
    }

    public class PasswordPromptResult
    {
        public bool LoginSuccessful { get; set; }
        public bool LoginCanceled { get; set; }
    }
}