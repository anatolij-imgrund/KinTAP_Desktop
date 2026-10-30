///
/// Implementierung von IPasswordService.
/// 

using System.Diagnostics;

public class PasswordService : IPasswordService
{
    private readonly string _passwordFilePath;
    private string _currentLoadedPassword = "";

    public PasswordService(string baseDataFolderPath)
    {
        string dataFolderPath = Path.Combine(baseDataFolderPath, "KinTAP");
        Directory.CreateDirectory(dataFolderPath);
        _passwordFilePath = Path.Combine(dataFolderPath, "config.pwd");

        EnsurePasswordFileExists();
    }

    /// <summary>
    /// Prüft, ob bereits eine Datei mit Passwort angelegt wurde (dies geschieht bei der ersten Passwortänderung). 
    /// Ist keine Datei vorhanden, wird das Standardpasswort verwendet, andernfalls wird das in der Datei gespeicherte Passwort gelesen.
    /// </summary>
    public void EnsurePasswordFileExists(string defaultPassword = "KinTAP2025")
    {
        try
        {
            if (File.Exists(_passwordFilePath))
            {
                string passwordFromFile = File.ReadAllText(_passwordFilePath).Trim();
                if (!string.IsNullOrEmpty(passwordFromFile))
                {
                    _currentLoadedPassword = passwordFromFile;
                }
                else
                {
                    _currentLoadedPassword = defaultPassword;
                    File.WriteAllText(_passwordFilePath, _currentLoadedPassword);
                }
            }
            else
            {
                _currentLoadedPassword = defaultPassword;
                File.WriteAllText(_passwordFilePath, _currentLoadedPassword);
            }
            Debug.WriteLine($"PasswordService: Passwort geladet: '{_currentLoadedPassword}'");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PasswordService Fehler: {ex.Message}");
            _currentLoadedPassword = defaultPassword;
        }
    }

    /// <summary>
    /// Gibt das aktuell verwendete Passwort zurück.
    /// </summary>
    public string GetCurrentPassword()
    {
        return _currentLoadedPassword;
    }

    /// <summary>
    /// Prüft, ob das eingegebene Passwort mit dem aktuellen Passwort übereinstimmt.
    /// </summary>
    public bool VerifyPassword(string enteredPassword)
    {
        return GetCurrentPassword() == enteredPassword;
    }

    /// <summary>
    /// Dient zum Ändern des Passworts.
    /// </summary>
    public bool ChangePassword(string oldPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            Debug.WriteLine("PasswordService Fehler: Neues Passwort darf nicht leer sein.");
            return false;
        }

        if (VerifyPassword(oldPassword))
        {
            try
            {
                File.WriteAllText(_passwordFilePath, newPassword);
                _currentLoadedPassword = newPassword;
                Debug.WriteLine("PasswordService: Passwort wurde geändert.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PasswordService Fehler: {ex.Message}");
                return false;
            }
        }
        Debug.WriteLine("PasswordService: Alte Passwort stimmt nicht.");
        return false;
    }
}