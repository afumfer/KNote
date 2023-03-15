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

namespace KNote.ClientWin.Views
{
    public partial class ChatGPTForm : Form
    {
        private Store _store;
        private OpenAIClient _openAIClient;

        string Organization = "";
        string ApiKey = "";
        List<ChatMessage> messages = new List<ChatMessage>();
        string prompt = "";
        string ErrorMessage = "";
        bool Processing = false;
        int TotalTokens = 0;

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

            try
            {
                Processing = true;
                ErrorMessage = "";

                prompt = textPrompt.Text;
                textResult.Text = "";

                var chatPrompts = new List<ChatPrompt>();

                // -------------
                chatPrompts.Add(new ChatPrompt("system", "You are helpful Assistant"));
                // Add all existing messages to chatPrompts
                foreach (var item in messages)
                {
                    chatPrompts.Add(new ChatPrompt(item.Role, item.Prompt));
                }
                // ---------------

                // Add the new message to chatPrompts
                chatPrompts.Add(new ChatPrompt("user", prompt));
                // Call ChatGPT
                // Create a new ChatRequest object with the chat prompts and pass
                // it to the API's GetCompletionAsync method
                var chatRequest = new ChatRequest(chatPrompts, OpenAI.Models.Model.GPT3_5_Turbo);
                var result = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                // Create a new Message object with the user's prompt and other
                // details and add it to the messages list
                messages.Add(new ChatMessage
                {
                    Prompt = prompt,
                    Role = "user",
                    Tokens = result.Usage.PromptTokens
                });
                // Create a new Message object with the response and other details
                // and add it to the messages list
                messages.Add(new ChatMessage
                {
                    Prompt = result.FirstChoice.Message,
                    Role = "assistant",
                    Tokens = result.Usage.CompletionTokens
                });
                // Update the total number of tokens used by the API
                TotalTokens = TotalTokens + result.Usage.TotalTokens;

                textResult.Text = result.FirstChoice.Message;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //throw;
            }
        }

        private void buttonRestart_Click(object sender, EventArgs e)
        {
            RestartChatGPT();
        }

        private void RestartChatGPT()
        {
            prompt = "Write a 10 word description of OpenAI ChatGPT";
            messages = new List<ChatMessage>();
            TotalTokens = 0;
            ErrorMessage = "";
        }

    }
}
