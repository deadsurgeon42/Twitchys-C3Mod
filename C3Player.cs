﻿using System;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;

namespace C3Mod
{
	public class C3Player
	{
		public int ChallengeNotifyCount = 0;
		public DateTime ChallengeTick = DateTime.UtcNow;
		public C3Player Challenging;
		public bool Dead = false;
		public int FFAScore = 0;
		public string GameType = "";
		public int Index;
		public C3Player KillingPlayer = null;
		public DateTime LastSpawn = DateTime.UtcNow;
		public int LivesUsed = 0;
		public bool SpawnProtectionEnabled = false;
		public bool Spectator = false;
		public int Team = 0;
		public Vector2[] tempflags = new Vector2[2];
		public Vector2[] tempspawns = new Vector2[2];

		public C3Player(int index)
		{
			Index = index;
		}

		public bool TerrariaDead
		{
			get { return Main.player[Index].dead; }
		}

		public int TerrariaTeam
		{
			get { return Main.player[Index].team; }
		}

		public string PlayerName
		{
			get { return Main.player[Index].name; }
		}

		public float tileX
		{
			get { return Main.player[Index].position.X / 16; }
		}

		public float tileY
		{
			get { return Main.player[Index].position.Y / 16; }
		}

		public TSPlayer TSPlayer
		{
			get { return C3Tools.GetTSPlayerByIndex(Index); }
		}

		public int ChallengeArena { get; set; }

		public void SendMessage(string message, Color color)
		{
			NetMessage.SendData((int) PacketTypes.ChatText, Index, -1, message, 255, color.R, color.G, color.B);
		}

		public void GiveItem(int type, string name, int width, int height, int stack)
		{
			TShock.Players[Index].GiveItem(type, name, width, height, stack);
		}
	}
}