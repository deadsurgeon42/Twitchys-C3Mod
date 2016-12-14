﻿using System;
using System.Collections.Generic;

namespace C3Mod
{
	public delegate void PvPDeathEventHandler(DeathArgs e);

	public delegate void GameEndEventHandler(GameEndArgs e);

	public delegate void FlagCaptureHandler(FlagCaptureArgs e);

	public delegate void FlagGrabbedHandler(FlagGrabbedArgs e);

	public delegate void ApocalypseWaveAdvanceHandler(ApocalypseWaveAdvanceArgs e);

	public class C3Events
	{
		public static event PvPDeathEventHandler OnPvPDeath;

		public static event GameEndEventHandler OnGameEnd;

		public static event FlagCaptureHandler OnFlagCapture;

		public static event FlagGrabbedHandler OnFlagGrabed;

		public static event ApocalypseWaveAdvanceHandler OnApocWaveAdvance;

		internal static void Death(C3Player killer, C3Player killed, string gametype, bool pvpkill)
		{
			var e = new DeathArgs();
			e.Killer = killer;
			e.Killed = killed;
			e.GameType = gametype;
			e.PvPKill = pvpkill;
			OnPvPDeath?.Invoke(e);
		}

		internal static void GameEnd(List<C3Player> winningteamplayers, List<C3Player> losingteamplayers, string gametype,
			int winningteamscore, int losingteamscore)
		{
			var e = new GameEndArgs();
			e.WinningTeamPlayers = winningteamplayers;
			e.LosingTeamPlayers = losingteamplayers;
			e.GameType = gametype;
			e.WinningTeamScore = winningteamscore;
			e.LosingTeamScore = losingteamscore;
			OnGameEnd?.Invoke(e);
		}

		internal static void FlagCapture(C3Player who, string gametype, string whoscored, int capturedteamscore,
			int otherteamscore)
		{
			var e = new FlagCaptureArgs();
			e.Who = who;
			e.GameType = gametype;
			e.WhoScored = whoscored;
			e.CapturedTeamScore = capturedteamscore;
			e.OtherTeamScore = otherteamscore;
			OnFlagCapture?.Invoke(e);
		}

		internal static void FlagGrabbed(C3Player who, string gametype)
		{
			var e = new FlagGrabbedArgs();
			e.Who = who;
			e.GameType = gametype;
			OnFlagGrabed?.Invoke(e);
		}

		internal static void WaveAdvance(List<C3Player> aliveplayers, List<C3Player> spectatingplayers, int nextwave)
		{
			var e = new ApocalypseWaveAdvanceArgs();
			e.AlivePlayers = aliveplayers;
			e.SpectatingPlayers = spectatingplayers;
			e.NextWave = nextwave;
			OnApocWaveAdvance?.Invoke(e);
		}

		internal static void VoteEvent(C3Player player, bool vote, bool join, string gametype)
		{
			var e = new VoteArgs();
			e.GameType = gametype;
			e.IsCallingVote = vote;
			e.IsJoiningVote = join;
			e.Player = player;
		}
	}

	public class DeathArgs : EventArgs
	{
		public string GameType;
		public C3Player Killed;
		public C3Player Killer;
		public bool PvPKill;
	}

	public class GameEndArgs : EventArgs
	{
		public string GameType;
		public List<C3Player> LosingTeamPlayers;
		public int LosingTeamScore;
		public List<C3Player> WinningTeamPlayers;
		public int WinningTeamScore;
	}

	public class FlagCaptureArgs : EventArgs
	{
		public int CapturedTeamScore;
		public string GameType;
		public int OtherTeamScore;
		public C3Player Who;
		public string WhoScored;
	}

	public class FlagGrabbedArgs : EventArgs
	{
		public string GameType;
		public C3Player Who;
	}

	public class ApocalypseWaveAdvanceArgs : EventArgs
	{
		public List<C3Player> AlivePlayers;
		public int NextWave;
		public List<C3Player> SpectatingPlayers;
	}

	public class VoteArgs : EventArgs
	{
		public string GameType;
		public bool IsCallingVote;
		public bool IsJoiningVote;
		public C3Player Player;
	}
}