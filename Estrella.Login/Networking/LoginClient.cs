﻿using System;
using System.Net.Sockets;
using System.Reflection;
using Estrella.FiestaLib.Networking;
using Estrella.Login.Handlers;
using Estrella.Util;

namespace Estrella.Login.Networking
{
    public sealed class LoginClient : Client
    {
        public bool IsAuthenticated { get; set; }
        public int AccountID { get; set; }
        public string Username { get; set; }
        public byte Admin { get; set; }
        public bool IsTransferring { get; set; }

        public LoginClient(Socket sock)
            : base(sock)
        {
            base.OnPacket += new EventHandler<PacketReceivedEventArgs>(LoginClient_OnPacket);
            base.OnDisconnect += new EventHandler<SessionCloseEventArgs>(LoginClient_OnDisconnect);
        }

        public ClientTransfer GenerateTransfer()
        {
            if (!IsAuthenticated) return null;
            string hash = Guid.NewGuid().ToString().Replace("-", "");
            return new ClientTransfer(AccountID, Username,0, Admin, this.Host, hash);
        }

        void LoginClient_OnDisconnect(object sender, SessionCloseEventArgs e)
        {
            Log.WriteLine(LogLevel.Debug, "{0} Disconnected.", base.Host);
            ClientManager.Instance.RemoveClient(this);


        }

        void LoginClient_OnPacket(object sender, PacketReceivedEventArgs e)
        {
            MethodInfo method = HandlerStore.GetHandler(e.Packet.Header, e.Packet.Type);
            if (method != null)
            {
                Action action = HandlerStore.GetCallback(method, this, e.Packet);
                Worker.Instance.AddCallback(action);
            }
            else
            {
                Log.WriteLine(LogLevel.Debug, "Header:{0} -> Type:{1}", e.Packet.Header, e.Packet.Type);
                Log.WriteLine(LogLevel.Debug, "Unhandled packet: {0}", e.Packet);
            }
        }
    }
}
