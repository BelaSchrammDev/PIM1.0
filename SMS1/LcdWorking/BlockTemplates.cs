namespace IngameScript
{
    partial class Program
    {
        public enum BlockType { Damage, Tank, Hydrogenengine, Gasgenerator, Airvent, Assembler, Battery, Reactor, Refinery, Jumpdrive, Cargocontainer, Connector, Weapon, };

        /// <summary>
        /// Default field templates of the block lists (batterys, reactors, ...) for the styles 1..4.
        /// Pure (no SE types), tested in SMS1.Tests (BlockTemplatesTests against the templates of SMS 1.4).
        ///
        /// One string per block type, in the order of BlockType: the styles separated by '|',
        /// the lines (field templates) of a style by ';'. An empty style uses style 2.
        /// A line starting with '^' is the head line of the list, not a field template.
        /// Field template: fields separated by ',', e.g. "@fN,,@fS_B(20%),@fS_R" (see MDIbox).
        /// </summary>
        public static class BlockTemplates
        {
            static readonly string[] Templates =
            {
                // Damage
                "@fN,,@fH_R|@fN,,@fH_B(35%),@fH_R|@fN,,@fBs;@fH_B(filled),@fH_R|",
                // Tank
                "@fN,,@fV_M,@fSP,@fV_R|@fN,,@fV_M(0),@fE,@fSP,@fV_B(25%),@fV_R|@fN,,@fE,@fV,@fV_M,(18);@fSP,@fV_B(filled),@fV_R|@fN,@fE, ,@fSP;Stored:,@fV,,Capacity:,@fV_M;@fV_B(filled),@fV_R",
                // Hydrogenengine
                "@fN,,@fE,@fO|@fN,,@fO,@fChT,@fVH_B,@fVH_R|@fN,,@fE,@fO,@fO_B(20%),@fO_R;@fChT,@fVH,@fVH_B(filled),@fVH_R|@fN,,@fO_M,/,@fE;Stored:(15),@fS(30),,@fChT;Output:(15),@fO(30),@fO_B(filled),@fO_R;Tank:(15),@fVH(30),@fVH_B(filled),@fVH_R",
                // Gasgenerator
                "@fN,,Ice:,@fIa,@fE|@fN,,@fE,/ O2:,@fOB(left),/ H2:,@fHB(left),Ice:,@fIa,@fF0_B(30%),@fF0_R|@fN,,@fE,/ Bottles - O2:,@fOB(left),/ H2:,@fHB(left),Ice:,@fIa,(13);Cargo:(22),@fF0_M,@fF0_B(filled),@fF0_R|@fN,,@fE;Ice:(15),@fIa(13),Bottles:,@fOB,O2 / ,@fHB,H2,;Cargo:(22),@fF0_M,@fF0_B(filled),@fF0_R",
                // Airvent
                "@fN,,@fPz,/,@fAt,@fV_R|@fN,,@fPz,/,@fAt,@fV_B(40%),@fV_R|@fN,,Mode:,@fPz,/,@fAt,(13);,@fV_B(60%),@fV_R|Air Vent,@fE,,Mode:,@fPz(left);@fAt,@fV_B(filled),@fV_R",
                // Assembler
                "@fC,@fWM,@fN, ,@fW,@fWa(left)|@fC,@fN, ,@fWa(0),@fW(left),@fWM|@fC,@fN, ,@fWa,@fW(left);@fWM, ,Next:,@fNWa,@fNW(left)|@fN, ,@fWM;,Now:,@fWa,@fW(left);,Next:,@fNWa,@fNW(left)",
                // Battery
                "@fN,,@fS_B(30%),@fS_R|@fN, ,@fChT,@fChM,@fS_B(20%),@fS_R|@fN,,@fChT,@fS,@fS_M,(20);(20),@fChM,@fS_B(filled),@fS_R|@fN,@fS_M(0),,@fChT;@fChM,,In:,@fI,Out:,@fO;@fS,@fS_B(filled),@fS_R",
                // Reactor
                "@fN,,@fE,/,@fO|@fN, ,@fChT,@fO,@fO_B(20%),@fO_R|@fN, ,Ingots:,@fIG,@fChT,@fE,(13);@fO,@fO_B(filled),@fO_R|@fN,@fE, ,Time:,@fChT;Stored:,@fS,,Ingots:,@fIG(24);@fO,@fO_B(filled),@fO_R",
                // Refinery
                "@fC,@fN, ,@fE,/,@fW(28)|@fC,@fN, ,@fWa(0),@fW(27 Left),@fV_B(20%),@fV_R|@fC,@fN,,@fW(27),@fWa(23),Next:,@fNW(27),@fNWa(23),(13);,@fV_B(60%),@fV_R|@fN,,@fW(27),@fWa(23);,Next:,@fNW(27),@fNWa(23);@fWM(25),@fV_B(filled),@fV_R",
                // Jumpdrive
                "@fN,,@fSt,@fS_R|@fN,,Status:,@fSt,/,@fChT,(20);(20),@fS_B(filled),@fS_R|@fN,,@fSt,/,@fChT,(20);Input:(20),@fI,@fI_B(filled),@fI_R;Stored:(20),@fS,@fS_B(filled),@fS_R|",
                // Cargocontainer
                "@fN,,@fF0_R|@fN,,@fF0_M(0),@fF0_B(40%),@fF0_R|@fN,,@fF0_M,(20);@fF0,@fF0_B(filled),@fF0_R|",
                // Connector
                "@fN,,@fSh|@fN,@fE,,@fSct,@fSt(0),@fSh|@fN,,[,@fE,],@fDN(40);@fSt(50),=>,@fSh,,@fSct|",
                // Weapon
                "@fN,,@fE,/,@fWt|^,Type,(20),Shots(15);@fN,,@fE,@fWt,@fAc|@fN,,@fE,/,@fWt;Ammo:(25 right),@fAo,,Shots:,@fAc|@fN,,@fE,/,@fWt;Inventory:(30 right),@fAa,x,@fAo,,Shots:,@fAc;Cargo:(30 right),@fF0,@fF0_M,@fF0_B(filled),@fF0_R",
            };

            // the lines of the style (1..4) of the block type
            public static string[] For(BlockType type, int style)
            {
                var styles = Templates[(int)type].Split('|');
                var lines = styles[style - 1];
                return (lines == "" ? styles[1] : lines).Split(';');
            }
        }
    }
}
