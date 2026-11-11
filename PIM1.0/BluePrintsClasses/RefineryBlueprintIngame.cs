using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    // Ingame part of RefineryBlueprint: everything that needs Sandbox/VRage types.
    partial class Program
    {
        static string GetTimeStringFromHours(double h)
        {
            if (h < 1)
            {
                if ((h * 60) > 1) return Math.Round(h * 60, 0) + " min.";
                else return Math.Round(h * 60 * 60, 0) + " s";
            }
            if (h < 24)
            {
                return Math.Round(h, 1) + " h";
            }
            double tage = Math.Round(h / 24, 1);
            if (tage > 365)
            {
                return Math.Round(tage / 365, 1) + " years";
            }
            if (tage < 1.1) return tage + " day";
            else return tage + " days";
        }

        // to be removed ---------------------------------------------------------------------------------
        void addPrio(string reftype, RefineryBlueprint type, int prio)
        {
            if (prio == 0) prio = 1;
            if (ingotprio.ContainsKey(reftype))
            {
                IPrio.GetBlueprintPrio(ingotprio[reftype], type, true).setPrio(prio);
            }
        }

        static Dictionary<string, List<IPrio>> ingotprio = new Dictionary<string, List<IPrio>>();
        //Ip
        public class IPrio : IComparable<IPrio>
        {
            public RefineryBlueprint refineryBP;
            public int prio = 0;
            public int initp = 0;
            public int CompareTo(IPrio other)
            {
                if (other.initp == initp) return 0;
                return other.initp > initp ? 1 : -1;
            }
            public IPrio(RefineryBlueprint bluePrint, int pr = 0)
            {
                refineryBP = bluePrint;
                prio = pr;
                initp = pr;
            }
            public void setPrio(int np)
            {
                if (np > 10000) np = 10000;
                prio = np;
                if (np == 0 || initp != np) initp = np;
            }
            public static IPrio GetBlueprintPrio(List<IPrio> ingotprioList, RefineryBlueprint bp, bool newip = false)
            {
                foreach (IPrio ip in ingotprioList)
                {
                    if (ip.refineryBP == bp) return ip;
                }
                if (newip)
                {
                    IPrio np = new IPrio(bp);
                    ingotprioList.Add(np);
                    return np;
                }
                else return null;
            }
        }
        // to be removed # end  ---------------------------------------------------------------------------------


        static Dictionary<string, Dictionary<RefineryBlueprint, int>> OrePrioConfig = new Dictionary<string, Dictionary<RefineryBlueprint, int>>();


        public partial class RefineryBlueprint
        {
            static DateTime FillTimestamp = DateTime.Now;
            static MyItemType[] ScrapItemTypes;
            static Dictionary<MyItemType, RefineryBlueprint> KnowScrapTypes = new Dictionary<MyItemType, RefineryBlueprint>();


            public static void FillInputOutputAmountAndETA()
            {
                if ((DateTime.Now - FillTimestamp).TotalSeconds > 12)
                {
                    FillTimestamp = DateTime.Now;
                    foreach (var bluePrint in RefineryBlueprints)
                    {
                        var OldInputAmount = bluePrint.InputAmount;
                        bluePrint.InputAmount = Lists.Data.Inventory.GetValueOrDefault(bluePrint.InputID, 0);
                        bluePrint.OutputAmount = Lists.Data.Inventory.GetValueOrDefault(bluePrint.OutputID, 0);
                        var OldAmountSnapshot = bluePrint.AmountSnapshot;
                        bluePrint.AmountSnapshot = DateTime.Now;
                        var diff = (OldInputAmount - bluePrint.InputAmount);
                        bluePrint.ETA_String = diff < 0 ? "..." : GetTimeStringFromHours((bluePrint.InputAmount / diff) * (bluePrint.AmountSnapshot - OldAmountSnapshot).TotalHours);
                    }
                }
            }


            public static bool IsKnowScrapType(MyItemType scrapType)
            {
                if (ScrapItemTypes == null) InitScrapTypeBlueprintTypes();
                return ScrapItemTypes.Contains(scrapType);
            }


            public static RefineryBlueprint GetScrapBlueprintByItemtypeOrCreateNew(MyItemType scrapType)
            {
                if (!KnowScrapTypes.ContainsKey(scrapType))
                {
                    KnowScrapTypes.Add(scrapType, new RefineryBlueprint(scrapType));
                    RefineryBlueprints.Add(KnowScrapTypes[scrapType]);
                }
                return KnowScrapTypes[scrapType];
            }


            static void InitScrapTypeBlueprintTypes()
            {
                ScrapItemTypes = new MyItemType[ScrapTypeBlueprintNames.Length];
                for (int i = 0; i < ScrapTypeBlueprintNames.Length; i++)
                {
                    var t = ScrapTypeBlueprintNames[i];
                    var parts = t.Split(' ');
                    var newType = MyItemType.Parse(IG_ + parts[0] + "/" + parts[1]);
                    ScrapItemTypes[i] = newType;
                }
            }


            // ----------------------------------------------------------------------------------------------------------------
            MyDefinitionId _definitionId;
            bool _definitionIdSet;

            // created on first use, so loading the blueprints (BluePrintLoadJob) needs no VRage types
            public MyDefinitionId Definition_id
            {
                get
                {
                    if (!_definitionIdSet)
                    {
                        MyDefinitionId.TryParse(BluePrintDefinitionPrefix + Name, out _definitionId);
                        _definitionIdSet = true;
                    }
                    return _definitionId;
                }
            }


            // special constructor for blueprints from the AWWScrap mod
            // the blueprints wont be realtime created and canot be predefined
            public RefineryBlueprint(MyItemType iScrapType)
            {
                InputID = GetPIMItemID(iScrapType);
                InputIDName = iScrapType.SubtypeId;
                Name = InputIDName + "ToIngots";
                IsScrap = true;
                _definitionIdSet = true;   // no real blueprint definition: keep the empty id
            }
        }
    }
}
