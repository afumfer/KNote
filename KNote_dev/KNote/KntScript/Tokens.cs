using System;
using System.Collections.Generic;
using System.Text;

namespace KntScript
{
    internal sealed class Tokens
    {
        // Constants to represent arithmitic tokens. 
        internal static readonly OperatorToken Add = new OperatorToken("ADD", 4);
        internal static readonly OperatorToken Sub = new OperatorToken("SUB", 4);
        internal static readonly OperatorToken Mul = new OperatorToken("MUL", 5);
        internal static readonly OperatorToken Div = new OperatorToken("DIV", 5);

        // relational operators
        internal static readonly OperatorToken LessThan = new OperatorToken("LESSTHAN", 3);
        internal static readonly OperatorToken GreaterThan = new OperatorToken("GREATERTHAN", 3);
        internal static readonly OperatorToken LessThanOrEqual = new OperatorToken("LESSTHANOREQUAL", 3);
        internal static readonly OperatorToken GreaterThanOrEqual = new OperatorToken("GREATERTHANOREQUAL", 3);
        internal static readonly OperatorToken Equal = new OperatorToken("EQUAL", 3);
        internal static readonly OperatorToken NotEqual = new OperatorToken("NOTEQUAL", 3);

        // logical operators
        internal static readonly OperatorToken And = new OperatorToken("AND", 2);
        internal static readonly OperatorToken AndBit = new OperatorToken("ANDBIT", 2);
        internal static readonly OperatorToken Or = new OperatorToken("OR", 1);
        internal static readonly OperatorToken OrBit = new OperatorToken("ORBIT", 1);
        internal static readonly OperatorToken Not = new OperatorToken("NOT", 6);

        // symbol tokens
        internal static readonly SymbolToken Semi = new SymbolToken("SEMI");
        internal static readonly SymbolToken Assignment = new SymbolToken("ASSIGNMENT");
        internal static readonly SymbolToken LeftBracket = new SymbolToken("LEFTBRACKET");
        internal static readonly SymbolToken RightBracket = new SymbolToken("RIGHBRACKET");
        internal static readonly SymbolToken Comma = new SymbolToken("COMMA");
        //public static readonly SymbolToken Dot = new SymbolToken("DOT");  // Discarded in this version
        internal static readonly SymbolToken Colon = new SymbolToken("COLON");
        internal static readonly SymbolToken LeftCurlyBracket = new SymbolToken("LEFTCURLYBRACKET");
        internal static readonly SymbolToken RightCurlyBracket = new SymbolToken("RIGHTCURLYBRACKET");

        // keyword tokens
        internal static readonly KeywordToken EndSequence = new KeywordToken("ENDSEQUENCE");
        internal static readonly KeywordToken Print = new KeywordToken("PRINT");
        internal static readonly KeywordToken PrintLine = new KeywordToken("PRINTLINE");
        internal static readonly KeywordToken Clear = new KeywordToken("CLEAR");
        internal static readonly KeywordToken Var = new KeywordToken("VAR");
        internal static readonly KeywordToken ReadVar = new KeywordToken("READVAR");
        internal static readonly KeywordToken For = new KeywordToken("FOR");
        internal static readonly KeywordToken To = new KeywordToken("TO");
        internal static readonly KeywordToken If = new KeywordToken("IF");
        internal static readonly KeywordToken Then = new KeywordToken("THEN");
        internal static readonly KeywordToken Else = new KeywordToken("ELSE");
        internal static readonly KeywordToken While = new KeywordToken("WHILE");
        internal static readonly KeywordToken Break = new KeywordToken("BREAK");
        internal static readonly KeywordToken New = new KeywordToken("NEW");
        internal static readonly KeywordToken ForEach = new KeywordToken("FOREACH");
        internal static readonly KeywordToken In = new KeywordToken("IN");

        internal static readonly KeywordToken Null = new KeywordToken("NULL");
    }

    internal class OperatorToken : Token
    {
        private readonly int _precedence = 0;

        public OperatorToken(string value, int precedence)
            : base(value)
        {
            _precedence = precedence;
        }

        public int Precedence
        {
            get { return _precedence; }
        }
    }

    internal class KeywordToken : Token
    {        
        public KeywordToken(string value)
            : base(value) {}
    }

    internal class IdentifierToken : Token
    {        
        public IdentifierToken(string value)
            : base(value) { }
    }

    internal class StringToken : Token
    {
        private string _value;

        public StringToken(string value)
            : base(value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value.ToString(); }
        }
    }

    internal class IntToken : Token
    {
        private int _value;

        public IntToken(string value)
            : base(value)
        {
            _value = int.Parse(value.ToString(),
                            System.Globalization.CultureInfo.InvariantCulture);
        }

        public int Value 
        {
            get { return _value; }
        }
    }

    internal class FloatToken : Token
    {
        private float _value;
        
        public FloatToken(string value)
            : base(value)
        {
            _value = float.Parse(value.ToString(),
                            System.Globalization.CultureInfo.InvariantCulture);
        }

        public float Value
        {
            get { return _value; }
        }
    }

    internal class DoubleToken : Token
    {
        private double _value;

        public DoubleToken(string value)
            : base(value)
        {
            _value = double.Parse(value.ToString(),
                            System.Globalization.CultureInfo.InvariantCulture);
        }

        public double Value
        {
            get { return _value; }
        }
    }

    internal class DecimalToken : Token
    {
        private decimal _value;

        public DecimalToken(string value)
            : base(value)
        {
            _value = decimal.Parse(value.ToString(),
                            System.Globalization.CultureInfo.InvariantCulture);
        }

        public decimal Value
        {
            get { return _value; }
        }
    }
    
    internal class DateTimeToken : Token
    {
        private DateTime _value;

        public DateTimeToken(string value)
            : base(value)
        {
            //#dd/mm/yyyy#
            _value = DateTime.Parse(value);
        }

        public DateTime Value
        {
            get { return _value; }
        }
    }
    
    internal class BoolToken : Token
    {        
        private bool _value;

        public BoolToken(string value)
            : base(value)
        {
            // true - false
            _value = bool.Parse(value);
        }

        public bool Value
        {
            get { return _value; }
        }
    }

    internal class SymbolToken : Token
    {
        public SymbolToken(string value)
            : base(value) { }
    }

    internal abstract class Token
    {
        private string _name = "";

        public Token(string s)
        {
            _name = s;
        }

        public string Name
        {
            get { return _name; }            
        }

        public override string ToString()
        {
            return _name;
        }
    }
}