using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        static string fT(double h)
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
        static string Fd(double d) { return F_D(d); }
        static string Fdp(double d) { return F_(d, pD); }
        static string F_D(double d, bool n = true) { return (d == 0 && !n) ? " " : d.ToString("0."); }
        static string Fgm(double d) { return Fs(d, "0.#", " m/s"); }
        static string Fkh(double d) { return Fs((d * 3.6f), "0.#", " km/h"); }
        static string Fmh(double d) { return Fs(((d * 3.6f) * 0.621f), "0.#", " mph"); }
        static string Fs(double d, string s, string p) { return d.ToString(s) + p; }
        static string Fp(double d) { return F_(d * 1000, pP); }
        static string Fph(double d) { return F_(d * 1000, pPh); }
        static string Fv(double fv) { return F_(fv, pV); }
        static string Fm(double d) { return F_(d, pM); }
        static string[]
            pD = new string[] { " 0.# m  " + SoftHyphen, " 0.#    " + SoftHyphen, " 0.# k " + SoftHyphen, " 0.# M" + SoftHyphen },
            pM = new string[] { " 0.0 g  ", " 0.0 kg", " 0.0 T  ", " 0.0 kT" },
            pV = new string[] { " 0.# mL", " 0.# L  ", " 0.# kL", " 0.# Ml" },
            pN = new string[] { " 0.# mN ", " 0.# N   ", " 0.# kN ", " 0.# MN" },
            pP = new string[] { " 0.#   W     ", " 0.0 kW   ", " 0.# MW  ", " 0.# GW   " },
            pPh = new string[] { " 0.#   Wh   ", " 0.0 kWh ", " 0.# MWh", " 0.# GWh " };
        static string F_(double a, string[] p)
        {
            if (a > 900000.0f) return (a / 1000000).ToString(p[3]);
            else if (a > 900.0f) return (a / 1000).ToString(p[2]);
            else if (a < 1.0f) (a * 1000).ToString(p[0]);
            return a.ToString(p[1]);
        }
        static string Fn(double d) { return F_(d, pN); }
        static string getPanelSubtype(IMyTextPanel tp) { return tp.BlockDefinition.ToString().Split('/')[1]; }
        static double gP(IMyTerminalBlock t, int i = 1) { var l = new List<string>(t.DetailedInfo.Split('\n')); if (l.Count > i) return getPstr(l[i]); return 0; }
        static double getPstr(string lstr)
        {
            var pl = lstr.Split(':')[1].Trim().Split(' ');
            double rd = Convert.ToDouble(pl[0]);
            if (pl[1] == "kW") return rd / 1000;
            else if (pl[1] == "MW") return rd;
            else return rd / 1000000;
        }
        static int getIntegerWithBounds(string cstr, int def = 0, int min = 0, int max = 100)
        {
            string tstr = cstr.Trim().Split('.')[0];
            int result;
            float wr;
            float.TryParse(tstr, out wr);
            result = (int)wr;
            if (result < min) result = min;
            if (result > max) result = max;
            return result;
        }
        static int gINT(string s)
        {
            var r = 0;
            var l = s.Split(':');
            if (l.Length > 1)
            {
                float w;
                if (float.TryParse(l[1].Trim().Split(' ')[0], out w)) r = (int)w;
            }
            return r;
        }
        static char B6C(Base6Directions.Direction co) { char o = ' '; switch (co) { case Base6Directions.Direction.Forward: o = 'F'; break; case Base6Directions.Direction.Backward: o = 'B'; break; case Base6Directions.Direction.Up: o = 'U'; break; case Base6Directions.Direction.Down: o = 'D'; break; case Base6Directions.Direction.Left: o = 'L'; break; default: o = 'R'; break; } return o; }
        static Vector3I getUpP(IMyCubeBlock cb) { int difp = (cb.Max.Y - cb.Min.Y) + 1; var upp = new Vector3I { }; upp.X = cb.Position.X; upp.Y = cb.Position.Y; upp.Z = cb.Position.Z; switch (B6C(cb.Orientation.Up).ToString()) { case "U": upp.Y -= difp; break; case "D": upp.Y += difp; break; case "F": upp.Z += difp; break; case "B": upp.Z -= difp; break; case "R": upp.X -= difp; break; case "L": upp.X += difp; break; } return upp; }
        static void w2ts(IMyTextSurface tp, string text, float fs, float xp) { tp.TextPadding = xp; tp.FontSize = fs; tp.Alignment = TextAlignment.LEFT; tp.ContentType = ContentType.TEXT_AND_IMAGE; tp.WriteText(text); }
    }
}
