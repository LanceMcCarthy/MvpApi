namespace MvpCompanion.Maui.Services;

public interface ITrayService
{
    void Initialize();

    Action ClickHandler { get; set; }
}