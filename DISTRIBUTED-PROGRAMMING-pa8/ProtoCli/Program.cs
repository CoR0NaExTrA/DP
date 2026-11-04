using System.Net.Http.Json;

class Program
{
    private const string ProtoKeyHost =
        "http://127.0.0.1:7777";

    static async Task Main( string[] args )
    {
        if ( args.Length == 0 )
        {
            Console.WriteLine(
                "Commands: set/get/keys" );

            return;
        }

        using var client = new HttpClient();

        switch ( args[ 0 ] )
        {
            case "set":
                {
                    if ( args.Length != 3 )
                    {
                        Console.WriteLine(
                            "Usage: set <key> <value>" );

                        return;
                    }

                    var key = args[ 1 ];

                    if ( !int.TryParse( args[ 2 ],
                            out var value ) )
                    {
                        Console.WriteLine(
                            "Invalid value" );

                        return;
                    }

                    var response =
                        await client.PostAsJsonAsync(
                            $"{ProtoKeyHost}/api/set",
                            new
                            {
                                key,
                                value
                            } );

                    Console.WriteLine(
                        response.StatusCode );

                    break;
                }

            case "get":
                {
                    if ( args.Length != 2 )
                    {
                        Console.WriteLine(
                            "Usage: get <key>" );

                        return;
                    }

                    var key = args[ 1 ];

                    var result =
                        await client.GetStringAsync(
                            $"{ProtoKeyHost}/api/get/{key}" );

                    Console.WriteLine( result );

                    break;
                }

            case "keys":
                {
                    if ( args.Length != 2 )
                    {
                        Console.WriteLine(
                            "Usage: keys <prefix>" );

                        return;
                    }

                    var prefix = args[ 1 ];

                    var result =
                        await client.GetStringAsync(
                            $"{ProtoKeyHost}/api/keys?prefix={prefix}" );

                    Console.WriteLine( result );

                    break;
                }

            default:
                {
                    Console.WriteLine(
                        "Unknown command" );

                    break;
                }
        }
    }
}