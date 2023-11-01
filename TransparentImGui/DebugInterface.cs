using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace ConLangInterpreter
{
    
    internal class DebugInterface : Overlay
    {
        private readonly DebugInterface Interface;
        private Program PrgInstance = new(new List<Instruction> { new Instruction(OperationCode.ADD, new OperationParameter(OpParamType.Value, 2), new OperationParameter(OpParamType.Value, 5)), new Instruction(OperationCode.STAC, new OperationParameter(OpParamType.Value, 0)), new Instruction(OperationCode.LDA, new OperationParameter(RegisterSelect.Accumulator)) });

        bool open = true;
        public DebugInterface()
        {
            Interface = this;
            Interface.Start();
        }

        protected override void Render()
        {
            if (!open) { Interface.Close(); return; }
            if (open)
            {
                ImGui.Begin("Debug Tool", ref open, ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.Text("Debug interface");

                ImGui.Separator();

                ImGui.Text("Instruction .. " + PrgInstance.InstructionPointer);
                ImGui.Text(" Total instructions count .. " + PrgInstance.Instructions.Count);

                if (PrgInstance.GetIsFinished())
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
                    } catch (Exception Err)
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
