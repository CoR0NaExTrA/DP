namespace Valuator;

public static class CountryRegionMap
{
    public static string GetRegion( string country ) => country switch
    {
        "Russia" => "RU",
        "France" => "EU",
        "Germany" => "EU",
        "UAE" => "ASIA",
        "India" => "ASIA",
        _ => throw new Exception( "Unknown country" )
    };
}
