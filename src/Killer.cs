
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace MessageManager
{
    public class KillerArguments : CommandArguments
    {
        public string Id { get; set; }
    }   

    public class Killer : SbsCommand
    {
        public Killer(KeyFetcher keyFetcher, KillerArguments a) :
            base(keyFetcher, a, EntityNameHelper.FormatSubscriptionPath(a.TopicQueueName, a.Name))
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
                    await Receiver.DeadLetterAsync(message.SystemProperties.LockToken);
                    return;
                }
                await Receiver.AbandonAsync(message.SystemProperties.LockToken);
            } while (message != null);
        }
    }
}