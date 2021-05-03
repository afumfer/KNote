using System;
using System.Collections.Generic;
using System.Text;

namespace KntScript
{
    #region Stmt's

    /* <stmt> := var <ident> = <expr>
     *  | <ident> = <expr>
     *  | for <ident> = <expr> to <expr> <stmt> end for 
     *  | while <expr> <stmt> end while
     *  | if <expr> <stmt> else <stmt> end if     
     *  | readvar <expr>:<ident>, ...
     *  | print <expr>
     *  | <stmt> ; <stmt>
     *  | <func_call>        
    */

    internal abstract class Stmt
    {
    }

    // var <ident> = <expr>
    internal class DeclareVar : Stmt
    {        
        public string Ident;        
        public Expr Expr;
    }

    // print <expr>
    internal class Print : Stmt
    {
        public Expr Expr;
    }

    // printline <expr>
    internal class PrintLine : Stmt
    {
        public Expr Expr;
    }

    // clear
    internal class Clear : Stmt
    {
        public string Tag;
    }

    // <ident> = <expr>
    internal class Assign : Stmt
    {        
        public string Ident;     
        public Expr Expr;
    }

    // for <ident> = <expr> to <expr> <stmt> end for
    internal class ForLoop : Stmt
    {        
        public string Ident;     
        public Expr From;
        public Expr To;
        public Stmt Body;
    }

    // foreach <ident> in <expr> <stmt> end foreach
    internal class ForEachLoop : Stmt
    {
        public string Ident;
        public Expr Colec;        
        public Stmt Body;
    }

    // if <expr> then <stmt> end if |
    // if <expr> then <stmt> else <stmt> end if 
    internal class IfStmt : Stmt
    {        
        public Expr TestExpr;        
        public Stmt BodyIf;
        public Stmt BodyElse;
        public bool DoElse;
    }

    // while <expr> <stmt> end while
    internal class WhileStmt : Stmt
    {
        public Expr TestExpr;
        public Stmt Body;
    }

    // break
    internal class BreakStmt : Stmt
    {
        public string Tag;
    }

    // <FunctionStmt(<arg>)
    internal class FunctionStmt : Stmt
    {
        public FunctionExpr Function;
    }

    // <readvar> := {<expr:ident>}
    internal class ReadVar : Stmt
    {        
        public Dictionary<Variable, Expr> Vars;

        public ReadVar()
        {
            Vars = new Dictionary<Variable, Expr>();
        }
    }

    // <stmt> ; <stmt>
    internal class Sequence : Stmt
    {
        public Stmt First;
        public Stmt Second;
    }

    #endregion

    #region Expr's

    /* <expr> := <string>
     *  | <int>
     *  | <float>
     *  | <double>
     *  | <decimal>
     *  | <DateTime>
     *  | <bin_expr>
     *  | <ident>
     *  | <unary_expr>
     *  | <fun_expr>
     *  | <newobject_exp>
     */
    internal abstract class Expr
    {
    }

    // <string> := " <string_elem>* "
    internal class StringVal : Expr
    {        
        public string Value;     
    }

    // <int> := <digit>+
    internal class IntVal : Expr
    {
        public int Value;
    }

    // <float>
    internal class FloatVal : Expr
    {        
        public float Value;     
    }

    // <double>
    internal class DoubleVal : Expr
    {
        public double Value;
    }

    // <decimal>
    internal class DecimalVal : Expr
    {
        public decimal Value;
    }

    // <DateTime>
    internal class DateTimeVal : Expr
    {
        public DateTime Value;
    }

    // <bool>
    internal class BoolVal : Expr
    {
        public bool Value;
    }

    // <null>
    internal class NullVal : Expr
    {
        public object Value { get { return null; } }
    }

    // <ident> := <char> <ident_rest>*
    // <ident_rest> := <char> | <digit>
    internal class Variable : Expr
    {        
        public string Ident;     
    }

    // <func_expr> := <ident> (<args>)
    internal class FunctionExpr : Expr
    {
        public string FunctionName;
        public List<Expr> Args;

        public FunctionExpr()
        {
            Args = new List<Expr>();
        }
    }

    // <new_object> := new <ident> (<args>)
    internal class NewObjectExpr : Expr
    {
        public string ClassName;
        public List<Expr> Args;

        public NewObjectExpr()
        {
            Args = new List<Expr>();
        }
    }

    // <bin_expr> := <expr> <bin_op> <expr>
    internal class BinaryExpr : Expr
    {
        public BinOp Op;
        public Expr Left;
        public Expr Right;        
    }

    // <unary_expr> := <bin_op> <expr>
    internal class UnaryExpr : Expr
    {
        public BinOp Op;
        public Expr Expression;
    }

    #endregion

    #region Others

    // <bin_op> := + | - | * | / | && | ! | || 
    internal enum BinOp
    {
        Add,
        Sub,
        Mul,
        Div,
        Or,
        And, 
        Not,
        Equal,
        NotEqual,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual
    }

    // TODO: Pending improvement 
    //       IndentObject is processed in CodeRun, it should be processed in Parser.

    public class ReadVarItem
    {
        //public Variable Var;
        public string VarIdent;
        public object VarValue;        
        public object Label;
        public string VarNewValueText;
    }

    internal class IdentObject
    {
        public string RootObj { get; set; }
        public List<string> ChainObjs { get; set; }
        public string Member { get; set; }
        public string PathObj { get; set; }

        public IdentObject(string ident)
        {
            string[] tmp;
            int posMember;

            if (string.IsNullOrEmpty(ident))
                return;

            ChainObjs = new List<string>();

            tmp = ident.Split('.');

            for (int i = 0; i < tmp.Length; i++)
            {
                if (i == 0)
                    RootObj = tmp[i];
                else if (i == tmp.Length - 1)
                    Member = tmp[i];
                else
                    ChainObjs.Add(tmp[i]);
            }
            if (Member != null)
            {
                posMember = ident.IndexOf(Member);
                if (posMember > 0)
                    PathObj = ident.Substring(0, ident.Length - Member.Length - 1);
                else
                    PathObj = Member;
            }
            else
                PathObj = RootObj;
        }
    }

    #endregion
}

