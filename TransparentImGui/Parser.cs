using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ConLangInterpreter
{
	internal struct TokenStatement
	{
		public TokenInstruction? Instruction { get; internal set; }
        public TokenInnerBlock? InnerBlock { get; internal set; }

		public TokenStatement(TokenInstruction _instruction)
		{
            Instruction = _instruction;
        }
        public TokenStatement(TokenInnerBlock _innerBlock)
        {
            InnerBlock = _innerBlock;
        }
    }

    internal struct TokenInstruction
	{
		public string opcode { get; internal set; }
		public string[] parameters { get; internal set; }
    }
    internal struct TokenVariable
	{
        public string Name { get; internal set; }
    }
    internal struct TokenBlock
	{
		public List<TokenStatement> Statements { get; internal set; }

        public TokenBlock()
		{
            Statements = new();
        }
    }
    internal struct TokenInnerBlock
    {
        public List<TokenStatement> Statements { get; internal set; }

        public TokenInnerBlock()
        {
            Statements = new();
        }
    }
    internal struct TokenFunctionParameter
	{
		public string Name { get; internal set; }
	}
	internal struct TokenFunctionDefinition
	{
		public string Name { get; internal set; }
		public TokenFunctionParameter[]? parameters { get; internal set; }
        public TokenBlock Block { get; internal set; }

        public TokenFunctionDefinition()
        {
            Block = new TokenBlock();
        }
    }

	internal static class Parser
	{
		public static void Parse(StreamReader CodeFile)
		{
			string? CodeLine = CodeFile.ReadLine();

            Dictionary<string, string> Aliases = new();
			List<string[]> ParseLines = new();

			while (CodeLine != null ) 
			{

				string[] tokens = CodeLine.Split(" ");

				for (int i = 0; i < tokens.Length; i++)
				{
                    tokens[i] = tokens[i].TrimStart();
                    tokens[i] = tokens[i].TrimEnd();
				}

                if (CodeLine == "")
                {
                    CodeLine = CodeFile.ReadLine();
                    continue;
                }

                if (tokens[0] == "#alias")
				{
					if (tokens[1] == "#alias") throw new InvalidOperationException("Cannot Alias '#alias'");
					Aliases.Add(tokens[1], tokens[2]);
					Console.WriteLine("Aliased " + tokens[1] + " as " + tokens[2]);
                    CodeLine = CodeFile.ReadLine();
                    continue;
                }

                if (tokens[0] == "func")
                {
					List<string> NewTokens = new();
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        tokens[i] = tokens[i].Replace(',', '\x0000');
                        tokens[i] = tokens[i].Replace('(', ' ');
                        tokens[i] = tokens[i].Replace(')', ' ');

						string[] SplitToken = tokens[i].Split(" ");
						foreach (string token in SplitToken)
						{
							if (token.TrimEnd() != "")
								NewTokens.Add(token.TrimEnd());
                        }
                    }
                    tokens = NewTokens.ToArray();
                }

                if (tokens[0] == "if")
                {
                    List<string> NewTokens = new();
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        tokens[i] = tokens[i].Replace(',', '\x0000');
                        tokens[i] = tokens[i].Replace('(', ' ');
                        tokens[i] = tokens[i].Replace(')', ' ');

                        string[] SplitToken = tokens[i].Split(" ");
                        foreach (string token in SplitToken)
                        {
                            if (token.TrimEnd() != "")
                                Console.WriteLine(token);
                                NewTokens.Add(token.TrimEnd());
                        }
                    }
                    tokens = NewTokens.ToArray();
                }

                foreach (KeyValuePair<string, string> alias in Aliases)
				{
					if (tokens[0] == alias.Value)
					{
						tokens[0] = alias.Key;
					}
				}

				List<string> TokensFinal = new();

				foreach (string token in tokens)
                {
					if (token.StartsWith("//"))
					{
						break;
					}

                    TokensFinal.Add(token);
                }

                ParseLines.Add(TokensFinal.ToArray());
				CodeLine = CodeFile.ReadLine();
			}

			Console.WriteLine("\n- Parsing -\n");

			List<TokenFunctionDefinition> Functions = new();

			Stack<TokenBlock> TokenBlocks = new();

			bool MainBlockStartFound = false;
			int OpenInnerBlocks = 0;
			
			for (int ParseLinesIterator = 0; ParseLinesIterator < ParseLines.Count; ParseLinesIterator++)
			{
                for (int i2 = 0; i2 < ParseLines[ParseLinesIterator].Length; i2++)
				{
					Console.WriteLine(ParseLines[ParseLinesIterator][i2].ToString());
                }
                Console.WriteLine("- - -");

                switch (ParseLines[ParseLinesIterator][0])
                {
                    case "Main":
						if (MainBlockStartFound) throw new Exception("Cannot have more than 1 main block.");

                        TokenFunctionDefinition Main = new TokenFunctionDefinition();

						Main.Name = "Main";
						Main.parameters = null;

                        
						Functions.Add(Main);
                        break;
                    case "{":
						if (!MainBlockStartFound)
							MainBlockStartFound = true;


                        TokenBlock Block = new();
                            
                        TokenBlocks.Push(Block);
                        break;
                    case "}":
						if (OpenInnerBlocks > 0)
						{
                            int index = Functions.Count - 1;
                            TokenFunctionDefinition Function = Functions[index];
                            TokenBlock TryPopResult;

                            if (TokenBlocks.TryPop(out TryPopResult) == false) throw new Exception("Tried to close inexistant codeblock (While parsing line \"" + (ParseLinesIterator + 1).ToString() + "\")\"");

							TokenInnerBlock innerBlock = new();

							foreach (TokenStatement Statement in TryPopResult.Statements)
							{
                                innerBlock.Statements.Add(Statement);
                            }

                            Function.Block.Statements.Add(new TokenStatement(innerBlock));

                            Functions[index] = Function;

                            OpenInnerBlocks -= 1;
                        }
						else
						{
                            int index = Functions.Count - 1;
                            TokenFunctionDefinition Function = Functions[index];
                            TokenBlock TryPopResult;
                            if (TokenBlocks.TryPop(out TryPopResult) == false) throw new Exception("Tried to close inexistant codeblock (While parsing line \"" + (ParseLinesIterator + 1).ToString() + "\")\"");
                            
                            foreach (TokenStatement Statement in TryPopResult.Statements)
                            {
                                Function.Block.Statements.Add(Statement);
                            }
                            
                            Function.Block = TryPopResult;

                            Functions[index] = Function;
                        }
                        break;
					case "":
						break;
					case "func":
                        TokenFunctionDefinition functionDef = new TokenFunctionDefinition();

                        functionDef.Name = ParseLines[ParseLinesIterator][1];

						List<TokenFunctionParameter> FunctionParameters = new();
						for (int functionParamsIterator = 2; functionParamsIterator < ParseLines[ParseLinesIterator].Length; functionParamsIterator++)
						{
							TokenFunctionParameter Parameter = new();
							Parameter.Name = ParseLines[ParseLinesIterator][functionParamsIterator];
                            FunctionParameters.Add(Parameter);
                        }
                        functionDef.parameters = FunctionParameters.ToArray();


                        Functions.Add(functionDef);
                        break;
                    case "if":
                        OpenInnerBlocks += 1;

                        break;
                    default:
						string OpCode = ParseLines[ParseLinesIterator][0];
						List<string> parameters = new();
						for (int i3 = 1; i3 < ParseLines[ParseLinesIterator].Length; i3++)
						{
							parameters.Add(ParseLines[ParseLinesIterator][i3]);
						}

						TokenBlock Try;

                        if (TokenBlocks.TryPeek(out Try)) 
						{
                            TokenInstruction instruction = new();
                            instruction.opcode = OpCode;
                            instruction.parameters = parameters.ToArray();
                            Try.Statements.Add(new TokenStatement(instruction));
						} 
						else
						{
							throw new Exception("Expected statements inside codeblock, got statements outside codeblock. (While parsing line " + (ParseLinesIterator + 1).ToString() + ")");
						}

						break;
                }
                
            }
            PrintTokenTree(Functions);
        }

		private static void PrintTokenTree(List<TokenFunctionDefinition> FunctionNodes)
		{
			Console.WriteLine("Printing generated tree...");

			foreach(TokenFunctionDefinition functionDef in FunctionNodes)
			{;
                Console.WriteLine(" Name : " + functionDef.Name);
				if (functionDef.parameters != null)
				{
                    Console.WriteLine(" Parameters : ");
                    foreach (TokenFunctionParameter functionParam in functionDef.parameters)
                    {
                        Console.WriteLine(functionParam.Name);
                    }
                }
                Console.WriteLine(" TokenBlock : ");
                foreach (TokenStatement Statement in functionDef.Block.Statements)
                {
                    if (Statement.Instruction != null)
                    {
                        Console.WriteLine(" Instruction : ");
                        Console.WriteLine(Statement.Instruction.Value.opcode);
                        Console.WriteLine(" Parameters : ");
                        foreach (string Parameter in Statement.Instruction.Value.parameters)
                        {
                            Console.WriteLine(Parameter);
                        }
                    }
                    if (Statement.InnerBlock != null)
                    {
                        Console.WriteLine(" InnerBlock : ");
                        PrintInnerBlock(Statement.InnerBlock);
                    }
                }
            }
		}
        private static void PrintInnerBlock(TokenInnerBlock? Block)
        {
            foreach (TokenStatement statement in Block.Value.Statements)
            {
                if (statement.Instruction != null)
                {
                    Console.WriteLine("Instruction : ");
                    Console.WriteLine(statement.Instruction.Value.opcode);
                    Console.WriteLine("Parameters : ");
                    foreach (string Parameter in statement.Instruction.Value.parameters)
                    {
                        Console.WriteLine(Parameter);
                    }
                }
                if (statement.InnerBlock != null)
                {
                    Console.WriteLine(" InnerBlock : ");
                    PrintInnerBlock(statement.InnerBlock);
                }
            }
        }

		private static int AllocateSymbolMemory(List<UInt16> UsedMemory)
		{
			throw new NotImplementedException();
		}
    }
}
