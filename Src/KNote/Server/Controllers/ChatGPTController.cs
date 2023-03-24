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
