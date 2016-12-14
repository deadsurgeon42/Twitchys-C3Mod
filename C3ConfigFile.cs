using System;
using System.IO;
using Newtonsoft.Json;

namespace C3Mod
{
	public class C3ConfigFile
	{
		internal static Action<C3ConfigFile> ConfigRead;

		public bool C3TeamsLocked = true;
		public bool CTFEnabled = false;
		public int CTFScoreLimit = 3;
		public int DuelNotifyInterval = 7;

		public int DuelScoreLimit = 3;

		public bool DuelsEnabled = false;

		public int DuelTimesToNotify = 5;
		public int FFAScorelimit = 10;

		public int FFASpawnProtectionTime = 10;
		public bool FreeForAllEnabled = false;

		public bool HealPlayersOnFlagCapture = false;
		public bool MonsterApocalypseEnabled = false;
		public int MonsterApocalypseIntermissionTime = 20;

		public int MonsterApocalypseLivesPerWave = 1;
		public int MonsterApocalypseMinimumPlayers = 1;
		public int MonsterApocalypseScoreNotifyInterval = 15;
		public bool OneFlagEnabled = false;
		public int OneFlagScorelimit = 3;
		public bool ReCountdownOnFlagCapture = false;
		public bool RespawnPlayersOnFlagCapture = false;
		public bool ShowWelcomeMessage = true;

		public int TDMScoreNotifyInterval = 30;
		public int TeamColor1 = 1;
		public int TeamColor2 = 3;
		public bool TeamDeathmatchEnabled = false;
		public int TeamDeathmatchScorelimit = 20;
		public bool TPLockEnabled = true;
		public int VoteMinimumPerTeam = 1;
		public int VoteNotifyInterval = 7;

		public int VoteTime = 30;

		internal static C3ConfigFile Read(string path)
		{
			if (!File.Exists(path))
				return new C3ConfigFile();
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return Read(fs);
			}
		}

		internal static C3ConfigFile Read(Stream stream)
		{
			using (var sr = new StreamReader(stream))
			{
				var cf = JsonConvert.DeserializeObject<C3ConfigFile>(sr.ReadToEnd());
				ConfigRead?.Invoke(cf);
				return cf;
			}
		}

		internal void Write(string path)
		{
			using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				Write(fs);
			}
		}

		internal void Write(Stream stream)
		{
			string str = JsonConvert.SerializeObject(this, Formatting.Indented);
			using (var sw = new StreamWriter(stream))
			{
				sw.Write(str);
			}
		}
	}
}