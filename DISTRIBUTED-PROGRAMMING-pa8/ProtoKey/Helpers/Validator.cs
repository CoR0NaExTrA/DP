using System.Text.RegularExpressions;

namespace ProtoKey.Helpers;

public static class Validator
{
    private static readonly Regex KeyRegex =
        new( "^[a-zA-Z0-9_.-]{1,1000}$" );

    private static readonly Regex PrefixRegex =
        new( "^[a-zA-Z0-9_.-]{0,1000}$" );

    public static bool IsValidKey( string key )
    {
        if ( string.IsNullOrWhiteSpace( key ) )
        {
            return false;
        }

        return KeyRegex.IsMatch( key );
    }

    public static bool IsValidPrefix( string prefix )
    {
        if ( prefix == null )
        {
            return false;
        }

        return PrefixRegex.IsMatch( prefix );
    }

    public static bool IsValidValue( int value )
    {
        return value >= int.MinValue &&
               value <= int.MaxValue;
    }

    public static bool IsValidSetRequest(
        string key,
        int value )
    {
        return IsValidKey( key ) &&
               IsValidValue( value );
    }
}