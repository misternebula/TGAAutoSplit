using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoSplitter
{
    public class EntryPoint
    {
        public static Socket socket;
        public const string ConfigFile = @"C:\Users\Henry\Downloads\the-glitched-attraction\The Glitched Attraction\log.txt";

		public static void LoadMod()
        {
            File.Delete(ConfigFile);
            File.WriteAllText(ConfigFile, "Mod Loaded!");

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ipAddress = IPAddress.Parse("127.0.0.1");
            var endPoint = new IPEndPoint(ipAddress, 16834);

            try
            {
				socket.Connect(endPoint);
			}
            catch
            {
                return;
            }

            var gm = new GameObject("AUTO SPLITTER");
            gm.AddComponent<AutoSplitterMod>();
            UnityEngine.Object.DontDestroyOnLoad(gm);
		}
    }
}
