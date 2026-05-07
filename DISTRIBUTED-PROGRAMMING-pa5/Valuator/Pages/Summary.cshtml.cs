using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IDatabase _redis;

    public SummaryModel( ILogger<SummaryModel> logger, IConnectionMultiplexer redis )
    {
        _logger = logger;
        _redis = redis.GetDatabase();
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet( string id )
    {
        _logger.LogDebug( id );

        string rankKey = "RANK-" + id;
        string similarityKey = "SIMILARITY-" + id;

        Rank = ( double )_redis.StringGet( rankKey );
        Similarity = ( double )_redis.StringGet( similarityKey );
    }

    public IActionResult OnGetData( string id )
    {
        string rankKey = "RANK-" + id;
        string similarityKey = "SIMILARITY-" + id;

        var rank = _redis.StringGet( rankKey );
        var similarity = _redis.StringGet( similarityKey );

        return new JsonResult( new
        {
            Rank = rank.HasValue ? ( double? )rank : null,
            Similarity = similarity.HasValue ? ( double? )similarity : null
        } );
    }
}
