using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConLangInterpreter;
using TransparentImGui;
using Veldrid.OpenGLBinding;

namespace TransparentImGui
{
	internal static class CodeGenerator
	{
		static List<Instruction> result;
		public static List<Instruction> Generate(List<TokenFunctionDefinition> tokens)
		{
			List<Instruction> result = new List<Instruction>();
			foreach (var token in tokens)
			{
				GenerateRecursive(token.Block.Statements);
			}
			throw new NotImplementedException();
		}
		private static void GenerateRecursive(List<TokenStatement> tokens)
		{
			foreach (var token in tokens)
			{
				if (result == null) result = new();
				if (token.Instruction != null)
				{
					Instruction Instruction = new();
					if (token.Instruction.Value.parameters.Length > 4) throw new OverflowException("Expected up to 4 parameters, got "+ token.Instruction.Value.parameters.Length+".");
					
					string param1;
					string param2;
					string param3;
					string param4;

					switch (token.Instruction.Value.parameters.Length)
					{
						case 0:
							Instruction = new Instruction(token.Instruction.Value.opcode);
							break;
						case 1:
							param1 = token.Instruction.Value.parameters[0];
							Instruction = new Instruction(token.Instruction.Value.opcode, ConvertToParameter(param1));
							break;
						case 2:
							param1 = token.Instruction.Value.parameters[0];
							param2 = token.Instruction.Value.parameters[1];
							Instruction = new Instruction(token.Instruction.Value.opcode, ConvertToParameter(param1), ConvertToParameter(param2));
							break;
						case 3:
							param1 = token.Instruction.Value.parameters[0];
							param2 = token.Instruction.Value.parameters[1];
							param3 = token.Instruction.Value.parameters[2];
							Instruction = new Instruction(token.Instruction.Value.opcode, ConvertToParameter(param1), ConvertToParameter(param2), ConvertToParameter(param3));
							break;
						case 4:
							param1 = token.Instruction.Value.parameters[0];
							param2 = token.Instruction.Value.parameters[1];
							param3 = token.Instruction.Value.parameters[2];
							param4 = token.Instruction.Value.parameters[3];
							Instruction = new Instruction(token.Instruction.Value.opcode, ConvertToParameter(param1), ConvertToParameter(param2), ConvertToParameter(param3), ConvertToParameter(param4));
							break;
					}
					result.Add(Instruction);
				}
			}
		}
		private static OperationParameter ConvertToParameter(string paramstring)
		{
			OperationParameter parameter = new();
			switch (paramstring[0])
			{
				case '&': //register
					switch (paramstring.Substring(1))
					{
						case "A":
							parameter.Register = RegisterSelect.RegisterA;
							break;
						case "B":
							parameter.Register = RegisterSelect.RegisterA;
							break;
						case "C":
							parameter.Register = RegisterSelect.RegisterA;
							break;
						case "D":
							parameter.Register = RegisterSelect.RegisterA;
							break;
						default:
							throw new Exception("Innexistan register : Expected value 'A','B','C' or 'D', got '" + paramstring.Substring(1) + "'.");
					}
					break;
				case '$': //adress
					parameter.Address = ushort.Parse(paramstring.Substring(1));
					break;
				case '#': //value litteral
					parameter.Value = ushort.Parse(paramstring.Substring(1));
					break;
				default: throw new Exception("Invalid addressing statement: " + paramstring);
			}
			return parameter;
		}
	}
}
