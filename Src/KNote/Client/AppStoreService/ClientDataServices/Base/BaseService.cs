namespace KNote.Client.AppStoreService.ClientDataServices.Base
{
    public class BaseService
    {
        protected readonly HttpClient _httpClient;

        protected readonly AppState _appState;

        public BaseService(AppState appState, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _appState = appState;
        }
    }
}
