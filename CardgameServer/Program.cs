using CardgameServer.game.truco;
using CardgameServer.player;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<PlayerContext>(opt => opt.UseInMemoryDatabase("PlayerList"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TrucoGames>(new TrucoGames());
builder.Services.AddMvc();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Truco}/{action=Index}/{id?}");

// Add static files
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.Run();
