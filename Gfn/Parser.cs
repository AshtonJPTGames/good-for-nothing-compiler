using System;
using System.Collections.Generic;
using System.Text;

public sealed class Parser
{
    private Stmt _result;
    private readonly IList<object> _tokens;
    private int _index;

    public Parser(IList<object> tokens)
    {
        _tokens = tokens;
        _index = 0;
    }

    public void Parse()
    {
		_result = ParseNextStmt();

        if (_index != _tokens.Count)
            throw new Exception("expected EOF");
    }

    public Stmt Result
    {
        get { return _result; }
    }

    private Stmt ParseNextStmt()
    {
        Stmt result;

        if (_index == _tokens.Count)
        {
            throw new Exception("expected statement, got EOF");
        }

		if (_tokens[_index].Equals("Class"))
		{
			result = ParseClass();
			Console.WriteLine("JKL");
			//--_index;
		}
		/*else if (_tokens[_index].Equals("print"))
		{
			result = ParsePrint();
		}*/
		/*else if (_tokens[_index].Equals("var"))
		{
			result = ParseVar();
		}*/
		/*else if (_tokens[_index].Equals("Integer"))
		{
			Console.WriteLine("PQR");
			result = ParseVar();
		}*/
		/*else if (_tokens[_index].Equals("Float"))
		{
			result = ParseVar();   		}
		else if (_tokens[_index].Equals("Boolean"))
		{
			result = ParseVar();
		}
        else if (_tokens[_index].Equals("read_int"))
        {
            result = ParseReadInt();
        }
        else if (_tokens[_index].Equals("read_string"))
        {
            result = ParseReadString();
        }
        else if (_tokens[_index].Equals("for"))
        {
            result = ParseForLoop();
        }
        else if (_tokens[_index] is string) // variable assignment
        {
            result = ParseAssignment();
        }*/
        else
        {
			Console.WriteLine("MOFUGGA: {0}", _tokens[_index].ToString());
			result = null;
            //throw new Exception("parse error at token " + _index + ": " + _tokens[_index]);
        }

        _index++;
		Console.WriteLine("MNO");


        if (_index >= _tokens.Count)    // no more to parse
            return result;

        /*if (_tokens[_index].Equals("end")) // end of for-loop NOTE: Can this be moved closer to the "for" part above?
            return result;*/

        return new Sequence { First = result, Second = ParseNextStmt() };
    }

	/*private Stmt ParseAssignment()
    {
        var ident = (string)_tokens[_index++];

        if (_index == _tokens.Count ||
            (Scanner.ArithToken)_tokens[_index] != Scanner.ArithToken.Equal)
            throw new Exception("expected '='");

        _index++;

        return new Assign { Ident = ident, Expr = ParseExpr() };
    }*/

	/*private Stmt ParseForLoop()
    {
        _index++;
        var forLoop = new ForLoop();

        if (_index >= _tokens.Count || !(_tokens[_index] is string))
            throw new Exception("expected identifier after 'for'");

        forLoop.Ident = (string)_tokens[_index];

        _index++;

        if (_index == _tokens.Count ||
            (Scanner.ArithToken)_tokens[_index] != Scanner.ArithToken.Equal)
            throw new Exception("for missing '='");

        _index++;

        forLoop.From = ParseExpr();

        if (_index == _tokens.Count ||
            !_tokens[_index].Equals("to"))
            throw new Exception("expected 'to' after for");

        _index++;

        forLoop.To = ParseExpr();   //TODO: Change compiler - loop ends one step early

        if (_index == _tokens.Count ||
            !_tokens[_index].Equals("do"))
            throw new Exception("expected 'do' after from expression in for loop");

        _index++;

        forLoop.Body = ParseNextStmt();

        if (_index == _tokens.Count ||
            !_tokens[_index].Equals("end"))
            throw new Exception("unterminated 'for' loop body");

        _index++;

        return forLoop;
    }*/

	/*private Stmt ParseReadString()
    {
        _index++;

        if (_index >= _tokens.Count || !(_tokens[_index] is string))
            throw new Exception("expected variable name after 'read_string'");

        return new ReadString((string)_tokens[_index++]);
    }*/

	/*private Stmt ParseReadInt()
    {
        _index++;

        if (_index >= _tokens.Count || !(_tokens[_index] is string))
            throw new Exception("expected variable name after 'read_int'");

        return new ReadInt((string)_tokens[_index++]);
    }*/

