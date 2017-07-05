using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull

public sealed class CodeGen
{
    private readonly AssemblyBuilder _asmb;
    private readonly ILGenerator _il;
    private readonly MethodBuilder _methb;
    private readonly ModuleBuilder _modb;
    private readonly string _moduleName;
    private readonly Stmt _stmt;
    public static Dictionary<string, LocalBuilder> SymbolTable;
    private readonly TypeBuilder _typeBuilder;

	private int pass;

    public CodeGen(Stmt stmt, string moduleName)
    {
		pass = -1;

		Console.WriteLine("IN CodeGen");
        if (string.IsNullOrEmpty(moduleName)) throw new ArgumentException("must have a module name", "moduleName");

        _stmt = stmt;
        _moduleName = moduleName;
        if (Path.GetFileName(moduleName) != moduleName)
        {
            throw new Exception("can only output into current directory!");
        }

        var filename = Path.GetFileNameWithoutExtension(moduleName);
        var asmName = new AssemblyName(filename);
        _asmb = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Save);
        _modb = _asmb.DefineDynamicModule(moduleName);
        _typeBuilder = _modb.DefineType("Foo");

        _methb = _typeBuilder.DefineMethod("Main", MethodAttributes.Static, typeof(void),
            Type.EmptyTypes);

        _il = _methb.GetILGenerator();
        SymbolTable = new Dictionary<string, LocalBuilder>();
    }

	// An empty source file input will result in the following output C# equivalent in CIL.
	/*
	 * Class Program {
	 *   public static void Main() {
	 *     bool is_running = true;
	 * 
	 *     while (is_running) {
	 *       
	 *     }
	 *   }
	 * }
	 */
    public void Compile()
    {
        GenStmt(_stmt);


        _il.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadLine"));

        _il.Emit(OpCodes.Ret);
        _typeBuilder.CreateType();
        _modb.CreateGlobalFunctions();
        _asmb.SetEntryPoint(_methb);
        _asmb.Save(_moduleName);
    }


    private void GenStmt(Stmt stmt)
    {
		Console.WriteLine("--> {0}", stmt.GetType().Name);
		if (stmt is Sequence)
		{
			pass++;
			var seq = (Sequence)stmt;
			Console.WriteLine("   --> {0}", seq.First.GetType().Name);
			Console.WriteLine("   --> {0}", seq.Second.GetType().Name);
			Console.WriteLine("PASS {0}", pass);
			Console.WriteLine("DOFIRST");
			GenStmt(seq.First);
			GenStmt(seq.Second);
		}

		/*else if (stmt is DeclareVar)
		{
			Console.WriteLine("CG: DeclareVar");
			// declare a local
			var declare = (DeclareVar)stmt;
			SymbolTable[declare.Ident] = _il.DeclareLocal(declare.Expr._GetType());

			// set the initial value
			var assign = new Assign { Ident = declare.Ident, Expr = declare.Expr };
			GenStmt(assign);
		}*/

		/*else if (stmt is Assign)
		{
			var assign = (Assign)stmt;
			GenerateLoadToStackForExpr(assign.Expr, assign.Expr._GetType());
			GenerateStoreFromStack(assign.Ident, assign.Expr._GetType());
		}*/
		else if (stmt is ClassDefinition)
		{
          	GenerateClass(stmt as ClassDefinition);
			Console.WriteLine("CG: GenerateClass - After Function");
		}
        /*else if (stmt is Print)
        {
            // the "print" statement is an alias for System.Console.WriteLine. 
            // it uses the string case
            GenerateLoadToStackForExpr(((Print)stmt).Expr, typeof(string));
            //Generate console.writeline
            _il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));
        }*/

        /*else if (stmt is ReadInt)
        {
            _il.Emit(OpCodes.Call,
                typeof(Console).GetMethod("ReadLine", BindingFlags.Public | BindingFlags.Static, null,
                    new Type[] { }, null));
            _il.Emit(OpCodes.Call,
                typeof(int).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null,
                    new[] { typeof(string) }, null));
            GenerateStoreFromStack(((ReadInt)stmt).Ident, typeof(int));
        }*/
        /*else if (stmt is ReadString)
        {
            _il.Emit(OpCodes.Call,
                typeof(Console).GetMethod("ReadLine", BindingFlags.Public | BindingFlags.Static, null,
                    new Type[] { }, null));
            GenerateStoreFromStack(((ReadString)stmt).Ident, typeof(string));
        }*/
        /*else if (stmt is ForLoop)
        {
            // example: 
            // for x = 0 to 100 do
            //   print "hello";
            // end;

            // x = 0
            var forLoop = (ForLoop)stmt;
            var assign = new Assign { Ident = forLoop.Ident, Expr = forLoop.From };
            GenStmt(assign);
            // jump to the test
            var test = _il.DefineLabel();
            _il.Emit(OpCodes.Br, test);

            // statements in the body of the for loop
            var body = _il.DefineLabel();
            _il.MarkLabel(body);
            GenStmt(forLoop.Body);

            // to (increment the value of x)
            _il.Emit(OpCodes.Ldloc, SymbolTable[forLoop.Ident]);
            _il.Emit(OpCodes.Ldc_I4, 1);
            _il.Emit(OpCodes.Add);
            GenerateStoreFromStack(forLoop.Ident, typeof(int));

            // **test** does x equal 100? (do the test)
            _il.MarkLabel(test);
            _il.Emit(OpCodes.Ldloc, SymbolTable[forLoop.Ident]);
            GenerateLoadToStackForExpr(forLoop.To, typeof(int));
            _il.Emit(OpCodes.Blt, body);
        }*/
        else
        {
			Console.WriteLine("We fucked up");
            //throw new Exception("don't know how to gen a " + stmt.GetType().Name);
        }
    }

	private void GenerateClass(ClassDefinition classDefinition)
	{
		Console.WriteLine("CG: GenerateClass");
// REF: https://msdn.microsoft.com/en-us/library/system.reflection.typeattributes(v=vs.110).aspx
		TypeBuilder tb = _modb.DefineType(classDefinition.name,
		                                  TypeAttributes.Public);
		tb.CreateType();

		Console.WriteLine("CG: GenerateClass - After tb.CreateType");

		/*foreach (Stmt stmt in classDefinition.body)
		{
			Console.WriteLine("CG: {0}", stmt.GetType());
		}*/
	}

    private void GenerateStoreFromStack(string name, Type type)
    {
		Console.WriteLine("CG: GenerateStoreFromStack");
        if (!SymbolTable.ContainsKey(name))
            throw new Exception("undeclared variable '" + name + "'");

        var locb = SymbolTable[name];
        var localType = locb.LocalType;
        
        if (localType != type)
            throw new Exception(string.Format("'{0}' is of type {1} but attempted to store value of type {2}", name,
                localType == null ? "<unknown>" : localType.Name, type.Name));

        _il.Emit(OpCodes.Stloc, SymbolTable[name]);
    }

    private void GenerateLoadToStackForExpr(Expr expr, Type expectedType)
    {
		Console.WriteLine("CG: GenerateLoadToStackForExpr");
        Type deliveredType;

        /*if (expr is StringLiteral)
        {
            deliveredType = typeof(string);
            _il.Emit(OpCodes.Ldstr, ((StringLiteral)expr).Value);
        }*/
        if (expr is IntLiteral)
        {
            deliveredType = typeof(int);
            _il.Emit(OpCodes.Ldc_I4, ((IntLiteral)expr).Value);
        }
        else if (expr is Variable)
        {
            var ident = ((Variable)expr).Ident;
            deliveredType = expr._GetType();

            if (!SymbolTable.ContainsKey(ident))
            {
                throw new Exception("undeclared variable '" + ident + "'");
            }

            _il.Emit(OpCodes.Ldloc, SymbolTable[ident]);
        }
        /*else if (expr is ArithExpr)
        {
            var arithExpr = (ArithExpr)expr;
            var left = arithExpr.Left;
            var right = arithExpr.Right;
            deliveredType = expr._GetType();

            GenerateLoadToStackForExpr(left, expectedType);
            GenerateLoadToStackForExpr(right, expectedType);
            switch (arithExpr.Op)
            {
                case ArithOp.Add:
                    _il.Emit(OpCodes.Add);
                    break;
                case ArithOp.Sub:
                    _il.Emit(OpCodes.Sub);
                    break;
                case ArithOp.Mul:
                    _il.Emit(OpCodes.Mul);
                    break;
                case ArithOp.Div:
                    _il.Emit(OpCodes.Div);
                    break;
                default:
                    throw new NotImplementedException("Don't know how to generate il load code for " + arithExpr.Op +
                                                      " yet!");
            }*/
        else
        {
            throw new Exception("don't know how to generate " + expr._GetType().Name);
        }

        if (deliveredType == expectedType) return;

        if (deliveredType != typeof (int) || expectedType != typeof (string))
            throw new Exception("can't coerce a " + deliveredType.Name + " to a " + expectedType.Name);

        _il.Emit(OpCodes.Box, typeof (int));
        _il.Emit(OpCodes.Callvirt, typeof (object).GetMethod("ToString"));
    }
}