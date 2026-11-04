using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShardLibrary;
using System.Security.Claims;
using System.Text.Json;
using Valuator.Models;

namespace Valuator.Pages;

public class LoginModel : PageModel
{
    private readonly RedisShardManager _shards;

    public LoginModel(
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

    public async Task<IActionResult> OnPost()
    {
        var db = _shards.Main;

        string key = "USER-" + Login;

        if ( !db.KeyExists( key ) )
        {
            return Page();
        }

        var json = db.StringGet( key );

        var user =
            JsonSerializer.Deserialize<User>(
                json! );

        bool valid =
            BCrypt.Net.BCrypt.Verify(
                Password,
                user!.PasswordHash );

        if ( !valid )
        {
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Login)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme );

        var principal =
            new ClaimsPrincipal( identity );

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal );

        return RedirectToPage( "/Index" );
    }
}