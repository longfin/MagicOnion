using MagicOnion;
using MessagePack;

namespace ChatApp.Shared.Services
{
    /// <summary>
    /// Client -> Server API
    /// </summary>
    public interface IChatService : IService<IChatService>
    {
        UnaryResult<Nil> GenerateException(string message);
        UnaryResult<Nil> SendReportAsync(string message);

        UnaryResult<byte[]> GetStateRaw(byte[] addressBytes);
    }
}
