using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConLangInterpreter;
using TransparentImGui;

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
					Instruction Instruction;
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
					}
				}
			}
		}
		private static OperationParameter ConvertToParameter(string paramstring)
		{
			OperationParameter parameter = new();
			switch (paramstring[0])
			{
				case '&': //register
					switch(paramstring.Substring(1))
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
							throw new Exception("Innexistan register : Expected avalue 'A','B','C' or 'D', got '"+ paramstring.Substring(1)+"'.");
					}
					break;
				case '$': //adress
					break;
				case '#': //value litteral
					break;
				case '@': //callable
					break;
			}
			return parameter;
		}
	}
}
