﻿using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public class NoteTypeWebApiService : INoteTypeWebApiService
{
    private readonly HttpClient _httpClient;

    private readonly AppState _appState;

    public NoteTypeWebApiService(AppState appState, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _appState = appState;
    }

    public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
    {
        return await _httpClient.GetFromJsonAsync<Result<List<NoteTypeDto>>>($"api/notetypes");
    }

    public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Result<NoteTypeDto>>($"api/notetypes/{id}");
    }

    public async Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto noteType)
    {
        HttpResponseMessage httpRes;

        if (noteType.NoteTypeId == Guid.Empty)
            httpRes = await _httpClient.PostAsJsonAsync($"api/notetypes", noteType);
        else
            httpRes = await _httpClient.PutAsJsonAsync($"api/notetypes", noteType);

        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTypeDto>>();

        return res;
    }

    public async Task<Result<NoteTypeDto>> DeleteAsync(Guid id)
    {
        var httpRes = await _httpClient.DeleteAsync($"api/notetypes/{id}");

        var res = await httpRes.Content.ReadFromJsonAsync<Result<NoteTypeDto>>();

        return res;
    }
}

