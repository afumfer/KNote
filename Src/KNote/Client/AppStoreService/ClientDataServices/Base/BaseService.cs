namespace KNote.Client.AppStoreService.ClientDataServices.Base
{
    public class BaseService
    {
        protected readonly HttpClient httpClient;

        protected readonly AppState appState;

        public BaseService(AppState appState, HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.appState = appState;
        }
    }
}
