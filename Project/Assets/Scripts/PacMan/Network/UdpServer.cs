﻿using Lidgren.Network;
using Foundation;
using iCarus.Network;
using iCarus.Singleton;

namespace PacMan
{
    public sealed class UdpServer : SingletonBehaviour<UdpServer>
    {
        public ConnectionManager connectionManager { get { return mConnectionManager; } }
        public MessageDispatcher messageDispatcher { get { return mUdpListener.dispatcher; } }

        public void Listen()
        {
            UdpListener.Configuration netConfig = new UdpListener.Configuration()
            {
                appIdentifier = AppConfig.Instance.pacMan.appIdentifier,
                port = AppConfig.Instance.pacMan.port,
                maxConnections = AppConfig.Instance.pacMan.maxConnection,
                onIncommingConnection = OnIncommingConnection,
                onConnectionStatusChanged = OnConnectionStatusChanged,
            };
            mUdpListener.Start(netConfig);
        }

        void Update()
        {
            mUdpListener.Update();
        }

        void OnDestroy()
        {
            mUdpListener.Stop();
        }

        bool OnIncommingConnection(NetConnection connection, string name, out string denyReason)
        {
            denyReason = null;
            GameLog.InfoFormat("Incomming connection {0} - {1}", connection.RemoteEndPoint, name);
            return true;
        }

        void OnConnectionStatusChanged(NetConnection connection, string reason)
        {
            GameLog.InfoFormat("Connection status changed {0} {1} {2}", connection.RemoteEndPoint, connection.Status, reason);
        }

        ConnectionManager mConnectionManager = new ConnectionManager();
        UdpListener mUdpListener = new UdpListener();
    }
}