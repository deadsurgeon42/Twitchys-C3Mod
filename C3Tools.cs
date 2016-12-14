using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;

namespace C3Mod
{
	public class C3Tools
	{
		internal static string C3ConfigPath
		{
			get { return Path.Combine(TShock.SavePath, "c3modconfig.json"); }
		}

		internal static void SetupConfig()
		{
			try
			{
				if (File.Exists(C3ConfigPath))
					C3Mod.C3Config = C3ConfigFile.Read(C3ConfigPath);
				C3Mod.C3Config.Write(C3ConfigPath);
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Error in config file");
				Console.ForegroundColor = ConsoleColor.Gray;
				TShock.Log.Error("Config Exception");
				TShock.Log.Error(ex.ToString());
			}
		}

		//Converted v2.2
		internal static string AssignTeam(C3Player who, string gametype)
		{
			switch (gametype)
			{
					//Converted v2.2

					#region CTF

				case "ctf":
				{
					if ((who.Team != 1) || (who.Team != 2))
					{
						var playerteam1 = 0;
						var playerteam2 = 0;

						foreach (var player in C3Mod.C3Players)
							if (player.Team == 1)
								playerteam1++;
							else if (player.Team == 2)
								playerteam2++;

						if (playerteam1 > playerteam2)
						{
							who.GameType = "ctf";
							who.Team = 2;
							switch (C3Mod.C3Config.TeamColor2)
							{
								case 1:
									return "Blue";
								case 2:
									return "Green";
								case 3:
									return "Blue";
								case 4:
									return "Yellow";
								case 5:
									return "Pink";
							}
						}
						else if (playerteam2 > playerteam1)
						{
							who.Team = 1;
							who.GameType = "ctf";

							switch (C3Mod.C3Config.TeamColor2)
							{
								case 1:
									return "Blue";
								case 2:
									return "Green";
								case 3:
									return "Blue";
								case 4:
									return "Yellow";
								case 5:
									return "Pink";
							}
						}
						else
						{
							var r = new Random();

							switch (r.Next(2) + 1)
							{
								case 1:
								{
									who.Team = 1;
									who.GameType = "ctf";
									switch (C3Mod.C3Config.TeamColor1)
									{
										case 1:
											return "Blue";
										case 2:
											return "Green";
										case 3:
											return "Blue";
										case 4:
											return "Yellow";
										case 5:
											return "Pink";
									}
									break;
								}
								case 2:
								{
									who.Team = 2;
									who.GameType = "ctf";
									switch (C3Mod.C3Config.TeamColor2)
									{
										case 1:
											return "Blue";
										case 2:
											return "Green";
										case 3:
											return "Blue";
										case 4:
											return "Yellow";
										case 5:
											return "Pink";
									}
									break;
								}
							}
						}
					}
					break;
				}

					#endregion CTF

					//Converted v2.2

					#region OneFlag

				case "oneflag":
				{
					if ((who.Team != 5) || (who.Team != 6))
					{
						var redteamplayers = 0;
						var blueteamplayers = 0;

						foreach (var player in C3Mod.C3Players)
							if (player.Team == 5)
								redteamplayers++;
							else if (player.Team == 6)
								blueteamplayers++;

						if (redteamplayers > blueteamplayers)
						{
							who.Team = 6;
							who.GameType = "oneflag";
							switch (C3Mod.C3Config.TeamColor2)
							{
								case 1:
									return "Blue";
								case 2:
									return "Green";
								case 3:
									return "Blue";
								case 4:
									return "Yellow";
								case 5:
									return "Pink";
							}
						}
						else if (blueteamplayers > redteamplayers)
						{
							who.Team = 5;
							who.GameType = "oneflag";
							switch (C3Mod.C3Config.TeamColor2)
							{
								case 1:
									return "Blue";
								case 2:
									return "Green";
								case 3:
									return "Blue";
								case 4:
									return "Yellow";
								case 5:
									return "Pink";
							}
						}
						else
						{
							var r = new Random();

							switch (r.Next(2) + 1)
							{
								case 1:
								{
									who.Team = 5;
									who.GameType = "oneflag";
									switch (C3Mod.C3Config.TeamColor1)
									{
										case 1:
											return "Blue";
										case 2:
											return "Green";
										case 3:
											return "Blue";
										case 4:
											return "Yellow";
										case 5:
											return "Pink";
									}
									break;
								}
								case 2:
								{
									who.Team = 6;
									who.GameType = "oneflag";
									switch (C3Mod.C3Config.TeamColor2)
									{
										case 1:
											return "Blue";
										case 2:
											return "Green";
										case 3:
											return "Blue";
										case 4:
											return "Yellow";
										case 5:
											return "Pink";
									}
									break;
								}
							}
						}
					}
					break;
				}

					#endregion OneFlag

					//Converted v2.2

					#region TDM

				case "tdm":
				{
					if ((who.Team != 7) || (who.Team != 8))
					{
						var redteamplayers = 0;
						var blueteamplayers = 0;

						foreach (var player in C3Mod.C3Players)
							if (player.Team == 7)
								redteamplayers++;
							else if (player.Team == 8)
								blueteamplayers++;

						if (redteamplayers > blueteamplayers)
						{
							who.Team = 8;
							who.GameType = "tdm";
							switch (C3Mod.C3Config.TeamColor2)
							{
								case 1:
									return "Blue";
								case 2:
									return "Green";
								case 3:
									return "Blue";
								case 4:
									return "Yellow";
								case 5:
									return "Pink";
							}
						}
						else if (blueteamplayers > redteamplayers)
						{
							who.Team = 7;
							who.GameType = "tdm";
							switch (C3Mod.C3Config.TeamColor1)
							{
								case 1:
									return "Blue";
								case 2:
									return "Green";
								case 3:
									return "Blue";
								case 4:
									return "Yellow";
								case 5:
									return "Pink";
							}
						}
						else
						{
							var r = new Random();

							switch (r.Next(2) + 1)
							{
								case 1:
								{
									who.Team = 7;
									who.GameType = "tdm";
									switch (C3Mod.C3Config.TeamColor1)
									{
										case 1:
											return "Blue";
										case 2:
											return "Green";
										case 3:
											return "Blue";
										case 4:
											return "Yellow";
										case 5:
											return "Pink";
									}
									break;
								}
								case 2:
								{
									who.Team = 8;
									who.GameType = "tdm";
									switch (C3Mod.C3Config.TeamColor2)
									{
										case 1:
											return "Blue";
										case 2:
											return "Green";
										case 3:
											return "Blue";
										case 4:
											return "Yellow";
										case 5:
											return "Pink";
									}
									break;
								}
							}
						}
					}
					break;
				}

					#endregion TDM
			}
			return "";
		}

