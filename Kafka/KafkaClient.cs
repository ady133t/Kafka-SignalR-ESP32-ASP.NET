using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace My_Dashboard.Kafka
{
    public class KafkaClient
    {
        private readonly IConfiguration _configuration;
        private const string Topic = "kafka-test";
        private readonly IConsumer<string, string> _consumer;
        private readonly AdminClient _adminClient;

        public KafkaClient(string topic, IConfiguration configuration)
        {
            //_configuration = ReadConfig();
            //_configuration["group.id"] = "csharp-group-2";
            //_configuration["auto.offset.reset"] = "earliest";

            //_consumer = new ConsumerBuilder<string, string>(_configuration.AsEnumerable()).Build();
            _configuration = configuration;
            
            _consumer = new ConsumerBuilder<string, string>(new 
                ConsumerConfig { 
                BootstrapServers= _configuration["Kafka:bootstrap.servers"], 
                GroupId= _configuration["Kafka:group.id"],
                AutoOffsetReset= Enum.Parse<AutoOffsetReset>(_configuration["Kafka:auto.offset.reset"]),
                EnableAutoCommit= bool.Parse(_configuration["Kafka:enable.auto.commit"]),
                //ClientId = "your-client-id", // Optional but useful for logging
                //GroupInstanceId = "your-group-instance-id" // Optional for static membership


            }).Build();

            _adminClient = new AdminClient(configuration);
        }


        public async Task<bool> subscribeTopic(string topic)
        {
            if (await _adminClient.createTopic(topic) == (true, "Topic is exist!"))
            {
                _consumer.Subscribe(topic); 
                return true;
            }

            return false;
               
        }
        public async Task<Dictionary<string, string>> WaitForReceivedDataAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Asynchronously poll for Kafka messages
                    var consumeResult = await Task.Run(() => { return _consumer.Consume(cancellationToken); });
                    Debug.WriteLine("received message = "+consumeResult?.Message);

                    if (consumeResult != null && consumeResult.Message != null)
                    {
                        var data = JsonConvert.DeserializeObject<dynamic>(consumeResult.Message.Value);
                        if (data != null)
                        {
                            var dictionary = new Dictionary<string, string>
                            {
                                //{ "label", (string)data?.label },
                                { "label", DateTime.Now.ToString("hh:mm tt") },
                                { "temp", (string)data?.value.temperature },
                                { "humi", (string)data?.value.humidity }
                            };

                            Debug.Assert(dictionary != null,"Null message received from broker.");
                            return dictionary;
                        }
                    }

                    // Wait for a short time to avoid tight polling loops
                    //await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    _consumer.Close();
                    Debug.WriteLine($"Error while consuming Kafka: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _consumer.Close();
                    Debug.WriteLine($"Error while consuming Kafka: {ex.Message}");
                    // Handle other exceptions, log them, or rethrow
                }
            }

            return null; // Return null if nothing received or cancelled
        }


        //public async Task<bool> createTopic(string topic)
        //{
           
        //    using var adminClient = new AdminClientBuilder( new AdminClientConfig { BootstrapServers="localhost:9092" }).Build();
        //    if (TopicExists(adminClient,topic)) return true;
        //        // Define topic specification


        //    var topicSpec = new TopicSpecification
        //    {
        //        Name = topic,
        //        NumPartitions = 1,
        //        ReplicationFactor = 1
        //    };

        //    // Create the topic
        //    try
        //    {
        //        await adminClient.CreateTopicsAsync(new List<TopicSpecification> { topicSpec });
        //        Debug.WriteLine($"Topic '{topicSpec.Name}' created successfully.");
        //        return true;
        //    }
        //    catch (CreateTopicsException e)
        //    {
        //        foreach (var result in e.Results)
        //        {
        //            Debug.WriteLine($"An error occurred creating topic {result.Topic}: {result.Error.Reason}");
        //        }

        //        return false;
        //    }

        //}

        //private static bool TopicExists(IAdminClient adminClient, string topicName)
        //{
        //    try
        //    {
        //        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        //        return metadata.Topics.Any(topic => topic.Topic == topicName);
        //    }
        //    catch (KafkaException e)
        //    {
        //        Console.WriteLine($"An error occurred fetching metadata: {e.Message}");
        //        return false;
        //    }
        //}


     
            private IConfiguration ReadConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddIniFile("client.properties", optional: false)
                .Build();
        }
    }
}
