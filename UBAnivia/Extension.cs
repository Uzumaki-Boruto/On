﻿using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace UBAnivia
{
    static class Extension
    {
        public static int GetValue(this Menu menu, string id, bool IsSlider = true)
        {
            if (IsSlider)
                return menu[id].Cast<Slider>().CurrentValue;
            else
                return menu[id].Cast<ComboBox>().CurrentValue;
        }
        public static bool Checked(this Menu menu, string id, bool IsCheckBox = true)
        {
            if (IsCheckBox)
                return menu[id].Cast<CheckBox>().CurrentValue;
            else
                return menu[id].Cast<KeyBind>().CurrentValue;
        }
        public static bool Try_To_Gap_Me(this Obj_AI_Base target, float range = 550f)
        {
            var Path = Prediction.Position.GetRealPath(target).Last();
            return Path.Distance(Player.Instance) < Player.Instance.Distance(target) && target.IsInRange(Player.Instance, range);
        }
        public static bool Unkillable(AIHeroClient target)
        {
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "UndyingRage"))
            {
                return true;
            }
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "ChronoShift"))
            {
                return true;
            }
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "JudicatorIntervention"))
            {
                return true;
            }
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "kindredrnodeathbuff"))
            {
                return true;
            }
            if (target.HasBuffOfType(BuffType.Invulnerability))
            {
                return true;
            }
            return target.IsInvulnerable;
        }
        public static bool HasR
        {
            get
            {
                return Storm == null && Spells.R.IsReady();
            }
        }
        public static bool QActive
        {
            get
            {
                return Missile != null && Player.HasBuff("FlashFrost");
            }
        }
        public static bool Chilled(this Obj_AI_Base target)
        {
            return target.HasBuff("Chilled") || (target.IsValidTarget(Spells.R.Range + Spells.R.Radius) && Spells.R.IsReady());
        }
        public static void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsValid)
            {
                if (obj.Name == "cryo_FlashFrost_Player_mis.troy")
                {
                    Missile = obj;
                }
                if (obj.Name.Contains("cryo_storm"))
                {
                    Storm = obj;
                }
            }
        }

        public static void Obj_AI_Base_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.IsValid)
            {
                if (obj.Name == "cryo_FlashFrost_Player_mis.troy")
                    Missile = null;
                if (obj.Name.Contains("cryo_storm"))
                    Storm = null;
            }
        }
        
        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (args.Slot == SpellSlot.Q)
            {
                QMissile_End = args.End;
            }
            if (args.Slot == SpellSlot.E && Spells.R.IsReady())
            {
                var unit = args.Target as AIHeroClient;
                if (unit != null && unit.IsValidTarget() &&
                    (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)
                    || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)
                    || Config.MiscMenu.Checked("Rauto")))
                {
                    var pred = Spells.R.GetPrediction(unit);
                    Spells.R.Cast(pred.CastPosition);
                }
            }
        }
        public static void Obj_AI_Base_OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (sender.HasBuffOfType(BuffType.Stun)
                 || sender.HasBuffOfType(BuffType.Snare)
                 || sender.HasBuffOfType(BuffType.Knockup)
                 || sender.HasBuffOfType(BuffType.Charm)
                 || sender.HasBuffOfType(BuffType.Fear)
                 || sender.HasBuffOfType(BuffType.Taunt)
                 || sender.HasBuffOfType(BuffType.Suppression)
                 || sender.HasBuffOfType(BuffType.Disarm)
                 || sender.HasBuffOfType(BuffType.Flee)
                 || sender.HasBuffOfType(BuffType.Polymorph))
                {
                    if (sender.IsValidTarget(Spells.Q.Range))
                    {
                        QTarget = sender as AIHeroClient;
                        Spells.Q.Cast(sender);
                    }
                    if (sender.IsValidTarget(Spells.R.Range))
                    {
                        Spells.R.Cast(sender);
                    }
                }
                if (args.Buff.Name == "Chilled" && Config.MiscMenu.Checked("Eauto") && Spells.E.IsReady() && sender.IsValidTarget(Spells.E.Range))
                {
                    Spells.E.Cast(sender);
                }
            }
        }
        public static GameObject Missile = null;
        public static GameObject Storm = null;
        public static AIHeroClient QTarget = null;
        public static Vector3 QMissile_End = new Vector3();
        public static void MissileTracker(EventArgs args)
        {
            if (Missile == null || Missile.IsDead || QTarget == null || Spells.QActive.IsReady())
            {
                Missile = null;
                QTarget = null;
                return;
            }
            else
            {
                var target = QTarget;
                var Rectangle = new Geometry.Polygon.Rectangle(Missile.Position, QMissile_End, 110f);
                var Rectangle2 = new Geometry.Polygon.Rectangle(Missile.Position, QMissile_End, 230f);
                var pred = Prediction.Position.PredictLinearMissile(target, Missile.Distance(QMissile_End), 110, 150, Spells.Q.Speed, int.MaxValue, Missile.Position);
                if (Rectangle2.IsInside(pred.UnitPosition))
                {
                    if (Rectangle.IsInside(pred.UnitPosition))
                    {
                        Core.DelayAction(() => Spells.QActive.Cast(), (int)(Missile.Distance(Player.Instance) / Spells.Q.Speed * 1000));
                    }
                    else
                    {
                        if (Missile.IsInRange(target, 230f))
                        {
                            Spells.QActive.Cast();
                        }
                    }
                }
                else
                {
                    if (Missile.CountEnemiesInRange(230f) > 0)
                    {
                        Spells.Q.Cast();
                    }
                }
            }
            if (Storm != null && Spells.ROff.IsReady() && Config.MiscMenu["turnoffR"].Cast<CheckBox>().CurrentValue)
            {
                if (Storm.CountEnemiesInRange(Spells.RMax.Radius) == 0 && Storm.CountEnemyMinionsInRange(Spells.RMax.Radius) == 0)
                {
                    Spells.ROff.Cast();
                }
            }
        }
    }
}
