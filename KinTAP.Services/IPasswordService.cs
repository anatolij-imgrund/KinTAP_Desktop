/// <summary>
/// Ein Interface bzw. eine Schnittstelle, die definiert, wie ein Passwort verarbeitet werden soll.
/// </summary>

public interface IPasswordService
{
    string GetCurrentPassword();
    bool VerifyPassword(string enteredPassword);
    bool ChangePassword(string oldPassword, string newPassword);
    void EnsurePasswordFileExists(string defaultPassword = "");
}
