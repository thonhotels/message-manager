using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace MessageManager
{
    public class SenderArguments : CommandArguments
    {
        public string Path { get; set; }
        public string Id { get; set; }
        public string Label { get; set; }
    }

    public class Sender 
    {
        private ISenderClient SenderClient { get; }

        public Sender(string connectionString, SenderArguments a) 
        {            
            SenderClient = a.Type == BusType.Queue ?
                (ISenderClient)new QueueClient(connectionString, a.TopicQueueName) :
                new TopicClient(connectionString, a.TopicQueueName); 
        }   

        public async Task Execute(string path, string id, string label)
        {
            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Path is not specified");
                return;
            }

            var messageBody = File.ReadAllText(path);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody))
            {
                Label = string.IsNullOrEmpty(label) ? path.Substring(0, path.Length - 5) : label,
                MessageId = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id
            };
            await SenderClient.SendAsync(message);

            Console.WriteLine($"Sent message to queue/subscription");
        }
    }
}