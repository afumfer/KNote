using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Threading.Tasks;
using OpenAI.Chat;
using OpenAI;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace KNote.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatGPTController : Controller
    {
        private readonly IKntService _service;
        private readonly IConfiguration _configuration;

        public ChatGPTController(IKntService service, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _service = service;
            _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;
            _configuration = configuration;
        }

        #region Old version
        //[HttpPost]   // POST api/chatgpt
        //public async Task<IActionResult> Post([FromBody] string prompt)
        //{            
        //    try
        //    {
        //        var Organization = _configuration["OpenAIServiceOptions:Organization"] ?? "";
        //        var ApiKey = _configuration["OpenAIServiceOptions:ApiKey"] ?? "";

        //        var api = new OpenAIClient(new OpenAIAuthentication(ApiKey, Organization));

        //        var chatPrompts = new List<ChatPrompt>();

        //        // -------------
        //        //chatPrompts.Add(new ChatPrompt("system", "You are helpful Assistant"));
        //        //// Add all existing messages to chatPrompts
        //        //foreach (var item in messages)
        //        //{
        //        //    chatPrompts.Add(new ChatPrompt(item.Role, item.Prompt));
        //        //}
        //        // ---------------

        //        chatPrompts.Add(new ChatPrompt("user", prompt));

        //        var chatRequest = new ChatRequest(chatPrompts, OpenAI.Models.Model.GPT4);
        //        var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);

        //        // TODO: return result with Entity = 
        //        //  new ChatMessage
        //        //{
        //        //    Prompt = result.FirstChoice.Message,
        //        //    Role = "assistant",
        //        //    Tokens = result.Usage.CompletionTokens
        //        //};

        //        var kresApi = new Result<string>();
        //        kresApi.Entity = result.FirstChoice.Message;                
        //        return Ok(kresApi);

        //    }
        //    catch (Exception ex)
        //    {
        //        var kresApi = new Result<string>();
        //        kresApi.AddErrorMessage("Generic error: " + ex.Message);
        //        return BadRequest(kresApi);
        //    }
        //}
        #endregion

        [HttpPost]   // POST api/chatgpt
        public async Task<IActionResult> Post([FromBody] List<ChatMessage> chatMessages)
        {
            try
            {
                var Organization = _configuration["OpenAIServiceOptions:Organization"] ?? "";
                var ApiKey = _configuration["OpenAIServiceOptions:ApiKey"] ?? "";

                var api = new OpenAIClient(new OpenAIAuthentication(ApiKey, Organization));

                var chatPrompts = new List<ChatPrompt>();
                
                chatPrompts.Add(new ChatPrompt("system", "You are helpful Assistant"));                
                foreach (var item in chatMessages)
                {
                    chatPrompts.Add(new ChatPrompt(item.Role, item.Prompt));
                }
                
                var chatRequest = new ChatRequest(chatPrompts, OpenAI.Models.Model.GPT4);
                var result = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
               
                var kresApi = new Result<ChatMessageOutput>();
                kresApi.Entity = new ChatMessageOutput
                {
                    Prompt = result.FirstChoice.Message,
                    Role = "assistant",
                    PromptTokens = result.Usage.PromptTokens,
                    CompletionTokens = result.Usage.CompletionTokens,
                    TotalTokens = result.Usage.TotalTokens

                };
                return Ok(kresApi);
            }
            catch (Exception ex)
            {
                var kresApi = new Result<string>();
                kresApi.AddErrorMessage("Generic error: " + ex.Message);
                return BadRequest(kresApi);
            }
        }

    }
}
