using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Blocks;
using Libplanet.Crypto;
using Libplanet.Net;
using Libplanet.Store;
using Libplanet.Tx;
using Nekoyume.Action;

namespace ChatApp.Server
{
    public class BlockChainHost
    {
        private PrivateKey _privateKey;
        private BlockChain<PolymorphicAction<ActionBase>> _blockChain;
        private DefaultStore _store;

        public event EventHandler<long> TipChanged;

        public BlockChainHost(
            PrivateKey privateKey,
            string storagePath,
            Block<PolymorphicAction<ActionBase>> genesis,
            int appProtocolVersion
        )
        {
            _privateKey = privateKey;
            _store = new DefaultStore(storagePath);
            _blockChain = new BlockChain<PolymorphicAction<ActionBase>>(
                new BlockPolicy<PolymorphicAction<ActionBase>>(
                    new RewardGold { Gold = 1},
                    TimeSpan.FromSeconds(10),
                    100000,
                    2048
                ),
                _store,
                genesis
            );
            
            _blockChain.TipChanged += (_, args) =>
            {
                TipChanged?.Invoke(this, args.Index);
            };
            
            // TODO
            /*
            _swarm = new Swarm<PolymorphicAction<ActionBase>>(
                _blockChain, 
                _privateKey, 
                appProtocolVersion
            );
            */
        }

        public async Task Mine(CancellationToken cancellationToken = default)
        {
            Address address = _privateKey.PublicKey.ToAddress();
            while (!cancellationToken.IsCancellationRequested)
            {
                var block = await _blockChain.MineBlock(address, DateTimeOffset.UtcNow, cancellationToken);
                Console.WriteLine($"Block[{block}] mined. [tx count: {block.Transactions.Count()}]");
            }
        }

        public void StageTransaction(Transaction<PolymorphicAction<ActionBase>> tx)
        {
            _blockChain.StageTransactions(new[] { tx }.ToImmutableHashSet());
        }

        public IValue GetState(Address address)
        {
            return _blockChain.GetState(address);
        }

        public long GetNextTxNonce(Address address)
        {
            return _blockChain.GetNextTxNonce(address);
        }
    }
}
