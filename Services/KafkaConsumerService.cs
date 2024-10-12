

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using My_Dashboard.Hubs;
using My_Dashboard.Kafka;
using My_Dashboard.Models.DB;
using System.Diagnostics;
using System.Threading;

namespace My_Dashboard.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IHubContext<SignalHub> _hubContext;
        //private readonly KafkaClient _kafkaClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceManager _serviceManager;
        private readonly CancellationTokenSource _cts = new();
        private CancellationTokenSource _rts = new ();
        private readonly IConfiguration _configuration;
        public KafkaConsumerService(IHubContext<SignalHub> hubContext, IServiceProvider serviceProvider, IConfiguration configuration, ServiceManager serviceManager)
        {
            _hubContext = hubContext;
            //_kafkaClient = new KafkaClient();
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _serviceManager = serviceManager;
            _serviceManager.RegisterService("KafkaConsumerService",stopToken:_cts, updateRts:() => _rts = new CancellationTokenSource());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _cts.Token))
            {
                //using (CancellationTokenSource linkedRts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _rts.Token))
                //{
                    var cancelToken = linkedCts.Token;
                    //var restartToken = linkedRts.Token;

                    while (!cancelToken.IsCancellationRequested)
                    {
                        List<Task> tasks = new List<Task>();
                        while (!_rts.Token.IsCancellationRequested)
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var dbContext = scope.ServiceProvider.GetRequiredService<MachineUtilizationContext>();
                                var machines = dbContext.Machines.Select(x => x).Where(x => x.IsActive);

                                foreach (var machine in machines)
                                {
                                    KafkaClient _kafkaClient = new KafkaClient(machine.Name + "_kafka", _configuration);
                                    var subsOK = await _kafkaClient.subscribeTopic(machine.Name + "_kafka");

                                    if (subsOK)
                                        tasks.Add(subWorker(_kafkaClient, machine.Name + "_kafka", _rts.Token));


                                }
                            }
                            // Wait for all tasks to complete
                            await Task.WhenAll(tasks);

                        }
                    _serviceManager.UpdateRestartService("KafkaConsumerService");
                   // // Example: dynamically adding tasks to the list
                   // List<Task> tasks = new List<Task>();
                   // while (!_rts.Token.IsCancellationRequested)
                   // {


                   //     //Suppose we dynamically decide to start 5 subworkers
                   //         int numberOfSubworkers = 5;
                   //     for (int i = 1; i <= numberOfSubworkers; i++)
                   //     {
                   //         tasks.Add(testSubworker(_rts.Token, i));
                   //         //tasks.Add(Task.Run(() => { while (true) { Debug.WriteLine("running " + 1); Thread.Sleep(1000); } }, _rts.Token));
                   //         //tasks.Add(Task.Run(() => { while (true) { Debug.WriteLine("running " + 2); Thread.Sleep(1000); } }, _rts.Token));

                   //      }

                   //     //Wait for all tasks to complete

                   //     await Task.WhenAll(tasks);

                   //     Debug.WriteLine("background task restarting...");

                   // }

                   // tasks.Clear();
                   //// _rts.TryReset();
                   // //_rts = new CancellationTokenSource();
                   // _serviceManager.UpdateRestartService("KafkaConsumerService");
                }

                //testSubworker(_rts.Token, 1);
                //testSubworker(_rts.Token, 2);
                //testSubworker(_rts.Token, 3);

            }


            
        }

        

         async Task testSubworker(CancellationToken stoppingToken, int i)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (i == 2)
                    break;
                Debug.WriteLine("running background.." + i);
                    await Task.Delay(1000);
                    //Thread.Sleep(1000);

            }

            Debug.WriteLine("background task cancelled." + i);

        }

        async Task subWorker(KafkaClient _kafkaClient,  string topic, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await _kafkaClient.WaitForReceivedDataAsync(stoppingToken);

                if (result != null)
                {
                    await _hubContext.Clients.All.SendAsync($"onReceiveCPU_{topic}", result["label"], result["temp"], result["humi"]);
                }

                // Wait for a short time to avoid tight polling loops
                //await Task.Delay(100, stoppingToken);

            }

        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {

            Debug.WriteLine("stopping background..");
            await base.StopAsync(stoppingToken);

            
        }
    }
}
