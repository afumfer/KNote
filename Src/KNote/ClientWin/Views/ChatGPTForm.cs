using KNote.ClientWin.Core;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;

using System.Net.NetworkInformation;
using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KNote.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace KNote.ClientWin.Views
{
    public partial class ChatGPTForm : Form
    {
        private Store _store;
        private OpenAIClient _openAIClient;

        private string Organization = "";
        private string ApiKey = "";
        private List<ChatMessage> chatMessages = new List<ChatMessage>();
        private string prompt = "";
        private StringBuilder chatTextMessasges = new StringBuilder();
        //private string ErrorMessage = "";
        //private bool Processing = false;
        private int TotalTokens = 0;

        public ChatGPTForm()
        {
            InitializeComponent();
        }

        public ChatGPTForm(Store store) : this()
        {
            _store = store;

            Organization = _store.AppConfig.ChatGPTOrganization;
            ApiKey = _store.AppConfig.ChatGPTApiKey;
        }

        private void ChatGPTForm_Load(object sender, EventArgs e)
        {

            try
            {
                _openAIClient = new OpenAIClient(new OpenAIAuthentication(ApiKey, Organization));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //throw;
            }
        }

        private async void buttonSend_Click(object sender, EventArgs e)
        {
            //Processing = true;
            //ErrorMessage = "";

            try
            {
                prompt = textPrompt.Text;

                var chatPrompts = new List<ChatPrompt>();

                // -------------
                chatPrompts.Add(new ChatPrompt("system", "You are helpful Assistant"));
                // Add all existing messages to chatPrompts
                foreach (var item in chatMessages)
                {
                    chatPrompts.Add(new ChatPrompt(item.Role, item.Prompt));
                }
                // ---------------

                chatPrompts.Add(new ChatPrompt("user", prompt));
                var chatRequest = new ChatRequest(chatPrompts, OpenAI.Models.Model.GPT4);

                var result = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);

                chatMessages.Add(new ChatMessage
                {
                    Prompt = prompt,
                    Role = "user",
                    Tokens = result.Usage.PromptTokens
                });
                // Create a new Message object with the response and other details
                // and add it to the messages list
                chatMessages.Add(new ChatMessage
                {
                    Prompt = result.FirstChoice.Message,
                    Role = "assistant",
                    Tokens = result.Usage.CompletionTokens
                });
                // Update the total number of tokens used by the API
                TotalTokens = TotalTokens + result.Usage.TotalTokens;

                chatTextMessasges.Append($"\r\n");
                chatTextMessasges.Append($">> User:\r\n");
                chatTextMessasges.Append($"{prompt}\r\n");
                chatTextMessasges.Append($"\r\n");
                chatTextMessasges.Append($">> Assistant:\r\n");
                chatTextMessasges.Append(result.FirstChoice.Message.ToString().Replace("\n", "\r\n"));
                chatTextMessasges.Append($"\r\n");
                chatTextMessasges.Append($"\r\n");
                textResult.Text = chatTextMessasges.ToString();                
                textResult.SelectionStart = textResult.Text.Length;
                textResult.ScrollToCaret();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //throw;                
                // ErrorMessage = "ex.Message";

            }
            //finally 
            //{
            //    Processing = true;             
            //}

        }

        private void buttonRestart_Click(object sender, EventArgs e)
        {
            RestartChatGPT();
        }

        private void RestartChatGPT()
        {
            prompt = "";
            chatMessages = new List<ChatMessage>();
            TotalTokens = 0;
            chatTextMessasges.Clear();
            textResult.Text = chatTextMessasges.ToString();
            textPrompt.Text = "";
            //ErrorMessage = "";
        }

    }
}
