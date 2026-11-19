using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IngameScript.Tests
{
    /// <summary>
    /// Tests for NetWorkingNode: handshake (hello/ack/alive), peer timeout and master election.
    /// The transport is replaced by lists of the sent messages.
    /// </summary>
    [TestClass]
    public class NetWorkingNodeTests
    {
        const long Me = 500;
        // SmsHello without prio: a hello of a script without master priority (e.g. PIM 1.1d) counts as priority 0
        const string PimHello = "type=PIM;version=PIM v1.1;prio=0", SmsHello = "type=SMS;version=SMS v1.5",
            SmsLargeHello = "type=SMS;version=SMS v1.5;prio=1";
        const double Beat = Program.NetWorkingNode.HeartbeatSeconds;

        readonly List<string> _broadcasts = new List<string>();
        readonly List<string> _sent = new List<string>();
        readonly HashSet<long> _otherConstruct = new HashSet<long>();
        Program.NetWorkingNode _node;

        [TestInitialize]
        public void Init()
        {
            _node = new Program.NetWorkingNode(Me, "PIM", "PIM v1.1", 0,
                (type, body) => _broadcasts.Add(type + "|" + body),
                (address, type, body) => { _sent.Add(address + "|" + type + "|" + body); return true; },
                address => !_otherConstruct.Contains(address));
        }

        void Receive(long source, string type, string body)
        {
            _node.Receive(source, Program.NetWorkingNode.Protocol, type, body);
        }

        [TestMethod]
        public void Update_FirstRun_BroadcastsHello()
        {
            _node.Update(100);
            CollectionAssert.AreEqual(new[] { "hello|" + PimHello }, _broadcasts);
        }

        [TestMethod]
        public void Update_AfterHeartbeatInterval_BroadcastsAlive()
        {
            _node.Update(100);
            _node.Update(100 + Beat - 0.1);
            Assert.AreEqual(1, _broadcasts.Count);
            _node.Update(100 + Beat);
            Assert.AreEqual("alive|" + PimHello, _broadcasts[1]);
        }

        [TestMethod]
        public void Hello_AddsPeerAndAnswersWithAck()
        {
            _node.Update(0);
            Receive(400, "hello", SmsHello);

            var peer = _node.Peers[400];
            Assert.AreEqual("SMS", peer.ScriptType);
            Assert.AreEqual("SMS v1.5", peer.Version);
            CollectionAssert.AreEqual(new[] { "400|ack|" + PimHello }, _sent);
        }

        [TestMethod]
        public void Hello_FromKnownPeer_IsAnsweredAgain()
        {
            // the peer was recompiled and lost its peer list
            _node.Update(0);
            Receive(400, "alive", SmsHello);
            Receive(400, "hello", SmsHello);
            Assert.AreEqual(1, _sent.Count);
        }

        [DataTestMethod]
        [DataRow("ack")]
        [DataRow("alive")]
        public void AckAndAlive_AddPeerWithoutAnswer(string type)
        {
            _node.Update(0);
            Receive(400, type, SmsHello);
            Assert.IsTrue(_node.Peers.ContainsKey(400));
            Assert.AreEqual(0, _sent.Count);
        }

        [TestMethod]
        public void OtherProtocol_IsIgnored()
        {
            _node.Update(0);
            _node.Receive(400, Program.NetWorkingNode.Protocol + 1, "hello", SmsHello);
            Assert.AreEqual(0, _node.Peers.Count);
            Assert.AreEqual(0, _sent.Count);
        }

        [TestMethod]
        public void OtherProtocol_IsListedAsIncompatibleAndTimesOut()
        {
            _node.Update(0);
            _node.Receive(400, Program.NetWorkingNode.Protocol + 1, "alive", SmsHello);
            Assert.AreEqual(Program.NetWorkingNode.Protocol + 1, _node.Incompatible[400].Protocol);
            Assert.AreEqual(0, _node.Peers.Count);
            _node.Update(Program.NetWorkingNode.TimeoutSeconds + 0.1);
            Assert.AreEqual(0, _node.Incompatible.Count);
        }

        [TestMethod]
        public void OwnMessage_IsIgnored()
        {
            _node.Update(0);
            Receive(Me, "hello", PimHello);
            Assert.AreEqual(0, _node.Peers.Count);
        }

        [TestMethod]
        public void SilentPeer_IsRemovedAfterTimeout()
        {
            _node.Update(0);
            Receive(400, "alive", SmsHello);
            _node.Update(Program.NetWorkingNode.TimeoutSeconds);
            Assert.IsTrue(_node.Peers.ContainsKey(400));
            _node.Update(Program.NetWorkingNode.TimeoutSeconds + 0.1);
            Assert.IsFalse(_node.Peers.ContainsKey(400));
        }

        [TestMethod]
        public void AppMessage_GoesToItsHandlerAndRefreshesThePeer()
        {
            long source = 0;
            string body = null;
            _node.Register("infos", (s, b) => { source = s; body = b; });
            _node.Update(0);
            Receive(400, "alive", SmsHello);
            _node.Update(10);
            Receive(400, "infos", "x=1");

            Assert.AreEqual(400, source);
            Assert.AreEqual("x=1", body);
            Assert.AreEqual(10, _node.Peers[400].LastSeen);
        }

        [TestMethod]
        public void UnknownAppMessage_IsIgnored()
        {
            _node.Update(0);
            Receive(400, "whatever", "");
            Assert.AreEqual(0, _sent.Count);
        }

        [TestMethod]
        public void IsMaster_NotDuringDiscovery()
        {
            Assert.IsFalse(_node.IsMaster, "before the first update");
            _node.Update(0);
            _node.Update(Beat - 0.1);
            Assert.IsTrue(_node.IsDiscovering);
            Assert.IsFalse(_node.IsMaster);
        }

        [TestMethod]
        public void IsMaster_AloneAfterDiscovery()
        {
            _node.Update(0);
            _node.Update(Beat);
            Assert.IsTrue(_node.IsMaster);
        }

        [TestMethod]
        public void IsMaster_LowerAddressOfSameTypeWins()
        {
            _node.Update(0);
            Receive(400, "ack", PimHello);
            Receive(600, "ack", PimHello);
            _node.Update(Beat);
            Assert.IsFalse(_node.IsMaster);
            Assert.AreEqual(400, _node.MasterAddress);
        }

        [TestMethod]
        public void IsMaster_LowerAddressOfOtherTypeDoesNotCount()
        {
            _node.Update(0);
            Receive(400, "ack", SmsHello);
            Receive(600, "ack", PimHello);
            _node.Update(Beat);
            Assert.IsTrue(_node.IsMaster);
        }

        [TestMethod]
        public void IsMaster_AgainWhenTheMasterTimesOut()
        {
            _node.Update(0);
            Receive(400, "ack", PimHello);
            _node.Update(Beat);
            Assert.IsFalse(_node.IsMaster);
            _node.Update(Program.NetWorkingNode.TimeoutSeconds + 0.1);
            Assert.IsTrue(_node.IsMaster);
        }

        [TestMethod]
        public void MasterOf_IsTheLowestAddressOfThatType()
        {
            _node.Update(0);
            Receive(700, "ack", SmsHello);
            Receive(400, "ack", PimHello);
            Receive(600, "ack", SmsHello);
            Assert.AreEqual(600, _node.MasterOf("SMS"));
            Assert.AreEqual(400, _node.MasterOf("PIM"));
        }

        [TestMethod]
        public void MasterOf_MyTypeIncludesMe_UnknownTypeIsZero()
        {
            _node.Update(0);
            Receive(400, "ack", SmsHello);
            Assert.AreEqual(Me, _node.MasterOf("PIM"));
            Assert.AreEqual(0, _node.MasterOf("XYZ"));
        }

        [TestMethod]
        public void Hello_ReadsThePriority_MissingIsZero()
        {
            _node.Update(0);
            Receive(400, "hello", SmsLargeHello);
            Receive(600, "hello", SmsHello);
            Assert.AreEqual(1, _node.Peers[400].Priority);
            Assert.AreEqual(0, _node.Peers[600].Priority);
        }

        [TestMethod]
        public void MasterOf_HigherPriorityWinsOverLowerAddress()
        {
            _node.Update(0);
            Receive(400, "ack", SmsHello);
            Receive(800, "ack", SmsLargeHello);
            Receive(700, "ack", SmsLargeHello);
            // among the same priority: the lowest address
            Assert.AreEqual(700, _node.MasterOf("SMS"));
        }

        [TestMethod]
        public void IsMaster_NotWhenAPeerOfMyTypeHasAHigherPriority()
        {
            _node.Update(0);
            Receive(900, "ack", "type=PIM;version=PIM v1.2;prio=1");
            _node.Update(Beat);
            Assert.IsFalse(_node.IsMaster);
            Assert.AreEqual(900, _node.MasterAddress);
        }

        [TestMethod]
        public void OtherConstruct_IsListedButNotMaster()
        {
            _otherConstruct.Add(400);
            _node.Update(0);
            Receive(400, "hello", PimHello);
            _node.Update(Beat);

            Assert.IsFalse(_node.Peers[400].SameConstruct);
            CollectionAssert.AreEqual(new[] { "400|ack|" + PimHello }, _sent);
            Assert.IsTrue(_node.IsMaster);
        }

        [TestMethod]
        public void OtherConstruct_AppMessageIsIgnored()
        {
            var called = false;
            _node.Register("infos", (s, b) => called = true);
            _otherConstruct.Add(400);
            _node.Update(0);
            Receive(400, "alive", SmsHello);
            Receive(400, "infos", "x=1");
            Assert.IsFalse(called);
        }

        [TestMethod]
        public void SameConstruct_IsCheckedAgainWithEveryAlive()
        {
            _node.Update(0);
            Receive(400, "alive", PimHello);
            Assert.IsTrue(_node.Peers[400].SameConstruct);
            _otherConstruct.Add(400); // undocked
            Receive(400, "alive", PimHello);
            Assert.IsFalse(_node.Peers[400].SameConstruct);
        }

        [TestMethod]
        public void SendAndBroadcast_UseTheTransport()
        {
            Assert.IsTrue(_node.Send(400, "infos", "a=1"));
            _node.Broadcast("news", "b=2");
            CollectionAssert.AreEqual(new[] { "400|infos|a=1" }, _sent);
            CollectionAssert.AreEqual(new[] { "news|b=2" }, _broadcasts);
        }
    }
}
