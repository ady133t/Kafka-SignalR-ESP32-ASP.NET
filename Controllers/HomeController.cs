using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using My_Dashboard.Hubs;
using My_Dashboard.Models;
using My_Dashboard.Models.DB;
using System.Diagnostics;

namespace My_Dashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<SignalHub> _chartHub; // SignalR hub context
        private readonly MachineUtilizationContext _machineUtilizationContext;

        public HomeController(ILogger<HomeController> logger, IHubContext<SignalHub> chartHub , MachineUtilizationContext machineUtilizationContext)
        {
            _logger = logger;
            _chartHub = chartHub; // Initialize SignalR hub context
            _machineUtilizationContext = machineUtilizationContext;
        }
       
        public IActionResult Index()
        {
           
            var machines = _machineUtilizationContext.Machines.Select(x => x).Where(x => x.IsActive).ToList();
            

            return View(machines);
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
