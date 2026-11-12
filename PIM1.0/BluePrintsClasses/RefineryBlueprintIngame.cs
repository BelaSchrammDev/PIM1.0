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
        // to be removed ---------------------------------------------------------------------------------
        void addPrio(string reftype, RefineryBlueprint type, int prio)
        {
            if (prio == 0) prio = 1;
            if (ingotprio.ContainsKey(reftype))
            {
                IPrio.GetBlueprintPrio(ingotprio[reftype], type, true).setPrio(prio);
            }
        }

        // to be removed # end  ---------------------------------------------------------------------------------


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
                        bluePrint.ETA_String = GetEtaString(OldInputAmount, bluePrint.InputAmount, (bluePrint.AmountSnapshot - OldAmountSnapshot).TotalHours);
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
                for (int i = 0; i < ScrapTypeBlueprintNames.Length; i++) ScrapItemTypes[i] = ItemTypesConverter.ToItemType(ScrapTypeBlueprintNames[i]);
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
                InputID = ItemTypesConverter.ToPimId(iScrapType);
                InputIDName = iScrapType.SubtypeId;
                Name = InputIDName + "ToIngots";
                IsScrap = true;
                _definitionIdSet = true;   // no real blueprint definition: keep the empty id
            }
        }
    }
}
