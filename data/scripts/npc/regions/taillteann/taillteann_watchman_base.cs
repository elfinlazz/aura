using Common.Constants;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class Taillteann_watchman_baseScript : NPCScript
{
	public override void OnLoad()
	{
		base.OnLoad();

		SetRace(10002);
		SetBody(height: 0.9999999f, fat: 1f, upper: 1f, lower: 1f);

		NPC.ColorA = 0x808080;
		NPC.ColorB = 0x808080;
		NPC.ColorC = 0x808080;		

		SetStand("monster/anim/ghostarmor/natural/ghostarmor_natural_stand_friendly");
	}
}