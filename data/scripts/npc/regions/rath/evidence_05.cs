using Common.Constants;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class Evidence_05Script : NPCScript
{
	public override void OnLoad()
	{
		SetName("_evidence_05");
		SetRace(990056);
		SetBody(height: 1f, fat: 1f, upper: 1f, lower: 1f);
		SetFace(skin: 0, eye: 0, eyeColor: 0, lip: 0);

		NPC.ColorA = 0x808080;
		NPC.ColorB = 0x808080;
		NPC.ColorC = 0x808080;		



		SetLocation(region: 411, x: 10316, y: 8108);

		SetDirection(40);
		SetStand("");
	}
}