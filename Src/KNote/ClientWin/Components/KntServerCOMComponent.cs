using Azure.Core;
using KNote.ClientWin.Core;
using KNote.Model;
using Microsoft.Identity.Client;
using mshtml;
using System.Collections;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;

namespace KNote.ClientWin.Components;

public class KntServerCOMComponent : ComponentBase, IDisposable
{
    #region Private fields

    private SerialPort _serialPort;
    private Queue _messageQueue;  

    private KntChatGPTComponent _chatGPT;
  
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

    public string PortName { get; set; } = "COM1";

    // --- BaudRate and HandShake

    // Q68            
    //BaudRate = 115200;
    //HandShake = 0; // Handshake.None

    // QL             
    //BaudRate = 19200;
    //HandShake = 2; // 3; // Handshake.RequestToSendXOnXOff (3) Handshake.RequestToSend (2); 

    public int BaudRate { get; set; } = 115200;
    public int HandShake { get; set; } = 0;
    
    // -----------------------------------------------------------------------

    public bool AutoCloseComponentOnViewExit { get; set; } = false;
    public bool ShowErrorMessagesOnInitialize { get; set; } = false;
    public string Tag { get; set; } = "KntServerCOMComponent v 0.1";

    #endregion

    #region Constructor

    public KntServerCOMComponent(Store store) : base(store)
    {
        ComponentName = "KntServerCOM Component";
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
        CloseService();
        return base.OnFinalized();
    }

    #endregion

    #region Public methods

    public void Send(string message)
    {
        _messageQueue.Enqueue(message);
    }

