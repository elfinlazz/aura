﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using Common.Constants;
using Common.Tools;
using Common.World;
using Common.Events;
using System.Collections.Generic;

namespace World.World
{
	public enum AttackResult { None, Stunned, OutOfRange, Okay }

	public static class MabiCombat
	{
		private enum CombatStunNormalSelf { VeryFast = 450, Fast = 520, Normal = 600, Slow = 800, VerySlow = 1000 }
		private enum CombatStunNormalTarget { VeryFast = 1200, Fast = 1700, Normal = 2000, Slow = 2800, VerySlow = 3000 }

		private enum CombatStunKnockbackSelf { VeryFast = 1200, Fast = 2500, Normal = 2500, Slow = 2500, VerySlow = 2500 }
		private enum CombatStunKnockbackTarget { VeryFast = 1500, Fast = 2000, Normal = 2000, Slow = 2800, VerySlow = 3000 }

		public static SkillResult MeleeAttack(MabiCreature source, MabiEntity targetEntity)
		{
			if (source.IsStunned())
				return SkillResult.AttackStunned;

			MabiCreature target = targetEntity as MabiCreature;
			if (target == null)
				return SkillResult.None;

			if (!WorldManager.InRange(source, target, (uint)(source.RaceInfo.AttackRange + 50)))
				return SkillResult.AttackOutOfRange;

			MabiSkill skill;

			if (source.ActiveSkillId != 0)
				skill = source.GetSkill(source.ActiveSkillId);
			else
				skill = source.GetSkill(SkillConst.MeleeCombatMastery);

			var handler = Skills.GetSkillUsedHandler((SkillConst)skill.Info.Id);

			if (handler == null)
			{
				Logger.Unimplemented("Missing skill: " + skill.Info.Id);
				return SkillResult.None;
			}

			var rightHand = source.GetItemInPocket(Pocket.LeftHand1);
			var leftHand = source.GetItemInPocket(Pocket.RightHand1);

			if (leftHand != null && (leftHand.Type != ItemType.Weapon && leftHand.Type != ItemType.Weapon2))
				leftHand = null;

			var sourceAction = new CombatAction();
			sourceAction.ActionType = CombatActionType.Hit;
			sourceAction.SkillId = (SkillConst)skill.Info.Id;
			sourceAction.Creature = source;
			sourceAction.TargetId = (target != null ? target.Id : 0);
			sourceAction.DualWield = (rightHand != null && leftHand != null);

			return handler(source, target, sourceAction, skill, 0, 0);
		}

		// Old ISNOGI code, that was extended till it got out of hand xD
		//public static AttackResult MeleeAttack(MabiCreature source, MabiCreature singleTarget)
		//{
		//    if (source.IsStunned())
		//        return AttackResult.Stunned;

		//    var skillId = (SkillConst)(source.ActiveSkill < 1 ? source.RaceInfo.CombatSkill : source.ActiveSkill);
		//    ulong targetId = 0;

		//    // TODO: We could make target a params and always pass an array... range logic would have to be applied in the handlers then though.
		//    var targets = new List<MabiCreature>();
		//    if (singleTarget != null)
		//    {
		//        // TODO: Range depends on weapon as well.
		//        if (!WorldManager.InRange(source, singleTarget, (uint)(source.RaceInfo.MeleeAttackRange + 50)))
		//            return AttackResult.OutOfRange;

		//        targets.Add(singleTarget);
		//        targetId = singleTarget.Id;
		//    }
		//    else
		//    {
		//        if (skillId == SkillConst.Windmill)
		//        {
		//            // TODO: Get actual range, maybe the creature skill should be passed.
		//            targets = WorldManager.Instance.GetCreaturesInRange(source, 500);
		//            if (targets.Count < 1)
		//                return AttackResult.OutOfRange;
		//        }
		//        else
		//        {
		//            return AttackResult.None;
		//        }
		//    }

		//    source.StopMove();
		//    foreach (var target in targets)
		//        target.StopMove();

