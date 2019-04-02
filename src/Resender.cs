
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace MessageManager
{
    public class ResenderArguments : CommandArguments
    {
    }

    public class Resender : SbsCommand
    {
        private ISenderClient Sender { get; }

        public Resender(KeyFetcher keyFetcher, ResenderArguments a) : 
            base(keyFetcher, a, EntityNameHelper.FormatDeadLetterPath(a.Type == BusType.Queue ? a.TopicQueueName : EntityNameHelper.FormatSubscriptionPath(a.TopicQueueName, a.Name)))
        {
            var k = string.IsNullOrEmpty(a.Key) ? keyFetcher.Getkey(a.NamespaceName, a.KeyName, a.TopicQueueName, a.Type).Replace("\"", "") : a.Key;
            var connectionString = $"Endpoint=sb://{a.NamespaceName}.servicebus.windows.net/;SharedAccessKeyName={a.KeyName};SharedAccessKey={k}";
            
            Sender = a.Type == BusType.Queue ?
                (ISenderClient)new QueueClient(connectionString, a.TopicQueueName) :
                new TopicClient(connectionString, a.TopicQueueName); 
        }    

        public async Task Execute()
        {
            Message message;
            int count = 0;
            do
            {
                message = await Receiver.ReceiveAsync(TimeSpan.FromSeconds(5));

                if (message != null)                
                {                    
                    await Sender.SendAsync(Copy(message));
                    await Receiver.CompleteAsync(message.SystemProperties.LockToken);
                    count++;
                }                
            } while (message != null);
            Console.WriteLine($"Resent {count} messages to queue/subscription");
        }

        public Message Copy(Message m) =>
            new Message(m.Body)
            {
                Label = m.Label,
                MessageId = m.MessageId
            };
    }
}