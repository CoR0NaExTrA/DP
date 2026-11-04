using Microsoft.AspNetCore.Mvc.RazorPages;
using ShardLibrary;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Valuator.Pages;

[Authorize]
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly RedisShardManager _shardManager;

    public SummaryModel( ILogger<SummaryModel> logger, RedisShardManager shardManager )
    {
        _logger = logger;
        _shardManager = shardManager;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet( string id )
    {
        var db = _shardManager.GetShardById( id );

        string currentUserId = User.FindFirst( ClaimTypes.NameIdentifier )!.Value;
        string authorId = _shardManager.Main.StringGet( "AUTHOR-" + id );

        if ( authorId != currentUserId )
        {
            Response.StatusCode = 403;
            return;
        }

        Rank = ( double )db.StringGet( "RANK-" + id );
        Similarity = ( double )db.StringGet( "SIMILARITY-" + id );
    }
}