	private Stmt ParseClass()
	{
		// Skip the "Class" keyword.
		NextToken();

		// Get class name.
		ExpectTokenType(typeof(string), 0);
		string className = (string)CurrentToken();
		NextToken();

		// Get left bracket.
		ExpectToken(Scanner.ArithToken.LeftBracket, 0);
		NextToken();

		IList<Stmt> classBody = new List<Stmt>();

		Console.WriteLine("ABC");
		// Get class body.
		while (!ExpectToken(Scanner.ArithToken.RightBracket, 0))
		{
			Console.WriteLine("0");
			if (CurrentToken().Equals("Integer"))
			{
				Console.WriteLine("1");
				// Skip "Integer" keyword.
                NextToken();
				Console.WriteLine("2");

				// Get variable name.
				ExpectTokenType(typeof(string), 0);
				string variableName = (string)CurrentToken();
                NextToken();
				Console.WriteLine("3");

				// Get "=".
				ExpectToken(Scanner.ArithToken.Equal, 3);
                NextToken();
				Console.WriteLine("4");

				// Get right-hand expression.
				Expr expression = ParseExpr();
				Console.WriteLine("5");

				NextToken();

				classBody.Add(new DeclareVar(variableName, expression));
				Console.WriteLine("6");
			}
		}
		Console.WriteLine("DEF");

		// Get right bracket.
		ExpectToken(Scanner.ArithToken.RightBracket, 0);
		NextToken();

		Console.WriteLine("GHI");

		return new ClassDefinition(className, classBody);
	}

	private Stmt ParseVar()
	{
		Console.WriteLine("PARSEVAR");
		_index++;

		if (_index >= _tokens.Count || !(_tokens[_index] is string))
			throw new Exception("expected variable name after 'var'");

		var ident = (string)_tokens[_index];

		_index++;

		if (_index == _tokens.Count ||
			(Scanner.ArithToken)_tokens[_index] != Scanner.ArithToken.Equal)
			throw new Exception(string.Format("expected = after 'var {0}'", ident));

		_index++;

		var expr = ParseExpr();
		return new DeclareVar(ident, expr);	 }

	private bool ExpectToken(object expectedToken, int errorType)
	{
		if (!NoMoreTokens() || CurrentTokenMatches(expectedToken)) { return true; }

		switch (errorType)
		{
			case 0:
				Console.WriteLine("ExpectTokenType_Error: Expected name after 'Class' keyword.");
				break;
			case 1:
				Console.WriteLine("ExpectTokenType_Expected '}' after class definition.");
				break;
			default:
				Console.WriteLine("ExpectTokenType_Error: Unexpected error. :(");
				break;
		}

		return false;
	}

	private void ExpectTokenType(Type expectedType, int errorType)
	{
		if (!NoMoreTokens() || TokenIsType(CurrentToken(), expectedType)) { return; }

		switch (errorType)
		{
			case 0:
				Console.WriteLine("ExpectTokenType_Error: Expected name after 'Class' keyword.");
				break;
			default:
				Console.WriteLine("ExpectTokenType_Error: Unexpected error. :(");
				break;
		}
	}

	private void NextToken()
	{
		++_index;
	}

	private object CurrentToken()
	{
		return _tokens[_index];
	}

	private bool TokenIsType(object token, Type t)
	{
		return token.GetType().Equals(t);
	}

	private bool NoMoreTokens()
	{
		return _index >= _tokens.Count;
	}

	private bool CurrentTokenMatches(object o)
	{
		return CurrentToken().Equals(o);
	}

    private Stmt ParsePrint()
    {
        _index++;
        return new Print(ParseExpr());
    }

    private Expr ParseExpr()
    {
		Console.WriteLine("PARSEEXPR");
        if (_index == _tokens.Count) throw new Exception("expected expression, got EOF");

        var nexttoken = _tokens[_index + 1];

        //check if this is a arithmetic-expr or simple expr
        if (
            (nexttoken is string && (string)nexttoken == "to") ||   // loop
            (nexttoken is string && (string)nexttoken == "do") ||   // loop
            (nexttoken is Scanner.ArithToken && (Scanner.ArithToken)nexttoken == Scanner.ArithToken.Semi)
            )
        {
            return ParseSimpleExpr();
        }

		// MY SOLUTION
		return null;

        /*var binexpr = new ArithExpr();
        switch ((Scanner.ArithToken)nexttoken)
        {
            case Scanner.ArithToken.Add:
                binexpr.Op = ArithOp.Add;
                break;
            case Scanner.ArithToken.Sub:
                binexpr.Op = ArithOp.Sub;
                break;
            case Scanner.ArithToken.Mul:
                binexpr.Op = ArithOp.Mul;
                break;
            case Scanner.ArithToken.Div:
                binexpr.Op = ArithOp.Div;
                break;
        }
        binexpr.Left = ParseSimpleExpr();
        _index++;
        binexpr.Right = ParseExpr();
        return binexpr;*/
    }

    private Expr ParseSimpleExpr()
    {
		Console.WriteLine("PARSESIMPLEEXPR");
        if (_index == _tokens.Count)
            throw new Exception("expected expression, got EOF");

        /*if (_tokens[_index] is StringBuilder)
        {
            var value = _tokens[_index++].ToString();
            var stringLiteral = new StringLiteral(value);
            return stringLiteral;
        }*/

        if (_tokens[_index] is int)
        {
            var intValue = (int)_tokens[_index++];
            var intLiteral = new IntLiteral(intValue);
            return intLiteral;
        }

        if (_tokens[_index] is string)
        {
            var ident = (string)_tokens[_index++];
            var var = new Variable(ident);
            return var;
        }

        throw new Exception("expected string literal, int literal, or variable");
    }
}