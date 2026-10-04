using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public static class TerminalBlockExtensions
    {
        public static bool IsMainConstruct(this IMyTerminalBlock block)
        {
            return block.IsSameConstructAs(Program.MyProgram.Me);
        }
    }
}
