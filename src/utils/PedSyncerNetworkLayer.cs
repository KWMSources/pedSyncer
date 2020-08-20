﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Channels;
using System.Threading.Tasks;
using AltV.Net.EntitySync.Events;

namespace AltV.Net.EntitySync.ServerEvent
{
    public class PedSyncerNetworkLayer : NetworkLayer
    {
        public override event ConnectionConnectEventDelegate OnConnectionConnect;
        public override event ConnectionDisconnectEventDelegate OnConnectionDisconnect;
        public override event ClientSubscribeEntityDelegate OnClientSubscribeEntity;
        public override event ClientUnsubscribeEntityDelegate OnClientUnsubscribeEntity;
        public override event EntityCreateEventDelegate OnEntityCreate;
        public override event EntityRemoveEventDelegate OnEntityRemove;
        public override event EntityPositionUpdateEventDelegate OnEntityPositionUpdate;
        public override event EntityDataUpdateEventDelegate OnEntityDataUpdate;
        public override event EntityNetOwnerUpdateEventDelegate OnEntityNetOwnerUpdate;

        private readonly struct ServerEntityEvent
        {
            public readonly byte EventType;

            public readonly IEntity Entity;

            public readonly Vector3 Position;

            public readonly IDictionary<string, object> ChangedData;

            public readonly bool NetOwner;

            public ServerEntityEvent(byte eventType, IEntity entity, Vector3 position,
                IDictionary<string, object> changedData, bool netOwner)
            {
                EventType = eventType;
                Entity = entity;
                Position = position;
                ChangedData = changedData;
                NetOwner = netOwner;
            }

            public ServerEntityEvent(byte eventType, IEntity entity) : this(eventType,
                entity, Vector3.Zero, null, false)
            {
            }
        }

        private readonly IDictionary<IClient, Channel<ServerEntityEvent>> serverEventChannels =
            new Dictionary<IClient, Channel<ServerEntityEvent>>();

        public PedSyncerNetworkLayer(ulong threadCount, IClientRepository clientRepository) : base(threadCount,
            clientRepository)
        {
            Alt.OnPlayerConnect += (player, reason) =>
            {
                var playerClient = new PlayerClient(threadCount, player.Id.ToString(), player);
                player.SetEntitySyncClient(playerClient);
                clientRepository.Add(playerClient);
                Task.Factory.StartNew(async obj =>
                {
                    var currPlayerClient = (PlayerClient)obj;
                    var channel = Channel.CreateUnbounded<ServerEntityEvent>(new UnboundedChannelOptions
                    { SingleReader = true });
                    lock (serverEventChannels)
                    {
                        serverEventChannels[currPlayerClient] = channel;
                    }

                    var channelReader = channel.Reader;
                    while (await channelReader.WaitToReadAsync())
                    {
                        while (channelReader.TryRead(out var entityEvent))
                        {
                            try
                            {
                                switch (entityEvent.EventType)
                                {
                                    case 1:
                                        Console.WriteLine("entitySync:create " + entityEvent.Entity.Id);
                                        if (entityEvent.ChangedData != null)
                                        {
                                            currPlayerClient.Emit("entitySync:create", entityEvent.Entity.Id,
                                                entityEvent.Entity.Type, entityEvent.Entity.Position,
                                                entityEvent.ChangedData);
                                        }
                                        else
                                        {
                                            currPlayerClient.Emit("entitySync:create", entityEvent.Entity.Id,
                                                entityEvent.Entity.Type, entityEvent.Entity.Position);
                                        }

                                        break;
                                    case 2:
                                        Console.WriteLine("entitySync:remove " + entityEvent.Entity.Id);
                                        currPlayerClient.Emit("entitySync:remove", entityEvent.Entity.Id,
                                            entityEvent.Entity.Type);
                                        break;
                                    case 3:
                                        if (entityEvent.Entity.NetOwner != currPlayerClient)
                                        {
                                            Console.WriteLine("entitySync:updatePosition " + entityEvent.Entity.Id);
                                            currPlayerClient.Emit("entitySync:updatePosition", entityEvent.Entity.Id,
                                            entityEvent.Entity.Type, entityEvent.Position);
                                        }
                                        break;
                                    case 4:
                                        if (entityEvent.Entity.NetOwner != currPlayerClient)
                                        {
                                            Console.WriteLine("entitySync:updateData " + entityEvent.Entity.Id);
                                            currPlayerClient.Emit("entitySync:updateData", entityEvent.Entity.Id,
                                            entityEvent.Entity.Type, entityEvent.ChangedData);
                                        }
                                        break;
                                    case 5:
                                        Console.WriteLine("entitySync:clearCache " + entityEvent.Entity.Id);
                                        currPlayerClient.Emit("entitySync:clearCache", entityEvent.Entity.Id,
                                            entityEvent.Entity.Type);
                                        break;
                                    case 6:
                                        Console.WriteLine("entitySync:netOwner " + entityEvent.Entity.Id);
                                        currPlayerClient.Emit("entitySync:netOwner", entityEvent.Entity.Id,
                                            entityEvent.Entity.Type, entityEvent.NetOwner);
                                        break;
                                }
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception);
                            }
                        }
                    }
                }, playerClient);
            };
            Alt.OnPlayerDisconnect += (player, reason) =>
            {
                var client = clientRepository.Remove(player.Id.ToString());
                if (client == null) return;
                Channel<ServerEntityEvent> channel;
                lock (serverEventChannels)
                {
                    if (!serverEventChannels.Remove(client, out channel)) return;
                }

                channel.Writer.Complete();
            };
        }

