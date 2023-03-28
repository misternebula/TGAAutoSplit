using dnlib.DotNet.Emit;
using dnpatch;
using Newtonsoft.Json;
using System.Diagnostics;

namespace TGAAutoSplit;

public class ConfigFile
{
	[JsonProperty("gameExePath")]
	public string GameExePath;

	[JsonProperty("livesplitExePath")]
	public string LivesplitExePath;
}

internal class Program
{
	static void Main()
	{
		var configFile = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText("config.json"));

		var gameFolder = Directory.GetParent(configFile.GameExePath);
		var managedFolder = Path.Combine(gameFolder.ToString(), @"The Glitched Attraction_Data\Managed");
		var assemblyPath = Path.Combine(managedFolder, "Assembly-CSharp.dll");

		Console.WriteLine($"Copying AutoSplitter mod to game folder...");

		try
		{
			File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "AutoSplitter.dll"), Path.Combine(managedFolder, "AutoSplitter.dll"), true);
			File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "AutoSplitter.pdb"), Path.Combine(managedFolder, "AutoSplitter.pdb"), true);
		}
		catch (IOException ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"- Failed to install mod. (The game is probably running?) : {ex.Message}");
			Console.ResetColor();
			Console.WriteLine($"Press any key to exit.");
			Console.ReadKey();
			return;
		}

		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("- Done.");
		Console.ResetColor();

		Console.WriteLine($"Installing LiveSplit.Server...");

		var livesplitFolder = Directory.GetParent(configFile.LivesplitExePath);
		var componentsFolder = Path.Combine(livesplitFolder.ToString(), "Components");

		try
		{
			File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "LiveSplit.Server/LiveSplit.Server.dll"), Path.Combine(componentsFolder, "LiveSplit.Server.dll"), true);
			File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "LiveSplit.Server/Noesis.Javascript.dll"), Path.Combine(componentsFolder, "Noesis.Javascript.dll"), true);
		}
		catch (IOException ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"- Failed to install livesplit plugin. (LiveSplit is probably running?) Error : {ex.Message}");
			Console.ResetColor();
			Console.WriteLine($"Press any key to exit.");
			Console.ReadKey();
			return;
		}

		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("- Done.");
		Console.ResetColor();

		Console.WriteLine($"Patching game assembly...");

		// patcher code modified from OWML my beloved

		var patcher = new dnpatch.Patcher(assemblyPath, true);

		var target = new Target
		{
			Class = "Manager_SetSettingsFirstTime",
			Method = "Start"
		};

		var instructions = patcher.GetInstructions(target).ToList();
		var patchedInstructions = GetPatchedInstructions(instructions);

		if (patchedInstructions.Count == 1)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("- Assembly already patched.");
			Console.ResetColor();
		}
		else
		{
			if (patchedInstructions.Count > 1)
			{
				Console.WriteLine($"- Removing corrupted patch(es).");
				foreach (var patchedInstruction in patchedInstructions)
				{
					instructions.Remove(patchedInstruction);
				}
			}

			var newInstruction = Instruction.Create(OpCodes.Call, patcher.BuildCall(typeof(AutoSplitter.EntryPoint), "LoadMod", typeof(void), new Type[] { }));
			instructions.Insert(0, newInstruction);

			target.Instructions = instructions.ToArray();

			Patch(patcher, target);
			Save(patcher);

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("- Done.");
			Console.ResetColor();
		}

		// keep console open
		Console.WriteLine($"Press any key to exit.");
		Console.ReadKey();
	}

	private static void Patch(dnpatch.Patcher patcher, Target target)
	{
		try
		{
			patcher.Patch(target);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error while patching: {ex}");
			throw;
		}
	}

	private static void Save(dnpatch.Patcher patcher)
	{
		try
		{
			patcher.Save(true);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error while saving patched game assembly: {ex}");
			throw;
		}
	}

	private static List<Instruction> GetPatchedInstructions(List<Instruction> instructions) =>
			instructions.Where(x => x.Operand != null && x.Operand.ToString().Contains(nameof(AutoSplitter.EntryPoint))).ToList();
}