		//// <summary>
		//// Broadcasts a message to all players in a running gametype
		//// </summary>
		//// <param name="gametype">"ctf","tdm","1v1","oneflag","ffa","apoc"</param>
		//// <param name="message"></param>
		//// <param name="color"></param>
		public static void BroadcastMessageToGametype(string gametype, string message, Color color)
		{
			foreach (var player in C3Mod.C3Players)
				if (player.GameType == gametype)
					player.SendMessage(message, color);
		}

		public static C3Player GetC3PlayerByIndex(int index)
		{
			foreach (var player in C3Mod.C3Players)
				if (player.Index == index)
					return player;
			return new C3Player(-1);
		}

		public static C3Player GetC3PlayerByName(string name)
		{
			foreach (var player in C3Mod.C3Players)
				if (player.PlayerName.ToLower() == name)
					return player;
			return null;
		}

		internal static TSPlayer GetTSPlayerByIndex(int index)
		{
			foreach (var player in TShock.Players)
				if ((player != null) && (player.Index == index))
					return player;
			return null;
		}

		internal static NPC GetNPCByIndex(int index)
		{
			foreach (var npc in Main.npc)
				if (npc.whoAmI == index)
					return npc;
			return new NPC();
		}

		internal static void ResetGameType(string gametype)
		{
			foreach (var player in C3Mod.C3Players)
				if (player.GameType == gametype)
				{
					player.GameType = "";
					player.Team = 0;
				}
		}
	}
}