        public override void SendEvent(IClient client, in EntityCreateEvent entityCreate)
        {
            Channel<ServerEntityEvent> channel;
            lock (serverEventChannels)
            {
                if (!serverEventChannels.TryGetValue(client, out channel)) return;
            }

            channel.Writer.TryWrite(new ServerEntityEvent(1, entityCreate.Entity, Vector3.Zero, entityCreate.Data,
                false));
        }

        public override void SendEvent(IClient client, in EntityRemoveEvent entityRemove)
        {
            Channel<ServerEntityEvent> channel;
            lock (serverEventChannels)
            {
                if (!serverEventChannels.TryGetValue(client, out channel)) return;
            }

            channel.Writer.TryWrite(new ServerEntityEvent(2, entityRemove.Entity));
        }

        public override void SendEvent(IClient client, in EntityPositionUpdateEvent entityPositionUpdate)
        {
            Channel<ServerEntityEvent> channel;
            lock (serverEventChannels)
            {
                if (!serverEventChannels.TryGetValue(client, out channel)) return;
            }

            channel.Writer.TryWrite(new ServerEntityEvent(3, entityPositionUpdate.Entity,
                entityPositionUpdate.Position, null, false));
        }

        public override void SendEvent(IClient client, in EntityDataChangeEvent entityDataChange)
        {
            Channel<ServerEntityEvent> channel;
            lock (serverEventChannels)
            {
                if (!serverEventChannels.TryGetValue(client, out channel)) return;
            }

            channel.Writer.TryWrite(new ServerEntityEvent(4, entityDataChange.Entity,
                Vector3.Zero, entityDataChange.Data, false));
        }

        public override void SendEvent(IClient client, in EntityClearCacheEvent entityClearCache)
        {
            Channel<ServerEntityEvent> channel;
            lock (serverEventChannels)
            {
                if (!serverEventChannels.TryGetValue(client, out channel)) return;
            }

            channel.Writer.TryWrite(new ServerEntityEvent(5, entityClearCache.Entity,
                Vector3.Zero, null, false));
        }

        public override void SendEvent(IClient client, in EntityNetOwnerChangeEvent entityNetOwnerChange)
        {
            Channel<ServerEntityEvent> channel;
            lock (serverEventChannels)
            {
                if (!serverEventChannels.TryGetValue(client, out channel)) return;
            }

            channel.Writer.TryWrite(new ServerEntityEvent(6, entityNetOwnerChange.Entity, Vector3.Zero, null,
                entityNetOwnerChange.State));
        }
    }
}