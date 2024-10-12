using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using My_Dashboard.Hubs;
using My_Dashboard.Services;
using My_Dashboard.Models.DB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);


var machineCstr = builder.Configuration.GetConnectionString("MachineConnection");

builder.Services.AddDbContext<MachineUtilizationContext>(option => { option.UseSqlServer(machineCstr); });
builder.Services.AddSingleton<ServiceManager>();


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
//app.UseAuthentication();
//app.UseAuthorization();

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
