using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using TShockAPI;

namespace C3Mod
{
	public class Team
	{
		public Team(string name, short index, Color teamColor)
		{
			Name = name;
			Index = index;
			TeamColor = teamColor;
		}

		public string Name { get; }
		public short Index { get; }
		public Color TeamColor { get; }
	}

}