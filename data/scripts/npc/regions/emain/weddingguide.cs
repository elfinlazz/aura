using Common.Constants;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class WeddingguideScript : NPCScript
{
	public override void OnLoad()
	{
		SetName("_weddingguide");
		SetRace(10002);
		SetBody(height: 0.7699999f, fat: 1.02f, upper: 1f, lower: 1.02f);
		SetFace(skin: 15, eye: 12, eyeColor: 8, lip: 2);

		NPC.ColorA = 0x0;
		NPC.ColorB = 0x0;
		NPC.ColorC = 0x0;		

		EquipItem(Pocket.Face, 0x1324, 0xF37078, 0xD98B22, 0x4366);
		EquipItem(Pocket.Hair, 0xFC7, 0xFFB39897, 0xFFB39897, 0xFFB39897);
		EquipItem(Pocket.Armor, 0x3B14, 0xFFEFE7EF, 0xFF3D4363, 0xFF713E5E);
		EquipItem(Pocket.Shoe, 0x4272, 0xFF1D1C31, 0xFFA4755B, 0xFFFFFFFF);

		SetLocation(region: 61, x: 6068, y: 5964);

		SetDirection(20);
		SetStand("");
	}
}