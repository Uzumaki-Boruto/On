﻿using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Notifications;
using SharpDX;

namespace UBZilean
{
    class More
    {
        public static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Zilean") return;

            Config.Dattenosa();
            Spells.InitSpells();
            InitEvents();
        }
        private static void InitEvents()
        {
            if (Config.DrawMenu["notif"].Cast<CheckBox>().CurrentValue && Config.DrawMenu["draw"].Cast<CheckBox>().CurrentValue)
            {
                var notStart = new SimpleNotification("UBZilean Load Status", "UBZilean sucessfully loaded.");
                Notifications.Show(notStart, 5000);
            }

            Game.OnTick += GameOnTick;
            Game.OnUpdate += Mode.AutoR;
            Game.OnUpdate += Mode.AutoHarass;
            Game.OnUpdate += Mode.Killsteal;
            //Game.OnWndProc += Flee.Game_OnWndProc;

            Orbwalker.OnPostAttack += Mode.Orbwalker_OnPostAttack;
            Gapcloser.OnGapcloser += Mode.Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Mode.Interrupter_OnInterruptableSpell;

            if (Config.DrawMenu["draw"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.OnDraw += OnDraw;
                Drawing.OnEndScene += Damages.Damage_Indicator;
            }
        }       

        private static void GameOnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;
            if (Player.Instance.IsDead) return;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            { Mode.Combo(); }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            { Mode.Harass(); }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            { Mode.LaneClear(); }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            { Mode.JungleClear(); }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            { Mode.Flee(); }

        }
        private static void OnDraw(EventArgs args)
        {
            if (Config.DrawMenu["Qdr"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Spells.Q.IsLearned ? Color.HotPink : Color.Zero, Spells.Q.Range, Player.Instance.Position);
            }            
            if (Config.DrawMenu["Edr"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Spells.E.IsLearned ? Color.AliceBlue : Color.Zero, Spells.E.Range, Player.Instance.Position);
            }
        }
    }
}
