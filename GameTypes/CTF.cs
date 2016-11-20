using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace C3Mod.GameTypes
{
  //Converted v2.2
  internal class CTF
  {
    public static bool CTFGameRunning;
    public static bool CTFGameCountdown;
    public static List<CTFArena> Arenas = new List<CTFArena>();
    public static Vector2[] flagPoints = new Vector2[2];
    public static Vector2[] spawnPoints = new Vector2[2];
    public static int Team1Score;
    public static int Team2Score;
    public static bool[] playersDead = new bool[Main.maxNetPlayers];
    public static DateTime countDownTick = DateTime.UtcNow;
    public static DateTime voteCountDown = DateTime.UtcNow;
    public static C3Player Team1FlagCarrier;
    public static C3Player Team2FlagCarrier;
    public static int StartCount = 5;
    public static int VoteCount;

    public static void OnUpdate(EventArgs args)
    {
      lock (C3Mod.C3Players)
      {
        if (C3Mod.VoteRunning && C3Mod.VoteType == "ctf")
        {
          int VotedPlayers = 0;
          int TotalPlayers = 0;

          foreach (C3Player player in C3Mod.C3Players)
          {
            if (player.GameType == "" || player.GameType == "ctf")
              TotalPlayers++;
            if (player.GameType == "ctf")
              VotedPlayers++;
          }

          if (VotedPlayers == TotalPlayers)
          {
            C3Tools.BroadcastMessageToGametype("ctf", "Vote to play Capture the Flag passed, Teleporting to start positions", Color.DarkCyan);
            C3Mod.VoteRunning = false;
            C3Mod.VoteType = "";
            Team2FlagCarrier = null;
            Team1FlagCarrier = null;
            Team2Score = 0;
            Team1Score = 0;
            bool[] playersDead = new bool[Main.maxNetPlayers];
            TpToSpawn();
            countDownTick = DateTime.UtcNow;
            CTFGameCountdown = true;
            return;
          }

          double tick = (DateTime.UtcNow - voteCountDown).TotalMilliseconds;
          if (tick > (C3Mod.C3Config.VoteNotifyInterval * 1000) && VoteCount > 0)
          {
            if (VoteCount != 1 && VoteCount < (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval))
            {
              C3Tools.BroadcastMessageToGametype("ctf", "Vote still in progress, please be patient", Color.Cyan);
              C3Tools.BroadcastMessageToGametype("", "Vote to play Capture the Flag in progress, type /join to join the lobby", Color.Cyan);
            }

            VoteCount--;
            voteCountDown = DateTime.UtcNow;
          }
          else if (VoteCount == 0)
          {
            C3Mod.VoteRunning = false;

            int team1players = 0;
            int team2players = 0;

            foreach (C3Player player in C3Mod.C3Players)
            {
              if (player.Team == 1)
                team1players++;
              else if (player.Team == 2)
                team2players++;
            }

            if (team1players >= C3Mod.C3Config.VoteMinimumPerTeam && team2players >= C3Mod.C3Config.VoteMinimumPerTeam)
            {
              C3Tools.BroadcastMessageToGametype("ctf", "Vote to play Capture the Flag passed, Teleporting to start positions", Color.DarkCyan);
              Team2FlagCarrier = null;
              Team1FlagCarrier = null;
              Team2Score = 0;
              Team1Score = 0;
              bool[] playersDead = new bool[Main.maxNetPlayers];
              TpToSpawn();
              countDownTick = DateTime.UtcNow;
              CTFGameCountdown = true;
            }
            else
              C3Tools.BroadcastMessageToGametype("ctf", "Vote to play Capture the Flag failed, Not enough players", Color.DarkCyan);
          }
        }

        if (CTFGameCountdown)
        {
          double tick = (DateTime.UtcNow - countDownTick).TotalMilliseconds;
          if (tick > 1000 && StartCount > -1)
          {
            if (TpToSpawn() > 0)
            {
              if (StartCount == 0)
              {
                C3Tools.BroadcastMessageToGametype("ctf", "Capture...The...Flag!!!", Color.Cyan);
                StartCount = 5;
                CTFGameCountdown = false;
                CTFGameRunning = true;
              }
              else
              {
                C3Tools.BroadcastMessageToGametype("ctf", "Game starting in " + StartCount + "...", Color.Cyan);
                countDownTick = DateTime.UtcNow;
                StartCount--;
              }
            }
            else
            {
              StartCount = 5;
              C3Tools.ResetGameType("ctf");
              return;
            }
          }
        }

        if (CTFGameRunning)
        {
          int team1players = 0;
          int team2players = 0;
          lock (C3Mod.C3Players)
          {
            foreach (C3Player player in C3Mod.C3Players)
            {
              if (player.TSPlayer == null)
              {
                C3Mod.C3Players.Remove(player);
                break;
              }

              if (player.GameType == "ctf")
              {
                if (!player.TSPlayer.TpLock)
                  if (C3Mod.C3Config.TPLockEnabled)
                  {
                    player.TSPlayer.TpLock = true;
                  }

                if (player.Team == 1)
                  team1players++;
                else if (player.Team == 2)
                  team2players++;
                if ((player.Team == 1 && Main.player[player.Index].team != C3Mod.C3Config.TeamColor1))
                  TShock.Players[player.Index].SetTeam(C3Mod.C3Config.TeamColor1);
                if ((player.Team == 2 && Main.player[player.Index].team != C3Mod.C3Config.TeamColor2))
                  TShock.Players[player.Index].SetTeam(C3Mod.C3Config.TeamColor2);
                if (!Main.player[player.Index].hostile)
                {
                  Main.player[player.Index].hostile = true;
                  NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, "", player.Index, 0f, 0f,
                                      0f);
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
                    if (player.Team == 1)
                      TShock.Players[player.Index].Teleport((int)spawnPoints[0].X * 16,
                                                            (int)spawnPoints[0].Y * 16);
                    else if (player.Team == 2)
                      TShock.Players[player.Index].Teleport((int)spawnPoints[1].X * 16,
                                                            (int)spawnPoints[1].Y * 16);
                    NetMessage.SendData(4, -1, player.Index, player.PlayerName, player.Index, 0f, 0f,
                                        0f, 0);
                    if (C3Mod.C3Config.TPLockEnabled)
                    {
                      player.TSPlayer.TpLock = true;
                    }
                  }
                }

                //Grab flag
                if (!player.Dead)
                {
                  if (player.Team == 1 && Team1FlagCarrier == null)
                  {
                    if ((int)player.tileX >= flagPoints[1].X - 2 &&
                        (int)player.tileX <= flagPoints[1].X + 2 &&
                        (int)player.tileY == (int)(flagPoints[1].Y - 3))
                    {
                      Team1FlagCarrier = player;
                      switch (C3Mod.C3Config.TeamColor1)
                      {
                        case 1:
                          {
                            C3Tools.BroadcastMessageToGametype("ctf",
                                                               Main.player[player.Index].
                                                                   name + " has the flag!",
                                                               Color.OrangeRed);
                            break;
                          }
                        case 2:
                          {
                            C3Tools.BroadcastMessageToGametype("ctf",
                                                               Main.player[player.Index].
                                                                   name + " has the flag!",
                                                               Color.Green);
                            break;
                          }
                        case 3:
                          {
                            C3Tools.BroadcastMessageToGametype("ctf",
                                                               Main.player[player.Index].
                                                                   name + " has the flag!",
                                                               Color.Blue);
                            break;
                          }
                        case 4:
                          {
                            C3Tools.BroadcastMessageToGametype("ctf",
                                                               Main.player[player.Index].
                                                                   name + " has the flag!",
                                                               Color.Yellow);
                            break;
                          }
                        case 5:
                          {
                            C3Tools.BroadcastMessageToGametype("ctf",
                                                               Main.player[player.Index].
                                                                   name + " has the flag!",
                                                               Color.Pink);
                            break;
                          }
                      }
                      C3Events.FlagGrabbed(player, "ctf");
                    }
                  }
                  if (player.Team == 2 && Team2FlagCarrier == null)
                  {
                    if ((int)player.tileX >= flagPoints[0].X - 2 &&
                        (int)player.tileX <= flagPoints[0].X + 2 &&
                        (int)player.tileY == (int)(flagPoints[0].Y - 3))
                    {
                      Team2FlagCarrier = player;
                      switch (C3Mod.C3Config.TeamColor2)
                      {
                        case 1:
                        {
                          C3Tools.BroadcastMessageToGametype("ctf",
                            Main.player[player.Index].
                              name + " has the flag!",
                            Color.OrangeRed);
                          break;
                        }
                        case 2:
                        {
                          C3Tools.BroadcastMessageToGametype("ctf",
                            Main.player[player.Index].
                              name + " has the flag!",
                            Color.Green);
                          break;
                        }
                        case 3:
                        {
                          C3Tools.BroadcastMessageToGametype("ctf",
                            Main.player[player.Index].
                              name + " has the flag!",
                            Color.Blue);
                          break;
                        }
                        case 4:
                        {
                          C3Tools.BroadcastMessageToGametype("ctf",
                            Main.player[player.Index].
                              name + " has the flag!",
                            Color.Yellow);
                          break;
                        }
                        case 5:
                          {
                            C3Tools.BroadcastMessageToGametype("ctf",
                                                               Main.player[player.Index].
                                                                   name + " has the flag!",
                                                               Color.Pink);
                            break;
                          }
                      }
                      C3Events.FlagGrabbed(player, "ctf");
                    }
                  }
                }
              }
            }
          }
          if (team1players == 0 || team2players == 0)
          {
            C3Tools.BroadcastMessageToGametype("ctf", "Capture the Flag stopped, Not enough players to continue", Color.DarkCyan);
            CTFGameRunning = false;
            TpToSpawns(false);
            C3Tools.ResetGameType("ctf");
            flagPoints = new Vector2[2];
            spawnPoints = new Vector2[2];
            return;
          }

          //Check on flag carriers
          if (Team2FlagCarrier != null)
          {
            //Make them drop the flag
            if (Team2FlagCarrier.TerrariaDead)
            {
              C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Dropped the flag!", Color.Blue);
              Team2FlagCarrier = null;
            }
            //Capture the flag
            else
            {
              if ((int)Team2FlagCarrier.tileX >= flagPoints[1].X - 2 && (int)Team2FlagCarrier.tileX <= flagPoints[1].X + 2 && (int)Team2FlagCarrier.tileY == (int)(flagPoints[1].Y - 3))
              {
                Team2Score++;
                switch (C3Mod.C3Config.TeamColor2)
                {
                  case 1:
                  {
                    switch (C3Mod.C3Config.TeamColor1)
                    {
                      case 2:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Red - " + Team2Score + " --- " + Team1Score + " - Green", Color.OrangeRed);
                        break;
                      case 3:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Red - " + Team2Score + " --- " + Team1Score + " - Blue", Color.OrangeRed);
                        break;
                      case 4:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Red - " + Team2Score + " --- " + Team1Score + " - Yellow", Color.OrangeRed);
                        break;
                      case 5:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Red - " + Team2Score + " --- " + Team1Score + " - Pink", Color.OrangeRed);
                        break;

                      }
                      break;
                  }
                  case 2:
                  {
                    switch (C3Mod.C3Config.TeamColor1)
                    {
                      case 1:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Green - " + Team2Score + " --- " + Team1Score + " - Red", Color.Green);
                        break;
                      case 3:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Green - " + Team2Score + " --- " + Team1Score + " - Blue", Color.Green);
                        break;
                      case 4:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Green - " + Team2Score + " --- " + Team1Score + " - Yellow", Color.Green);
                        break;
                      case 5:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Green - " + Team2Score + " --- " + Team1Score + " - Pink", Color.Green);
                        break;

                      }
                      break;
                  }
                  case 3:
                  {
                    switch (C3Mod.C3Config.TeamColor1)
                    {
                      case 1:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Blue - " + Team2Score + " --- " + Team1Score + " - Red", Color.Blue);
                        break;
                      case 2:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Blue - " + Team2Score + " --- " + Team1Score + " - Green", Color.Blue);
                        break;
                      case 4:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Blue - " + Team2Score + " --- " + Team1Score + " - Yellow", Color.Blue);
                        break;
                      case 5:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Blue - " + Team2Score + " --- " + Team1Score + " - Pink", Color.Blue);
                        break;

                      }
                      break;
                  }
                  case 4:
                  {
                    switch (C3Mod.C3Config.TeamColor1)
                    {
                      case 1:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team2Score + " --- " + Team1Score + " - Red", Color.Yellow);
                        break;
                      case 2:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team2Score + " --- " + Team1Score + " - Green", Color.Yellow);
                        break;
                      case 3:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team2Score + " --- " + Team1Score + " - Blue", Color.Yellow);
                        break;
                      case 5:
                        C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team2Score + " --- " + Team1Score + " - Pink", Color.Yellow);
                        break;

                      }
                      break;
                  }
                  case 5:
                    {
                      switch (C3Mod.C3Config.TeamColor1)
                      {
                        case 1:
                          C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Pink - " + Team2Score + " --- " + Team1Score + " - Red", Color.Pink);
                          break;
                        case 2:
                          C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Pink - " + Team2Score + " --- " + Team1Score + " - Green", Color.Pink);
                          break;
                        case 3:
                          C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Pink - " + Team2Score + " --- " + Team1Score + " - Blue", Color.Pink);
                          break;
                        case 4:
                          C3Tools.BroadcastMessageToGametype("ctf", Team2FlagCarrier.PlayerName + ": Scores!  Pink - " + Team2Score + " --- " + Team1Score + " - Yellow", Color.Pink);
                          break;

                      }
                      break;
                    }

                }
                C3Events.FlagCapture(Team2FlagCarrier, "ctf", "Team2", Team2Score, Team1Score);
                Team2FlagCarrier = null;

                if (C3Mod.C3Config.RespawnPlayersOnFlagCapture && Team2Score != C3Mod.C3Config.CTFScoreLimit)
                {
                  Team1FlagCarrier = null;
                  TpToSpawn();
                }

                if (C3Mod.C3Config.ReCountdownOnFlagCapture && Team2Score != C3Mod.C3Config.CTFScoreLimit)
                {
                  Team1FlagCarrier = null;
                  CTFGameRunning = false;
                  CTFGameCountdown = true;
                }

                if (C3Mod.C3Config.HealPlayersOnFlagCapture)
                {
                  Item heart = TShock.Utils.GetItemById(58);
                  Item star = TShock.Utils.GetItemById(184);

                  foreach (C3Player player in C3Mod.C3Players)
                  {
                    if (player.GameType == "ctf")
                    {
                      player.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                      player.GiveItem(star.type, star.name, star.width, star.height, 20);
                    }
                  }
                }
              }
            }
          }
          if (Team1FlagCarrier != null)
          {
            if (Team1FlagCarrier.TerrariaDead)
            {
              C3Tools.BroadcastMessageToGametype("ctf", Team1FlagCarrier.PlayerName + ": Dropped the flag!", Color.OrangeRed);
              Team1FlagCarrier = null;
            }
            else
            {
              if ((int)Team1FlagCarrier.tileX >= flagPoints[0].X - 2 && (int)Team1FlagCarrier.tileX <= flagPoints[0].X + 2 && (int)Team1FlagCarrier.tileY == (int)(flagPoints[0].Y - 3))
              {
                Team1Score++;
                switch (C3Mod.C3Config.TeamColor1)
                {
                  case 1:
                    switch (C3Mod.C3Config.TeamColor2)
                    {
                      case 2:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Red - " + Team1Score + " --- " + Team2Score +
                          " - Green", Color.OrangeRed);
                        break;
                      case 3:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Red - " + Team1Score + " --- " + Team2Score +
                          " - Blue", Color.OrangeRed);
                        break;
                      case 4:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Red - " + Team1Score + " --- " + Team2Score +
                          " - Yellow", Color.OrangeRed);
                        break;
                      case 5:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Red - " + Team1Score + " --- " + Team2Score +
                          " - Pink", Color.Green);
                        break;
                    }
                    break;
                  case 2:
                  {
                    switch (C3Mod.C3Config.TeamColor2)
                    {
                      case 1:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Green - " + Team1Score + " --- " + Team2Score +
                          " - Red", Color.Green);
                        break;
                      case 3:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Green - " + Team1Score + " --- " + Team2Score +
                          " - Blue", Color.Green);
                        break;
                      case 4:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Green - " + Team1Score + " --- " + Team2Score +
                          " - Yellow", Color.Green);
                        break;
                      case 5:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Green - " + Team1Score + " --- " + Team2Score +
                          " - Pink", Color.Green);
                        break;
                      }
                    break;
                  }
                  case 3:
                  {
                    switch (C3Mod.C3Config.TeamColor2)
                    {
                      case 1:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Blue - " + Team1Score + " --- " + Team2Score +
                          " - Red", Color.Blue);
                        break;
                      case 2:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Blue - " + Team1Score + " --- " + Team2Score +
                          " - Green", Color.Blue);
                        break;
                      case 4:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Blue - " + Team1Score + " --- " + Team2Score +
                          " - Yellow", Color.Blue);
                        break;
                      case 5:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Blue - " + Team1Score + " --- " + Team2Score +
                          " - Pink", Color.Blue);
                        break;
                      }
                      break;
                  }
                  case 4:
                  {
                    switch (C3Mod.C3Config.TeamColor2)
                    {
                      case 1:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team1Score + " --- " + Team2Score +
                          " - Red", Color.Yellow);
                        break;
                      case 2:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team1Score + " --- " + Team2Score +
                          " - Green", Color.Yellow);
                        break;
                      case 3:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team1Score + " --- " + Team2Score +
                          " - Blue", Color.Yellow);
                        break;
                      case 5:
                        C3Tools.BroadcastMessageToGametype("ctf",
                          Team1FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team1Score + " --- " + Team2Score +
                          " - Pink", Color.Yellow);
                        break;
                      }
                      break;
                  }
                  case 5:
                    {
                      switch (C3Mod.C3Config.TeamColor2)
                      {
                        case 1:
                          C3Tools.BroadcastMessageToGametype("ctf",
                            Team1FlagCarrier.PlayerName + ": Scores!  Pink - " + Team1Score + " --- " + Team2Score +
                            " - Red", Color.
                            Pink);
                          break;
                        case 2:
                          C3Tools.BroadcastMessageToGametype("ctf",
                            Team1FlagCarrier.PlayerName + ": Scores!  Pink - " + Team1Score + " --- " + Team2Score +
                            " - Green", Color.Pink);
                          break;
                        case 3:
                          C3Tools.BroadcastMessageToGametype("ctf",
                            Team1FlagCarrier.PlayerName + ": Scores!  Pink - " + Team1Score + " --- " + Team2Score +
                            " - Blue", Color.Pink);
                          break;
                        case 4:
                          C3Tools.BroadcastMessageToGametype("ctf",
                            Team1FlagCarrier.PlayerName + ": Scores!  Pink - " + Team1Score + " --- " + Team2Score +
                            " - Yellow", Color.Pink);
                          break;
                      }
                      break;
                    }

                }
                C3Events.FlagCapture(Team1FlagCarrier, "ctf", "Team1", Team1Score, Team2Score);
                Team1FlagCarrier = null;

                if (C3Mod.C3Config.RespawnPlayersOnFlagCapture && Team1Score != C3Mod.C3Config.CTFScoreLimit)
                {
                  Team2FlagCarrier = null;
                  TpToSpawn();
                }

                if (C3Mod.C3Config.ReCountdownOnFlagCapture && Team1Score != C3Mod.C3Config.CTFScoreLimit)
                {
                  Team2FlagCarrier = null;
                  CTFGameRunning = false;
                  CTFGameCountdown = true;
                }

                if (C3Mod.C3Config.HealPlayersOnFlagCapture)
                {
                  Item heart = TShock.Utils.GetItemById(58);
                  Item star = TShock.Utils.GetItemById(184);

                  foreach (C3Player player in C3Mod.C3Players)
                  {
                    if (player.GameType == "ctf")
                    {
                      player.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                      player.GiveItem(star.type, star.name, star.width, star.height, 20);
                    }
                  }
                }
              }
            }
          }
        }

        if (Team2Score == C3Mod.C3Config.CTFScoreLimit)
        {
          CTFGameRunning = false;
          switch (C3Mod.C3Config.TeamColor2)
          {
            case 1:
              C3Tools.BroadcastMessageToGametype("ctf", "RED TEAM WINS!", Color.OrangeRed);
              break;
            case 2:
              C3Tools.BroadcastMessageToGametype("ctf", "GREEN TEAM WINS!", Color.Green);
              break;
            case 3:
              C3Tools.BroadcastMessageToGametype("ctf", "BLUE TEAM WINS!", Color.Blue);
              break;
            case 4:
              C3Tools.BroadcastMessageToGametype("ctf", "YELLOW TEAM WINS!", Color.Yellow);
              break;
            case 5:
              C3Tools.BroadcastMessageToGametype("ctf", "PINK TEAM WINS!", Color.Pink);
              break;
          }
          List<C3Player> LostPlayers = new List<C3Player>();
          List<C3Player> WonPlayers = new List<C3Player>();

          foreach (C3Player player1 in C3Mod.C3Players)
          {
            if (player1.GameType == "ctf")
            {
              if (player1.Team == 2)
                WonPlayers.Add(player1);
              if (player1.Team == 1)
                LostPlayers.Add(player1);
            }
          }

          C3Events.GameEnd(WonPlayers, LostPlayers, "ctf", Team2Score, Team1Score);

          TpToSpawns(false);
          C3Tools.ResetGameType("ctf");
          flagPoints = new Vector2[2];
          spawnPoints = new Vector2[2];
          return;
        }
        if (Team1Score == C3Mod.C3Config.CTFScoreLimit)
        {
          CTFGameRunning = false;
          switch (C3Mod.C3Config.TeamColor1)
          {
            case 1:
              C3Tools.BroadcastMessageToGametype("ctf", "RED TEAM WINS!", Color.OrangeRed);
              break;
            case 2:
              C3Tools.BroadcastMessageToGametype("ctf", "GREEN TEAM WINS!", Color.Green);
              break;
            case 3:
              C3Tools.BroadcastMessageToGametype("ctf", "BLUE TEAM WINS!", Color.Blue);
              break;
            case 4:
              C3Tools.BroadcastMessageToGametype("ctf", "YELLOW TEAM WINS!", Color.Yellow);
              break;
            case 5:
              C3Tools.BroadcastMessageToGametype("ctf", "PINK TEAM WINS!", Color.Pink);
              break;
          }
          List<C3Player> LostPlayers = new List<C3Player>();
          List<C3Player> WonPlayers = new List<C3Player>();

          foreach (C3Player player1 in C3Mod.C3Players)
          {
            if (player1.GameType == "ctf")
            {
              if (player1.Team == 1)
                WonPlayers.Add(player1);
              if (player1.Team == 2)
                LostPlayers.Add(player1);
            }
          }

          C3Events.GameEnd(WonPlayers, LostPlayers, "ctf", Team1Score, Team2Score);

          TpToSpawns(false);
          C3Tools.ResetGameType("ctf");
          flagPoints = new Vector2[2];
          spawnPoints = new Vector2[2];
        }
      }
    }

    public static int TpToSpawn()
    {
      try
      {
        int playersred = 0;
        int playersblue = 0;

        for (int i = 0; i < C3Mod.C3Players.Count; i++)
        {
          if ((C3Mod.C3Players[i].Team == 1 && Main.player[C3Mod.C3Players[i].Index].team != C3Mod.C3Config.TeamColor1))
            TShock.Players[C3Mod.C3Players[i].Index].SetTeam(C3Mod.C3Config.TeamColor1);
          else if (C3Mod.C3Players[i].Team == 2 && Main.player[C3Mod.C3Players[i].Index].team != C3Mod.C3Config.TeamColor2)
            TShock.Players[C3Mod.C3Players[i].Index].SetTeam(C3Mod.C3Config.TeamColor2);

          if (C3Mod.C3Players[i].Team == 1)
          {
            playersred++;
            C3Mod.C3Players[i].TSPlayer.TpLock = false;

            if (spawnPoints[0] == Vector2.Zero)
            {
              if ((int)C3Mod.C3Players[i].TSPlayer.X / 16 != (int)(spawnPoints[0].X) || (int)C3Mod.C3Players[i].TSPlayer.Y / 16 != (int)(spawnPoints[0].Y))
                TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)spawnPoints[0].X * 16, (int)spawnPoints[0].Y * 16);
            }
            else
                if ((int)C3Mod.C3Players[i].TSPlayer.X / 16 != (int)spawnPoints[0].X || (int)C3Mod.C3Players[i].TSPlayer.Y / 16 != (int)spawnPoints[0].Y)
              TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)spawnPoints[0].X * 16, (int)spawnPoints[0].Y * 16);

            if (C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
          }

          if (C3Mod.C3Players[i].Team == 2)
          {
            playersblue++;
            C3Mod.C3Players[i].TSPlayer.TpLock = false;

            if (spawnPoints[1] == Vector2.Zero)
            {
              if ((int)C3Mod.C3Players[i].TSPlayer.X / 16 != (int)(flagPoints[1].X) || (int)C3Mod.C3Players[i].TSPlayer.Y / 16 != (int)(flagPoints[1].Y))
                TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)flagPoints[1].X * 16, (int)flagPoints[1].Y * 16);
            }
            else
                  if ((int)C3Mod.C3Players[i].TSPlayer.X / 16 != (int)spawnPoints[1].X || (int)C3Mod.C3Players[i].TSPlayer.Y / 16 != (int)spawnPoints[1].Y)
              TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)spawnPoints[1].X * 16, (int)spawnPoints[1].Y * 16);

            if (C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
          }
        }

        if (playersred == 0 || playersblue == 0)
        {
          C3Tools.BroadcastMessageToGametype("ctf", "Not enough players to start CTF", Color.DarkCyan);
          CTFGameRunning = false;
          CTFGameCountdown = false;
          TpToSpawns(false);
          C3Tools.ResetGameType("ctf");
          flagPoints = new Vector2[2];
          spawnPoints = new Vector2[2];
          return 0;
        }
        return 1;
      }
      catch { return 0; }
    }

    public static void TpToSpawns(bool pvpstate)
    {
      for (int i = 0; i < C3Mod.C3Players.Count; i++)
      {
        if (C3Mod.C3Players[i].Team == 1)
        {
          C3Mod.C3Players[i].TSPlayer.TpLock = false;
          Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
          NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
          TShock.Players[C3Mod.C3Players[i].Index].Spawn();
          TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
        }
        if (C3Mod.C3Players[i].Team == 2)
        {
          C3Mod.C3Players[i].TSPlayer.TpLock = false;
          Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
          NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
          TShock.Players[C3Mod.C3Players[i].Index].Spawn();
          TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
        }
      }
    }
  }

  internal class CTFArena
  {
    public Vector2[] Flags = new Vector2[2];
    public string Name = "";
    public Vector2[] Spawns = new Vector2[2];

    public CTFArena(Vector2 redflag, Vector2 blueflag, Vector2 redspawn, Vector2 bluespawn, string name)
    {
      Flags[0] = redflag;
      Flags[1] = blueflag;
      Spawns[0] = redspawn;
      Spawns[1] = bluespawn;
      Name = name;
    }
  }
}