using KNote.ClientWin.Core;
using KNote.Model;
using System.Collections;
using System.IO.Ports;
using System.Text;

namespace KNote.ClientWin.Components;

public class KntServerCOMComponent : ComponentBase, IDisposable
{
    #region Private fields

    private SerialPort _serialPort;
    private Queue _messageQueue;  
    private CancellationTokenSource _cancellationTokenSource;    
    private bool _showViewMessage;
    private readonly Dictionary<char, byte> _convTable;
    private readonly KntChatGPTComponent _chatGPT;    

    #endregion

    #region Properties 

    private bool _runningService;
    public bool RunningService
    {
        get { return _runningService; }
        private set 
        { 
            _runningService = value;
            if (_serverCOMView != null)
                _serverCOMView.RefreshStatus();
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
            if (_serverCOMView != null)
                _serverCOMView.RefreshStatus();
        }
    }

    public string PortName { get; set; }

    public int BaudRate { get; set; }

    public int HandShake { get; set; }        

    public bool AutoCloseComponentOnViewExit { get; set; }

    public bool ShowErrorMessagesOnInitialize { get; set; }

    #endregion

    #region Constructor

    public KntServerCOMComponent(Store store) : base(store)
    {
        ComponentName = "KntServerCOM Component";

        // --- BaudRate and HandShake
        // Q68            
        //   BaudRate = 115200;
        //   HandShake = 0; // Handshake.None
        // QL             
        //   BaudRate = 19200;
        //   HandShake = 2; // 3; // Handshake.RequestToSendXOnXOff (3) Handshake.RequestToSend (2); 
        PortName = "COM1";
        BaudRate = 115200;
        HandShake = (int)Handshake.None;

        // --- Control flags
        AutoCloseComponentOnViewExit = false;
        ShowErrorMessagesOnInitialize = false;
        _showViewMessage = true;
                
        _convTable = LoadQDOSCharacterSetTable();

        // ChatGPT included component
        _chatGPT = new KntChatGPTComponent(store);
        _chatGPT.Run();
    }

    #endregion

    #region Events

    public event EventHandler<ComponentEventArgs<string>> ReceiveMessage;

    #endregion 

    #region Protected methods 

    protected override Result<EComponentResult> OnInitialized()
    {
        try
        {            
            StartService();            
            return new Result<EComponentResult>(EComponentResult.Executed);
        }
        catch (OperationCanceledException)
        {
            // not doing anything
            var res = new Result<EComponentResult>(EComponentResult.Error);
            return res;
        }
        catch (Exception ex)
        {
            var res = new Result<EComponentResult>(EComponentResult.Error);
            var _statusInfo = $"KntServerCOM component. The connection could not be started. Error: {ex.Message}.";
            res.AddErrorMessage(_statusInfo);            
            if (ShowErrorMessagesOnInitialize)
                ServerCOMView.ShowInfo(_statusInfo, KntConst.AppName);
            return res;
        }
    }
    
