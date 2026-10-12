namespace KNote.Client.Shared;

/// <summary>An action of a row or item, shown in its ⋮ menu (KntActionsMenu). Danger: drawn in the danger color.</summary>
public record KntAction(string Text, string Icon, Func<Task> Run, bool Danger = false);
