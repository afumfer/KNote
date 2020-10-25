using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace KntScript
{
    public class KntSEngine
    {
        #region Private members 

        private bool _visibleInOutDevice;
        private Library _functionLibrary;

        #endregion 

        #region Properties
                
        private IInOutDevice _inOutDevice;
        public IInOutDevice InOutDevice
        {
            get
            {
                return _inOutDevice;
            }
        }

        private Dictionary<string, object> _symbolTable;
        public Dictionary<string, object> SymbolTable
        {
            get { return _symbolTable; }        
        }

        #endregion

        #region Constructor

        public KntSEngine(IInOutDevice inOutDevice, Library functionLibrary, bool visibleInOutDevice = true)
        {            
            _inOutDevice = inOutDevice;
            _functionLibrary = functionLibrary;
            _symbolTable = new Dictionary<string, object>();
            _visibleInOutDevice = visibleInOutDevice;
        }
        
        public KntSEngine(IInOutDevice inOutDevice) : this(inOutDevice, new Library(), true)
        {

        }

        #endregion

        #region Public methods

        public void Run(string sourceCode)
        {
            try
            {
                if (_visibleInOutDevice)
                {
                    _inOutDevice.Show();
                    _inOutDevice.LockForm(true);
                }

                Scanner scanner = new Scanner(sourceCode);                
                Parser parser = new Parser(scanner.TokensList);                
                CodeRun codeRun = new CodeRun(_inOutDevice, _functionLibrary, _symbolTable);
                codeRun.Run(parser.Result);

                if (_visibleInOutDevice)                                    
                    _inOutDevice.LockForm(false);
                
            }
            catch (Exception err)
            {
                if (!_visibleInOutDevice)
                    _inOutDevice.Show();
                _inOutDevice.LockForm(false);
                _inOutDevice.Print(err.Message);
                //throw err; // ??
            }        
        }

        public void RunFile(string sourceCodeFile)
        {
            if (File.Exists(sourceCodeFile))
            {
                using (TextReader input = File.OpenText(sourceCodeFile))
                {
                    var sourceCode = input.ReadToEnd().ToString();
                    Run(sourceCode);
                }
            }
            else
            {
                throw new Exception("Source code file no exist.");
            }
        }

        public void AddVar(string ident, object value)
        {            
            if (string.IsNullOrEmpty(ident))
                throw new System.Exception(" variable is required ");

            if (!_symbolTable.ContainsKey(ident))
                _symbolTable.Add(ident, value);
            else
                throw new System.Exception(" variable '" + ident + "' already declared");            
        }

        public object GetVar(string ident)
        {
            if (_symbolTable.ContainsKey(ident))                
                return _symbolTable[ident];
            else                
                throw new System.Exception(" undeclared variable '" + ident);
        }

        public void ClearAllVars()
        {
            _symbolTable.Clear();
        }

        #endregion
    }
}
