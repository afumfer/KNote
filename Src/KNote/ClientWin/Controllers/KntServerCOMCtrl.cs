using KNote.ClientWin.Core;
using KNote.Model;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;

namespace KNote.ClientWin.Controllers;

public class KntServerCOMCtrl : CtrlBase, IDisposable
{
    #region Private fields

    private SerialPort _serialPort;
    // Shared by the reader thread (responses), the sender thread and the AI streaming callbacks.
    private readonly ConcurrentQueue<string> _messageQueue = new();
    private readonly KNoteAIAssistantCtrl _aiAssistant;

    private CancellationTokenSource _cancellationTokenSource;    
    private bool _showViewMessage;
    private static readonly Dictionary<char, byte> _convTable = LoadQDOSCharacterSetTable();

    #endregion

    #region Constants

    private const byte EofByte = 26;
    private static readonly string EofMessage = ((char)EofByte).ToString();

    // Wire commands (protocol contract with the retro clients).
    private const string CmdAi = "#ai";
    private const string CmdAiRestart = "#airestart";
    private const string CmdEcho = "#echo";

    // Deprecated names kept as aliases of the new commands for already written retro programs.
    private const string CmdAiLegacy = "#chatgpt";
    private const string CmdAiRestartLegacy = "#restartchatgpt";

    #endregion 

    #region Properties 

    private bool _runningService;
    public bool RunningService
    {
        get { return _runningService; }
        private set 
        { 
            _runningService = value;
            NotifyStatusChanged();
        }
    }

    private string _error;
    public string Error
    {
        get { return _error; }          
    }

    private string _statusInfo;
    public string StatusInfo
    {
        get { return _statusInfo; }        
    }

    private bool _messageSending = false;
    public bool MessageSending
    {
        get { return _messageSending; }
        private set
        {
            _messageSending = value;
            NotifyStatusChanged();
        }
    }

    public string PortName { get; set; }

    public int BaudRate { get; set; }

    public int HandShake { get; set; }

    public int RetroDelay { get; set; }

    public bool AutoCloseCtrlOnViewExit { get; set; }

    public bool ShowErrorMessagesOnInitialize { get; set; }

    #endregion

    #region Constructor

    public KntServerCOMCtrl(Store store) : base(store)
    {
        ControllerName = "KntServerCOM Controller";

        // --- BaudRate and HandShake

        // Q68            
        BaudRate = 115200;
        HandShake = (int)Handshake.None;  // 0

        // QL             
        //BaudRate = 19200; //  4800; // 9600; // 19200; 
        //HandShake = (int)Handshake.None;  // HandShake: => // 2 (RequestToSend) // 3 (RequestToSendXOnXOff ) // 0 (None)) ??

        PortName = "COM1";

        // Delay for retrocomputers (in miliseconds)
        RetroDelay = 20;
        
        // --- Control flags
        AutoCloseCtrlOnViewExit = false;
        ShowErrorMessagesOnInitialize = false;
        _showViewMessage = true;
                
        // AI Assistant included controller (any provider/model: OpenAI, Anthropic, Ollama).
        // Must stay a field: CtrlBase.FinalizeViewsController finalizes CtrlBase fields by reflection.
        _aiAssistant = new KNoteAIAssistantCtrl(store);
        // Without configured providers Run() would pop up an error dialog from inside this constructor;
        // the requests are answered with an explanatory message instead (see ExecuteAiRequest).
        if (store.AppConfig.AiProviderRefs.Count > 0)
            _aiAssistant.Run();
    }

    #endregion

    #region Events

    public event EventHandler<ControllerEventArgs<string>> ReceiveMessage;

    #endregion 

    #region Protected methods 

    protected override Result<EControllerResult> OnInitialized()
    {
        // Starting the service is best effort: a missing or busy COM port must not make the controller
        // fail. It stays available (with the service stopped, and the reason in Error/StatusInfo) so the
        // view can still be shown and the user can fix the settings and start the service later.
        if (!StartService() && ShowErrorMessagesOnInitialize)
            ServerCOMView.ShowInfo(_error, KntConst.AppName);

        return new Result<EControllerResult>(EControllerResult.Executed);
    }
    
    protected override Result<EControllerResult> OnFinalized()
    {
        var res = base.OnFinalized();
        _showViewMessage = false;  // for hide show info in view.
        StopService();
        return res;
    }

