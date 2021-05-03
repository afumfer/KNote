using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KntScript
{
    internal sealed class Scanner
    {
        #region Constructors

        internal Scanner(TextReader input)
        {
            _result = new List<Token>();
            Scan(input);
        }

        internal Scanner(string input) :
            this(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input))))
        {

        }

        #endregion

        #region Properties

        private readonly IList<Token> _result;
        internal IList<Token> TokensList
        {
            get { return _result; }
        }

        #endregion

        #region Private Methods

        private void Scan(TextReader input)
        {
            while (input.Peek() != -1)
            {
                char ch = (char)input.Peek();

                //
                // Scan individual tokens
                //

                // eat the current char and skip ahead!
                if (char.IsWhiteSpace(ch))
                {                    
                    input.Read();
                }
                //Comment
                else if (ch.ToString() == "'")
                {
                    
                    ch = ReadComment(input, ch);
                }
                // keyword or identifier
                else if (char.IsLetter(ch) || ch == '_')
                {                                        
                    _result.Add(ReadKeywordOrIdentifier(input, ch));

                }
                // string literal
                else if (ch == '"')
                {
                    _result.Add(ReadStringLiteral(input, ch));
                }
                // numeric literal
                else if (char.IsDigit(ch))
                {
                    _result.Add(ReadNumericLiteral(input, ch));
                }
                // datetime literal
                else if (ch == '#')
                {
                    _result.Add(ReadDateTimeLiteral(input, ch));
                }
                // operator or symbol 
                else
                    _result.Add(ReadOperatorOrSymbol(input, ch));
            }
        }

        #endregion

        #region Utils

        private char ReadComment(TextReader input, char ch)
        {
            // Comment, eat full line
            while ((ch = (char)input.Peek()) != '\n')  
            {
                if (input.Read() == -1)
                {
                    break;
                }
            }
            // eat newline character
            if (ch == '\n')
            {
                input.Read();
            }

            return ch;
        }

        private Token ReadKeywordOrIdentifier(TextReader input, char ch)
        {
            StringBuilder accum = new StringBuilder();
            Token token;

            while (char.IsLetter(ch) || char.IsDigit(ch) || ch == '_' || ch == '.')
            {
                accum.Append(ch);
                input.Read();

                if (input.Peek() == -1)
                {
                    break;
                }
                else
                {
                    ch = (char)input.Peek();
                }
            }

            // literal operators 
            if (accum.ToString() == "or")
                token = Tokens.Or;
            else if (accum.ToString() == "and")
                token = Tokens.And;
            else if (accum.ToString() == "not")
                token = Tokens.Not;

            // end block 
            else if (accum.ToString() == "end")
                token = Tokens.EndSequence;

            // keywords
            else if (accum.ToString() == "print")
                token = Tokens.Print;
            else if (accum.ToString() == "printline")
                token = Tokens.PrintLine;
            else if (accum.ToString() == "clear")
                token = Tokens.Clear;
            else if (accum.ToString() == "var")
                token = Tokens.Var;
            else if (accum.ToString() == "readvar")
                token = Tokens.ReadVar;
            else if (accum.ToString() == "for")
                token = Tokens.For;
            else if (accum.ToString() == "to")
                token = Tokens.To;
            else if (accum.ToString() == "if")
                token = Tokens.If;
            else if (accum.ToString() == "then")
                token = Tokens.Then;
            else if (accum.ToString() == "else")
                token = Tokens.Else;
            else if (accum.ToString() == "while")
                token = Tokens.While;
            else if (accum.ToString() == "break")
                token = Tokens.Break;
            else if (accum.ToString() == "new")
                token = Tokens.New;
            else if (accum.ToString() == "true")               
                token = new BoolToken("true");
            else if (accum.ToString() == "false")
                token = new BoolToken("false");
            else if (accum.ToString() == "foreach")
                token = Tokens.ForEach;
            else if (accum.ToString() == "in")
                token = Tokens.In;
            else if (accum.ToString() == "null")
                token = Tokens.Null;
                
            // identifier
            else
            {
                IdentifierToken identifier = new IdentifierToken(accum.ToString());
                token = identifier;
            }

            return token;
        }

        private StringToken ReadStringLiteral(TextReader input, char ch)
        {
            StringBuilder accum = new StringBuilder();
            
            input.Read(); // skip "

            if (input.Peek() == -1)
            {
                throw new System.Exception("unterminated string literal");
            }

            while ((ch = (char)input.Peek()) != '"')
            {
                accum.Append(ch);
                input.Read();

                if (input.Peek() == -1)
                {
                    throw new System.Exception("unterminated string literal");
                }
            }

            // skip the terminating "
            input.Read();

            return new StringToken(accum.ToString());
        }

        private Token ReadNumericLiteral(TextReader input, char ch)
        {
            StringBuilder accum = new StringBuilder();
            // TODO: check that there are no two continuous points
            while (char.IsDigit(ch) || ch == '.')
            {
                accum.Append(ch);
                input.Read();

                if (input.Peek() == -1)
                    break;
                else
                    ch = (char)input.Peek();
            }

            // F -> Float // M -> Decimal
            ch = (char)input.Peek();

            if (ch == 'f' || ch == 'F')
            {
                input.Read();
                return new FloatToken(accum.ToString());
            }
            else if (ch == 'm' || ch == 'M')
            {
                input.Read();
                return new DecimalToken(accum.ToString());
            }
            else if (!accum.ToString().Contains("."))
                return new IntToken(accum.ToString());
            else 
                return new DoubleToken(accum.ToString());

        }

        private DateTimeToken ReadDateTimeLiteral(TextReader input, char ch)
        {
            StringBuilder accum = new StringBuilder();

            input.Read(); // skip #

            if (input.Peek() == -1)
            {
                throw new System.Exception("unterminated datetime literal");
            }

            while ((ch = (char)input.Peek()) != '#')
            {
                accum.Append(ch);
                input.Read();

                if (input.Peek() == -1)
                {
                    throw new System.Exception("unterminated datetime literal");
                }
            }

            // skip the terminating #
            input.Read();

            return new DateTimeToken(accum.ToString());
        }

        private Token ReadOperatorOrSymbol(TextReader input, char ch)
        {
            char chTmp;
            Token token;

            switch (ch)
            {
                case '+':
                    input.Read();
                    token = Tokens.Add;
                    break;

                case '-':
                    input.Read();
                    token = Tokens.Sub;
                    break;

                case '*':
                    input.Read();
                    token = Tokens.Mul;
                    break;

                case '/':
                    input.Read();
                    token = Tokens.Div;
                    break;

                case '=':
                    input.Read(); // eat =
                    chTmp = (char)input.Peek();
                    if (chTmp == '=')
                    {
                        input.Read(); // eat =
                        token = Tokens.Equal;
                    }
                    else
                    {
                        token = Tokens.Assignment;
                    }                    
                    break;

                case ';':
                    input.Read();
                    token = Tokens.Semi;
                    break;

                case ':':
                    input.Read();
                    token = Tokens.Colon;
                    break;

                case ',':
                    input.Read();
                    token = Tokens.Comma;
                    break;

                case '(':
                    input.Read();
                    token = Tokens.LeftBracket;
                    break;

                case ')':
                    input.Read();
                    token = Tokens.RightBracket;
                    break;

                case '{':
                    input.Read();
                    token = Tokens.LeftCurlyBracket;
                    break;

                case '}':
                    input.Read();
                    token = Tokens.RightCurlyBracket;
                    break;

                case '<':
                    input.Read(); // eat <
                    chTmp = (char)input.Peek();
                    if (chTmp == '>')
                    {
                        input.Read(); // eat >
                        token = Tokens.NotEqual;
                    }
                    else if (chTmp == '=')
                    {
                        input.Read(); // eat =
                        token = Tokens.LessThanOrEqual;
                    }
                    else
                    {
                        token = Tokens.LessThan;
                    }                    
                    break;

                case '>':
                    input.Read(); // eat >
                    chTmp = (char)input.Peek();
                    if (chTmp == '=')
                    {
                        input.Read(); // eat =
                        token = Tokens.GreaterThanOrEqual;
                    }
                    else
                    {
                        token = Tokens.GreaterThan;
                    }                    
                    break;

                case '!':
                    input.Read(); // eat !
                    chTmp = (char)input.Peek();
                    if (chTmp == '=')
                    {
                        input.Read(); // eat =
                        token = Tokens.NotEqual;
                    }
                    else
                    {
                        token = Tokens.Not;
                    }                    
                    break;

                case '&':
                    input.Read(); // eat &
                    chTmp = (char)input.Peek();
                    if (chTmp == '&')
                    {
                        input.Read(); // eat &
                        token = Tokens.And;
                    }
                    else
                    {
                        token = Tokens.AndBit;
                    }                    
                    break;

                case '|':
                    input.Read(); // eat |
                    chTmp = (char)input.Peek();
                    if (chTmp == '|')
                    {
                        input.Read(); // eat |
                        token = Tokens.Or;
                    }
                    else
                    {
                        token = Tokens.OrBit;
                    }                    
                    break;

                default:
                    throw new System.Exception("Scanner encountered unrecognized character '" + ch + "'");
            }

            return token;
        }

        #endregion
    }
}