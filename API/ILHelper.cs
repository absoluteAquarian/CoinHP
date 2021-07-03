using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.OS;
using System;
using System.IO;

namespace CoinHP.API{
	public static class ILHelper{
		private static void PrepareInstruction(Instruction instr, out string offset, out string opcode, out string operand){
			offset = $"IL_{instr.Offset :X4}:";

			opcode = instr.OpCode.Name;
			
			if(instr.Operand is null)
				operand = "";
			else if(instr.Operand is ILLabel label)
				operand = $"IL_{label.Target.Offset :X4}";
			else
				operand = instr.Operand.ToString();
		}

		public static void CompleteLog(ILCursor c, bool beforeEdit = false){
			if(!EditsLoader.LogILEdits)
				return;

			int index = 0;

			//Get the method name
			string method = c.Method.Name;
			if(!method.Contains("ctor"))
				method = method.Substring(method.LastIndexOf(':') + 1);
			else
				method = method.Substring(method.LastIndexOf('.'));

			if(beforeEdit)
				method += " - Before";
			else
				method += " - After";

			//And the storage path
			string path = Platform.Current.GetStoragePath();
			path = Path.Combine(path, "Terraria", "ModLoader", "CoinHP");
			Directory.CreateDirectory(path);

			//Get the class name
			string type = c.Method.Name;
			type = type.Substring(0, type.IndexOf(':'));
			type = type.Substring(type.LastIndexOf('.') + 1);

			FileStream file = File.Open(Path.Combine(path, $"{type}.{method}.txt"), FileMode.Create);
			using(StreamWriter writer = new StreamWriter(file)){
				writer.WriteLine(DateTime.Now.ToString("'['ddMMMyyyy '-' HH:mm:ss']'"));
				writer.WriteLine($"// ILCursor: {c.Method}");
				do{
					PrepareInstruction(c.Instrs[index], out string offset, out string opcode, out string operand);
					
					writer.WriteLine($"{offset, -10}{opcode}   {operand}");
					index++;
				}while(index < c.Instrs.Count);
			}
		}

		public static void UpdateInstructionOffsets(ILCursor c){
			if(!EditsLoader.LogILEdits)
				return;

			var instrs = c.Instrs;
			int curOffset = 0;

			Instruction[] ConvertToInstructions(ILLabel[] labels){
				Instruction[] ret = new Instruction[labels.Length];
				for(int i = 0; i < labels.Length; i++)
					ret[i] = labels[i].Target;
				return ret;
			}

			foreach(var ins in instrs){
				ins.Offset = curOffset;

				if(ins.OpCode != OpCodes.Switch)
					curOffset += ins.GetSize();
				else{
					//'switch' opcodes don't like having the operand as an ILLabel[] when calling GetSize()
					//thus, this is required to even let the mod compile

					Instruction copy = Instruction.Create(ins.OpCode, ConvertToInstructions((ILLabel[])ins.Operand));
					curOffset += copy.GetSize();
				}
			}
		}

		public static void InitMonoModDumps(){
			if(!EditsLoader.LogILEdits)
				return;

			Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE","Auto");
			Environment.SetEnvironmentVariable("MONOMOD_DMD_DEBUG","1");

			string dumpDir = Path.GetFullPath("MonoModDump");

			Directory.CreateDirectory(dumpDir);

			Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP",dumpDir);
		}

		public static void DeInitMonoModDumps(){
			if(!EditsLoader.LogILEdits)
				return;

			Environment.SetEnvironmentVariable("MONOMOD_DMD_DEBUG","0");
		}

		public static string GetInstructionString(ILCursor c, int index){
			if(index < 0 || index >= c.Instrs.Count)
				return "ERROR: Index out of bounds.";

			PrepareInstruction(c.Instrs[index], out string offset, out string opcode, out string operand);

			return $"{offset} {opcode}   {operand}";
		}
	}
}
