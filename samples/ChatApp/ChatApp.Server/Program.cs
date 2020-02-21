using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Grpc.Core;
using Libplanet;
using Libplanet.Action;
using Libplanet.Blocks;
using Libplanet.Crypto;
using Libplanet.Tx;
using MagicOnion.Client;
using MagicOnion.Hosting;
using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nekoyume.Action;
using Nekoyume.Model.Item;
using Nekoyume.Shared.Hubs;
using static Nekoyume.Action.ActionBase;

namespace ChatApp.Server
{
    class Program
    {
        static async Task Main(string[] _)
        {
            var privateKey = new PrivateKey();
            var chainHost = ChatService._blockChainHost;

            var host = MagicOnionHost
                .CreateDefaultBuilder(useSimpleConsoleLogger: true)
                .Build();
            
            Task miner = chainHost.Mine();
            Task monitor = Task.Run(async () => 
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    Console.WriteLine($"State: {chainHost.GetState(privateKey.PublicKey.ToAddress())}");
                }
            });
            
            Task player = Task.Run(async () =>
            {
                var playerPrivateKey = new PrivateKey();
                var playerAddress = playerPrivateKey.PublicKey.ToAddress();
                var avatarAddress = new PrivateKey().PublicKey.ToAddress();
                
                await Task.Delay(TimeSpan.FromSeconds(5));
                chainHost.StageTransaction(
                    Transaction<PolymorphicAction<ActionBase>>.Create(
                        chainHost.GetNextTxNonce(playerAddress),
                        playerPrivateKey,
                        new PolymorphicAction<ActionBase>[]
                        {
                            new CreateAvatar 
                            {
                                index = 0,
                                hair = 0,
                                lens = 0,
                                name = avatarAddress.ToHex().Substring(0, 20),
                                avatarAddress = avatarAddress
                            }
                        }
                    )
                );
                
                while (true)
                {
                    Console.WriteLine($"Avatar: {chainHost.GetState(avatarAddress)}");
                    chainHost.StageTransaction(
                        Transaction<PolymorphicAction<ActionBase>>.Create(
                            chainHost.GetNextTxNonce(playerAddress),
                            playerPrivateKey,
                            new PolymorphicAction<ActionBase>[]
                            {
                                new HackAndSlash
                                {
                                    avatarAddress = avatarAddress,
                                    completedQuestIds = new List<int>(),
                                    equipments = new List<Equipment>(),
                                    foods = new List<Consumable>(),
                                    worldId = 1,
                                    stageId = 1,
                                    WeeklyArenaAddress = new PrivateKey().PublicKey.ToAddress()
                                }
                            }
                        )
                    );
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            });

            Task dummy = Task.Run(async () =>
            {
                var client = StreamingHubClient.Connect<IActionEvaluationHub, IActionEvaluationHubReceiver>(
                    new Channel("localhost", 12345, ChannelCredentials.Insecure), 
                    null
                );
                await client.JoinAsync();
                
                chainHost.TipChanged += async (o, index) =>
                {
                    await client.UpdateTipAsync(index);
                };
                var renderer = new ActionRenderer(RenderSubject, UnrenderSubject);
                renderer.EveryRender<ActionBase>().Subscribe(async ev =>
                {
                    var formatter = new BinaryFormatter();
                    using (var s = new MemoryStream())
                    {
                        formatter.Serialize(s, ev);
                        await client.BroadcastAsync(s.ToArray());
                    }
                });

                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });

            await Task.WhenAll(
                host.StartAsync(),
                miner,
                monitor
            );
        }
    }
}
