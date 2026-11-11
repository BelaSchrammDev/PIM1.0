using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for ChooseAmmo / GetAmmoPriority: which ammo a gun with several accepted types is loaded with.
    /// </summary>
    [TestClass]
    public class AmmoPriorityTests
    {
        const string Gun = "LargeGatlingTurret";
        const string Std = "AmmoMagazine Standard", Ap = "AmmoMagazine ArmorPiercing", He = "AmmoMagazine Explosive";

        static readonly List<string> Types = new List<string> { Std, Ap, He };

        readonly Dictionary<string, float> _stock = new Dictionary<string, float>();
        int _guns = 10;

        [TestInitialize]
        public void SetUp()
        {
            Program.AmmoPrioConfig.Clear();
        }

        [TestCleanup]
        public void TearDown()
        {
            Program.AmmoPrioConfig.Clear();
        }

        string Choose(List<string> types = null)
        {
            return Program.ChooseAmmo(Gun, types ?? Types, a => _stock.TryGetValue(a, out var v) ? v : 0, a => _guns);
        }

        void Configure(string ammo, int prio)
        {
            if (!Program.AmmoPrioConfig.ContainsKey(Gun)) Program.AmmoPrioConfig.Add(Gun, new Dictionary<string, int>());
            Program.AmmoPrioConfig[Gun][ammo] = prio;
        }

        // ------------------------------------------------------------------------------------ default priority

        [TestMethod]
        public void DefaultPriority_FollowsTheOrderOfTheAcceptedTypes()
        {
            Assert.AreEqual(3, Program.GetAmmoPriority(Gun, Std, 0, 3));
            Assert.AreEqual(2, Program.GetAmmoPriority(Gun, Ap, 1, 3));
            Assert.AreEqual(1, Program.GetAmmoPriority(Gun, He, 2, 3));
        }

        [TestMethod]
        public void NoAcceptedType_NoAmmo()
        {
            Assert.AreEqual("", Choose(new List<string>()));
        }

        [TestMethod]
        public void OneAcceptedType_IsUsedEvenOutOfStock()
        {
            Assert.AreEqual(Std, Choose(new List<string> { Std }));
        }

        [TestMethod]
        public void NothingInStock_NoAmmo()
        {
            Assert.AreEqual("", Choose());
        }

        [TestMethod]
        public void AllInStock_TheFirstAcceptedTypeIsUsed()
        {
            _stock[Std] = 500; _stock[Ap] = 500; _stock[He] = 500;

            Assert.AreEqual(Std, Choose());
        }

        [TestMethod]
        public void FirstTypeOutOfStock_TheNextOneIsUsed()
        {
            _stock[Ap] = 500; _stock[He] = 500;

            Assert.AreEqual(Ap, Choose());
        }

        // ------------------------------------------------------------------------------------ enough for every gun

        [TestMethod]
        public void FewMagazinesOfThePreferredType_DoNotUnloadAllGuns()
        {
            _stock[Std] = 3;      // 10 guns
            _stock[Ap] = 500;

            Assert.AreEqual(Ap, Choose());
        }

        [TestMethod]
        public void OneMagazinePerGun_IsEnough()
        {
            _stock[Std] = 10;
            _stock[Ap] = 500;

            Assert.AreEqual(Std, Choose());
        }

        [TestMethod]
        public void NoTypeHasEnough_TheHighestPriorityInStockIsUsed()
        {
            _stock[Ap] = 2;
            _stock[He] = 4;

            Assert.AreEqual(Ap, Choose());
        }

        // ------------------------------------------------------------------------------------ configured priority

        [TestMethod]
        public void ConfiguredPriority_OverridesTheDefault()
        {
            _stock[Std] = 500; _stock[Ap] = 500; _stock[He] = 500;
            Configure(He, 10);

            Assert.AreEqual(He, Choose());
        }

        [TestMethod]
        public void PriorityZero_TypeIsNotUsed()
        {
            _stock[Std] = 500; _stock[Ap] = 500;
            Configure(Std, 0);

            Assert.AreEqual(Ap, Choose());
        }

        [TestMethod]
        public void AllTypesPriorityZero_NoAmmo()
        {
            _stock[Std] = 500; _stock[Ap] = 500; _stock[He] = 500;
            Configure(Std, 0); Configure(Ap, 0); Configure(He, 0);

            Assert.AreEqual("", Choose());
        }

        [TestMethod]
        public void SamePriority_TheFirstAcceptedTypeWins()
        {
            _stock[Ap] = 500; _stock[He] = 500;
            Configure(Ap, 5); Configure(He, 5);

            Assert.AreEqual(Ap, Choose());
        }

        [TestMethod]
        public void ConfigurationOfAnotherGunType_IsIgnored()
        {
            _stock[Std] = 500; _stock[He] = 500;
            Program.AmmoPrioConfig.Add("OtherGun", new Dictionary<string, int> { { He, 99 } });

            Assert.AreEqual(Std, Choose());
        }
    }
}
