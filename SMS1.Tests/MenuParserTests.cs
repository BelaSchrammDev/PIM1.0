using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript
{
    // test access to the private members of the script (only in the test assembly)
    partial class Program
    {
        static string tHeadLine = "@maingridname,,<,Welcome to SMS Terminal,>,,@time,/,@date";
        public static MenuDef Parse(string definition) { return ParseMenu(definition, "HEAD"); }
        public static string[] Params(string line, int count) { return MenuParams(line, count); }
        public static string DefaultMenuDefinition { get { return new Program().defMenu; } }
    }
}

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for the menu parser: the new syntax (@site/@link/@action/@end/@title), the old syntax
    /// (@newsite/@menu/@command/@alternative/@endsite/@titleline) and the error lines.
    /// </summary>
    [TestClass]
    public class MenuParserTests
    {
        static Program.SiteDef Site(Program.MenuDef menu, string id) { return menu.Sites.Single(s => s.Id == id); }

        [TestMethod]
        public void Params_LastGetsTheRest_Trimmed_MissingAreEmpty()
        {
            CollectionAssert.AreEqual(new[] { "a", "Title, with (commas)" }, Program.Params("  @site( a ,  Title, with (commas) )", 2));
            CollectionAssert.AreEqual(new[] { "x", "", "" }, Program.Params("@link(x)", 3));
            CollectionAssert.AreEqual(new[] { "" }, Program.Params("@body", 1));
            // no closing parenthesis: up to the end of the line
            CollectionAssert.AreEqual(new[] { "a", "b" }, Program.Params("@link(a,b", 2));
        }

        [TestMethod]
        public void DefaultMenu_OldSyntax_SitesEntriesAndGaps()
        {
            var menu = Program.Parse(Program.DefaultMenuDefinition);
            Assert.AreEqual("", menu.Errors);
            Assert.IsTrue(menu.Classic);
            Assert.AreEqual(18, menu.Sites.Count);
            Assert.AreEqual("MAIN", menu.Sites[0].Id);
            // the aliases of @alternative are links
            CollectionAssert.AreEqual(new[] { "LINV_DOCK", "LPRODUCTION", "LENERGY", "LHOXY" }, Site(menu, "MAIN").Entries);
            Assert.AreEqual("INV_COMP", menu.Targets["LINV_DOCK"][1]);
            // a gap stays at its position (button 1 is empty)
            CollectionAssert.AreEqual(new[] { "", "LPROD_REF", "LPROD_ASS", "LMAIN" }, Site(menu, "DAMAGE").Entries);
            // commands are actions
            CollectionAssert.AreEqual(new[] { "ACTRL_REF", "ANOCTRL_REF", "LRESPRIO", "LPRODUCTION" }, Site(menu, "PROD_REF").Entries);
            CollectionAssert.AreEqual(new[] { "Control_all Refinerys", "PIM:ctrl_ref", "" }, menu.Targets["ACTRL_REF"]);
            // body and title line
            StringAssert.Contains(Site(menu, "PROD_REF").Body, "oreprio(sort amount,col 3)");
            Assert.AreEqual("@maingridname,,< Terminal Mainpage >,,@time,@date", Site(menu, "MAIN").TitleLine);
            Assert.AreEqual("@maingridname,,<,Welcome to SMS Terminal,>,,@time,/,@date", Site(menu, "DAMAGE").TitleLine);
        }

        [TestMethod]
        public void NewSyntax_AnyNumberOfEntries_InOrder()
        {
            var menu = Program.Parse(
                "@title(Top)\n" +
                "@site(main, Main page)\n@link(a)\n@link(b, Go to B, now)\n@action(ctrl_ref, Control all Refinerys)\n@link(c)\n@link(a)\n@action(x, Text, #blocks_on#, Lights, Hangar)\n" +
                "@body\n  line 1\n@end\n" +
                "@site(a, Page A)\n@title(Own title)\n@end\n@site(b, Page B)\n@end\n@site(c, Page C)\n@end");
            Assert.AreEqual("", menu.Errors);
            Assert.IsFalse(menu.Classic);
            var main = Site(menu, "MAIN");
            CollectionAssert.AreEqual(new[] { "LA", "LB|Go to B, now", "ACTRL_REF", "LC", "LA", "AX" }, main.Entries);
            Assert.AreEqual("Top", main.TitleLine);
            Assert.AreEqual("Own title", Site(menu, "A").TitleLine);
            Assert.AreEqual("  line 1\n", main.Body);
            // a link with its own text, the text of a site link is the site title
            CollectionAssert.AreEqual(new[] { "Go to B, now", "B" }, menu.Targets["LB|Go to B, now"]);
            CollectionAssert.AreEqual(new[] { "Page A", "A" }, menu.Targets["LA"]);
            // an action without command: the built-in #id# (control: to PIM); with command: the blocks get the rest
            CollectionAssert.AreEqual(new[] { "Control all Refinerys", "PIM:ctrl_ref", "" }, menu.Targets["ACTRL_REF"]);
            CollectionAssert.AreEqual(new[] { "Text", "#blocks_on#", "Lights, Hangar" }, menu.Targets["AX"]);
        }

        [TestMethod]
        public void SiteAndAction_WithTheSameId_AreSeparate()
        {
            var menu = Program.Parse("@site(x, Site X)\n@link(x)\n@action(x, Action X)\n@end");
            Assert.AreEqual("", menu.Errors);
            CollectionAssert.AreEqual(new[] { "LX", "AX" }, Site(menu, "X").Entries);
            Assert.AreEqual("Site X", menu.Targets["LX"][0]);
            Assert.AreEqual("Action X", menu.Targets["AX"][0]);
        }

        [TestMethod]
        public void Errors_UnknownIdsBecomeGaps()
        {
            var menu = Program.Parse("@site(main, Main)\n@link(nothere)\n@link(nothere, Text)\n@action(noaction)\n@link(main)\n@end");
            CollectionAssert.AreEqual(new[] { "", "", "", "LMAIN" }, Site(menu, "MAIN").Entries);
            StringAssert.Contains(menu.Errors, "  / !! unknown NOTHERE in MAIN\n");
            StringAssert.Contains(menu.Errors, "unknown NOACTION in MAIN");
            // classic: unknown ids too
            menu = Program.Parse("@newsite(main,Main)\n@menu(nix,,main)\n@endsite");
            CollectionAssert.AreEqual(new[] { "", "", "LMAIN", "" }, Site(menu, "MAIN").Entries);
            StringAssert.Contains(menu.Errors, "unknown NIX in MAIN");
        }

        [TestMethod]
        public void Errors_DoubleSite_MissingEnd_MixedSyntax()
        {
            var menu = Program.Parse("@site(a, A)\n@body\none\n@end\n@site(a, A2)\n@body\ntwo\n@end");
            Assert.AreEqual(1, menu.Sites.Count);
            Assert.AreEqual("one\n", menu.Sites[0].Body);
            StringAssert.Contains(menu.Errors, "double site: A");

            menu = Program.Parse("@site(a, A)\n@site(b, B)");
            StringAssert.Contains(menu.Errors, "no @end: A");
            StringAssert.Contains(menu.Errors, "no @end: B");

            menu = Program.Parse("@site(a, A)\n@menu(a)\n@link(a)\n@end");
            Assert.IsTrue(menu.Classic);
            StringAssert.Contains(menu.Errors, "mixed syntax");
        }

        [TestMethod]
        public void CommentsAndMenuLine_AreIgnored_CaseInsensitive()
        {
            var menu = Program.Parse("menu=custom\n  / a comment\n@SITE(Main, M)\n@Link(main)\n@BODY\n/ not in the body\nx\n@END");
            Assert.AreEqual("", menu.Errors);
            CollectionAssert.AreEqual(new[] { "LMAIN" }, Site(menu, "MAIN").Entries);
            Assert.AreEqual("x\n", Site(menu, "MAIN").Body);
        }

        [TestMethod]
        public void ControlActions_GoToPim_AlsoInOldMenus()
        {
            var menu = Program.Parse("@command(c1,Control all,#ctrl_ref#)\n@command(c2,No,#noctrl_ass#)\n@site(main, M)\n@action(ctrl_all, All)\n" +
                "@action(own, Own, PIM:ctrl_ref)\n@action(bas, Lights, BAS:lights_on)\n@action(on, On, #blocks_on#, Lights)\n@end");
            Assert.AreEqual("PIM:ctrl_ref", menu.Targets["AC1"][1]);
            Assert.AreEqual("PIM:noctrl_ass", menu.Targets["AC2"][1]);
            Assert.AreEqual("PIM:ctrl_all", menu.Targets["ACTRL_ALL"][1]);
            Assert.AreEqual("PIM:ctrl_ref", menu.Targets["AOWN"][1]);
            Assert.AreEqual("BAS:lights_on", menu.Targets["ABAS"][1]);
            Assert.AreEqual("#blocks_on#", menu.Targets["AON"][1]);
            // the default menu: its control commands too
            Assert.AreEqual("PIM:ctrl_ref", Program.Parse(Program.DefaultMenuDefinition).Targets["ACTRL_REF"][1]);
        }
    }
}
