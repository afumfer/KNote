using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// The assistant's View › What the AI receives: the system message put together, read only, with its size and its layers.
// Also KNote's instructions of a task (KNote Atlas), without layers.
public partial class AiPromptViewForm : KntForm
{
    public AiPromptViewForm()
    {
        InitializeComponent();
    }

    public AiPromptViewForm(string title, AiComposedPromptDto composed) : this()
    {
        Text = title;
        labelSummary.Text = AiPromptComposer.Summary(composed);
        textPrompt.Text = (composed.Text ?? "").ReplaceLineEndings("\r\n");
    }

    public AiPromptViewForm(string title, string text) : this()
    {
        Text = title;
        labelSummary.Text = "";
        textPrompt.Text = (text ?? "").ReplaceLineEndings("\r\n");
    }
}