    #endregion

    #region Public methods

    public void Send(string message)
    {
        if (!RunningService)
            return;

        _messageQueue.Enqueue(message);
        _messageQueue.Enqueue(EofMessage);
    }

    public void StopService()
    {
        if (RunningService == false)
        {
            if (_serverCOMView != null)
                if(_showViewMessage)
                    _serverCOMView.ShowInfo("The service is already stopped.");
            return;
        }

        try
        {
            // Cancel first so the worker threads leave their loops before the port is closed.
            _cancellationTokenSource?.Cancel();
            RunningService = false;

            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();
                _serialPort.Dispose();
            }
            _statusInfo = "Com and service closed ...";
        }
        catch { }
    }

    // Returns false (with the reason in Error/StatusInfo) instead of throwing when the service cannot be
    // started, typically because the port does not exist on this computer or is in use.
    public bool StartService()
    {
        if(RunningService == true)
        {
            if (_serverCOMView != null)
                _serverCOMView.ShowInfo("Service is already running.");
            return true;
        }

        _error = null;

        try
        {
            if (!SerialPort.GetPortNames().Contains(PortName, StringComparer.OrdinalIgnoreCase))
                return FailStart($"The port '{PortName}' is not available on this computer.");

            _serialPort = new SerialPort(PortName, BaudRate, Parity.None, 8, StopBits.Two);
            _serialPort.Handshake = (Handshake)HandShake;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 5000;
            _serialPort.Encoding = Encoding.ASCII;
            _serialPort.Open();
        }
        catch (Exception ex)
        {
            _serialPort?.Dispose();
            return FailStart($"The port '{PortName}' could not be opened: {ex.Message}");
        }

        _cancellationTokenSource = new CancellationTokenSource();

        _messageQueue.Clear();
        _aiAssistant?.RestartAIAssistant();

        _statusInfo = "Com started ...";

        // RunningService must be true before the worker threads start: they loop while it is set.
        RunningService = true;

        var port = _serialPort;
        var cancellationToken = _cancellationTokenSource.Token;
        Task.Factory.StartNew(() => Server(port, cancellationToken), cancellationToken,
            TaskCreationOptions.LongRunning, TaskScheduler.Default);

        return true;
    }

    #endregion

    #region Private methods

    private void NotifyStatusChanged()
    {
        // Local copy: the field is reset to null when the controller is finalized.
        var view = _serverCOMView;
        view?.RefreshStatus();
    }

    private bool FailStart(string error)
    {
        _error = error;
        _statusInfo = $"KntServerCOM controller. The service could not be started. {error}";
        NotifyStatusChanged();
        return false;
    }

    private void Server(SerialPort port, CancellationToken cancellationToken)
    {
        Task.Factory.StartNew(() => Read(port, cancellationToken), cancellationToken,
            TaskCreationOptions.LongRunning, TaskScheduler.Default);

        while (RunningService && !cancellationToken.IsCancellationRequested)
        {
            if (_messageQueue.TryDequeue(out var msg))
            {
                if (msg != null)
                    SendMessage(port, msg);
            }
            else
                Thread.Sleep(Math.Max(RetroDelay, 1));
        }
    }

    private void SendMessage(SerialPort port, string messageSource)
    {
        _statusInfo = "Sending ...";
        MessageSending = true;        

        try
        {
            if (!string.IsNullOrEmpty(messageSource))
            {
                byte[] bMessage = ConverUtf8StringToClientOSBytes(messageSource);

                //// Option 1, for debug
                //for (int i = 0; i < bMessage.Length; i++)
                //{
                //    if (!RunningService)
                //        break;
                //    _serialPort.Write(bMessage, i, 1);
                //    // This is necesary for QL/Q68
                //    Thread.Sleep(RetroDelay);
                //}

                //// Option 2. // This option not work in for QL/Q68
                //_serialPort.Write(bMessage, 0, bMessage.Length);

                // Option 3. // This option work fine in for QL/Q68
                var chunkSize = 16;
                for (var i = 0; i < bMessage.Length; i += chunkSize)
                {
                    if (i + chunkSize > bMessage.Length)
                        chunkSize = bMessage.Length - i;
                    port.Write(bMessage, i, chunkSize);
                    Thread.Sleep(RetroDelay);
                }
            }
        }
        catch (TimeoutException) { }
        catch (Exception e) { _error = e.Message; }

        _statusInfo = "Sended ...";        
        MessageSending = false;
    }

    private void Read(SerialPort port, CancellationToken cancellationToken)
    {
        var messageIn = new StringBuilder();

        while (RunningService && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                messageIn.Clear();

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (port.BytesToRead == 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    byte b = (byte)port.ReadByte();
                    if (b == EofByte)
                        break;
                    messageIn.Append(ConvertByteToClientCSChar(b));
                }

                if (cancellationToken.IsCancellationRequested)
                    break;

                var messageText = messageIn.ToString();
                _statusInfo = $"Recived: {messageText}";
                try
                {
                    ReceiveMessage?.Invoke(this, new ControllerEventArgs<string>(messageText));
                }
                catch (Exception e) { _error = e.Message; }  // a failing subscriber must not lose the request

                DispatchRequest(GetKComRequest(messageText));
            }
            catch (TimeoutException) { }
            catch (Exception e)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                _error = e.Message;
                if (!port.IsOpen)
                {
                    // The port went away while running (e.g. a USB adapter was unplugged).
                    StopService();
                    _statusInfo = $"The port '{PortName}' was closed unexpectedly: {e.Message}";
                    NotifyStatusChanged();
                    break;
                }
            }
        }
    }

    private KComRequest GetKComRequest(string messageIn)
    {
        // TODO: Parse the message here correctly (this is a quick and dirty implementation).

        KComRequest kComReq = new KComRequest();

        if (string.IsNullOrEmpty(messageIn))
            messageIn = $"{CmdEcho}:\nError, invalid message.";

        // If there is no command, the default command is the AI assistant
        if (!messageIn.StartsWith("#"))
            messageIn = $"{CmdAi}:\n" + messageIn;

        try
        {
            var indHeader = messageIn.IndexOf('\n');
            var indCommand = messageIn.IndexOf(':');

            if (indHeader == -1)
            {
                kComReq.Command = messageIn.Substring(0, indCommand);
                kComReq.Body = messageIn.Substring(indCommand + 1, messageIn.Length - indCommand - 1);
                return kComReq;
            }

            var header = messageIn.Substring(0, indHeader);
            var headerArray = header.Split(':');

            kComReq.Command = headerArray[0];

            if (!string.IsNullOrEmpty(headerArray[1]))
            {
                var colecParams = headerArray[1].Split(';');
                foreach (var parN in colecParams)
                {
                    var par = parN.Split('=');
                    kComReq.Parameters.Add(par[0], par[1]);
                }
            }

            kComReq.Body = messageIn.Substring(messageIn.IndexOf('\n') + 1, messageIn.Length - indHeader - 1);

            return kComReq;
        }
        catch (Exception)
        {
            kComReq.Command = CmdEcho;
            kComReq.Body = "Error, invalid message.";
            return kComReq;
        }
    }

    private void DispatchRequest(KComRequest req)
    {
        // Dispatch actions:
        // TODO: Select the correct action, use the command pattern here.

        if (req.Command == CmdAi || req.Command == CmdAiLegacy)
            ExecuteAiRequest(req.Body);
        else if (req.Command == CmdAiRestart || req.Command == CmdAiRestartLegacy)
            ExecuteAiRestartRequest();
        else if (req.Command == CmdEcho)
            ExecuteEchoRequest(req.Body);
        else
            ExecuteEchoRequest(req.Body);
    }

    private void ExecuteEchoRequest(string request)
    {
        // TODO: In the future, add a header here for responses.
        // ...

        _messageQueue.Enqueue($"Echo for request [{request}]");

        // Signal for end of stream.  
        _messageQueue.Enqueue(EofMessage);
    }

    private void ExecuteAiRestartRequest()
    {
        _aiAssistant?.RestartAIAssistant();
        _messageQueue.Enqueue(EofMessage);
    }

    // async void: nothing awaits it, so every failure (no provider configured, invalid API key, network
    // error...) must be handled here or it would take the whole application down. The error is sent back
    // to the device as the answer.
    private async void ExecuteAiRequest(string request)
    {
        // Local copy: the field is reset to null when the controller is finalized.
        var aiAssistant = _aiAssistant;

        try
        {
            if (aiAssistant?.CurrentProviderRef == null)
                throw new InvalidOperationException("No AI provider is configured. Add one in the KNote AI providers options.");

            aiAssistant.StreamToken += _aiAssistant_StreamToken;
            try
            {
                await aiAssistant.StreamCompletionAsync(request);
            }
            finally
            {
                aiAssistant.StreamToken -= _aiAssistant_StreamToken;
            }
        }
        catch (Exception ex)
        {
            _error = ex.Message;
            _messageQueue.Enqueue($"AI assistant error: {ex.Message}");
        }
        finally
        {
            _messageQueue.Enqueue(EofMessage);
        }
    }

    private void _aiAssistant_StreamToken(object sender, ControllerEventArgs<string> e)
    {
        var token = e.Entity?.ToString();
        if (!string.IsNullOrEmpty(token))
            _messageQueue.Enqueue(token);
    }

    #endregion 

    #region Utils 

    // internal (not private) only so ClientWin.Tests can exercise the conversion without a COM port.
    internal static byte[] ConverUtf8StringToClientOSBytes(string sourceText)
    {
        var outQDos = new List<byte>(sourceText.Length);

        for (var i = 0; i < sourceText.Length; i++)
        {
            var c = sourceText[i];
            if (c == '\r')
            {
                // CRLF -> LF (the '\n' is converted in the next iteration); a lone CR -> LF.
                if (i + 1 < sourceText.Length && sourceText[i + 1] == '\n')
                    continue;
                c = '\n';
            }
            outQDos.Add(ConvertCharToClientCSByte(c));
        }
        return outQDos.ToArray();
    }

    private static byte ConvertCharToClientCSByte(char c)
    {
        if (_convTable.ContainsKey(c))
            return _convTable[c];
        else
            return (byte)c;
    }

    private static char ConvertByteToClientCSChar(byte b)
    {
        if (b < 128)  // ASCII standard
            return (char)b;
        else
            return _convTable.Where(v => v.Value == b).FirstOrDefault().Key;
    }

    private static Dictionary<char, byte> LoadQDOSCharacterSetTable()
    {
        // QDOS (Sinclair QL) character set table  

        Dictionary<char, byte> table = new Dictionary<char, byte>();
       
        table.Add(' ', 32);
        table.Add('!', 33);
        table.Add('"', 34);
        table.Add('#', 35);
        table.Add('$', 36);
        table.Add('%', 37);
        table.Add('&', 38);
        table.Add('\'', 39);
        table.Add('(', 40);
        table.Add(')', 41);
        table.Add('*', 42);
        table.Add('+', 43);
        table.Add(',', 44);
        table.Add('-', 45);
        table.Add('.', 46);
        table.Add('/', 47);
        table.Add('0', 48);
        table.Add('1', 49);
        table.Add('2', 50);
        table.Add('3', 51);
        table.Add('4', 52);
        table.Add('5', 53);
        table.Add('6', 54);
        table.Add('7', 55);
        table.Add('8', 56);
        table.Add('9', 57);
        table.Add(':', 58);
        table.Add(';', 59);
        table.Add('<', 60);
        table.Add('=', 61);
        table.Add('>', 62);
        table.Add('?', 63);
        table.Add('@', 64);
        table.Add('A', 65);
        table.Add('B', 66);
        table.Add('C', 67);
        table.Add('D', 68);
        table.Add('E', 69);
        table.Add('F', 70);
        table.Add('G', 71);
        table.Add('H', 72);
        table.Add('I', 73);
        table.Add('J', 74);
        table.Add('K', 75);
        table.Add('L', 76);
        table.Add('M', 77);
        table.Add('N', 78);
        table.Add('O', 79);
        table.Add('P', 80);
        table.Add('Q', 81);
        table.Add('R', 82);
        table.Add('S', 83);
        table.Add('T', 84);
        table.Add('U', 85);
        table.Add('V', 86);
        table.Add('W', 87);
        table.Add('X', 88);
        table.Add('Y', 89);
        table.Add('Z', 90);
        table.Add('[', 91);
        table.Add('\\', 92);
        table.Add(']', 93);
        table.Add('^', 94);
        table.Add('_', 95);
        table.Add('£', 96);
        table.Add('a', 97);
        table.Add('b', 98);
        table.Add('c', 99);
        table.Add('d', 100);
        table.Add('e', 101);
        table.Add('f', 102);
        table.Add('g', 103);
        table.Add('h', 104);
        table.Add('i', 105);
        table.Add('j', 106);
        table.Add('k', 107);
        table.Add('l', 108);
        table.Add('m', 109);
        table.Add('n', 110);
        table.Add('o', 111);
        table.Add('p', 112);
        table.Add('q', 113);
        table.Add('r', 114);
        table.Add('s', 115);
        table.Add('t', 116);
        table.Add('u', 117);
        table.Add('v', 118);
        table.Add('w', 119);
        table.Add('x', 120);
        table.Add('y', 121);
        table.Add('z', 122);
        table.Add('{', 123);
        table.Add('|', 124);
        table.Add('}', 125);
        table.Add('~', 126);
        table.Add('©', 127);
        table.Add('ä', 128);
        table.Add('ã', 129);
        table.Add('å', 130);
        table.Add('é', 131);
        table.Add('ö', 132);
        table.Add('õ', 133);
        table.Add('ø', 134);
        table.Add('ü', 135);
        table.Add('ç', 136);
        table.Add('ñ', 137);
        table.Add('æ', 138);
        table.Add('œ', 139);
        table.Add('á', 140);
        table.Add('à', 141);
        table.Add('â', 142);
        table.Add('ë', 143);
        table.Add('è', 144);
        table.Add('ê', 145);
        table.Add('ï', 146);
        table.Add('í', 147);
        table.Add('ì', 148);
        table.Add('î', 149);
        table.Add('ó', 150);
        table.Add('ò', 151);
        table.Add('ô', 152);
        table.Add('ú', 153);
        table.Add('ù', 154);
        table.Add('û', 155);
        table.Add('ß', 156);
        table.Add('¢', 157);
        table.Add('¥', 158);
        table.Add('`', 159);
        table.Add('Ä', 160);
        table.Add('Ã', 161);
        table.Add('Å', 162);
        table.Add('É', 163);
        table.Add('Ö', 164);
        table.Add('Õ', 165);
        table.Add('Ø', 166);
        table.Add('Ü', 167);
        table.Add('Ç', 168);
        table.Add('Ñ', 169);
        table.Add('Æ', 170);
        table.Add('Œ', 171);
        table.Add('α', 172);
        table.Add('δ', 173);
        table.Add('θ', 174);
        table.Add('λ', 175);
        table.Add('μ', 176);
        table.Add('π', 177);
        table.Add('Φ', 178);
        table.Add('¡', 179);
        table.Add('¿', 180);
        table.Add('§', 182);
        table.Add('¤', 183);
        table.Add('«', 184);
        table.Add('»', 185);
        table.Add('°', 186);
        table.Add('÷', 187);
        table.Add('←', 188);
        table.Add('→', 189);
        table.Add('↑', 190);
        table.Add('↓', 191);
        
        return table;
    }

    #endregion

    #region IView

    IViewServerCOM _serverCOMView;
    protected IViewServerCOM ServerCOMView
    {
        get
        {
            if (_serverCOMView == null)
                _serverCOMView = Store.FactoryViews.Registry.Resolve<KntServerCOMCtrl, IViewServerCOM>(this);
            return _serverCOMView;
        }
    }

    public void ShowServerCOMView(bool autoCloseCtrlOnViewExit)
    {
        AutoCloseCtrlOnViewExit = autoCloseCtrlOnViewExit;
        ServerCOMView.ShowView();
    }

    // For use in KntScript
    public void ShowServerCOMView()
    {

        if (ControllerState == EControllerState.Started)
        {
            ServerCOMView.ShowView();
        }
        else
        {
            ServerCOMView.ShowInfo("KntChat controller is no started.");
        }
    }

    public void VisibleView(bool visible)
    {
        ServerCOMView.VisibleView(visible);
    }

    #endregion

    #region IDisposable

    public override void Dispose()
    {
        StopService();
        base.Dispose();
    }

    #endregion

    #region Request / Response types

    private class KComRequest
    {
        public string Command { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public string Body { get; set; }
    }

    #endregion 
}
