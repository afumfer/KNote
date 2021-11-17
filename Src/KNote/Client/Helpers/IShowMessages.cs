namespace KNote.Client.Helpers;

public interface IShowMessages
{
    Task ShowErrorMessage(string mensaje);
    Task ShowOkMessage(string mensaje);
}

