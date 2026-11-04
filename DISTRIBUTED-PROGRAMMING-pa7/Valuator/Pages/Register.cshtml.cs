using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShardLibrary;
using System.Text.Json;
using Valuator.Models;

namespace Valuator.Pages;

public class RegisterModel : PageModel
{
    private readonly RedisShardManager _shards;

    public RegisterModel(
        RedisShardManager shards )
    {
        _shards = shards;
    }

    [BindProperty]
    public string Login { get; set; }

    [BindProperty]
    public string Password { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        var db = _shards.Main;

        string key = "USER-" + Login;

        if ( db.KeyExists( key ) )
        {
            return Page();
        }

        var user = new User
        {
            Id = Guid.NewGuid(),

            Login = Login,

            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    Password )
        };

        db.StringSet(
            key,
            JsonSerializer.Serialize( user ) );

        return RedirectToPage(
            "/Login" );
    }
}