		//    var rightHand = source.GetItem(Pocket.LeftHand1 + source.WeaponSet);
		//    var leftHand = source.GetItem(Pocket.RightHand1 + source.WeaponSet);
		//    var magazine = source.GetItem(Pocket.Arrow1 + source.WeaponSet);

		//    // TODO: Check left hand for weapon.

		//    float baseAttackDamage = 0.0f;
		//    byte downHitCount = 0;

		//    if (rightHand == null && leftHand == null)
		//    {
		//        downHitCount += (byte)source.RaceInfo.DefaultDownHitCount;
		//    }
		//    else
		//    {
		//        if (rightHand != null) downHitCount += (byte)rightHand.Info.DownHitCount;
		//        if (leftHand != null) downHitCount += (byte)leftHand.Info.DownHitCount;
		//    }

		//    bool knockback = false, knockdown = false, critical = false;

		//    ushort stunTimeTarget = 0, stunTimeSource = 0;
		//    uint prevCombatActionId = 0;

		//    int atkSpeed = (rightHand != null ? rightHand.OptionInfo.AttackSpeed : source.RaceInfo.DefaultAttackSpeed);

		//    // Run max 2 times (dual wield)
		//    for (byte i = 1; i <= 2; ++i)
		//    {
		//        var combatArgs = new CombatEventArgs();

		//        // Example: Balance = 80%, 80% + (((100% - 80% = 20%) - ((100% - 80% = 20%) * 2 = 40% * 0~100% = 0~40%)) = -20~20% * 0~100%) = 60~100%
		//        // I actually have no idea if this is how the calculation works =)
		//        var rnd = RandomProvider.Get();
		//        var rndBalance = source.GetBalance();

		//        if (skillId == SkillConst.MeleeCombatMastery)
		//        {
		//            // No weapons = bare hands
		//            if (leftHand == null && rightHand == null)
		//            {
		//                baseAttackDamage = 10; // Test value

		//                combatArgs.Hit = 1;
		//                combatArgs.HitsMax = 1;
		//            }
		//            else
		//            {
		//                // In the second run we'll use the left hand
		//                var weapon = (i == 1 ? rightHand : leftHand);
		//                atkSpeed = weapon.OptionInfo.AttackSpeed;

		//                // Test calculation
		//                // TODO: Yea, this doesn't make sense any more like this.
		//                baseAttackDamage = weapon.OptionInfo.AttackMin + (weapon.OptionInfo.AttackMax - weapon.OptionInfo.AttackMin);

		//                // Is crit?
		//                if (rnd.NextDouble() <= source.GetCritical())
		//                {
		//                    baseAttackDamage *= 1.1f;
		//                    critical = true;
		//                }

		//                // If we're in loop one and left hand is null,
		//                // there won't be a second run.
		//                combatArgs.Hit = i;
		//                combatArgs.HitsMax = (byte)(leftHand == null ? 1 : 2);
		//            }
		//        }
		//        else if (skillId == SkillConst.Smash)
		//        {
		//            knockdown = true;

		//            atkSpeed += 1;

		//            if (rightHand == null && leftHand == null)
		//            {
		//                baseAttackDamage = 10;
		//            }
		//            else
		//            {
		//                baseAttackDamage = rightHand.OptionInfo.AttackMin + (rightHand.OptionInfo.AttackMax - rightHand.OptionInfo.AttackMin);
		//                if (leftHand != null)
		//                    baseAttackDamage += leftHand.OptionInfo.AttackMin + (leftHand.OptionInfo.AttackMax - leftHand.OptionInfo.AttackMin);
		//            }

		//            float multiplier = 1.5f;

		//            var rank = (SkillRank)source.Skills.Find(a => a.Info.Id == (ushort)SkillConst.Smash).Info.Rank;
		//            switch (rank)
		//            {
		//                case SkillRank.RF:
		//                case SkillRank.RE:
		//                case SkillRank.RD:
		//                case SkillRank.RC:
		//                case SkillRank.RB:
		//                case SkillRank.RA:
		//                    multiplier = 2.0f + (rank - SkillRank.RF) * 10;
		//                    break;
		//                case SkillRank.R9:
		//                case SkillRank.R8:
		//                case SkillRank.R7:
		//                case SkillRank.R6:
		//                    multiplier = 3.0f + (rank - SkillRank.R9) * 10;
		//                    break;
		//                case SkillRank.R5:
		//                case SkillRank.R4:
		//                case SkillRank.R3:
		//                case SkillRank.R2:
		//                    multiplier = 4.0f + (rank - SkillRank.R5) * 20;
		//                    break;
		//                case SkillRank.R1:
		//                    multiplier = 5.0f;
		//                    break;
		//            }

