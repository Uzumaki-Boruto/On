﻿using System;
using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;

namespace UBActivator
{
    class Extensions
    {
        public static bool IsImportant(Obj_AI_Minion minion)
        {
            return minion.IsValidTarget()
                && (minion.Name.ToLower().Contains("baron")
                || minion.Name.ToLower().Contains("dragon")
                || minion.Name.ToLower().Contains("herald"));
        }
        public static bool CanUseOnChamp
        {
            get
            {
                if (Spells.Smite != null && Spells.Smite.IsReady())
                {
                    var name = Spells.Smite.ToString().ToLower();
                    if (name.Contains("smiteduel") || name.Contains("smiteplayerganker"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public static byte lv = 1;
        public static int QOff = 0, WOff = 0, EOff = 0, ROff;

    }
}
