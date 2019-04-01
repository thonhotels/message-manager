
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace MessageManager
{
    public class ResenderArguments
    {
        public string NamespaceName { get; set; }
        public string KeyName { get; set; }
        public string Key { get; set; }
        public string TopicName { get; set; }
        public string Name { get; set; }
    }
    public class Resender
    {
        private MessageReceiver Receiver { get; }
        private TopicClient Sender { get; }

        public Resender(KeyFetcher keyFetcher, ResenderArguments a)
        {
            var k = string.IsNullOrEmpty(a.Key) ? keyFetcher.Getkey(a.NamespaceName, a.KeyName, a.TopicName).Replace("\"", "") : a.Key;
            if (string.IsNullOrEmpty(k))
            {
                Console.WriteLine($"Failed to get key from azure. \nCheck if this command works:\n az servicebus topic authorization-rule keys list -g <rg name> --namespace-name <ns> --name {a.KeyName} --topic-name {a.TopicName} --query primaryKey ");
                throw new Exception("Failed to get key from azure");
            }
            var connectionString = $"Endpoint=sb://{a.NamespaceName}.servicebus.windows.net/;SharedAccessKeyName={a.KeyName};SharedAccessKey={k}";

            var subscriptionPath = EntityNameHelper.FormatSubscriptionPath(a.TopicName, a.Name);

            Receiver = new MessageReceiver(connectionString, EntityNameHelper.FormatDeadLetterPath(subscriptionPath));  
            Sender = new TopicClient(connectionString, a.TopicName); 
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