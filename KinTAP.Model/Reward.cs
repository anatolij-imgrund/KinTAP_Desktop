using CommunityToolkit.Mvvm.ComponentModel;

namespace KinTAP.Model;

public partial class Reward : ObservableObject
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

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private int _cost;
    public int Cost
    {
        get => _cost;
        set => SetProperty(ref _cost, value);
    }

    public Reward()
    {
        
    }
}