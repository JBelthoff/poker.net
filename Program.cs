using Microsoft.Data.SqlClient;
using poker.net.Services;
using poker.net.Interfaces;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

bool useSql = builder.Configuration.GetValue<bool>("UseSqlServer");

// Deck provider toggle
if (useSql)
{
    builder.Services.AddScoped<IDeckService, SqlDeckService>();

    // DB-only registrations
    builder.Services.AddScoped<IDbConnection>(sp =>
        new SqlConnection(builder.Configuration.GetConnectionString("DBConn")));
    builder.Services.AddScoped<DbHelper>();
}
else
{
    builder.Services.AddSingleton<IDeckService, StaticDeckService>();
}

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Optional: log which mode you’re in
app.Logger.LogInformation("Startup mode: UseSqlServer = {UseSql}", useSql);

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
