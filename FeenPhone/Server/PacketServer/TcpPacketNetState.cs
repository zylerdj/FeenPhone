﻿using Alienseed.BaseNetworkServer;
using Alienseed.BaseNetworkServer.PacketServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FeenPhone.Server.PacketServer
{
    class TcpPacketNetState : BaseTcpPacketNetState, IFeenPhonePacketNetState
    {
        public IFeenPhoneClientNotifier Notifier { get; private set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private ushort _LastPing;
        public ushort LastPing
        {
            get { return _LastPing; }
            set
            {
                _LastPing = value;
                InvokePropertyChanged("LastPing");
            }
        }

        private readonly ServerPacketHandler Handler;
        public TcpPacketNetState(System.Net.Sockets.NetworkStream stream, IPEndPoint ep, int readBufferSize)
            : base(stream, ep, readBufferSize)
        {
            Handler = new ServerPacketHandler(this);
            Notifier = new PacketClientNotificationHandler(this);
            Reader.OnReadData += OnRead;
        }

        protected override void Reader_OnBufferOverflow(object sender, BufferOverflowArgs e)
        {
            Console.WriteLine("Buffer overflow from {0}", this.ClientIdentifier);
            e.handled = true;
        }

        protected void OnRead(object sender, DataReadEventArgs args)
        {
            Queue<byte> InStream = args.data;

            if (InStream.Count > 0)
            {
                byte[] bytes = new byte[InStream.Count];

                InStream.CopyTo(bytes, 0);

                Handler.Handle(InStream);
            }

        }

        public void LoginSuccess()
        {
            Packet.WriteLoginStatus(Writer, true);

            var users = BaseServer.Users.Where(m => m != null);
            if (ServerHost.LocalClient != null)
            {
                users = users.Concat(new Alienseed.BaseNetworkServer.Accounting.IUser[] { ServerHost.LocalClient.LocalUser });
            }
            Packet.WriteUserList(Writer, users.Where(m => m.ID != this.User.ID));
        }

        public bool Login(string Username, string password)
        {
            var user = FeenPhone.Accounting.AccountHandler.Login(Username, password);

            LogLine("Login {0}: {1}", user != null ? "SUCCESS" : "FAILED", user != null ? user.Username : Username);

            if (user == null) return false;

            return LoginSetUser(user, true);
        }

        private void InvokePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propName));
        }

        IPacketWriter IFeenPhonePacketNetState.Writer
        {
            get { return (IPacketWriter)Writer; }
        }
    }
}