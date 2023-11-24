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
	internal enum InstructionCondition
	{
		IF_GREATER,
		IF_LESSER,
		IF_EQUAL,
		IF_NOT_EQUAL,
		IF_GREATER_EQUAL,
		IF_LESSER_EQUAL,
	}

	internal struct TokenCondition
	{
		public string Operand1;
		public InstructionCondition ConditionOperator;
		public string Operand2;
	}

	internal struct TokenStatement
	{
		public TokenInstruction? Instruction { get; internal set; }
		public TokenBlock? InnerBlock { get; internal set; }

		public TokenCondition? Condition { get; internal set; }

		public TokenStatement(TokenInstruction _instruction)
		{
			Instruction = _instruction;
		}
		public TokenStatement(TokenBlock _innerBlock)
		{
			InnerBlock = _innerBlock;
		}
		public TokenStatement(TokenBlock _innerBlock, TokenCondition _condition)
		{
			InnerBlock = _innerBlock;
			Condition = _condition;
		}
	}

	internal struct TokenInstruction
	{
		public OperationCode opcode { get; internal set; }
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
						tokens[i] = tokens[i].Replace('(', '\x0000');
						tokens[i] = tokens[i].Replace(')', '\x0000');

						string[] SplitToken = tokens[i].Split(" ");
						foreach (string token in SplitToken)
						{
							if (token != "" || token != "\x0000")
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

				if (tokens[0].StartsWith("//"))
				{
					CodeLine = CodeFile.ReadLine();
					continue;
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

			List<TokenFunctionDefinition> Functions;
			
			for (int ParseLinesIterator = 0; ParseLinesIterator < ParseLines.Count; ParseLinesIterator++)
			{
				for (int i2 = 0; i2 < ParseLines[ParseLinesIterator].Length; i2++)
				{
					Console.WriteLine(ParseLines[ParseLinesIterator][i2].ToString());
				}
				Console.WriteLine("- - -");
			}

			GenerateTokenTree(ParseLines, out Functions);

			PrintTokenTree(Functions);
		}

		private static void GenerateTokenTree(List<string[]> Tokens,out List<TokenFunctionDefinition> Functions)
		{
			Functions = new();

			Stack<TokenBlock> TokenBlocks = new();
			Stack<TokenCondition> ConditionsStack = new();

			for (int i = 0; i < Tokens.Count; i++)
			{
				switch (Tokens[i][0])
				{
					case "Main":
						TokenFunctionDefinition TKFuncMain = new();
						TKFuncMain.Name = "Main";
						TKFuncMain.parameters = null;
						Functions.Add( TKFuncMain );
						break;
					case "func":
						TokenFunctionDefinition TKFuncDef = new();
						TKFuncDef.Name = Tokens[i][1];

						List<TokenFunctionParameter> FunctionParams = new();

						for (int FuncParamIterator = 2; FuncParamIterator < Tokens[i].Length; FuncParamIterator++)
						{
							var Param = new TokenFunctionParameter();
							Param.Name = Tokens[i][FuncParamIterator];
							FunctionParams.Add(Param);
						}
						TKFuncDef.parameters = FunctionParams.ToArray();
						Functions.Add(TKFuncDef);
						break;
					case "if":
						TokenCondition condition = new();

						condition.Operand1 = Tokens[i][1];
						condition.ConditionOperator = convertToCondition(Tokens[i][2]);
						condition.Operand2 = Tokens[i][3];

						ConditionsStack.Push(condition);
						break;
					case "{":
						TokenBlocks.Push( new TokenBlock() );
						break;
					case "}":
						if (TokenBlocks.Count < 1) throw new Exception("Cannot close innexistant codeblock");

						if (TokenBlocks.Count == 1)
						{
							TokenBlock FuncBlock = TokenBlocks.Pop();
							var func = Functions[Functions.Count - 1];
							func.Block = FuncBlock;
							Functions[Functions.Count - 1] = func;
						} 
						else if ( TokenBlocks.Count > 1)
						{
							TokenBlock InnerBlock = TokenBlocks.Pop();
							TokenStatement InnerBlockStatement;
							TokenBlock CurrentBlock = TokenBlocks.Pop();

							InnerBlockStatement = CurrentBlock.Statements[CurrentBlock.Statements.Count - 1];

							InnerBlockStatement.InnerBlock = InnerBlock;

							CurrentBlock.Statements[CurrentBlock.Statements.Count - 1] = InnerBlockStatement;
							
							if (ConditionsStack.Count > 0)
							{
								TokenCondition Cond = ConditionsStack.Pop();
								InnerBlockStatement.Condition = Cond;
							}

							TokenBlocks.Push( CurrentBlock );
						}
						break;
					default:
						if (TokenBlocks.Count < 1) throw new Exception("Cannot parse expression outside code block");
						TokenStatement Statement = new();
						TokenInstruction Instruction = new();

						OperationCode OpCode;
						Enum.TryParse(Tokens[i][0], out OpCode);

						Instruction.opcode = OpCode;

						Console.WriteLine(OpCode.ToString());

						List<string> InstructionParams = new();

						for (int i2 = 1; i2 < Tokens[i].Length; i2++)
						{
							InstructionParams.Add(Tokens[i][i2]);
						}
						Instruction.parameters = InstructionParams.ToArray();
						Statement.Instruction = Instruction;
						TokenBlocks.Peek().Statements.Add(Statement);
						break;
				}
				PrintTokenTree(Functions);
			}
		}

		private static InstructionCondition convertToCondition(string cond)
		{
			switch (cond)
			{
				case "==":
					return InstructionCondition.IF_EQUAL;
				case "!=":
					return InstructionCondition.IF_NOT_EQUAL;
				case ">":
					return InstructionCondition.IF_GREATER;
				case "<":
					return InstructionCondition.IF_LESSER;
				case ">=":
					return InstructionCondition.IF_LESSER_EQUAL;
				case "<=":
					return InstructionCondition.IF_GREATER_EQUAL;
				default:
					throw new Exception("No matching condition for "+cond);
			}
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
						if (Statement.Condition != null)
						{

						}
						Console.WriteLine(" InnerBlock : ");
						PrintInnerBlock(Statement.InnerBlock);
					}
				}
			}
		}
		private static void PrintInnerBlock(TokenBlock? Block)
		{
			if (Block == null) return;

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
	}
}
