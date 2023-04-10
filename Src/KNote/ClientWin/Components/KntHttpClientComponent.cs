﻿using KNote.ClientWin.Core;
using KNote.Model;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KNote.ClientWin.Components
{
    public class KntHttpClientComponent : ComponentBase
    {
        #region Private fields 

        private HttpClient _httpClient;

        #endregion

        #region Public properties
        public int TimeOut { get; set; } = 60000;
        public HttpResponseMessage HttpResponseMessage { get; private set; }
        public string Content { get; private set; }
        public int StatusCode { get; private set; }
        public string ReasonPhrase { get; private set; }
        #endregion


        #region Constructor 

        public KntHttpClientComponent(Store store) : base(store)
        {
            ComponentName = "KntHttpClient Component";
        }

        #endregion

        #region Protected override methods 

        public event EventHandler<ComponentEventArgs<HttpResponseMessage>> ReceiveResponse;
        protected override Result<EComponentResult> OnInitialized()
        {
            try
            {                
                _httpClient = new HttpClient();
                _httpClient.Timeout = TimeSpan.FromMilliseconds(TimeOut);

                return new Result<EComponentResult>(EComponentResult.Executed);
            }
            catch (Exception ex)
            {
                var res = new Result<EComponentResult>(EComponentResult.Error);
                var resMessage = $"KntHttpClient component. The connection could not be started. Error: {ex.Message}.";
                res.AddErrorMessage(resMessage);                
                return res;
            }
        }

        #endregion

        #region Public Methods

        public async Task<bool> GetAsync(string url)
        {
            bool res;
            try
            {
                ClearProperties();

                HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(url);

                HttpResponseMessage = httpResponseMessage;
                StatusCode = (int)httpResponseMessage.StatusCode;
                ReasonPhrase = httpResponseMessage.ReasonPhrase;
                if (httpResponseMessage.IsSuccessStatusCode)
                {                    
                    Content = await httpResponseMessage.Content.ReadAsStringAsync();
                    res = true;
                }
                else
                {
                    res = false;
                }
                ReceiveResponse?.Invoke(this, new ComponentEventArgs<HttpResponseMessage>(httpResponseMessage));
            }
            //catch (TaskCanceledException ex)
            //{
            //    if (ex.CancellationToken.IsCancellationRequested)
            //    {
            //        Console.WriteLine("La solicitud fue cancelada.");
            //    }
            //    else
            //    {
            //        Console.WriteLine("Se agotó el tiempo de espera de la solicitud.");
            //    }
            //}
            catch (Exception ex)
            {                
                StatusCode = 999;
                ReasonPhrase = $"HttpClient exception: {ex.Message}";
                res = false;
            }

            return res;
        }

        // --------------------------------------------------------------------------
        // Warning: this method can cause a deadlock in single-threaded environments
        // (for example, Windows Forms or WPF applications) or ASP.NET applications.
        // It is recommended to use the asynchronous version of this method.
        // Use only in KntScript
        public bool Get(string url)
        {
            bool res;
            res = Task.Run(() => GetAsync(url)).Result;
            return res;
        }
        // --------------------------------------------------------------------------

        #region Experimental (in construction ...)
        
        // TODO: Check null references. Use component properties.

        public async Task<string> PostAsync(string url, string jsonContent)
        {
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task PutAsync(string url, string jsonContent)
        {
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string url)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }
        
        #endregion 

        #endregion

        #region Private methods

        private void ClearProperties()
        {
            Content = "";
            StatusCode = 0;
            ReasonPhrase = "";            
        }

        #endregion 

        #region IDisposable

        public override void Dispose()
        {
            if (_httpClient != null)
            {                
                _httpClient.Dispose();
            }

            base.Dispose();
        }

        #endregion
    }
}
