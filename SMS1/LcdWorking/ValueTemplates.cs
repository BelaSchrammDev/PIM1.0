using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        // Field templates of the value, gas, solar, inventory, resource and power commands, as data like BlockTemplates:
        // the styles 1..4 separated by '|' (empty: style 2), the lines (field templates) of a style by ';',
        // a line starting with '^' is the head line. Placeholders: '$' = the field (e.g. "@fS" -> "@fS_B(20%)"),
        // '#' = the prefix (first fields, or the label of a power line). Pure (no SE types), tested in SMS1.Tests (ValueTemplatesTests).

        public const string
            // a value with max, bar and ratio (thrust, damage, power values); prefix: name/grid fields
            ValueTemplate = "#,$(0),$_R|#,$(0),$_M,$_B(20%),$_R|#,$(0),$_M,(19);,$_B(60%),$_R|#,$(0),$_R;,$_B(70%)",
            // hydrogen, oxygen, cargo volume: field @fVH, @fVO, @fVC
            HoxyTemplate = "@fG,,$(0),$_M(0),$_R|@fG,,$,$_M,$_B(28%),$_R|@fG,,$(0),$_M,(20);,$_B(60%),$_R|@fG,,$(0);$_B(filled),$_R",
            SolarTemplate = "@fG,@fM(0),,@fO(0),@fO_R|@fG,@fM(0),,@fO(0),@fO_M,@fO_B(18%),@fO_R|@fM,,Max:,@fM_M,@fM_B(30%),@fM_R;@fG,,Out:,@fO,@fO_B(30%),@fO_R|",
            InventoryTemplate = "@fN,,@fAm(0)|@fAm(40%),@fN,|,@fN,@fAm|@fAm,,@fN",
            ResourcesTemplate = "^,Ingot,(21),Ore,(7);,@fN,:,@fRi,/,@fRo|^,(40),Ore ,(6),Ref.,(5),Ingot,;,@fN(30),@fRo,@fRr,@fRi,||",
            // one power line (output, input, stored); prefix: the label
            PowerLineTemplate = "#,$,/,$_M|#(18),$,$_B(20%),$_R|#(18),$,$_M,$_R|#(18),$,$_M,$_B(20%),$_R",
            // power table: the width of an empty cell (also in the head line) and a cell with value
            PowerTableWidths = "(20)|(31)|(31),(20)|(31),(31)",
            PowerTableCell = "$_R|$|$,$_R|$,$_M";

        // the lines of the style (1..4), with field and prefix filled in
        public static string[] StyleLines(string templates, int style, string field = "", string prefix = "")
        {
            var styles = templates.Split('|');
            var lines = styles[style - 1];
            return (lines == "" ? styles[1] : lines).Replace("$", field).Replace("#", prefix).Split(';');
        }

        // the power values of the commands: S stored, M available (max), E output, I input; others (cargo F01) have no field
        static string PowerField(char value) { var i = "SMEI".IndexOf(value); return i < 0 ? "" : "@f" + "SMOI"[i]; }

        // the lines of a power variable: output, input (if it has one), stored (if it has one); line: the command has a line
        public static List<string> PowerLines(int style, bool hasInput, bool hasStored, bool line)
        {
            var result = new List<string>();
            var first = "@fN,";
            if (style == 4)
            {
                result.Add("@fN," + (line ? Cfilled : ""));
                first = "";
            }
            var labels = (style == 1 ? "out:|in:|stored:" : "Output:|Input:|Stored:").Split('|');
            for (int i = 0; i < 3; i++)
                if (i == 0 || (i == 1 ? hasInput : hasStored))
                {
                    result.Add(first + "," + StyleLines(PowerLineTemplate, style, "@f" + "OIS"[i], labels[i])[0]);
                    first = "";
                }
            return result;
        }

        public static string PowerTableHeadline(int style, string values, bool grid)
        {
            var result = (grid ? "Grid," : "") + ",Type";
            foreach (var c in values)
            {
                var i = "SMEI".IndexOf(c);
                result += ",|," + (i < 0 ? "" : "Stored|Avali.|Output|Input".Split('|')[i]) + StyleLines(PowerTableWidths, style)[0];
            }
            return result;
        }

        // has: the variable has this value
        public static string PowerTableRow(int style, string values, Func<char, bool> has)
        {
            var result = "";
            foreach (var c in values)
                result += ",|," + StyleLines(has(c) ? PowerTableCell : PowerTableWidths, style, PowerField(c))[0];
            return result;
        }
    }
}
