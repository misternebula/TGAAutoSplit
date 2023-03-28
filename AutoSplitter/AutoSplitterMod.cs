using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoSplitter
{
	public class ConfigFile
	{
		[JsonProperty("debugMode")]
		public bool debugMode;
	}

	internal class AutoSplitterMod : MonoBehaviour
	{
		bool _isRunActive = false;

		bool _splitForFNAF4;
		bool _splitForFNAF2;
		bool _splitForFNAF3;
		bool _splitForFNAFSL;
		bool _splitForEnnard;
		bool _splitForBackAlley;
		bool _splitForEnd;

		bool debugMode;

		private static readonly GUIStyle guiGUIStyle = new GUIStyle();

		public void Start()
		{
			guiGUIStyle.fontSize = 12;

			var configFile = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText($"{Application.dataPath}/Managed/modConfig.json"));

			debugMode = configFile.debugMode;
		}

		public void OnGUI()
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}

			guiGUIStyle.normal.textColor = Color.white;
			GUI.contentColor = Color.white;

			if (SceneManager.GetActiveScene().name == "Main Menu")
			{
				GUI.Label(new Rect(20, 10, 100, 100), "Autosplitter mod V1.2.0, by _nebula.", guiGUIStyle);
			}

			if (debugMode)
			{
				guiGUIStyle.fontSize = 30;
				GUI.Label(new Rect(30, 30, 100, 100), "DEBUG MODE ENABLED", guiGUIStyle);
				guiGUIStyle.fontSize = 12;
			}
		}

		public void Update()
		{
			if (debugMode)
			{
				#region FNAF 4
				if (FindObjectOfType<nBB_AI1>() != null)
				{
					FindObjectOfType<nBB_AI1>().flashed = true;
					FindObjectOfType<nBB_AI1>().stade = 0;
				}

				if (FindObjectOfType<nightmarionne_AI1>() != null)
				{
					FindObjectOfType<nightmarionne_AI1>().stade = 0;
				}

				if (FindObjectOfType<nFredbear_AI1>() != null)
				{
					FindObjectOfType<nFredbear_AI1>().stade = 0;
				}

				if (FindObjectOfType<Freddles_AI1>() != null)
				{
					FindObjectOfType<Freddles_AI1>().how_easy = 99999;
					FindObjectOfType<Freddles_AI1>().playerOut = true;
				}
				#endregion
				#region FNAF 2
				if (FindObjectOfType<Mangle_AI1>() != null)
				{
					FindObjectOfType<Mangle_AI1>().enabled = false;
				}

				if (FindObjectOfType<Mangle_AI2>() != null)
				{
					FindObjectOfType<Mangle_AI2>().enabled = false;
				}

				if (FindObjectOfType<ToyChica_AI1>() != null)
				{
					FindObjectOfType<ToyChica_AI1>().enabled = false;
				}

				if (FindObjectOfType<WitheredFreddy_AI1>() != null)
				{
					FindObjectOfType<WitheredFreddy_AI1>().enabled = false;
				}
				#endregion
				#region FNAF3
				if (FindObjectOfType<Dreadbear_AI1>() != null)
				{
					FindObjectOfType<Dreadbear_AI1>().enabled = false;
					FindObjectOfType<Dreadbear_AI1>().stade = 1;
				}
				if (FindObjectOfType<Springtrap_AI1>() != null)
				{
					FindObjectOfType<Springtrap_AI1>().enabled = false;
					FindObjectOfType<Springtrap_AI1>().stade = 0;
				}
				if (FindObjectOfType<PhantomFoxy_AI1>() != null)
				{
					FindObjectOfType<PhantomFoxy_AI1>().enabled = false;
					FindObjectOfType<PhantomFoxy_AI1>().stade = 0;
				}
				if (FindObjectOfType<PhantomBalloonBoy_AI1>() != null)
				{
					FindObjectOfType<PhantomBalloonBoy_AI1>().enabled = false;
				}
				#endregion
				#region FNAF SL
				if (FindObjectOfType<FuntimeFoxy_AI1>() != null)
				{
					FindObjectOfType<FuntimeFoxy_AI1>().deactivated = true;
				}
				if (FindObjectOfType<FuntimeFreddy_AI1>() != null)
				{
					FindObjectOfType<FuntimeFreddy_AI1>().deactivated = true;
				}
				if (FindObjectOfType<Ballora_AI1>() != null)
				{
					FindObjectOfType<Ballora_AI1>().deactivated = true;
				}
				#endregion
				#region ENNARD
				if (FindObjectOfType<Ennard_AI>() != null)
				{
					FindObjectOfType<Ennard_AI>().enabled = false;
				}
				#endregion
				#region BACK ALLEY
				if (FindObjectOfType<Afton_AI>() != null)
				{
					FindObjectOfType<Afton_AI>().enabled = false;
					FindObjectOfType<Afton_AI>().stade = 0;
				}
				if (FindObjectOfType<ScrapBaby_AI>() != null)
				{
					FindObjectOfType<ScrapBaby_AI>().enabled = false;
					FindObjectOfType<ScrapBaby_AI>().stade = 0;
				}
				if (FindObjectOfType<MoltenFreddy_AI>() != null)
				{
					FindObjectOfType<MoltenFreddy_AI>().enabled = false;
					FindObjectOfType<MoltenFreddy_AI>().stade = 0;
				}
				#endregion
			}

			if (SceneManager.GetActiveScene().name == "Main Menu")
			{
				if (_isRunActive)
				{
					Reset();
				}

				_splitForFNAF4 = false;
				_splitForFNAF2 = false;
				_splitForFNAF3 = false;
				_splitForFNAFSL = false;
				_splitForEnnard = false;
				_splitForBackAlley = false;
				_splitForEnd = false;
				_isRunActive = false;
			}

			if (!_isRunActive)
			{
				var playerMove = FindObjectOfType<Player_Move>();
				if (playerMove != null)
				{
					if (playerMove.walking)
					{
						SendStartTimer();
						_isRunActive = true;
					}
				}

				return;
			}

			if (!_splitForFNAF4)
			{
				var deactivateRoom = FindObjectOfType<FNAF4_DeactivateRoom>();
				if (deactivateRoom != null && deactivateRoom.nFredbear_AI1.playerOut)
				{
					Split();
					_splitForFNAF4 = true;
				}
			}

			if (!_splitForFNAF2)
			{
				var deactivateRoom = FindObjectOfType<FNAF2_DeactivateRoom>();
				if (deactivateRoom != null && deactivateRoom.GetValue<bool>("on"))
				{
					Split();
					_splitForFNAF2 = true;
				}
			}

			if (!_splitForFNAF3)
			{
				var endCinematic = FindObjectOfType<FNAF3_EndCinematic>();
				if (endCinematic != null && endCinematic.startCinematic)
				{
					Split();
					_splitForFNAF3 = true;
				}
			}

			if (!_splitForFNAFSL)
			{
				var elevatorDoors = FindObjectOfType<FNAFSL_FinalButton>();
				if (elevatorDoors != null && elevatorDoors.GetValue<bool>("startSubtitles"))
				{
					Split();
					_splitForFNAFSL = true;
				}
			}

			if (!_splitForEnnard)
			{
				var cinematicManager = FindObjectOfType<Warehouse_FinalCinematicManager>();
				if (cinematicManager != null && (cinematicManager.leftCinematic.activeSelf || cinematicManager.rightCinematic.activeSelf))
				{
					Split();
					_splitForEnnard = true;
				}
			}

			if (!_splitForBackAlley)
			{
				var endEvents = FindObjectOfType<BackAlley_CinematicEnd>();
				if (endEvents != null && endEvents.cinematic.activeSelf)
				{
					Split();
					_splitForBackAlley = true;
				}
			}

			if (!_splitForEnd)
			{
				var finalTrigger = FindObjectOfType<Mind_FinalCinematicTrigger>();
				if (finalTrigger != null && finalTrigger.GetValue<GameObject>("cinematic").activeSelf)
				{
					Split();
					_splitForEnd = true;
				}
			}
		}

		public void SendStartTimer()
		{
			EntryPoint.socket.Send(Encoding.UTF8.GetBytes("starttimer\r\n"));
		}

		public void Split()
		{
			EntryPoint.socket.Send(Encoding.UTF8.GetBytes("split\r\n"));
		}

		public void Reset()
		{
			EntryPoint.socket.Send(Encoding.UTF8.GetBytes("reset\r\n"));
		}
	}
}