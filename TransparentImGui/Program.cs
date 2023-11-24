using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ConLangInterpreter
{

	public enum OperationCode
	{
		LDAC, // Load Accumulator.
		LDA, // Load Register (A->D)
		LDB,
		LDC,
		LDD,

		STAC, // Store Accumulator
		STA, // Store Register (A->D)
		STB,
		STC,
		STD,

		SUB,
		ADD,
		MULT,
		DIV,

		JMP,
		JMP_IF_GREATER,
		JMP_IF_LESSER,
		JMP_IF_EQUAL,
		JMP_IF_NOT_EQUAL,
		JMP_IF_GREATER_EQUAL,
		JMP_IF_LESSER_EQUAL,

		STACK_PUSH,
		STACK_POP,
		STACK_PEEK,

		PRNT,

		PRG_END
	}

	public struct Instruction
	{
		public readonly OperationCode OpCode;
		public readonly OperationParameter? Param1;
		public readonly OperationParameter? Param2;
		public readonly OperationParameter? Param3;
		public readonly OperationParameter? Param4;

		public Instruction(OperationCode _OpCode)
		{
			this.OpCode = _OpCode;
			this.Param1 = null;
			this.Param2 = null;
			this.Param3 = null;
			this.Param4 = null;
		}
		public Instruction(OperationCode _OpCode, OperationParameter? _Param1)
		{
			this.OpCode = _OpCode;
			this.Param1 = _Param1;
			this.Param2 = null;
			this.Param3 = null;
			this.Param4 = null;
		}
		public Instruction(OperationCode _OpCode, OperationParameter? _Param1, OperationParameter? _Param2)
		{
			this.OpCode = _OpCode;
			this.Param1 = _Param1;
			this.Param2 = _Param2;
			this.Param3 = null;
			this.Param4 = null;
		}
		public Instruction(OperationCode _OpCode, OperationParameter? _Param1, OperationParameter? _Param2, OperationParameter? _Param3)
		{
			this.OpCode = _OpCode;
			this.Param1 = _Param1;
			this.Param2 = _Param2;
			this.Param3 = _Param3;
			this.Param4 = null;
		}
		public Instruction(OperationCode _OpCode, OperationParameter? _Param1, OperationParameter? _Param2, OperationParameter? _Param3, OperationParameter? _Param4)
		{
			this.OpCode = _OpCode;
			this.Param1 = _Param1;
			this.Param2 = _Param2;
			this.Param3 = _Param3;
			this.Param4 = _Param4;
		}
	}

	public enum OpParamType
	{
		Value,
		Address,
		Register,
		Accumulator,
		INT_Symbol
	}
	public enum RegisterSelect
	{
		Accumulator,
		RegisterA,
		RegisterB,
		RegisterC,
		RegisterD,
	}

	public struct OperationParameter
	{
		public OpParamType Type;

		public UInt16? Value;
		public UInt16? Address;
		public UInt16? Symbol;
		public RegisterSelect? Register;

		public OperationParameter(OpParamType _Type, UInt16 _Value)
		{
			this.Value = null;
			this.Register = null;

			switch (_Type)
			{
				case OpParamType.Value:
					this.Value = _Value;
					this.Type = _Type; 
					break;
				case OpParamType.Address:
					this.Address = _Value;
					this.Type = _Type;
					break;
				default:
					Interpreter.Error(new Exception("Tried creating invalid instruction parameter."));
					break;
			}
		}
		public OperationParameter(RegisterSelect _Register)
		{
			this.Value = null;
			this.Register = null;

			this.Type = OpParamType.Register;
			this.Register = _Register;
		}
	}

	enum ProgramStatus
	{
		Stepping,
		Running,
		Finished
	}

	internal class Program
	{
		public List<Instruction> Instructions { get; private set;  }
		public int InstructionPointer { get; internal set; }

		public ProgramStatus Status { get; internal set; }

		public UInt16[] Memory { get; private set;  }
		public UInt16[] Registers { get; private set;  }
		public Stack<UInt16> Stack { get; private set; }

		public bool running = false;

		public Program(List<Instruction> _instructions)
		{
			Instructions = _instructions;
			InstructionPointer = 0;
			Init();

		}

		public Program(List<Instruction> _instructions, int pointerStart)
		{
			Instructions = _instructions;
			InstructionPointer = pointerStart;
			Init();
		}

		private void Init()
		{
			Memory = new UInt16[UInt16.MaxValue];

			Registers = new ushort[5];
			Console.WriteLine("Ready");
		}

		internal void Reset()
		{
			running = false;

			for (int i = 0; i < Memory.Length; i++)
			{
				Memory[i] = 0;
			}

			for (int i = 0; i < Registers.Length; i++)
			{
				Registers[i] = 0;
			}

			InstructionPointer = 0;
			Status = ProgramStatus.Stepping;

			Console.WriteLine("Reset successfull");
		}

		UInt16 GetRawValue(OperationParameter parameter)
		{
			if (parameter.Type == OpParamType.Value)
			{
				if (parameter.Value == null) throw new Exception("instructions[instructionPointer].Parameter[?].Value is null");

				return (UInt16)parameter.Value;
			}
			else if (parameter.Type == OpParamType.Address)
			{
				if (parameter.Register == null) throw new Exception("instructions[instructionPointer].Parameter[?].Value is null");

				return Memory[Registers[(int)parameter.Value]];
			}
			else if (parameter.Type == OpParamType.Register)
			{
				if (parameter.Register == null) throw new Exception("instructions[instructionPointer].Parameter[?].Register is null");

				return Registers[(int)parameter.Register];
			} else
			{
				throw new Exception("instructions[instructionPointer].Parameter[?].Type is null");
			}
			
		}

		public void DumpMemory(UInt16 start, UInt16 end, bool dumpRegisters)
		{
			if (!dumpRegisters)
			{
				Console.WriteLine("- Register Dump -");

				Console.WriteLine("Accumulator .. " + Registers[(int)RegisterSelect.Accumulator]);

				Console.WriteLine("Register A .. " + Registers[(int)RegisterSelect.RegisterA]);
				Console.WriteLine("Register B .. " + Registers[(int)RegisterSelect.RegisterB]);
				Console.WriteLine("Register C .. " + Registers[(int)RegisterSelect.RegisterC]);
				Console.WriteLine("Register D .. " + Registers[(int)RegisterSelect.RegisterD]);
			}

			Console.WriteLine("- Memory Dump -");
			for (int i = start; i < end; i++) 
			{
				Console.Write(Memory[i].ToString() + " ");
				(int _, int Rem) = Math.DivRem(i, 20);
				if ( Rem == 0 ) { Console.Write("\n"); }

			}
			
			Console.Write("\n");
		}

		public bool GetIsFinished()
		{
			if (InstructionPointer >= Instructions.Count || Status == ProgramStatus.Finished)
			{
				Status = ProgramStatus.Finished;
				return true;
			}
			return false;
		}

		private protected void EndProgram()
		{
			running = false;
			InstructionPointer = 0;
			Status = ProgramStatus.Finished;
		}

		public void StepFrame()
		{
			if (InstructionPointer >= Instructions.Count)
			{
				running = false;
				return;
			}
				

			switch (Instructions[InstructionPointer].OpCode)
			{
				case OperationCode.LDA:
					Registers[(int)RegisterSelect.RegisterA] = GetRawValue((OperationParameter)Instructions[InstructionPointer].Param1);
					break;
				case OperationCode.STA:
					Memory[GetRawValue((OperationParameter)Instructions[InstructionPointer].Param1)] = Registers[(int)RegisterSelect.RegisterA];
					break;
				case OperationCode.LDB:
					Registers[(int)RegisterSelect.RegisterB] = Memory[GetRawValue((OperationParameter)Instructions[InstructionPointer].Param1)];
					break;
				case OperationCode.STB:
					Memory[GetRawValue((OperationParameter)Instructions[InstructionPointer].Param1)] = Registers[(int)RegisterSelect.RegisterB];
					break;
				case OperationCode.LDC:
					Registers[(int)RegisterSelect.RegisterC] = Memory[GetRawValue((OperationParameter)Instructions[InstructionPointer].Param1)];
					break;
				case OperationCode.STC:
					Memory[GetRawValue((OperationParameter)Instructions[InstructionPointer].Param1)] = Registers[(int)RegisterSelect.RegisterC];
					break;
				case OperationCode.LDD:
					Registers[(int)RegisterSelect.RegisterD] = Memory[GetRawValue((OperationParameter)Instructions[InstructionPointer].Param1)];
					break;
				case OperationCode.STD:
					Memory[GetRawValue((OperationParameter)Instructions[InstructionPointer].Param1)] = Registers[(int)RegisterSelect.RegisterD];
					break;
				case OperationCode.LDAC:
					Registers[(int)RegisterSelect.Accumulator] = Memory[GetRawValue((OperationParameter)Instructions[InstructionPointer].Param1)];
					break;
				case OperationCode.STAC:
					Memory[GetRawValue((OperationParameter)Instructions[InstructionPointer].Param1)] = Registers[(int)RegisterSelect.Accumulator];
					break;
				case OperationCode.ADD:
					Registers[(int)RegisterSelect.Accumulator] = (UInt16)(GetRawValue((OperationParameter)Instructions[InstructionPointer].Param1) + GetRawValue((OperationParameter)Instructions[InstructionPointer].Param2));
					break;
				case OperationCode.SUB:
					Registers[(int)RegisterSelect.Accumulator] = (UInt16)(GetRawValue((OperationParameter)Instructions[InstructionPointer].Param1) - GetRawValue((OperationParameter)Instructions[InstructionPointer].Param2));
					break;

				case OperationCode.PRG_END:
					EndProgram();
					break;
				default:
					Interpreter.Error(new NotImplementedException("OperationCode " + Instructions[InstructionPointer].OpCode.ToString() + " is not implemented.\nEmily if you're the one coding FIX THIS!\n"));
					running = false;
					break;
			}
			if (InstructionPointer < Instructions.Count)
				InstructionPointer++;
		}
	}
}
