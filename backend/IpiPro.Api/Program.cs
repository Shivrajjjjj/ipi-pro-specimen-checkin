using Microsoft.EntityFrameworkCore;
using IpiPro.Api.Context;
using IpiPro.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HeaderTenantProvider>();

// Configure SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=ipipro.db"));

// Configure CORS - Allow all origins for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

// Add Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Middleware
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    Console.WriteLine("🔧 Initializing database...");
    
    try
    {
        db.Database.EnsureDeleted();  // Fresh start
        db.Database.EnsureCreated();
        DbInitializer.Seed(db);
        Console.WriteLine("✅ Database initialized successfully with seed data");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database initialization error: {ex.Message}");
        throw;
    }
}

Console.WriteLine("🚀 IPI Pro API starting...");
app.Run();