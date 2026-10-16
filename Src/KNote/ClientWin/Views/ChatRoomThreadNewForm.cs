using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

// The topic of a new chat room thread, asked by KntChatForm (the thread is created by KntChatCtrl).
public partial class ChatRoomThreadNewForm : KntForm
{
    public ChatRoomThreadNewForm()
    {
        InitializeComponent();
        textTopic.MaxLength = ChatRoomThreadInfoDto.MaxTopicLength;
    }

    public string Topic => textTopic.Text.Trim();

    private void buttonAccept_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(textTopic.Text))
        {
            ShowInfo("The topic of the thread is required.");
            textTopic.Focus();
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
