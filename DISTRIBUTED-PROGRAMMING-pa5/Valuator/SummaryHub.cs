using Microsoft.AspNetCore.SignalR;

public class SummaryHub : Hub
{
    public Task JoinGroup( string id )
    {
        return Groups.AddToGroupAsync( Context.ConnectionId, id );
    }
}