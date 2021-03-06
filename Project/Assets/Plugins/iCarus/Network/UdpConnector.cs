﻿using System;

using Protocol;
using FlatBuffers;
using Lidgren.Network;

namespace iCarus.Network
{
    public class UdpConnector
    {
        public string host { get { return mConfig.host; } }
        public int port { get { return mConfig.port; } }
        public string appIdentifier { get { return mConfig.netPeerConfig.AppIdentifier; } }
        public MessageDispatcher dispatcher { get { return mDispatcher; } }
        public NetConnectionStatus connectionStatus { get { return null != mNetClient ? mNetClient.ConnectionStatus : NetConnectionStatus.Disconnected; } }
        public NetClient netClient { get { return mNetClient; } }

        public delegate void OnNetStatusChanged(UdpConnector client, NetConnectionStatus status, string reason);
        public OnNetStatusChanged onNetStatusChanged;

        public class Configuration
        {
            public string host = "localhost";
            public int port = 65534;
            public NetPeerConfiguration netPeerConfig;
            public OnNetStatusChanged onNetStatusChanged;
        }

        public void Start(Configuration config)
        {
            mConfig = config;
            onNetStatusChanged = config.onNetStatusChanged;
            #if VERBOSE_DEBUG
            mConfig.netPeerConfig.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            #endif

            mNetClient = new NetClient(mConfig.netPeerConfig);
            mNetClient.Start();
        }

        public void Connect(NetOutgoingMessage hailMessage = null)
        {
            if (connectionStatus == NetConnectionStatus.Disconnected)
                mNetClient.Connect(host, port, hailMessage);
        }

        [Obsolete]
        public void Connect(string name)
        {
            if (connectionStatus == NetConnectionStatus.Disconnected)
            {
                NetOutgoingMessage approval = mNetClient.CreateMessage();
                approval.Write(name);
                mNetClient.Connect(host, port, approval);
            }
        }

        public void Stop()
        {
            if (null != mNetClient)
                mNetClient.Shutdown("Disconnected");
        }

        public void Update()
        {
            if (null == mNetClient)
                return;

            NetIncomingMessage message;
            while (null != (message = mNetClient.ReadMessage()))
            {
                try
                {
                    switch (message.MessageType)
                    {
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.ErrorMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.VerboseDebugMessage:
                            {
                                HandleDebugMessage(message);
                                break;
                            }
                        case NetIncomingMessageType.StatusChanged:
                            {
                                HandleStatusChanged(message);
                                break;
                            }
                        case NetIncomingMessageType.Data:
                            {
                                HandleData(message);
                                break;
                            }
                        default:
                            {
                                NetLog.WarnFormat(
                                    "Unhandled message type:{0}, bytes:{1}",
                                    message.MessageType,
                                    message.LengthBytes);
                                break;
                            }
                    }
                }
                catch (Exception e)
                {
                    NetLog.Exception(e);
                }
                mNetClient.Recycle(message);
            }
        }

        public NetOutgoingMessage CreateMessage()
        {
            return mNetClient.CreateMessage();
        }

        public NetOutgoingMessage CreateMessage(MessageID id, FlatBufferBuilder fbb)
        {
            NetOutgoingMessage msg = mNetClient.CreateMessage();
            msg.Write((ushort)id);
            ushort len = (ushort)fbb.Offset;
            msg.Write(len);
            msg.Write(fbb.DataBuffer.Data, fbb.DataBuffer.Position, fbb.Offset);
            return msg;
        }

		public NetSendResult SendMessage(NetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel = 0)
        {
            return mNetClient.SendMessage(msg, method, sequenceChannel);
        }

        #region internal
        void HandleDebugMessage(NetIncomingMessage message)
        {
            switch (message.MessageType)
            {
                case NetIncomingMessageType.DebugMessage:
                    {
                        NetLog.Debug(message.ReadString());
                        break;
                    }
                case NetIncomingMessageType.ErrorMessage:
                    {
                        NetLog.Error(message.ReadString());
                        break;
                    }
                case NetIncomingMessageType.WarningMessage:
                    {
                        NetLog.Warn(message.ReadString());
                        break;
                    }
                case NetIncomingMessageType.VerboseDebugMessage:
                    {
                        NetLog.Debug(message.ReadString());
                        break;
                    }
            }
        }

        void HandleStatusChanged(NetIncomingMessage message)
        {
            // <?> 接收到这个消息的时候, mClient.connectionStatus是否和status一致
            NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
            string reason = message.ReadString();
            if (null != onNetStatusChanged)
                onNetStatusChanged(this, status, reason);

            NetLog.DebugFormat(
                "connection({0}) status changed: {1}:{2}", 
                message.SenderEndPoint,
                status,
                reason);
        }

        void HandleData(NetIncomingMessage message)
        {
            MessageID id = (MessageID)message.ReadUInt16();
            ushort len = message.ReadUInt16();
            ByteBuffer byteBuffer = ByteBufferPool.Alloc(len);
            try
            {
                message.ReadBytes(byteBuffer.Data, 0, len);
                var result = dispatcher.Fire(message.SenderConnection, id, byteBuffer, message);
                if (result != MessageHandleResult.Processing)
                    ByteBufferPool.Dealloc(ref byteBuffer);
            }
            catch (Exception e)
            {
                ByteBufferPool.Dealloc(ref byteBuffer);
                NetLog.Exception("HandleData throws exception", e);
            }
        }

        NetClient mNetClient;
        Configuration mConfig;
        MessageDispatcher mDispatcher = new MessageDispatcher();
        #endregion internal
    }
}
