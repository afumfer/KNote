using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections;

using KntScript;
using KNote.ClientWin.Views;

namespace KNote.ClientWin.Views
{
    public partial class ServerCOMForm : Form
    {
        #region Private members

        static bool _continue;
        static bool _messageSending = false;
        static SerialPort _serialPort;
        static Queue  _messageQueue;
        static string _error;
        //
        StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;

        #endregion

        #region Constructor

        public ServerCOMForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Form event handler

        private void ServerCOMForm_Load(object sender, EventArgs e)
        {

        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            // TODO: Personalise params COM

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
            _continue = true;
            _messageQueue = new Queue();

            //Task.Factory.StartNew(() => Write(), TaskCreationOptions.LongRunning);
            //Task.Factory.StartNew(() => Write(), TaskCreationOptions.AttachedToParent);

            //Task.Factory.StartNew(() => Read(), TaskCreationOptions.LongRunning);
            //await Task.Factory.StartNew(() => Read(), TaskCreationOptions.AttachedToParent);

            Task.Factory.StartNew(() => Server(), TaskCreationOptions.LongRunning);
            
            statusLabelInfo.Text = "Com started, press Stop button to cancel.";
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            CloseSerialPort();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
            {
                MessageBox.Show("Serial port is closed. Press Start button.");
                return;
            }

            if (_messageSending)
            {
                MessageBox.Show("Sending message ... try later");
                return;
            }

            _messageQueue.Enqueue(textBoxSend.Text);
            //_messageQueue.Enqueue(textBoxSend.Text + '\n');
        }

        private void ServerCOMForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_continue)
            {
                MessageBox.Show("Comm is opened. Press close button before close this form.");
                e.Cancel = true;
            }
        }

        #endregion
        
        private void Server()
        {
            Task.Factory.StartNew(() => Read(), TaskCreationOptions.LongRunning);

            while (_continue)
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

            while (_continue)
            {
                try
                {
                    messageIn = _serialPort.ReadLine();

                    listBoxEcho.Invoke((MethodInvoker)delegate
                    {
                        // Running on the UI thread                        
                        listBoxEcho.Items.Add("Recived: " + messageIn);
                    });
                    
                    //output = ExecuteScript(messageIn);

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

        #region  Old, deprecated  
        //private List<string> ExecuteScript(string script)
        //{
        //    //object xoutput;   
        //    var outputClient = new List<string>();
        //    int antErrorCode = 0;
        //    string antErrorDescription = "";
        //    bool flagExpresion = true;

        //    if (String.IsNullOrEmpty(script))
        //        return null;

        //    if (script[0] == '$')
        //    {
        //        flagExpresion = false;
        //        script = script.Substring(1);
        //    }
        //    else
        //        flagExpresion = true;



        //    if (flagExpresion)
        //    {
        //        script = @"_ANTERRORTRAP = true; _output = " + script;
        //    }
        //    else
        //    {
        //        script = @"_ANTERRORTRAP = true; " + script;
        //        script += @" _consoleOutput = GetOutContent();";
        //        //script += @" HideOutWindow();";
        //    }

        //    var engine = new KntScript.KntSEngine(new InOutDeviceForm());

        //    if (flagExpresion)
        //        engine.AddVar("_output", null);
        //    else
        //        engine.AddVar("_consoleOutput", null);


        //    engine.AddVar("_ANTERRORCODE", -1);
        //    engine.AddVar("_ANTERRORDESCRIPTION", "Not executed.");

        //    //engine.VisibleInOutDevice = false;
        //    //engine.FunctionLibrary = new KntTestServiceLibrary();
        //    //engine.Run();
        //    antErrorCode = (int)engine.GetVar("_ANTERRORCODE");
        //    antErrorDescription = (string)engine.GetVar("_ANTERRORDESCRIPTION");

        //    //engine.InOutDevice.Close();

        //    //if(flagExpresion)
        //    //    xoutput = engine.GetVar("_output") ?? "";
        //    //else
        //    //    xoutput = (string)engine.GetVar("_consoleOutput");


        //    //if(xoutput.GetType() == typeof(string))
        //    //    outputClient.Add(xoutput.ToString() + '\n');
        //    //else if ( xoutput.GetType().GetInterface(nameof(IEnumerable)) != null)
        //    //    foreach (var x in (IEnumerable)xoutput)
        //    //    {           
        //    //        outputClient.Add(x.ToString() + '\n'); 
        //    //    }
        //    //else
        //    //    outputClient.Add(xoutput.ToString() + '\n');

        //    return outputClient;            
        //}
        #endregion 

        public void SendMessage(string messageSource)
        {
            statusLabelInfo.Text = "Sending ...";
            _messageSending = true;

            byte[] bMessage = ConverTextToQDOSBytes(messageSource);

            //byte[] eos = new byte[1];
            //eos[0] = 0;

            try
            {
                if (!String.IsNullOrEmpty(messageSource))
                {
                    for (int i = 0; i < bMessage.Length; i++)
                    {
                        if (!_continue)
                            break;
                        _serialPort.Write(bMessage, i, 1);
                        Thread.Sleep(20); 
                    }                    
                }
                    
            }
            catch (TimeoutException) { }
            catch (Exception e) { _error = e.Message; }

            statusLabelInfo.Text = "Sended ...";
            _messageSending = false;
        }


        private void CloseSerialPort()
        {
            _continue = false;

            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                    _serialPort.Close();
            }
            catch { }            
        }

        #region Utils / Tests

        public static void ConverFileAnsiToQdos(string sourceFile, string targetFile)
        {
            byte[] bSource = File.ReadAllBytes(sourceFile);

            int lenB = bSource.Length;
            byte[] bTarget = new byte[lenB];

            for (int i = 0; i < lenB; i++)
                bTarget[i] = ConvertToQdosByte(bSource[i]);

            File.WriteAllBytes(targetFile, bTarget);
        }

        public static byte[] ConverTextToQDOSBytes(string sourceText)
        {
            byte[] bSource = Encoding.Default.GetBytes(sourceText);
            
            int lenB = bSource.Length;
            byte[] bTarget = new byte[lenB];

            for (int i = 0; i < lenB; i++)
                bTarget[i] = ConvertToQdosByte(bSource[i]);            

            return bTarget;
        }
        
        private static byte[] EosQdos()
        {
            byte[] eos = new byte[1];
            eos[0] = 255;
            return eos;
        }

        private static byte ConvertToQdosByte(byte x)
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

        private void TestSendBinary1()
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

        private void TestSendBinary2()
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
                label1.Text = i.ToString();
                label1.Refresh();
            }

            MessageBox.Show("fin");
        }

        private void TestSendBinary3()
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

            if(nRes > 0)
                _serialPort.Write(bf, i, nRes);

            MessageBox.Show("fin");
        }



        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            //var l = ExecuteScript("(10+10);");
            //var l = ExecuteScript("QTestGetFolders();");
            //var l = ExecuteScript("@printline \"Hola hola\";");

            TestSendBinary2();

        }
    }
}
