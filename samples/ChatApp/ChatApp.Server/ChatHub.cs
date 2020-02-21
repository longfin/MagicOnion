using System.Threading.Tasks;
using MagicOnion.Server.Hubs;
using Nekoyume.Shared.Hubs;

namespace ChatApp.Server
{
    /// <summary>
    /// Chat server processing.
    /// One class instance for one connection.
    /// </summary>
    public class ChatHub : StreamingHubBase<IActionEvaluationHub, IActionEvaluationHubReceiver>, IActionEvaluationHub
    {
        private IGroup group;

        public async Task JoinAsync()
        {
            group = await Group.AddAsync(string.Empty);
        }


        public async Task LeaveAsync()
        {
            await group.RemoveAsync(Context);
        }

        public async Task BroadcastAsync(byte[] outputStates)
        {   
            Broadcast(group).OnRender(outputStates);
            await Task.CompletedTask;
        }

        public async Task UpdateTipAsync(long index)
        {
            Broadcast(group).OnTipChanged(index);
            await Task.CompletedTask;
        }

        protected override ValueTask OnDisconnected()
        {
            // handle disconnection if needed.
            // on disconnecting, if automatically removed this connection from group.
            return CompletedTask;
        }
    }
}
