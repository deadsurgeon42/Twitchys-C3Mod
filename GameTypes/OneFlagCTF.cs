using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;

namespace C3Mod.GameTypes
{
	internal class OneFlagCTF
	{
		public static bool OneFlagGameRunning;
		public static bool OneFlagGameCountdown;
		public static List<OneFlagArena> Arenas = new List<OneFlagArena>();
		public static Vector2 FlagPoint;
		public static Vector2[] SpawnPoint = new Vector2[2];
		public static int Team1Score;
		public static int Team2Score;
		public static bool[] playersDead = new bool[Main.maxNetPlayers];
		public static DateTime countDownTick = DateTime.UtcNow;
		public static DateTime voteCountDown = DateTime.UtcNow;
		public static C3Player FlagCarrier;
		public static int StartCount = 5;
		public static int VoteCount;

		public static void OnUpdate(EventArgs args)
		{
			lock (C3Mod.C3Players)
			{
				if (C3Mod.VoteRunning && (C3Mod.VoteType == "oneflag"))
				{
					var VotedPlayers = 0;
					var TotalPlayers = 0;

					foreach (var player in C3Mod.C3Players)
					{
						if ((player.GameType == "") || (player.GameType == "oneflag"))
							TotalPlayers++;
						if (player.GameType == "oneflag")
							VotedPlayers++;
					}

					if (VotedPlayers == TotalPlayers)
					{
						C3Tools.BroadcastMessageToGametype("oneflag", "Vote to play One Flag CTF passed, Teleporting to start positions",
							Color.DarkCyan);
						C3Mod.VoteRunning = false;
						C3Mod.VoteType = "";
						FlagCarrier = null;
						Team1Score = 0;
						Team2Score = 0;
						var playersDead = new bool[Main.maxNetPlayers];
						TpToOneFlagSpawns();
						countDownTick = DateTime.UtcNow;
						OneFlagGameCountdown = true;
						return;
					}

					double tick = (DateTime.UtcNow - voteCountDown).TotalMilliseconds;
					if ((tick > C3Mod.C3Config.VoteNotifyInterval * 1000) && (VoteCount > 0))
					{
						if ((VoteCount != 1) && (VoteCount < C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval))
						{
							C3Tools.BroadcastMessageToGametype("oneflag", "Vote still in progress, please be patient", Color.Cyan);
							C3Tools.BroadcastMessageToGametype("", "Vote to play One Flag CTF in progress, type /join to join the lobby",
								Color.Cyan);
						}

						VoteCount--;
						voteCountDown = DateTime.UtcNow;
					}
					else if (VoteCount == 0)
					{
						C3Mod.VoteRunning = false;

						var redteamplayers = 0;
						var blueteamplayers = 0;

						foreach (var player in C3Mod.C3Players)
							if (player.Team == 5)
								redteamplayers++;
							else if (player.Team == 6)
								blueteamplayers++;

						if ((redteamplayers >= C3Mod.C3Config.VoteMinimumPerTeam) &&
						    (blueteamplayers >= C3Mod.C3Config.VoteMinimumPerTeam))
						{
							C3Tools.BroadcastMessageToGametype("oneflag", "Vote to play One Flag CTF passed, Teleporting to start positions",
								Color.DarkCyan);
							FlagCarrier = null;
							Team1Score = 0;
							Team2Score = 0;
							var playersDead = new bool[Main.maxNetPlayers];
							TpToOneFlagSpawns();
							countDownTick = DateTime.UtcNow;
							OneFlagGameCountdown = true;
						}
						else
							C3Tools.BroadcastMessageToGametype("oneflag", "Vote to play One Flag CTF failed, Not enough players",
								Color.DarkCyan);
					}
				}
				if (OneFlagGameCountdown)
				{
					double tick = (DateTime.UtcNow - countDownTick).TotalMilliseconds;
					if ((tick > 1000) && (StartCount > -1))
						if (TpToOneFlagSpawns() > 0)
						{
							if (StartCount == 0)
							{
								C3Tools.BroadcastMessageToGametype("oneflag", "Capture...The...Flag!!!", Color.Cyan);
								StartCount = 5;
								OneFlagGameCountdown = false;
								OneFlagGameRunning = true;
							}
							else
							{
								C3Tools.BroadcastMessageToGametype("oneflag", "Game starting in " + StartCount + "...", Color.Cyan);
								countDownTick = DateTime.UtcNow;
								StartCount--;
							}
						}
						else
						{
							StartCount = 5;
							C3Tools.ResetGameType("oneflag");
							return;
						}
				}

				if (OneFlagGameRunning)
				{
					var team1players = 0;
					var team2players = 0;
					lock (C3Mod.C3Players)
					{
						foreach (var player in C3Mod.C3Players)
						{
							if (player.TSPlayer == null)
							{
								C3Mod.C3Players.Remove(player);
								break;
							}

							if (player.GameType == "oneflag")
							{
								if (!player.TSPlayer.TpLock)
									if (C3Mod.C3Config.TPLockEnabled)
										player.TSPlayer.TpLock = true;

								if (player.Team == 5)
									team1players++;
								else if (player.Team == 6)
									team2players++;

								if ((player.Team == 5) && (Main.player[player.Index].team != C3Mod.C3Config.TeamColor1))
									TShock.Players[player.Index].SetTeam(C3Mod.C3Config.TeamColor1);
								else if ((player.Team == 6) && (Main.player[player.Index].team != C3Mod.C3Config.TeamColor2))
									TShock.Players[player.Index].SetTeam(C3Mod.C3Config.TeamColor2);

								if (!Main.player[player.Index].hostile)
								{
									Main.player[player.Index].hostile = true;
									NetMessage.SendData((int) PacketTypes.TogglePvp, -1, -1, "", player.Index);
								}

								//Respawn on flag
								if (Main.player[player.Index].dead)
									player.Dead = true;
								else
								{
									if (player.Dead)
									{
										player.Dead = false;
										player.TSPlayer.TpLock = false;

										if (player.Team == 5)
											TShock.Players[player.Index].Teleport((int) SpawnPoint[0].X * 16,
												(int) SpawnPoint[0].Y * 16);
										else if (player.Team == 6)
											TShock.Players[player.Index].Teleport((int) SpawnPoint[1].X * 16,
												(int) SpawnPoint[1].Y * 16);
										NetMessage.SendData(4, -1, player.Index, player.PlayerName, player.Index);
										if (C3Mod.C3Config.TPLockEnabled)
											player.TSPlayer.TpLock = true;
									}
								}

								//Grab flag
								if (!player.Dead)
									if (FlagCarrier == null)
										if (((int) player.tileX <= (int) FlagPoint.X + 2) &&
										    ((int) player.tileX >= (int) FlagPoint.X - 2) &&
										    ((int) player.tileY == (int) FlagPoint.Y - 3))
										{
											FlagCarrier = player;

											if (player.Team == 5)
												switch (C3Mod.C3Config.TeamColor1)
												{
													case 1:
													{
														C3Tools.BroadcastMessageToGametype("oneflag",
															Main.player[player.Index]
																.name +
															" has the flag!",
															Color.OrangeRed);
														break;
													}
													case 2:
													{
														C3Tools.BroadcastMessageToGametype("oneflag",
															Main.player[player.Index]
																.name +
															" has the flag!",
															Color.
																Green);
														break;
													}
													case 3:
													{
														C3Tools.BroadcastMessageToGametype("oneflag",
															Main.player[player.Index]
																.name +
															" has the flag!",
															Color.Blue);
														break;
													}
													case 4:
													{
														C3Tools.BroadcastMessageToGametype("oneflag",
															Main.player[player.Index]
																.name +
															" has the flag!",
															Color.Yellow);
														break;
													}
												}
											else if (player.Team == 6)
												switch (C3Mod.C3Config.TeamColor2)
												{
													case 1:
													{
														C3Tools.BroadcastMessageToGametype("oneflag",
															Main.player[player.Index]
																.name +
															" has the flag!",
															Color.OrangeRed);
														break;
													}
													case 2:
													{
														C3Tools.BroadcastMessageToGametype("oneflag",
															Main.player[player.Index]
																.name +
															" has the flag!",
															Color.Green);
														break;
													}
													case 3:
													{
														C3Tools.BroadcastMessageToGametype("oneflag",
															Main.player[player.Index]
																.name +
															" has the flag!",
															Color.Blue);
														break;
													}
													case 4:
													{
														C3Tools.BroadcastMessageToGametype("oneflag",
															Main.player[player.Index]
																.name +
															" has the flag!",
															Color.Yellow);
														break;
													}
												}
											C3Events.FlagGrabbed(FlagCarrier, "oneflag");
										}
							}
						}
					}
					if ((team1players == 0) || (team2players == 0))
					{
						C3Tools.BroadcastMessageToGametype("oneflag", "One Flag CTF stopped, Not enough players to continue",
							Color.DarkCyan);
						OneFlagGameRunning = false;
						SendToSpawn(false);
						C3Tools.ResetGameType("oneflag");
						return;
					}

					//Check on flag carrier
					if (FlagCarrier != null)
						if (Main.player[FlagCarrier.Index].dead)
						{
							if (FlagCarrier.Team == 5)
								switch (C3Mod.C3Config.TeamColor1)
								{
									case 1:
									{
										C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.OrangeRed);
										break;
									}
									case 2:
									{
										C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.Green);
										break;
									}
									case 3:
									{
										C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.Blue);
										break;
									}
									case 4:
									{
										C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.Yellow);
										break;
									}
								}
							else if (FlagCarrier.Team == 6)
								switch (C3Mod.C3Config.TeamColor2)
								{
									case 1:
									{
										C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.OrangeRed);
										break;
									}
									case 2:
									{
										C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.Green);
										break;
									}
									case 3:
									{
										C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.Blue);
										break;
									}
									case 4:
									{
										C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.Yellow);
										break;
									}
								}

							FlagCarrier = null;
						}
						//Capture the flag
						else
						{
							if (FlagCarrier.Team == 5)
							{
								if (((int) FlagCarrier.tileX <= (int) SpawnPoint[0].X + 2) &&
								    ((int) FlagCarrier.tileX >= (int) SpawnPoint[0].X - 2) &&
								    ((int) FlagCarrier.tileY == (int) SpawnPoint[0].Y - 3))
								{
									Team1Score++;

									switch (C3Mod.C3Config.TeamColor1)
									{
										case 1:
										{
											if (C3Mod.C3Config.TeamColor2 == 2)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Red - " + Team1Score + " --- " + Team2Score + " - Green",
													Color.OrangeRed);
											else if (C3Mod.C3Config.TeamColor2 == 3)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Red - " + Team1Score + " --- " + Team2Score + " - Blue",
													Color.OrangeRed);
											else if (C3Mod.C3Config.TeamColor2 == 4)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Red - " + Team1Score + " --- " + Team2Score + " - Yellow",
													Color.OrangeRed);
											break;
										}
										case 2:
										{
											if (C3Mod.C3Config.TeamColor2 == 1)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Green - " + Team1Score + " --- " + Team2Score + " - Red", Color.Green);
											else if (C3Mod.C3Config.TeamColor2 == 3)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Green - " + Team1Score + " --- " + Team2Score + " - Blue", Color.Green);
											else if (C3Mod.C3Config.TeamColor2 == 4)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Green - " + Team1Score + " --- " + Team2Score + " - Yellow",
													Color.Green);
											break;
										}
										case 3:
										{
											if (C3Mod.C3Config.TeamColor2 == 1)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Blue - " + Team1Score + " --- " + Team2Score + " - Red", Color.Blue);
											else if (C3Mod.C3Config.TeamColor2 == 2)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Blue - " + Team1Score + " --- " + Team2Score + " - Green", Color.Blue);
											else if (C3Mod.C3Config.TeamColor2 == 4)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Blue - " + Team1Score + " --- " + Team2Score + " - Yellow", Color.Blue);
											break;
										}
										case 4:
										{
											if (C3Mod.C3Config.TeamColor2 == 1)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team1Score + " --- " + Team2Score + " - Red",
													Color.Yellow);
											else if (C3Mod.C3Config.TeamColor2 == 2)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team1Score + " --- " + Team2Score + " - Green",
													Color.Yellow);
											else if (C3Mod.C3Config.TeamColor2 == 3)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team1Score + " --- " + Team2Score + " - Blue",
													Color.Yellow);
											break;
										}
									}
									C3Events.FlagCapture(FlagCarrier, "oneflag", "Team1", Team1Score, Team2Score);
									FlagCarrier = null;

									if (C3Mod.C3Config.RespawnPlayersOnFlagCapture && (Team1Score != C3Mod.C3Config.CTFScoreLimit))
										TpToOneFlagSpawns();

									if (C3Mod.C3Config.ReCountdownOnFlagCapture && (Team1Score != C3Mod.C3Config.CTFScoreLimit))
									{
										OneFlagGameRunning = false;
										OneFlagGameCountdown = true;
									}

									if (C3Mod.C3Config.HealPlayersOnFlagCapture)
									{
										var heart = TShock.Utils.GetItemById(58);
										var star = TShock.Utils.GetItemById(184);

										foreach (var player in C3Mod.C3Players)
											if (player.GameType == "ctf")
											{
												player.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
												player.GiveItem(star.type, star.name, star.width, star.height, 20);
											}
									}
								}
							}
							else if (FlagCarrier.Team == 6)
							{
								if (((int) FlagCarrier.tileX <= (int) SpawnPoint[1].X + 2) &&
								    ((int) FlagCarrier.tileX >= (int) SpawnPoint[1].X - 2) &&
								    ((int) FlagCarrier.tileY == (int) SpawnPoint[1].Y - 3))
								{
									Team2Score++;
									switch (C3Mod.C3Config.TeamColor2)
									{
										case 1:
										{
											if (C3Mod.C3Config.TeamColor1 == 2)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Red - " + Team2Score + " --- " + Team1Score + " - Green",
													Color.OrangeRed);
											else if (C3Mod.C3Config.TeamColor1 == 3)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Red - " + Team2Score + " --- " + Team1Score + " - Blue",
													Color.OrangeRed);
											else if (C3Mod.C3Config.TeamColor1 == 4)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Red - " + Team2Score + " --- " + Team1Score + " - Yellow",
													Color.OrangeRed);
											break;
										}
										case 2:
										{
											if (C3Mod.C3Config.TeamColor1 == 1)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Green - " + Team2Score + " --- " + Team1Score + " - Red", Color.Green);
											else if (C3Mod.C3Config.TeamColor1 == 3)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Green - " + Team2Score + " --- " + Team1Score + " - Blue", Color.Green);
											else if (C3Mod.C3Config.TeamColor1 == 4)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Green - " + Team2Score + " --- " + Team1Score + " - Yellow",
													Color.Green);
											break;
										}
										case 3:
										{
											if (C3Mod.C3Config.TeamColor1 == 1)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Blue - " + Team2Score + " --- " + Team1Score + " - Red", Color.Blue);
											else if (C3Mod.C3Config.TeamColor1 == 2)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Blue - " + Team2Score + " --- " + Team1Score + " - Green", Color.Blue);
											else if (C3Mod.C3Config.TeamColor1 == 4)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Blue - " + Team2Score + " --- " + Team1Score + " - Yellow", Color.Blue);
											break;
										}
										case 4:
										{
											if (C3Mod.C3Config.TeamColor1 == 1)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team2Score + " --- " + Team1Score + " - Red",
													Color.Yellow);
											else if (C3Mod.C3Config.TeamColor1 == 2)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team2Score + " --- " + Team1Score + " - Green",
													Color.Yellow);
											else if (C3Mod.C3Config.TeamColor1 == 3)
												C3Tools.BroadcastMessageToGametype("oneflag",
													FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team2Score + " --- " + Team1Score + " - Blue",
													Color.Yellow);
											break;
										}
									}
									C3Events.FlagCapture(FlagCarrier, "oneflag", "Team2", Team2Score, Team1Score);
									FlagCarrier = null;

									if (C3Mod.C3Config.RespawnPlayersOnFlagCapture && (Team2Score != C3Mod.C3Config.CTFScoreLimit))
										TpToOneFlagSpawns();

									if (C3Mod.C3Config.ReCountdownOnFlagCapture && (Team2Score != C3Mod.C3Config.CTFScoreLimit))
									{
										OneFlagGameRunning = false;
										OneFlagGameCountdown = true;
									}

									if (C3Mod.C3Config.HealPlayersOnFlagCapture)
									{
										var heart = TShock.Utils.GetItemById(58);
										var star = TShock.Utils.GetItemById(184);

										foreach (var player in C3Mod.C3Players)
											if (player.GameType == "ctf")
											{
												player.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
												player.GiveItem(star.type, star.name, star.width, star.height, 20);
											}
									}
								}
							}
						}

					if (Team1Score == C3Mod.C3Config.CTFScoreLimit)
					{
						OneFlagGameRunning = false;
						if (C3Mod.C3Config.TeamColor2 == 1)
							C3Tools.BroadcastMessageToGametype("ctf", "RED TEAM WINS!", Color.OrangeRed);
						else if (C3Mod.C3Config.TeamColor2 == 2)
							C3Tools.BroadcastMessageToGametype("ctf", "GREEN TEAM WINS!", Color.Green);
						else if (C3Mod.C3Config.TeamColor2 == 3)
							C3Tools.BroadcastMessageToGametype("ctf", "BLUE TEAM WINS!", Color.Blue);
						else if (C3Mod.C3Config.TeamColor2 == 4)
							C3Tools.BroadcastMessageToGametype("ctf", "YELLOW TEAM WINS!", Color.Yellow);

						var LostPlayers = new List<C3Player>();
						var WonPlayers = new List<C3Player>();

						foreach (var player1 in C3Mod.C3Players)
							if (player1.GameType == "oneflag")
							{
								if (player1.Team == 6)
									WonPlayers.Add(player1);
								if (player1.Team == 5)
									LostPlayers.Add(player1);
							}

						C3Events.GameEnd(WonPlayers, LostPlayers, "oneflag", Team2Score, Team1Score);

						SendToSpawn(false);
						C3Tools.ResetGameType("oneflag");
						FlagPoint = new Vector2();
						SpawnPoint = new Vector2[2];
						return;
					}
					if (Team2Score == C3Mod.C3Config.CTFScoreLimit)
					{
						OneFlagGameRunning = false;
						if (C3Mod.C3Config.TeamColor1 == 1)
							C3Tools.BroadcastMessageToGametype("ctf", "RED TEAM WINS!", Color.OrangeRed);
						else if (C3Mod.C3Config.TeamColor1 == 2)
							C3Tools.BroadcastMessageToGametype("ctf", "GREEN TEAM WINS!", Color.Green);
						else if (C3Mod.C3Config.TeamColor1 == 3)
							C3Tools.BroadcastMessageToGametype("ctf", "BLUE TEAM WINS!", Color.Blue);
						else if (C3Mod.C3Config.TeamColor1 == 4)
							C3Tools.BroadcastMessageToGametype("ctf", "YELLOW TEAM WINS!", Color.Yellow);

						var LostPlayers = new List<C3Player>();
						var WonPlayers = new List<C3Player>();

						foreach (var player1 in C3Mod.C3Players)
							if (player1.GameType == "oneflag")
							{
								if (player1.Team == 5)
									WonPlayers.Add(player1);
								if (player1.Team == 6)
									LostPlayers.Add(player1);
							}

						C3Events.GameEnd(WonPlayers, LostPlayers, "oneflag", Team1Score, Team2Score);

						SendToSpawn(false);
						C3Tools.ResetGameType("oneflag");
						FlagPoint = new Vector2();
						SpawnPoint = new Vector2[2];
					}
				}
			}
		}

		public static int TpToOneFlagSpawns()
		{
			var team1players = 0;
			var team2players = 0;

			for (var i = 0; i < C3Mod.C3Players.Count; i++)
			{
				if ((C3Mod.C3Players[i].Team == 5) && (Main.player[C3Mod.C3Players[i].Index].team != C3Mod.C3Config.TeamColor1))
					TShock.Players[C3Mod.C3Players[i].Index].SetTeam(C3Mod.C3Config.TeamColor1);
				else if ((C3Mod.C3Players[i].Team == 6) && (Main.player[C3Mod.C3Players[i].Index].team != C3Mod.C3Config.TeamColor2))
					TShock.Players[C3Mod.C3Players[i].Index].SetTeam(C3Mod.C3Config.TeamColor2);

				if (C3Mod.C3Players[i].Team == 5)
				{
					team1players++;
					C3Mod.C3Players[i].TSPlayer.TpLock = false;
					if ((C3Mod.C3Players[i].tileX != (int) SpawnPoint[0].X) ||
					    (C3Mod.C3Players[i].tileY != (int) (SpawnPoint[0].Y - 3)))
						TShock.Players[C3Mod.C3Players[i].Index].Teleport((int) SpawnPoint[0].X * 16, (int) SpawnPoint[0].Y * 16);
					if (C3Mod.C3Config.TPLockEnabled) C3Mod.C3Players[i].TSPlayer.TpLock = true;
				}
				else if (C3Mod.C3Players[i].Team == 6)
				{
					team2players++;
					C3Mod.C3Players[i].TSPlayer.TpLock = false;
					if ((C3Mod.C3Players[i].tileX != (int) SpawnPoint[1].X) ||
					    (C3Mod.C3Players[i].tileY != (int) (SpawnPoint[1].Y - 3)))
						TShock.Players[C3Mod.C3Players[i].Index].Teleport((int) SpawnPoint[1].X * 16, (int) SpawnPoint[1].Y * 16);
					if (C3Mod.C3Config.TPLockEnabled) C3Mod.C3Players[i].TSPlayer.TpLock = true;
				}
			}

			if ((team1players == 0) || (team2players == 0))
			{
				C3Tools.BroadcastMessageToGametype("oneflag", "Not enough players to start One Flag CTF", Color.DarkCyan);
				OneFlagGameRunning = false;
				OneFlagGameCountdown = false;
				FlagPoint = new Vector2();
				SpawnPoint = new Vector2[2];
				return 0;
			}
			return 1;
		}

		public static void SendToSpawn(bool pvpstate)
		{
			for (var i = 0; i < C3Mod.C3Players.Count; i++)
				if (C3Mod.C3Players[i].Team == 5)
				{
					C3Mod.C3Players[i].TSPlayer.TpLock = false;
					Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
					NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index);
					TShock.Players[C3Mod.C3Players[i].Index].Spawn();
					TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
				}
				else if (C3Mod.C3Players[i].Team == 6)
				{
					C3Mod.C3Players[i].TSPlayer.TpLock = false;
					Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
					NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index);
					TShock.Players[C3Mod.C3Players[i].Index].Spawn();
					TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
				}
		}
	}

	internal class OneFlagArena
	{
		public Vector2 Flag;
		public string Name = "";
		public Vector2[] Spawns = new Vector2[2];

		public OneFlagArena(Vector2 flag, Vector2 redspawn, Vector2 bluespawn, string name)
		{
			Flag = flag;
			Spawns[0] = redspawn;
			Spawns[1] = bluespawn;
			Name = name;
		}
	}
}