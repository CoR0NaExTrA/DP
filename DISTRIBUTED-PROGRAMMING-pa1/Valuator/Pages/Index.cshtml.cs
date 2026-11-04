using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDatabase _redis;

    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redis )
    {
        _logger = logger;
        _redis = redis.GetDatabase();
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost( string text )
    {
        _logger.LogDebug( text );

        string id = Guid.NewGuid().ToString();

        string textKey = "TEXT-" + id;
        string rankKey = "RANK-" + id;
        string similarityKey = "SIMILARITY-" + id;

        _redis.StringSet( textKey, text );

        double rank = CalculateRank( text );
        _redis.StringSet( rankKey, rank );

        const string allTextsKey = "ALL_TEXTS";

        bool exists = _redis.SetContains( allTextsKey, text );
        double similarity = exists ? 1.0 : 0.0;

        _redis.StringSet( similarityKey, similarity );

        if ( !exists )
        {
            _redis.SetAdd( allTextsKey, text );
        }

        return Redirect( $"summary?id={id}" );
    }

    private double CalculateRank( string text )
    {
        if ( string.IsNullOrEmpty( text ) )
            return 0;

        int nonAlphabetic = text.Count( c =>
            !( char.IsLetter( c ) &&
              ( IsLatin( c ) || IsCyrillic( c ) ) ) );

        return ( double )nonAlphabetic / text.Length;
    }

    private bool IsLatin( char c ) =>
        ( c >= 'a' && c <= 'z' ) || ( c >= 'A' && c <= 'Z' );

    private bool IsCyrillic( char c ) =>
        ( c >= 'а' && c <= 'я' ) ||
        ( c >= 'А' && c <= 'Я' ) ||
        c == 'ё' || c == 'Ё';

}
