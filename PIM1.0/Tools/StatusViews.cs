using System;
using System.Collections.Generic;

namespace IngameScript
{
    // Infos and hints of the PB display and how long they are shown. Pure (no SE types), tested in StatusViewsTests.
    // The manager infos and the text of the PB display are in StatusMessage.cs.
    partial class Program
    {
        static List<View> viewList = new List<View>();

        public abstract class View
        {
            public enum ViewType { NONE, INFO, WARNING }
            readonly DateTime burn = DateTime.Now;
            public ViewType type = ViewType.NONE;
            readonly int SecondsToLive = -1;
            public StringBuilderExtended InfoString = new StringBuilderExtended(1000);
            public View()
            {
                type = ViewType.INFO;
            }
            public View(ViewType itype)
            {
                type = itype;
            }
            public View(string itext, ViewType itype, int sec = -1)
            {
                InfoString.SetText(itext);
                type = itype;
                SecondsToLive = sec;
            }
            public double AgeInSec { get { return (DateTime.Now - burn).TotalSeconds; } }
            public virtual StringBuilderExtended GetInfoText() { return InfoString; }
            /// <summary>Removed from the display by SweepViews.</summary>
            public virtual bool IsOver() { return SecondsToLive != -1 && AgeInSec > SecondsToLive; }
        }


        public class Info : View
        {
            public Info(string text, int sec = -1) : base(text, ViewType.INFO, sec) { }
        }


        /// <summary>
        /// A hint of the PB display. It is shown as long as it is reported (SetWarning) in every cycle;
        /// SweepViews at the end of the cycle removes the hints that were not reported.
        /// The jobs that report hints therefore run before ViewManagerJob.
        /// </summary>
        public class Warning : View
        {
            public enum ID
            {
                NONE,
                CARGOUSEHEAVY,
                CARGOUSEFULL,
                CARGOMISSING,
                CARGORECOMMENDED,
                REFINERYNOTSUPPORTED
            }

            // the current cycle, advanced by SweepViews
            public static int Cycle;

            public readonly ID WarningID;
            public readonly string subType;
            int lastSeen;

            /// <param name="alternative">CARGOMISSING: another tag that takes the items, e.g. "Armory" for "Ammo"</param>
            public Warning(ID warn_ID, string isubtype = "", string alternative = "") : base(ViewType.WARNING)
            {
                WarningID = warn_ID;
                subType = isubtype;
                SetInfoString(isubtype);
                if (alternative != "") InfoString.Append(" or ...(sms," + alternative.ToLower() + ")");
                Seen();
            }

            public void Seen() { lastSeen = Cycle; }

            public override bool IsOver() { return lastSeen < Cycle; }

            void SetInfoString(string subType)
            {
                switch (WarningID)
                {
                    case ID.REFINERYNOTSUPPORTED:
                        InfoString.SetText("! refinery '", subType, "' not supported.");
                        break;
                    case ID.CARGORECOMMENDED:
                        InfoString.SetText("   - ", subType, " found. you can define cargo for it with ...(sms,", subType.ToLower(), ")");
                        break;
                    case ID.CARGOMISSING:
                        InfoString.SetText("!! please define cargo for ", subType, ". name container like ...(sms,", subType.ToLower(), ")");
                        break;
                    case ID.CARGOUSEFULL:
                        InfoString.SetText("!!!! cargo with ", subType, " is full !!!!!");
                        break;
                    case ID.CARGOUSEHEAVY:
                        InfoString.SetText("!!! cargo with ", subType, " is heavy.");
                        break;
                }
            }
        }


        public static void SetInfo(string text, int seconds = 10)
        {
            viewList.Add(new Info(text, seconds));
        }


        /// <summary>Reports a hint for this cycle, see Warning.</summary>
        public static void SetWarning(Warning.ID warnID, string subtype = "", string alternative = "")
        {
            var w = GetWarning(warnID, subtype);
            if (w == null) viewList.Add(new Warning(warnID, subtype, alternative));
            else w.Seen();
        }


        public static Warning GetWarning(Warning.ID warnID, string subtype)
        {
            foreach (var v in viewList)
            {
                var w = v as Warning;
                if (w != null && w.WarningID == warnID && w.subType == subtype) return w;
            }
            return null;
        }


        /// <summary>End of the cycle: removes the infos whose time is over and the hints not reported in this cycle.</summary>
        public static void SweepViews()
        {
            for (int i = viewList.Count - 1; i >= 0; i--) if (viewList[i].IsOver()) viewList.RemoveAt(i);
            Warning.Cycle++;
        }
    }
}