		//            baseAttackDamage *= multiplier;

		//            combatArgs.Hit = 1;
		//            combatArgs.HitsMax = 1;
		//        }
		//        else if (skillId == SkillConst.Windmill)
		//        {
		//            knockdown = true;

		//            if (rightHand == null && leftHand == null)
		//            {
		//                baseAttackDamage = 10;
		//            }
		//            else
		//            {
		//                baseAttackDamage = rightHand.OptionInfo.AttackMin + (rightHand.OptionInfo.AttackMax - rightHand.OptionInfo.AttackMin);
		//                if (leftHand != null)
		//                    baseAttackDamage += leftHand.OptionInfo.AttackMin + (leftHand.OptionInfo.AttackMax - leftHand.OptionInfo.AttackMin);
		//            }

		//            float multiplier = 1.6f; // R9

		//            baseAttackDamage *= multiplier;

		//            combatArgs.Hit = 1;
		//            combatArgs.HitsMax = 1;
		//        }
		//        else
		//        {
		//            if (source.Client != null)
		//                source.Client.Send(PacketCreator.SystemMessage(source, "Unsupported skill."));

		//            return AttackResult.Stunned;
		//        }

		//        stunTimeSource = GetStunTimeSource(atkSpeed, knockback || knockdown);

		//        // Source hits
		//        {
		//            var action = new CombatAction();
		//            action.Creature = source;
		//            //action.Enemy = target;
		//            action.TargetId = targetId;
		//            action.ActionType = CombatAction.Type.Hit;
		//            action.SkillId = skillId;
		//            action.StunTime = stunTimeSource;
		//            action.Critical = critical;
		//            action.Knockback = knockback;
		//            action.Knockdown = knockdown;
		//            //action.Finish = target.IsDead();
		//            action.DualWield = (combatArgs.HitsMax > 1);
		//            combatArgs.CombatActions.Add(action);
		//        }

		//        foreach (var target in targets)
		//        {
		//            if (target.IsStunned(true))
		//            {
		//                target.KnockBackCounter++;

		//                if (target.KnockBackCounter >= downHitCount)
		//                {
		//                    knockback = true;
		//                }
		//            }
		//            else if (!target.IsStunned())
		//            {
		//                target.KnockBackCounter = 0;
		//            }

		//            if (target.Target != source)
		//            {
		//                target.Target = source;
		//                target.BattleState = 1;
		//                WorldManager.Instance.CreatureChangeStance(target, 0);
		//            }

		//            var attackDamage = baseAttackDamage * ((1.0f - rndBalance) - ((1.0f - rndBalance) * 2 * (float)rnd.NextDouble())) * (float)rnd.NextDouble();
		//            target.TakeDamage(attackDamage);

		//            stunTimeTarget = GetStunTimeTarget(atkSpeed + 1, knockback || knockdown);

		//            source.SetStun(stunTimeSource);
		//            target.SetStun(stunTimeTarget);

		//            combatArgs.CombatActionId = _actionId++;
		//            combatArgs.PrevCombatActionId = prevCombatActionId;

		//            // Target takes damage
		//            {
		//                var action = new CombatAction();
		//                action.Creature = target;
		//                action.Enemy = source;
		//                action.ActionType = CombatAction.Type.TakeDamage;
		//                action.SkillId = skillId;
		//                action.CombatDamage = attackDamage;
		//                action.StunTime = stunTimeTarget;
		//                action.Critical = critical;
		//                action.Knockback = knockback;
		//                action.Knockdown = knockdown;
		//                action.Finish = target.IsDead();
		//                if (action.Knockback || action.Knockdown || action.Finish)
		//                {
		//                    var pos = CalculateKnockbackPos(source, target, 375);
		//                    target.SetPosition(pos.X, pos.Y);
		//                }
		//                combatArgs.CombatActions.Add(action);
		//            }
		//        }