    public void CloseService()
    {
        RunningService = false;

        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();
            _statusInfo = "Com and servide closed ...";
        }
        catch { }
    }

    public void StartService()
    {
        _serialPort = new SerialPort(PortName, BaudRate, Parity.None, 8, StopBits.Two);
        _serialPort.Handshake = (Handshake)HandShake;
        _serialPort.ReadTimeout = 5000;
        _serialPort.WriteTimeout = 5000;
        _serialPort.Open();        
        _messageQueue = new Queue();
        _statusInfo = "Com started ...";
        Task.Factory.StartNew(() => Server(), TaskCreationOptions.LongRunning);        
        RunningService = true;
    }

    #endregion

    #region Private methods

    private void Server()
    {
        Task.Factory.StartNew(() => Read(), TaskCreationOptions.LongRunning);                

        while (RunningService)
        {
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
        MessageSending = true;

        _statusInfo = "Sending ...";

        byte[] bMessage = ConverTextToQDOSBytes(messageSource);

        try
        {
            if (!String.IsNullOrEmpty(messageSource))
            {
                for (int i = 0; i < bMessage.Length; i++)
                {
                    if (!RunningService)
                        break;
                    _serialPort.Write(bMessage, i, 1);
                    // This is necesary for QL/Q68
                    Thread.Sleep(20); 
                }
            }

        }
        catch (TimeoutException) { }
        catch (Exception e) { _error = e.Message; }

        _statusInfo = "Sended ...";
        
        MessageSending = false;
    }

    private void Read()
    {
        string messageIn;
        string command = "";
        string body = "";

        while (RunningService)
        {
            try
            {
                messageIn = _serialPort.ReadLine();

                byte[] utf8EncodedBytes = Encoding.ASCII.GetBytes(messageIn);

                _statusInfo = $"Recived: {messageIn}";
                ReceiveMessage?.Invoke(this, new ComponentEventArgs<string>(messageIn));

                (command, body) = ProcessTypeRequest(messageIn);

                // TODO: Select the correct action, employ the command pattern here.
                if (command == "$chatgpt")
                    ExecuteChatGptRequest(body);
                else if (command == "$echo")
                    ExecuteEchoRequest(body);
                else
                    ExecuteEchoRequest(body);

                messageIn = "";
            }
            catch (TimeoutException) { }
            catch (Exception e) { _error = e.Message; }
        }
    }

    (string command, string msg) ProcessTypeRequest(string messageIn)
    {
        // TODO Parse the command here correctly (this is provisional).

        //StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;

        if (!messageIn.StartsWith("$"))
            messageIn = "$chatgpt:" + messageIn;

        var cmd = messageIn.Split(':');
        return (cmd[0], cmd[1]);
    }

    private void ExecuteEchoRequest(string request)
    {
        _messageQueue.Enqueue($"Echo for request [{request}]");
    }

    private async void ExecuteChatGptRequest(string request)
    {
        _chatGPT.StreamToken += _chatGPT_StreamToken;
        await _chatGPT.StreamCompletionAsync(request);
        _chatGPT.StreamToken -= _chatGPT_StreamToken;
    }

    private void _chatGPT_StreamToken(object sender, ComponentEventArgs<string> e)
    {
        _messageQueue.Enqueue(e.Entity?.ToString()); 
    }

    #endregion 

    #region Utils / Tests

    private byte[] ConverTextToQDOSBytes(string sourceText, string encodingStr = "-UTF8")
    {
        var table = LoadQDosTable();
        List<byte> outQDos = new List<byte>();
        var enc = Encoding.UTF8;

        byte[] utf8EncodedBytes = Encoding.UTF8.GetBytes(sourceText);
        string utf8DecodedString = Encoding.UTF8.GetString(utf8EncodedBytes);

        var charArray = utf8DecodedString.ToCharArray();

        for(var i = 0; i < charArray.Count(); i++)
        {
            var c = charArray[i];
            if (c == '\r')
            {
                var c2 = charArray[++i];
                if (c2 == '\n')
                    AddByteToQDosList(table, outQDos, c2);
                else
                {
                    AddByteToQDosList(table, outQDos, '\n');
                    AddByteToQDosList(table, outQDos, c2);
                }
            }
            else
                AddByteToQDosList(table, outQDos, c);
        }
        return outQDos.ToArray();

    }

    public static void AddByteToQDosList(Dictionary<char, byte> table, List<byte> outQDos, char c)
    {
        byte charN;

        if (table.ContainsKey(c))
        {
            outQDos.Add(table[c]);
        }
        else
        {
            charN = (byte)c;
            if (charN > 191)
                charN = 1;
            outQDos.Add((byte)c);
        }
    }

    private Dictionary<char, byte> LoadQDosTable()
    {
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

    public void TestSendBinary1()
    {
        FileStream fs = new FileStream("cheetah_scr", FileMode.Open, FileAccess.Read);

        byte[] b1 = new byte[fs.Length];
        /*Read block of bytes from stream into the byte array*/
        fs.Read(b1, 0, System.Convert.ToInt32(fs.Length));
        /*Close the File Stream*/
        fs.Close();

        byte[] b0 = new byte[15];
        int lenni, lea, leb, lec;
        lenni = b1.Length;
        lenni--;
        lea = lenni / 65536;
        leb = (lenni - (lea * 65536)) / 256;
        lec = (lenni - ((lea * 65536) + (leb * 256)));

        //b0[0] = 255;
        //b0[1] = 0;
        //b0[2] = (byte)lea;
        //b0[3] = (byte)leb;
        //b0[4] = (byte)lec;

        b0[0] = 255;
        b0[1] = 0;
        b0[2] = 0;
        b0[3] = 125;
        b0[4] = 0;
        b0[5] = 0;
        b0[6] = 0;
        b0[7] = 0;
        b0[8] = 0;
        b0[9] = 0;
        b0[10] = 0;
        b0[11] = 0;
        b0[12] = 0;
        b0[13] = 1;
        b0[14] = 218;

        //sersend(255);
        //  sersend(0);
        //  sersend(lea);
        //  sersend(leb);
        //  sersend(lec);


        //for(int n=5;n<15;n++)
        //{
        //    b0[n] = 0;
        //}


        //_serialPort.Write(b0, 0, b0.Length);
        //_serialPort.Write(b1, 0, b1.Length);

        byte[] b3 = new byte[b0.Length + b1.Length];

        for (int i = 0; i < b0.Length; i++)
            b3[i] = b0[i];

        for (int i = 0; i < b1.Length; i++)
            b3[i + b0.Length] = b1[i];

        for (int i = 0; i < b3.Length; i++)
        {
            _serialPort.Write(b3, i, 1);
            Thread.Sleep(40);
        }
    }

    public void TestSendBinary2()
    {
        //string file = "rest";
        string file = "cheetah_scr";

        FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        byte[] bf = new byte[fs.Length];
        fs.Read(bf, 0, System.Convert.ToInt32(fs.Length));
        fs.Close();

        for (int i = 0; i < bf.Length; i++)
        {
            _serialPort.Write(bf, i, 1);
            //Thread.Sleep(10);
            //label1.Text = i.ToString();
            //label1.Refresh();
        }

        MessageBox.Show("fin");
    }

    public void TestSendBinary3()
    {
        string file = "rest";
        //string file = "cheetah_scr"; 

        FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        byte[] bf = new byte[fs.Length];
        fs.Read(bf, 0, System.Convert.ToInt32(fs.Length));
        fs.Close();

        int seg = 100;
        int len = bf.Length;

        int nSeg = 0;
        int nRes = 0;


        if (len <= seg)
            nRes = len;
        else
        {
            nSeg = len / seg;
            nRes = len % seg;
        }

        int i = 0;

        for (int j = 1; j <= nSeg; j++)
        {
            _serialPort.Write(bf, i, seg);
            i = j * seg;
        }

        if (nRes > 0)
            _serialPort.Write(bf, i, nRes);

        MessageBox.Show("fin");
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
        CloseService();

        base.Dispose();
    }

    #endregion 

}
