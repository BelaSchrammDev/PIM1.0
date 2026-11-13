using Sandbox.ModAPI.Ingame;
using System;
using VRage;

namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// IGC transport for NetWorkingNode, limited to the current construct.
        /// Every message: tag NetWorkingNode.Tag, data (protocol, type, body).
        /// No message callback: Poll at the start of every run, so the messages cause no extra runs.
        /// </summary>
        public class NetWorkingIgc
        {
            public readonly NetWorkingNode Node;
            readonly IMyIntergridCommunicationSystem _igc;
            readonly IMyBroadcastListener _listener;
            double _now;

            public NetWorkingIgc(IMyIntergridCommunicationSystem igc, string scriptType, string version)
            {
                _igc = igc;
                _listener = igc.RegisterBroadcastListener(NetWorkingNode.Tag);
                Node = new NetWorkingNode(igc.Me, scriptType, version,
                    (type, body) => igc.SendBroadcastMessage(NetWorkingNode.Tag, Envelope(type, body), TransmissionDistance.CurrentConstruct),
                    (address, type, body) => igc.SendUnicastMessage(address, NetWorkingNode.Tag, Envelope(type, body)));
            }

            static MyTuple<int, string, string> Envelope(string type, string body)
            {
                return new MyTuple<int, string, string>(NetWorkingNode.Protocol, type, body);
            }

            public void Poll(TimeSpan sinceLastRun)
            {
                _now += sinceLastRun.TotalSeconds;
                Node.Update(_now);
                Drain(_listener);
                Drain(_igc.UnicastListener);
            }

            // the queues are limited: always read everything
            void Drain(IMyMessageProvider provider)
            {
                while (provider.HasPendingMessage)
                {
                    var message = provider.AcceptMessage();
                    if (message.Tag != NetWorkingNode.Tag || !(message.Data is MyTuple<int, string, string>)) continue;
                    var data = (MyTuple<int, string, string>)message.Data;
                    Node.Receive(message.Source, data.Item1, data.Item2, data.Item3);
                }
            }
        }
    }
}
