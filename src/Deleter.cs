using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageManager.Key;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace MessageManager
{
    public class DeleterArguments : CommandArguments
    {
        public bool Dead { get; set; }
        public string Id { get; set; }
    } 

    public class Deleter : SbsCommand
    {
        public Deleter(string connectionString, DeleterArguments a) : 
            base(connectionString, GetEntityPath(a.Type, a.TopicQueueName, a.Name, a.Dead))
        {
        }      

        public async Task Execute(string id, bool force)
        {
            if (string.IsNullOrEmpty(id) && !force)
            {
                Console.WriteLine($"No id specified. Use --force if you intend to delete all messages");
            }
            var tokens = await ReceiveMessages(id);
            var tasks = tokens
                            .Select(t => Receiver.AbandonAsync(t));
            await Task.WhenAll(tasks);    
        }    

        private async Task<IEnumerable<string>> ReceiveMessages(string id)
        {
            var lockTokens = new List<string>();
            Message message;
            do
            {
                message = await Receiver.ReceiveAsync(TimeSpan.FromSeconds(5));

                if (message == null)                
                {                    
                    Console.WriteLine($"Message with id {id} was not found");
                    lockTokens.Add(message.SystemProperties.LockToken);
                    break;
                }

                if (string.IsNullOrEmpty(id) || message.MessageId == id)
                {
                    await Receiver.CompleteAsync(message.SystemProperties.LockToken);
                    Console.WriteLine($"Message with id {message.MessageId} was removed");
                }
                lockTokens.Add(message.SystemProperties.LockToken);
            } while (message != null);
            return lockTokens;
        }      
    }
}