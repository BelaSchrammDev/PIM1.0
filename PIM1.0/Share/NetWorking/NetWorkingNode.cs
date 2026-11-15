using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>A script known from its hello/alive messages.</summary>
        public class NetWorkingPeer
        {
            public long Address;
            public string ScriptType, Version;
            public double LastSeen;
            /// <summary>Same construct as me: only these count for the master and may send application messages.</summary>
            public bool SameConstruct;
        }

        /// <summary>
        /// Script-to-script networking without any game dependency (the transport is NetWorkingIgc).
        /// Handshake: "hello" (broadcast at start, everyone answers with "ack" by unicast),
        /// then "alive" every HeartbeatSeconds; a peer silent for TimeoutSeconds is removed.
        /// Master: after DiscoverySeconds the script with the lowest address of its type on my construct.
        /// Peers on other constructs are listed (Peers), but their application messages are ignored.
        /// Shared with other scripts: no references to PIM code here.
        /// </summary>
        public class NetWorkingNode
        {
            // only constants, never type or member names: minify=full renames those
            public const string Tag = "BelaOkuma.Net", Hello = "hello", Ack = "ack", Alive = "alive", KeyType = "type", KeyVersion = "version";
            public const int Protocol = 1;
            // a PB in standby (Update100) runs every ~1.67 s, so its alive comes every ~3.3 s: the timeout covers 3 of them
            public const double HeartbeatSeconds = 3, TimeoutSeconds = 10, DiscoverySeconds = HeartbeatSeconds;

            public readonly long Me;
            public readonly string ScriptType;
            public readonly Dictionary<long, NetWorkingPeer> Peers = new Dictionary<long, NetWorkingPeer>();

            // transport: (type, body) to all, (address, type, body) to one
            readonly Action<string, string> _broadcast;
            readonly Func<long, string, string, bool> _send;
            readonly Func<long, bool> _isSameConstruct;
            readonly string _helloBody;
            readonly Dictionary<string, Action<long, string>> _handlers = new Dictionary<string, Action<long, string>>();
            readonly List<long> _timedOut = new List<long>();

            double _now, _startedAt, _nextBeat;
            bool _started;

            public NetWorkingNode(long me, string scriptType, string version, Action<string, string> broadcast, Func<long, string, string, bool> send, Func<long, bool> isSameConstruct)
            {
                Me = me;
                ScriptType = scriptType;
                _broadcast = broadcast;
                _send = send;
                _isSameConstruct = isSameConstruct;
                _helloBody = KeyType + "=" + scriptType + ";" + KeyVersion + "=" + version;
            }

            /// <summary>Waiting for the answers to the first hello: no master decision yet.</summary>
            public bool IsDiscovering
            {
                get { return !_started || _now - _startedAt < DiscoverySeconds; }
            }

            /// <summary>The lowest address of my script type on my construct (me included).</summary>
            public long MasterAddress
            {
                get { return MasterOf(ScriptType); }
            }

            /// <summary>The lowest address of a script type on my construct (me included if it is my type), 0 if none is known.</summary>
            public long MasterOf(string scriptType)
            {
                var master = scriptType == ScriptType ? Me : long.MaxValue;
                foreach (var p in Peers.Values) if (p.SameConstruct && p.ScriptType == scriptType && p.Address < master) master = p.Address;
                return master == long.MaxValue ? 0 : master;
            }

            public bool IsMaster
            {
                get { return !IsDiscovering && MasterAddress == Me; }
            }

            /// <summary>Handler for an application message type (not hello/ack/alive).</summary>
            public void Register(string type, Action<long, string> handler)
            {
                _handlers[type] = handler;
            }

            public void Broadcast(string type, string body)
            {
                _broadcast(type, body);
            }

            /// <summary>False if the addressee is not reachable.</summary>
            public bool Send(long address, string type, string body)
            {
                return _send(address, type, body);
            }

            /// <summary>Every run, before the received messages: time in seconds, sends hello/alive, removes silent peers.</summary>
            public void Update(double now)
            {
                _now = now;
                if (!_started)
                {
                    _started = true;
                    _startedAt = now;
                    _nextBeat = now + HeartbeatSeconds;
                    _broadcast(Hello, _helloBody);
                }
                else if (now >= _nextBeat)
                {
                    _nextBeat = now + HeartbeatSeconds;
                    _broadcast(Alive, _helloBody);
                }

                _timedOut.Clear();
                foreach (var p in Peers.Values) if (now - p.LastSeen > TimeoutSeconds) _timedOut.Add(p.Address);
                foreach (var a in _timedOut) Peers.Remove(a);
            }

            public void Receive(long source, int protocol, string type, string body)
            {
                if (protocol != Protocol || source == Me) return;

                switch (type)
                {
                    case Hello:
                        AddPeer(source, body);
                        _send(source, Ack, _helloBody);
                        return;
                    case Ack:
                    case Alive:
                        AddPeer(source, body);
                        return;
                }

                NetWorkingPeer peer;
                if (Peers.TryGetValue(source, out peer)) peer.LastSeen = _now;
                // other constructs: only listed, no talking (yet)
                if (!_isSameConstruct(source)) return;

                Action<long, string> handler;
                if (_handlers.TryGetValue(type, out handler)) handler(source, body);
            }

            void AddPeer(long address, string body)
            {
                NetWorkingPeer peer;
                if (!Peers.TryGetValue(address, out peer))
                {
                    peer = new NetWorkingPeer { Address = address };
                    Peers[address] = peer;
                }
                peer.ScriptType = NetWorkingParser.GetValue(body, KeyType);
                peer.Version = NetWorkingParser.GetValue(body, KeyVersion);
                peer.LastSeen = _now;
                // checked again with every alive: docking/undocking changes it
                peer.SameConstruct = _isSameConstruct(address);
            }
        }
    }
}
