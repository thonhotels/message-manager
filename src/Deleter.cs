using System;
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

        public async Task Execute(string id)
        {
            Message message;
            do
            {
                message = await Receiver.ReceiveAsync(TimeSpan.FromSeconds(5));

                if (message == null)                
                {                    
                    Console.WriteLine($"Message with id {id} was not found");
                    return;
                }

                if (message.MessageId == id)
                {
                    await Receiver.CompleteAsync(message.SystemProperties.LockToken);
                    Console.WriteLine($"Message with id {id} was removed");
                    return;
                }
                await Receiver.AbandonAsync(message.SystemProperties.LockToken);
            } while (message != null);
        }          
    }
}