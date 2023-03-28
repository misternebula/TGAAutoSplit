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

		if (!File.Exists(Path.Combine(managedFolder, "AutoSplitter.dll")))
		{
			File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "AutoSplitter.dll"), Path.Combine(managedFolder, "AutoSplitter.dll"));
		}

		if (!File.Exists(Path.Combine(managedFolder, "AutoSplitter.pdb")))
		{
			File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "AutoSplitter.pdb"), Path.Combine(managedFolder, "AutoSplitter.pdb"));
		}

		Console.WriteLine($"Installing LiveSplit.Server...");

		var livesplitFolder = Directory.GetParent(configFile.LivesplitExePath);
		var componentsFolder = Path.Combine(livesplitFolder.ToString(), "Components");

		if (!File.Exists(Path.Combine(componentsFolder, "LiveSplit.Server.dll")))
		{
			File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "LiveSplit.Server/LiveSplit.Server.dll"), Path.Combine(componentsFolder, "LiveSplit.Server.dll"));
		}

		if (!File.Exists(Path.Combine(componentsFolder, "Noesis.Javascript.dll")))
		{
			File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "LiveSplit.Server/Noesis.Javascript.dll"), Path.Combine(componentsFolder, "Noesis.Javascript.dll"));
		}

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
			Console.WriteLine($"- Assembly is already patched.");
			return;
		}

		if (patchedInstructions.Count > 1)
		{
			Console.WriteLine($"Removing corrupted patch from Manager_SetSettingsFirstTime.Start.");
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
