﻿using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public class NoteWebApiService : INoteWebApiService
    {
        private readonly HttpClient _httpClient;

        public NoteWebApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<List<NoteInfoDto>>> GetHomeNotesAsync()
        {
            return await _httpClient.GetFromJsonAsync<Result<List<NoteInfoDto>>>("api/notes/homenotes");
        }

        public async Task<Result<NoteDto>> GetAsync(Guid noteId)
        {                        
            return await _httpClient.GetFromJsonAsync<Result<NoteDto>>($"api/notes/{noteId}");
        }

        public async Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null)
        {
            return await _httpClient.GetFromJsonAsync<Result<NoteDto>>($"api/notes/new");
        }

        public async Task<Result<NoteDto>> SaveAsync(NoteDto note, bool updateStatus = true)
        {            
            HttpResponseMessage httpRes;

            if (note.NoteId == Guid.Empty)
                httpRes = await _httpClient.PostAsJsonAsync<NoteDto>($"api/notes", note);
            else
                httpRes = await _httpClient.PutAsJsonAsync<NoteDto>($"api/notes", note);

            var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>();

            return res;
        }

        public async Task<Result<NoteDto>> DeleteAsync(Guid noteId)
        {
            var httpRes = await _httpClient.DeleteAsync($"api/notes/{noteId}");

            var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteDto>>();

            return res;
        }

        public async Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid noteId)
        {
            return await _httpClient.GetFromJsonAsync<Result<List<ResourceDto>>>($"api/notes/{noteId}/getresources");
        }

        public Task<Result<ResourceDto>> GetResourceAsync(Guid resourceId)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto resource)
        {                        
            HttpResponseMessage httpRes;

            if (resource.ResourceId == Guid.Empty)
                httpRes = await _httpClient.PostAsJsonAsync<ResourceDto>($"api/notes/saveresource", resource);
            else
                httpRes = await _httpClient.PutAsJsonAsync<ResourceDto>($"api/notes/saveresource", resource);

            var res = await httpRes.Content.ReadFromJsonAsync<Result<ResourceDto>>();

            return res;
        }

        public async Task<Result<ResourceDto>> DeleteResourceAsync(Guid resourceId)
        {                        
            var httpRes = await _httpClient.DeleteAsync($"api/notes/deleteresource/{resourceId}");
            var res = await httpRes.Content.ReadFromJsonAsync<Result<ResourceDto>>();
            return res;
        }

        public async Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId)
        {
            return await _httpClient.GetFromJsonAsync<Result<List<NoteTaskDto>>>($"api/notes/{noteId}/GetNoteTasks");
        }

        public Task<Result<NoteTaskDto>> GetNoteTaskAsync(Guid noteTaskId)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto noteTask)
        {            
            HttpResponseMessage httpRes;

            if (noteTask.NoteTaskId == Guid.Empty)
                httpRes = await _httpClient.PostAsJsonAsync<NoteTaskDto>($"api/notes/savenotetask", noteTask);
            else
                httpRes = await _httpClient.PutAsJsonAsync<NoteTaskDto>($"api/notes/savenotetask", noteTask);

            var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTaskDto>>();

            return res;
        }

        public async Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid noteTaskId)
        {                     
            var httpRes = await _httpClient.DeleteAsync($"api/notes/deletenotetask/{noteTaskId}");
            var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTaskDto>>();
            return res;
        }

    }
}
