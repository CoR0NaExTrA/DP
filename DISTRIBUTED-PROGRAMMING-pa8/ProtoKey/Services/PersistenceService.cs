using System.Threading.Channels;
using ProtoKey.Models;

namespace ProtoKey.Services;

public class PersistenceService
{
    public Channel<StorageCommand> Channel { get; }
        = System.Threading.Channels.Channel
            .CreateUnbounded<StorageCommand>();
}