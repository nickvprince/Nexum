//using Microsoft.AspNetCore.Authentication.Negotiate;
using App.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient("BlazorClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WebAppSettings:APIBaseUri"] + ":" + builder.Configuration["WebAppSettings:APIBasePort"]);
});

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Nexum.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

/*builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});*/
//builder.Services.AddRazorPages();

builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<UserPermissionSetService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TenantService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();
//app.MapBlazorHub();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
//app.Run($"https://0.0.0.0:" + builder.Configuration["WebAppSettings:BasePort"]);
