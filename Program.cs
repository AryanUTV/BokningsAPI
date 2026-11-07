using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BokningsApi.Data;
using BokningsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BokningsAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var key = builder.Configuration["Jwt:Key"]
                        ?? throw new InvalidOperationException("Jwt:Key saknas i konfigurationen.");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                    };
                });

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Skapa/uppdatera databasen vid start och lägg in startdata om den är tom
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();

                if (!db.Rooms.Any())
                {
                    db.Rooms.AddRange(
                        new Room { Name = "Grupprum 1", Capacity = 6 },
                        new Room { Name = "Aulan", Capacity = 100 });
                    db.SaveChanges();
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/", () => "BokningsApi körs. Prova /api/rooms");
            app.MapControllers();

            // Servern talar om vilken port vi ska använda via miljövariabeln PORT
            var port = Environment.GetEnvironmentVariable("PORT");
            if (port != null)
            {
                app.Run($"http://0.0.0.0:{port}");
            }
            else
            {
                app.Run();
            }
        }
    }
}