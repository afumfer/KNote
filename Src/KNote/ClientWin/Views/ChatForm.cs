using KNote.ClientWin.Core;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KNote.ClientWin.Views;

public partial class ChatForm : Form
{    
    private HubConnection hubConnection;
    private Store _store;

    public ChatForm()
    {
        InitializeComponent();
    }

    public ChatForm(Store store) : this()
    {
        _store = store;

    }

    private async void ChatForm_Load(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_store.AppConfig.ChatHubUrl))
        {
            MessageBox.Show("Chat hub url is not defined. Set the chat hub url y Options menú.", "KaNote");
            this.Close();
            return;
        }

        try
        {
            hubConnection = new HubConnectionBuilder()
                           .WithUrl(_store.AppConfig.ChatHubUrl)
                           .Build();

            // Si se desconecta => debe intentar reconectar
            hubConnection.Closed += async (error) =>
            {
                Thread.Sleep(5000);
                await hubConnection.StartAsync();
            };

            hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                var encodeMessage = $"{user}: {message}";
                listMessages.Items.Add(encodeMessage);
                listMessages.Refresh();
            });

            Text += $" [{_store.AppUserName}]";
            labelServer.Text = _store.AppConfig.ChatHubUrl;

            await hubConnection.StartAsync();
        }
        catch (Exception)
        {
            listMessages.Items.Add($"The connection could not be started.");
        }
    }

    private async void buttonSend_Click(object sender, EventArgs e)
    {
        try
        {
            UseWaitCursor = true;
            if (hubConnection.State == HubConnectionState.Disconnected)
                await hubConnection.StartAsync();

            if (hubConnection.State == HubConnectionState.Connected)
                await hubConnection.SendAsync("SendMessage", _store.AppUserName, textMessage.Text);

            textMessage.Text = "";
        }
        catch (Exception ex)
        {
            listMessages.Items.Add($"The message coundn't be sent. Error: {ex.Message}");
            UseWaitCursor = false;
        }
        finally
        {
            UseWaitCursor = false;
        }
    }
}
