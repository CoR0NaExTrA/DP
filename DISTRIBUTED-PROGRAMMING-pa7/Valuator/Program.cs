using ShardLibrary;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.Services.AddControllers();

        builder.Services
        .AddAuthentication(
            CookieAuthenticationDefaults.AuthenticationScheme )

        .AddCookie( options =>
        {
            options.LoginPath = "/Login";

            options.AccessDeniedPath = "/Login";

            options.Cookie.Name = "valuator_auth";

            options.ExpireTimeSpan = TimeSpan.FromHours( 3 );
        } );

        var shardManager = RedisConnectionsFactory.Create( builder.Configuration );

        builder.Services.AddSingleton( shardManager );

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapRazorPages();

        app.MapControllers();

        app.Run();
    }
}
