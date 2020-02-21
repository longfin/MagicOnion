using System.IO;
using Bencodex;
using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Blocks;
using Libplanet.Crypto;
using Libplanet.Tx;
using MagicOnion;
using MagicOnion.Server;
using Nekoyume.Action;
using Nekoyume.Shared.Services;

namespace ChatApp.Server
{
    public class ChatService : ServiceBase<IBlockChainService>, IBlockChainService
    {
        internal static BlockChainHost _blockChainHost;

        public ChatService()
        {
        }

        static ChatService()
        {
            Block<PolymorphicAction<ActionBase>> genesis = 
                Block<PolymorphicAction<ActionBase>>.Deserialize(
                    File.ReadAllBytes(@"C:\Users\Swen Mun\Documents\nekoyume-unity\nekoyume\Assets\StreamingAssets\genesis-block-dev")
                );
            var privateKey = new PrivateKey();
            _blockChainHost = new BlockChainHost(
                privateKey,
                @"C:\Users\Swen Mun\AppData\Local\planetarium\9c-standalone",
                genesis,
                1
            );
        }

        public UnaryResult<bool> PutTransaction(byte[] txBytes)
        {
            Transaction<PolymorphicAction<ActionBase>> tx = 
                Transaction<PolymorphicAction<ActionBase>>.Deserialize(txBytes);

            try
            {
                tx.Validate();
                _blockChainHost.StageTransaction(tx);
            
                return UnaryResult(true);
            }
            catch (InvalidTxException)
            {
                return UnaryResult(false);
            }
        }
        
        public UnaryResult<byte[]> GetState(byte[] addressBytes)
        {
            Address address = new Address(addressBytes);
            IValue state = _blockChainHost.GetState(address);
            byte[] encoded = new Codec().Encode((state is null) ? new Null() : state);
            return UnaryResult(encoded);
        }

        public UnaryResult<long> GetNextTxNonce(byte[] addressBytes)
        {
            Address address = new Address(addressBytes);
            return UnaryResult(_blockChainHost.GetNextTxNonce(address));
        }
    }
}
