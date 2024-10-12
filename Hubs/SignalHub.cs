using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using My_Dashboard.Kafka;
using Newtonsoft.Json;
using System.Diagnostics;

namespace My_Dashboard.Hubs
{
    public class SignalHub:Hub
    {

        KafkaClient kafkaClient = null; 
        public SignalHub() {

        }

        async Task onReceivedKafka()
        {
          

        }

        public async Task SendMessage( string label, string usage)
        {
            await Clients.All.SendAsync("onReceiveCPU", label, usage);
        }

        public async Task SendCPU(int cpu_usage)
        {
            await Clients.All.SendAsync("onReceiveCPU", cpu_usage);
        }

        List<string> clients = new List<string>();
        //This method is called when a client connects to the hub
        public override async Task OnConnectedAsync()
        {
            // Get the connection ID of the connected client
            var connectionId = Context.ConnectionId;
            

             await Clients.Caller.SendAsync("onReceiveCPU", "label", $"Welcome! Your connection ID is {connectionId}");


            await base.OnConnectedAsync();
        }
    }
}
