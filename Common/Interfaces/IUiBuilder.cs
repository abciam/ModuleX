namespace Common.Interfaces;

public interface IUiBuilder
{
    void AddAction(string title, Action onClick);

    void AddInfo(string text);
}