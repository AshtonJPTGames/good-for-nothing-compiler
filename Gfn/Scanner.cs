using System;
using System.Collections.Generic; // IList, List.
using System.IO; // TextReader.
using System.Text; // StringBuilder.

public sealed class Scanner
{
    public enum ArithToken
    {
        //Add,
        //Sub,
        //Mul,
        //Div,
        Equal,
        Semi,
		LeftBracket,
		RightBracket
    }

    private readonly IList<object> _result;

    public Scanner(TextReader input)
    {
        _result = new List<object>();
        Scan(input);

		foreach (object o in _result)
		{
			Console.WriteLine("--> {0}", o.ToString());
		}
    }

    public IList<object> Tokens
    {
        get { return _result; }
    }

    private void Scan(TextReader input)
    {
        while (input.Peek() != -1)
        {
            var ch = (char)input.Peek();

            if (char.IsWhiteSpace(ch))
            {
                // eat the current char and skip ahead!
                input.Read();
            }
            else if (char.IsLetter(ch)/* || ch == '_'*/)
            {
                // keyword or identifier
                ScanIdent(input, ch);
            }
            /*else if (ch == '"')
            {
                // string literal
                ScanString(input);
            }*/
            else if (char.IsDigit(ch))
            {
                // numeric literal
                ScanNumber(input, ch);
            }
            else if (ch == ';')
            {
                // end of statement
                ScanSemi(input);
            }
            else
            {
                // arithmetic tokens such as + - / * =
                ScanArith(input, ch);
            }
        }
    }

    private void ScanArith(TextReader input, char ch)
    {
        switch (ch)
        {
            /*case '+':
                input.Read();
                _result.Add(ArithToken.Add);
                break;
            case '-':
                input.Read();
                _result.Add(ArithToken.Sub);
                break;
            case '*':
                input.Read();
                _result.Add(ArithToken.Mul);
                break;
            case '/':
                input.Read();
                _result.Add(ArithToken.Div);
                break;*/
            case '=':
                input.Read();
                _result.Add(ArithToken.Equal);
                break;
			case '{':
				input.Read();
				_result.Add(ArithToken.LeftBracket);
				break;
			case '}':
				input.Read();
				_result.Add(ArithToken.RightBracket);
				break;
            default:
                throw new Exception("Scanner encountered unrecognized character '" + ch + "'");
        }
    }

    private void ScanSemi(TextReader input)
    {
        input.Read();
        _result.Add(ArithToken.Semi);
    }

    private void ScanNumber(TextReader input, char ch)
    {
        var accum = new StringBuilder();

        while (char.IsDigit(ch))
        {
            accum.Append(ch);
            input.Read();

            if (input.Peek() == -1)
            {
                break;
            }
            ch = (char)input.Peek();
        }

        _result.Add(int.Parse(accum.ToString()));
    }

    /*private void ScanString(TextReader input)
    {
        char ch;
        var accum = new StringBuilder();

        input.Read(); // skip the '"'

        if (input.Peek() == -1)
        {
            throw new Exception("unterminated string literal");
        }

        while ((ch = (char)input.Peek()) != '"')
        {
            accum.Append(ch);
            input.Read();

            if (input.Peek() == -1)
            {
                throw new Exception("unterminated string literal");
            }
        }

        // skip the terminating "
        input.Read();
        _result.Add(accum);
    }*/

    private void ScanIdent(TextReader input, char ch)
    {
        var accum = new StringBuilder();

        while (char.IsLetter(ch) || ch == '_')
        {
            accum.Append(ch);
            input.Read();

            if (input.Peek() == -1)
            {
                break;
            }
            ch = (char)input.Peek();
        }

        _result.Add(accum.ToString());
    }
}