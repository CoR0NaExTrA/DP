using Microsoft.AspNetCore.Mvc;
using ProtoKey.Helpers;
using ProtoKey.Models;
using ProtoKey.Services;

namespace ProtoKey.Controllers;

[ApiController]
[Route( "api" )]
public class StorageController : ControllerBase
{
    private readonly StorageService _storage;

    private readonly PersistenceService _persistence;

    public StorageController(
        StorageService storage,
        PersistenceService persistence )
    {
        _storage = storage;
        _persistence = persistence;
    }

    [HttpPost( "set" )]
    public async Task<IActionResult> Set(
    [FromBody] SetRequest request )
    {
        if ( !Validator.IsValidSetRequest(
                request.Key,
                request.Value ) )
        {
            return BadRequest();
        }

        var command = new StorageCommand
        {
            Type = "set",
            Key = request.Key,
            Value = request.Value
        };

        await _persistence.Channel.Writer
            .WriteAsync( command );

        await _storage.Channel.Writer
            .WriteAsync( command );

        await command.Completion.Task;

        return Ok();
    }

    [HttpGet( "get/{key}" )]
    public async Task<IActionResult> Get( string key )
    {
        if ( !Validator.IsValidKey( key ) )
        {
            return BadRequest( "Invalid key" );
        }

        var command = new StorageCommand
        {
            Type = "get",
            Key = key
        };

        await _storage.Channel.Writer
            .WriteAsync( command );

        var result =
            await command.Completion.Task;

        return Ok( result );
    }

    [HttpGet( "keys" )]
    public async Task<IActionResult> Keys(
    [FromQuery] string prefix = "" )
    {
        if ( !Validator.IsValidPrefix( prefix ) )
        {
            return BadRequest( "Invalid prefix" );
        }

        var command = new StorageCommand
        {
            Type = "keys",
            Key = prefix
        };

        await _storage.Channel.Writer
            .WriteAsync( command );

        var result =
            await command.KeysCompletion.Task;

        return Ok( result );
    }
}