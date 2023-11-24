using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ClickableTransparentOverlay;
using ImGuiNET;
using Vortice.Win32;

namespace ConLangInterpreter
{
	internal static class Да
	{
		public static bool isOpen { get; internal set; } = false;
	}
	internal class DebugInterface : Overlay
	{
		private readonly DebugInterface Interface;
		private Program PrgInstance = new(new List<Instruction> { });

		bool open = true;
		bool FileDialogOpen = false;

		byte[] fileNameBuffer = new byte[128];
		public DebugInterface()
		{
			Interface = this;
			Interface.Start();
		}

		public void OpenFile(string filename)
		{
			try
			{
				string s = @"..\..\..\" + filename.TrimEnd().TrimStart();
				Console.WriteLine(s);
				Console.WriteLine(filename);
				StreamReader FileStream = new(s);
				Parser.Parse(FileStream);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
			
		}

		protected override void Render()
		{
			if (FileDialogOpen)
			{
				ImGui.Begin("File Selector", ref FileDialogOpen);

				ImGui.InputText("##SelectFile", fileNameBuffer, 128);

				if (ImGui.Button("Open File"))
				{
					FileDialogOpen = false;
					List<byte> ByteArr = new();
					foreach (byte v in fileNameBuffer)
					{
						if (v != 0)
						{
							ByteArr.Add(v);
						}
					}
					OpenFile(Encoding.Default.GetString(ByteArr.ToArray()));
				}

				ImGui.End();
			}
			if (!open) { Interface.Close(); return; }
			if (open)
			{
				ImGui.Begin("Interpreter", ref open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.MenuBar);

				if (ImGui.BeginMenuBar())
				{
					if (ImGui.MenuItem("Open..."))
					{
						FileDialogOpen = true;
					}
					ImGui.EndMenuBar();
				}


				ImGui.Text("Interpreter v1");

				ImGui.Separator();

				ImGui.Text("Instruction .. " + PrgInstance.InstructionPointer);
				ImGui.Text(" Total instructions count .. " + PrgInstance.Instructions.Count);

				if (PrgInstance.GetIsFinished() && !(PrgInstance.Instructions.Count == 0))
				{
					ImGui.Text("Program finished");
					ImGui.SameLine();
					if (ImGui.Button("Restart"))
					{
						PrgInstance.Reset();
					}
				}

				ImGui.Separator();

				ImGui.Text("Accumulator .. " + PrgInstance.Registers[(int)RegisterSelect.Accumulator]);
				ImGui.Spacing();
				ImGui.Text("Register A .. " + PrgInstance.Registers[(int)RegisterSelect.RegisterA]);
				ImGui.Text("Register B .. " + PrgInstance.Registers[(int)RegisterSelect.RegisterB]);
				ImGui.Text("Register C .. " + PrgInstance.Registers[(int)RegisterSelect.RegisterC]);
				ImGui.Text("Register D .. " + PrgInstance.Registers[(int)RegisterSelect.RegisterD]);

				ImGui.Separator();


				if (ImGui.Button("Dump Memory (0x0 -> 0xffff)"))
				{
					PrgInstance.DumpMemory(0, 0xffff, true);
				}

				if (ImGui.Button("Dump Memory (0x0 -> 0xff)"))
				{
					PrgInstance.DumpMemory(0, 0xff, true);
				}

				ImGui.Text("Step Forward");
				ImGui.SameLine();
				if (ImGui.ArrowButton("ArrowBtnTest", ImGuiDir.Right))
				{
					try
					{
						PrgInstance.StepFrame();
					}
					catch (Exception Err)
					{
						Interpreter.Error(Err);
					}
				}

				ImGui.Text("Running : " + PrgInstance.running);
				ImGui.SameLine();

				if (ImGui.Button("Toggle program state"))
				{
					PrgInstance.running = !PrgInstance.running;
				}

				if (PrgInstance.running)
				{
					try
					{
						PrgInstance.StepFrame();
					}
					catch (Exception Err)
					{
						PrgInstance.running = false;
						Interpreter.Error(Err);
					}
				}

				ImGui.End();
			}
		}
	}
}