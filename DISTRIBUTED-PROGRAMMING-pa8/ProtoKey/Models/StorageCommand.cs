namespace ProtoKey.Models;

public class StorageCommand
{
    public string Type { get; set; } = "";

    public string Key { get; set; } = "";

    public int Value { get; set; }

    public TaskCompletionSource<int?> Completion
    { get; set; } = new();

    public TaskCompletionSource<List<string>>
        KeysCompletion
    { get; set; } = new();
}