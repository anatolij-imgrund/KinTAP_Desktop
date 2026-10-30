using CommunityToolkit.Mvvm.ComponentModel;

namespace KinTAP.Model;

public partial class Child : ObservableObject
{
    private int _id;
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private int _points;
    public int Points
    {
        get => _points;
        set => SetProperty(ref _points, value);
    }

    [ObservableProperty]
    private string? _profileImagePath = "/Assets/default_avatar.png";

    [ObservableProperty]
    private string? _selectedAvatarPath = "/Assets/default_avatar.png";

    [ObservableProperty]
    private string? _selectedThemeColor = "AliceBlue";

    public Child()
    {
        
    }
}