    protected override Result<EComponentResult> OnFinalized()
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
        _messageQueue.Enqueue(message);
        _messageQueue.Enqueue((char)26);
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
            RunningService = false;

            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();
            _statusInfo = "Com and servide closed ...";

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }
        catch { }
    }

    public void StartService()
    {
        if(RunningService == true)
        {
            if (_serverCOMView != null)
                _serverCOMView.ShowInfo("Service is already running.");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        _serialPort = new SerialPort(PortName, BaudRate, Parity.None, 8, StopBits.Two);
        _serialPort.Handshake = (Handshake)HandShake;        
        _serialPort.ReadTimeout = 5000;
        _serialPort.WriteTimeout = 5000;
        _serialPort.Encoding = Encoding.ASCII;
        _serialPort.Open();   
        
        _messageQueue = new Queue();
        _chatGPT.RestartChatGPT();

        _statusInfo = "Com started ...";

        Task.Factory.StartNew(() => Server(cancellationToken), cancellationToken);        
        
        RunningService = true;
    }

    #endregion

    #region Private methods

    private void Server(CancellationToken cancellationToken)
    {
        Task.Factory.StartNew(() => Read(cancellationToken), cancellationToken);                        

        while (RunningService)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            if (_messageQueue.Count > 0)
            {
                var msg = _messageQueue.Dequeue();
                if (msg != null)
                {
                    SendMessage(msg.ToString());
                }
            }
            else
                Thread.Sleep(40);
        }        
    }

    private void SendMessage(string messageSource)
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
                //    Thread.Sleep(20);
                //}

                //Option 2. // This option not work in for QL/Q68
                //_serialPort.Write(bMessage, 0, bMessage.Length);

                // Option 3. // This option work fine in for QL/Q68
                var chunkSize = 16;
                for (var i = 0; i < bMessage.Length; i += chunkSize)
                {
                    if (i + chunkSize > bMessage.Length)
                        chunkSize = bMessage.Length - i;
                    _serialPort.Write(bMessage, i, chunkSize);
                    Thread.Sleep(20);
                }
            }
        }
        catch (TimeoutException) { }
        catch (Exception e) { _error = e.Message; }

        _statusInfo = "Sended ...";        
        MessageSending = false;
    }

    private void Read(CancellationToken cancellationToken)
    {
        string messageIn;

        while (RunningService)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
                                
                messageIn = "";

                while (true)
                {
                    if(_serialPort.BytesToRead > 0)
                    {
                        byte b = (byte)_serialPort.ReadByte();
                        if (b ==  26)  // 26=(EOF)
                            break;                                                 
                        messageIn += ConvertByteToClientCSChar(b);                        
                    }
                }
               
                _statusInfo = $"Recived: {messageIn}";
                ReceiveMessage?.Invoke(this, new ComponentEventArgs<string>(messageIn));
                
                DispatchRequest(GetKComRequest(messageIn));

                messageIn = "";
            }
            catch (TimeoutException) { }
            catch (Exception e) { _error = e.Message; }
        }
    }

    private KComRequest GetKComRequest(string messageIn)
    {
        // TODO: Parse the message here correctly (this is a quick and dirty implementation).

        KComRequest kComReq = new KComRequest();

        if (string.IsNullOrEmpty(messageIn))
            messageIn = "#echo:\nError, invalid message.";

        // If there is no command, the default command is chatgpt
        if (!messageIn.StartsWith("#"))
            messageIn = "#chatgpt:\n" + messageIn;

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
            kComReq.Command = "#echo";
            kComReq.Body = "Error, invalid message.";
            return kComReq;
        }
    }

    private void DispatchRequest(KComRequest req)
    {
        // Dispatch actions:
        // TODO: Select the correct action, use the command pattern here.

        if (req.Command == "#chatgpt")
            ExecuteChatGptRequest(req.Body);
        else if (req.Command == "#restartchatgpt")
            ExecuteRestartChatGptRequest();
        else if (req.Command == "#echo")
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
        _messageQueue.Enqueue((char)26);  // byte #26 = EOF 
    }

    private void ExecuteRestartChatGptRequest()
    {
        _chatGPT.RestartChatGPT();
        _messageQueue.Enqueue((char)26);
    }

    private async void ExecuteChatGptRequest(string request)
    {
        _chatGPT.StreamToken += _chatGPT_StreamToken;        
        await _chatGPT.StreamCompletionAsync(request);
        _chatGPT.StreamToken -= _chatGPT_StreamToken;
        
        _messageQueue.Enqueue((char)26); 
    }

    private void _chatGPT_StreamToken(object sender, ComponentEventArgs<string> e)
    {
        _messageQueue.Enqueue(e.Entity?.ToString());
        //Thread.Sleep(20);
    }

    #endregion 

    #region Utils 

    private byte[] ConverUtf8StringToClientOSBytes(string sourceText)
    {
        List<byte> outQDos = new List<byte>();

        byte[] utf8EncodedBytes = Encoding.UTF8.GetBytes(sourceText);
        string utf8DecodedString = Encoding.UTF8.GetString(utf8EncodedBytes);

        var charArray = utf8DecodedString.ToCharArray();

        for (var i = 0; i < charArray.Count(); i++)
        {
            var c = charArray[i];
            if (c == '\r')
            {
                var c2 = charArray[++i];
                if (c2 == '\n')                    
                    outQDos.Add(ConvertCharToClientCSByte(c2));
                else
                {
                    outQDos.Add(ConvertCharToClientCSByte('\n'));
                    outQDos.Add(ConvertCharToClientCSByte(c2));
                }
            }
            else
                outQDos.Add(ConvertCharToClientCSByte(c));

        }
        return outQDos.ToArray();
    }

    private byte ConvertCharToClientCSByte(char c)
    {
        if (_convTable.ContainsKey(c))
            return _convTable[c];
        else
            return (byte)c;
    }

    private char ConvertByteToClientCSChar(byte b)
    {
        if (b < 128)  // ASCII standard
            return (char)b;
        else        
            return _convTable.Where(v => v.Value == b).FirstOrDefault().Key;    
    }

    private Dictionary<char, byte> LoadQDOSCharacterSetTable()
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
                _serverCOMView = Store.FactoryViews.View(this);
            return _serverCOMView;
        }
    }

    public void ShowServerCOMView(bool autoCloseComponentOnViewExit)
    {
        AutoCloseComponentOnViewExit = autoCloseComponentOnViewExit;
        ServerCOMView.ShowView();
    }

    // For use in KntScript
    public void ShowServerCOMView()
    {

        if (ComponentState == EComponentState.Started)
        {
            ServerCOMView.ShowView();
        }
        else
        {
            ServerCOMView.ShowInfo("KntChat component is no started.");
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
