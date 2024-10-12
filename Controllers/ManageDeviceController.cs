using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using My_Dashboard.Kafka;
using My_Dashboard.Models.DB;
using My_Dashboard.Services;
using System;

namespace My_Dashboard.Controllers
{
    public class ManageDeviceController : Controller
    {
        private readonly MachineUtilizationContext _machineUtilizationContext;
        private readonly ServiceManager _serviceManager;
        private readonly IConfiguration _configuration;
        private readonly AdminClient _adminClient;
        public ManageDeviceController(MachineUtilizationContext machineUtilizationContext, ServiceManager serviceManager , IConfiguration configuration)
        {
            _machineUtilizationContext = machineUtilizationContext;
            _serviceManager = serviceManager;
            _configuration = configuration;
            _adminClient = new AdminClient(configuration);
        }
        public IActionResult Index()
        {
            var machines = _machineUtilizationContext.Machines.Select(x=>x).ToList();
      
            return View(machines);
        }


        [HttpPost]
        public IActionResult generateNew()
        {
           
            return new JsonResult(new { result = new Machine() { Name="",MachineTypeId=1 } }) { StatusCode = 200 };
        }

        [HttpPost]
        public IActionResult RestartService()
        {
            _serviceManager.RestartService("KafkaConsumerService");
            return new JsonResult(new { result = "OK" }) { StatusCode = 200 };
        }

        [HttpPost]
        public IActionResult Update([FromBody] Machine machine)
        {
            var machineRow = _machineUtilizationContext.Machines.Find(machine.MachineId);

            if (machineRow != null)
            {
                machineRow.Name = machine.Name;
                machineRow.IsActive= machine.IsActive;
                _machineUtilizationContext.Entry(machineRow).State = EntityState.Modified; //optional
                _machineUtilizationContext.SaveChanges();
                return new JsonResult(new { result = "OK" } ) { StatusCode = 200 };
            }
            else
            {
                return new JsonResult(new { result = "Not Found" }) { StatusCode = 404 };

            }
        }


        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Machine machine )
        {

            if (await _adminClient.createTopic(machine.Name + "_kafka") == (true, $"Topic '{machine.Name + "_kafka"}' created successfully."))
            {
                _machineUtilizationContext.Machines.Add(machine);
                var result = _machineUtilizationContext.SaveChanges();

                var getID = _machineUtilizationContext.Machines.Select(x => new { x.MachineId, x.Name }).Where(x => x.Name.Equals(machine.Name)).FirstOrDefault().MachineId;

                return new JsonResult(new { result = getID }) { StatusCode = 200 };
            }
             return new JsonResult(new { result = 0 }) { StatusCode = 404 };
        }


        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] Machine machine)
        {

            var machineRow = _machineUtilizationContext.Machines.Find(machine.MachineId);

            if (machineRow != null)
            {
                if(await _adminClient.DeleteTopic(machineRow.Name + "_kafka") == (true, "Successfully delete Topic"))
                {
                    _machineUtilizationContext.Remove(machineRow);
                    _machineUtilizationContext.SaveChanges();
                    return new JsonResult(new { result = "OK" }) { StatusCode = 200 };
                }
                else
                {
                    return new JsonResult(new { result = "Kafka Topic cannot be deleted" }) { StatusCode = 500 };
                }
                
                
            }
            else
            {
                return new JsonResult(new { result = "Row Not Found" }) { StatusCode = 404 };

            }
        }

        //public IActionResult Update()
        //{
        //    return View();
        //}

        //public IActionResult Delete()
        //{
        //    return View();
        //}

    }
}
