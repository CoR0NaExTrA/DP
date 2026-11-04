using System.Threading.Channels;
using ProtoKey.Models;

namespace ProtoKey.Services;

public class StorageService
{
    public Dictionary<string, int> Data { get; }
        = new();

    public Channel<StorageCommand> Channel { get; }
        = System.Threading.Channels.Channel
            .CreateUnbounded<StorageCommand>();
}