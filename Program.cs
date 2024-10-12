using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using My_Dashboard.Hubs;
using My_Dashboard.Identity;
using My_Dashboard.Services;
using My_Dashboard.Models.DB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

//var IDCstr = builder.Configuration.GetConnectionString("IdentityConnection");
var machineCstr = builder.Configuration.GetConnectionString("MachineConnection");
//builder.Services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(IDCstr));
builder.Services.AddDbContext<MachineUtilizationContext>(option => { option.UseSqlServer(machineCstr); });
builder.Services.AddSingleton<ServiceManager>();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount =  false)
//            .AddEntityFrameworkStores<ApplicationDbContext>()
//            .AddDefaultUI();

//builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddAuthentication(options =>
{
    //options.DefaultScheme = "Cookies";
    //options.DefaultChallengeScheme = "oidc";
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;

})
.AddCookie(options =>
{
    //options.Cookie.Name = "My-SSO";
    options.ExpireTimeSpan = TimeSpan.FromSeconds(3);

})
.AddOpenIdConnect(options =>
{
    options.Authority = "https://localhost:5001/";
    options.ClientId = "dashboard";
    options.ClientSecret = "dashboard-secret";
    options.ResponseType = OpenIdConnectResponseType.Code;
    //options.SaveTokens = true;
    options.RequireHttpsMetadata = false;
    options.UsePkce = true;
    options.CallbackPath = "/signin-oidc";
    //options.GetClaimsFromUserInfoEndpoint = true;
    //options.Scope.Clear();
    options.Scope.Add("api");
}
);


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();

// Register your background service
builder.Services.AddHostedService<KafkaConsumerService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllerRoute(
//        name: "default",
//        pattern: "{controller=Home}/{action=Index}/{id?}");
//    endpoints.MapRazorPages(); // This line enables the default Identity UI
//});

app.MapHub<SignalHub>("/signalHub");

app.Run();