		//        // Broadcast what we got so far
		//        WorldManager.Instance.CreatureCombatAction(source, singleTarget, combatArgs);
		//        if (skillId != SkillConst.MeleeCombatMastery)
		//        {
		//            WorldManager.Instance.Broadcast(new MabiPacket(0x7927, source.Id).PutShort((ushort)skillId), SendTargets.Range, source);
		//        }
		//        WorldManager.Instance.CreatureCombatSubmit(source, combatArgs.CombatActionId);

		//        WorldManager.Instance.CreatureStatsUpdate(source);
		//        WorldManager.Instance.CreatureStatsUpdate(singleTarget);

		//        // Break out of loop once we hit the max hits (for bare, 1h, other skills, etc)
		//        if (combatArgs.Hit == combatArgs.HitsMax || singleTarget.IsDead())
		//            break;

		//        prevCombatActionId = combatArgs.CombatActionId;
		//    }

		//    return AttackResult.Okay;
		//}

		public static ushort CalculateStunSource(int weaponSpeed, bool knockback)
		{
			if (knockback)
			{
				switch (weaponSpeed)
				{
					case 0x00: return (ushort)CombatStunKnockbackSelf.VeryFast;
					case 0x01: return (ushort)CombatStunKnockbackSelf.Fast;
					case 0x02: return (ushort)CombatStunKnockbackSelf.Normal;
					case 0x03: return (ushort)CombatStunKnockbackSelf.Slow;
					default: return (ushort)CombatStunKnockbackSelf.VerySlow;
				}
			}
			else
			{
				switch (weaponSpeed)
				{
					case 0x00: return (ushort)CombatStunNormalSelf.VeryFast;
					case 0x01: return (ushort)CombatStunNormalSelf.Fast;
					case 0x02: return (ushort)CombatStunNormalSelf.Normal;
					case 0x03: return (ushort)CombatStunNormalSelf.Slow;
					default: return (ushort)CombatStunNormalSelf.VerySlow;
				}
			}
		}

		public static ushort CalculateStunTarget(int weaponSpeed, bool knockback)
		{
			if (knockback)
			{
				switch (weaponSpeed)
				{
					case 0x00: return (ushort)CombatStunKnockbackTarget.VeryFast;
					case 0x01: return (ushort)CombatStunKnockbackTarget.Fast;
					case 0x02: return (ushort)CombatStunKnockbackTarget.Normal;
					case 0x03: return (ushort)CombatStunKnockbackTarget.Slow;
					default: return (ushort)CombatStunKnockbackTarget.VerySlow;
				}
			}
			else
			{
				switch (weaponSpeed)
				{
					case 0x00: return (ushort)CombatStunNormalTarget.VeryFast;
					case 0x01: return (ushort)CombatStunNormalTarget.Fast;
					case 0x02: return (ushort)CombatStunNormalTarget.Normal;
					case 0x03: return (ushort)CombatStunNormalTarget.Slow;
					default: return (ushort)CombatStunNormalTarget.VerySlow;
				}
			}
		}

		public static MabiVertex CalculateKnockbackPos(MabiCreature source, MabiCreature target, uint distance)
		{
			return CalculateKnockbackPos(source.GetPosition(), target.GetPosition(), distance);
		}

		public static MabiVertex CalculateKnockbackPos(MabiVertex source, MabiVertex target, uint distance)
		{
			if (source.Equals(target))
				return new MabiVertex(source.X + 1, source.Y + 1);

			double deltax = (double)target.X - source.X;
			double deltay = (double)target.Y - source.Y;

			double deltaxy = Math.Sqrt(Math.Pow(deltax, 2) + Math.Pow(deltay, 2));

			double nx = target.X + (distance / deltaxy) * (deltax);
			double ny = target.Y + (distance / deltaxy) * (deltay);

			return new MabiVertex((uint)nx, (uint)ny);
		}
	}
}
