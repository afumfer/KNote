using KNote.ClientWin.Core;
using KNote.Model;
using System.Collections;
using System.IO.Ports;
using System.Text;

namespace KNote.ClientWin.Components;

public class KntServerCOMComponent : ComponentBase, IDisposable
{
    #region Private fields

    private bool _runningService;
    private bool _messageSending = false;
    private SerialPort _serialPort;
    private Queue _messageQueue;

    StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;

    #endregion

    #region Properties 

    private string _error;
    public string Error
    {
        get { return _error; }  
        private set { _error = value; }
    }

    private string _statusInfo;
    public string StatusInfo
    {
        get { return _statusInfo; }
        private set { _statusInfo = value; }
    }

    #endregion

    #region Properties 

    public bool AutoCloseComponentOnViewExit { get; set; } = false;
    public bool ShowErrorMessagesOnInitialize { get; set; } = false;
    public string Tag { get; set; } = "KntServerCOMComponent v 0.1";

    #endregion

    #region Constructor

    public KntServerCOMComponent(Store store) : base(store)
    {
        ComponentName = "KntServerCOM Component";
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
            // TODO: !!! Personalise params COM, move to public method for restart.

            // For QL
            //_serialPort = new SerialPort("COM1", 19200, Parity.None, 8, StopBits.Two);
            //_serialPort.Handshake = Handshake.RequestToSendXOnXOff;
            //// Set the read/write timeouts
            //_serialPort.ReadTimeout = 5000;
            //_serialPort.WriteTimeout = 5000;

            // For Q68   
            _serialPort = new SerialPort("COM1", 115200, Parity.None, 8);
            _serialPort.Handshake = Handshake.None;
            //_serialPort.Handshake = Handshake.RequestToSendXOnXOff;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 5000;

            _serialPort.Open();
            _runningService = true;
            _messageQueue = new Queue();

            //Task.Factory.StartNew(() => Write(), TaskCreationOptions.LongRunning);
            //Task.Factory.StartNew(() => Write(), TaskCreationOptions.AttachedToParent);

            //Task.Factory.StartNew(() => Read(), TaskCreationOptions.LongRunning);
            //await Task.Factory.StartNew(() => Read(), TaskCreationOptions.AttachedToParent);

            Task.Factory.StartNew(() => Server(), TaskCreationOptions.LongRunning);

            _statusInfo = "Com started ...";

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
        CloseSerialPort();
        return base.OnFinalized();
    }

    #endregion

    #region Public methods

    public void Send(string message)
    {
        _messageQueue.Enqueue(message);
    }

    public void CloseSerialPort()
    {
        _runningService = false;

        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();
            _statusInfo = "Com and servide closed ...";
        }
        catch { }
    }

    #endregion

    #region Private methods

    private void Server()
    {
        Task.Factory.StartNew(() => Read(), TaskCreationOptions.LongRunning);

        while (_runningService)
        {
            if (_messageQueue.Count > 0)
            {
                var msg = _messageQueue.Dequeue();
                SendMessage(msg.ToString());
            }
            else
                Thread.Sleep(40);
        }
    }
    
    private void Read()
    {
        string messageIn = "";
        List<string> output;
        int i = 0;

        while (_runningService)
        {
            try
            {
                messageIn = _serialPort.ReadLine();

                _statusInfo = $"Recived: {messageIn}";
                ReceiveMessage?.Invoke(this, new ComponentEventArgs<string>(messageIn));

                output = ExecuteRequest(messageIn);

                if (output != null)
                {
                    foreach (string msg in output)
                    {
                        i++;
                        _messageQueue.Enqueue(i.ToString() + " >>" + msg);
                    }
                }

                messageIn = "";
            }
            catch (TimeoutException) { }
            catch (Exception e) { _error = e.Message; }
        }
    }

    private List<string> ExecuteRequest(string request)
    {
        // TODO: Demo 
        var outputClient = new List<string>();

        outputClient.Add($"Response: xxxxx");
        outputClient.Add($" -- (for request{request}");

        return outputClient;
    }

    private void SendMessage(string messageSource)
    {        
        _statusInfo = "Sending ...";
        _messageSending = true;

        byte[] bMessage = ConverTextToQDOSBytes(messageSource);

        try
        {
            if (!String.IsNullOrEmpty(messageSource))
            {
                for (int i = 0; i < bMessage.Length; i++)
                {
                    if (!_runningService)
                        break;
                    _serialPort.Write(bMessage, i, 1);
                    Thread.Sleep(20);
                }
            }

        }
        catch (TimeoutException) { }
        catch (Exception e) { _error = e.Message; }

        _statusInfo = "Sended ...";
        _messageSending = false;
    }

    #endregion 

    #region Utils / Tests

    private void ConverFileAnsiToQdos(string sourceFile, string targetFile)
    {
        byte[] bSource = File.ReadAllBytes(sourceFile);

        int lenB = bSource.Length;
        byte[] bTarget = new byte[lenB];

        for (int i = 0; i < lenB; i++)
            bTarget[i] = ConvertToQdosByte(bSource[i]);

        File.WriteAllBytes(targetFile, bTarget);
    }

    private byte[] ConverTextToQDOSBytes(string sourceText)
    {
        byte[] bSource = Encoding.Default.GetBytes(sourceText);

        int lenB = bSource.Length;
        byte[] bTarget = new byte[lenB];

        for (int i = 0; i < lenB; i++)
            bTarget[i] = ConvertToQdosByte(bSource[i]);

        return bTarget;
    }

    private byte ConvertToQdosByte(byte x)
    {
        //TODO: Refactor pending

        char c;
        byte res = x;

        c = (char)x;

        switch (c)
        {
            case 'á':
                res = 140;
                break;
            case 'é':
                res = 131;
                break;
            case 'í':
                res = 147;
                break;
            case 'ó':
                res = 150;
                break;
            case 'ú':
                res = 153;
                break;
            case 'Á':
                res = 140;
                break;
            case 'É':
                res = 163;
                break;
            case 'Í':
                res = 147;
                break;
            case 'Ó':
                res = 150;
                break;
            case 'Ú':
                res = 153;
                break;
            case 'ñ':
                res = 137;
                break;
            case 'Ñ':
                res = 169;
                break;
            case '¿':
                res = 180;
                break;
            case '¡':
                res = 179;
                break;
        }

        switch (x)
        {
            case 147:
                res = 34;
                break;
            case 148:
                res = 34;
                break;
            case 145:
                res = 39;
                break;
            case 146:
                res = 39;
                break;
            case 151:
                res = 45;
                break;
        }

        return res;
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

    IViewChat _serverCOMView;
    protected IViewChat ServerCOMView
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
        CloseSerialPort();

        base.Dispose();
    }

    #endregion 

}
