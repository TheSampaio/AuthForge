using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

var app = builder.Build();

// Deployment targets like Render don't offer an easy pre-deploy shell step, so the app
// applies any pending EF Core migrations itself on startup. Safe to run on every boot:
// already-applied migrations are no-ops.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Unversioned and anonymous on purpose: infrastructure (e.g. Render's health check) probes
// this before the app is known to be authenticated or API-versioned correctly.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();