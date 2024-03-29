﻿using ImGuiNET;
using ClickableTransparentOverlay;

namespace ConLangInterpreter
{
	internal class Interpreter
	{
		public Interpreter() 
		{
			_ = new DebugInterface();
		}

		public static void Error(Exception Err)
		{
			Console.WriteLine("Something went HORRIBLY wrong.");
			Console.WriteLine(Err.Message);
			if (Err.InnerException != null)
			{
				Error(Err.InnerException);
			}
		}

#pragma warning disable IDE0060 // Make Visual Studio forget that _args is not used
		static void Main(string[] _args)
#pragma warning restore IDE0060 // Restore the warning for other methods
		{
			_ = new Interpreter();
		}
	}
}