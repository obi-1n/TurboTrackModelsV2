using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TurboTrackModels_Dotnet_Core.Models;

var builder = WebApplication.CreateBuilder(args);

//Database
builder.Services.AddDbContext<TTMDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("TTMConnection")));

//Identity
builder.Services
    .AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<TTMDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ShoppingCartService>();
builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("Stripe"));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TTMDbContext>();
    await dbContext.Database.MigrateAsync();
    await DatabaseInitialiser.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();