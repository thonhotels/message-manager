
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace MessageManager
{
    public class ListerArguments : CommandArguments
    {
        public bool Dead { get; set; }
        public bool Details { get; set; }
    }
    public class Lister : SbsCommand
    {
        public Lister(string connectionString, ListerArguments a) : 
            base(connectionString, GetEntityPath(a.Type, a.TopicQueueName, a.Name, a.Dead))
        {
        }

        public async Task Execute(bool detailed)
        {
            Message message;
            do
            {
                message = await Receiver.PeekAsync();

                if (message != null)                
                {
                    if (detailed)
                        Console.WriteLine($"Peeked: {JsonConvert.SerializeObject(message, Formatting.Indented)}");
                    Console.WriteLine($"MessageId: {message.MessageId}\nBody: {Encoding.UTF8.GetString(message.Body)}");
                }
            } while (message != null);
        }
    }
}
