using Confluent.Kafka.Admin;
using Confluent.Kafka;
using System.Diagnostics;

namespace My_Dashboard.Kafka
{
    public class AdminClient
    {
        private readonly IConfiguration _configuration;
        private readonly IAdminClient _adminClient;

        public AdminClient(IConfiguration configuration)
        {
            _configuration = configuration;
            _adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _configuration["Kafka:bootstrap.servers"] }).Build();

        }
        public async Task<(bool,string)> createTopic(string topic)
        {

            if (TopicExists( topic)) return (true,"Topic is exist!");
            // Define topic specification


            var topicSpec = new TopicSpecification
            {
                Name = topic,
                NumPartitions = 1,
                ReplicationFactor = 1
            };

            // Create the topic
            try
            {
                await _adminClient.CreateTopicsAsync(new List<TopicSpecification> { topicSpec });
                Debug.WriteLine($"Topic '{topicSpec.Name}' created successfully.");
                return (true, $"Topic '{topicSpec.Name}' created successfully.");
            }
            catch (CreateTopicsException e)
            {
                foreach (var result in e.Results)
                {
                    Debug.WriteLine($"An error occurred creating topic {result.Topic}: {result.Error.Reason}");
                }

                return (false,e.Message);
            }
            catch(Exception e)
            {
                return (false, e.Message);
            }

        }


        public async Task<(bool, string)> DeleteTopic(string topic)
        {

            try
            {
                await _adminClient.DeleteTopicsAsync(new[] { topic });
                return (true, "Successfully delete Topic"); 

            }
            catch (DeleteTopicsException e)
            {
                foreach (var result in e.Results)
                {
                    if (result.Error.IsError)
                    {
                        Debug.WriteLine($"Failed to delete topic '{result.Topic}': {result.Error.Reason}");
                    }
                    else
                    {
                        Debug.WriteLine($"Topic '{result.Topic}' deleted successfully.");
                    }
                }
                return (false, e.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error: {ex.Message}");
                return (false, ex.Message);
            }

            
        }

       private bool TopicExists( string topicName)
        {
            try
            {
                var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                return metadata.Topics.Any(topic => topic.Topic == topicName);
            }
            catch (KafkaException e)
            {
                Console.WriteLine($"An error occurred fetching metadata: {e.Message}");
                return false;
            }
        }

    }
}
