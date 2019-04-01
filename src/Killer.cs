
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace MessageManager
{
    public class KillerArguments
    {
        public string NamespaceName { get; set; }
        public string KeyName { get; set; }
        public string Key { get; set; }
        public string TopicName { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
    }   

    public class Killer
    {
        private MessageReceiver Receiver { get; }

        public Killer(KeyFetcher keyFetcher, KillerArguments a)
        {
            var k = string.IsNullOrEmpty(a.Key) ? keyFetcher.Getkey(a.NamespaceName, a.KeyName, a.TopicName).Replace("\"", "") : a.Key;
            if (string.IsNullOrEmpty(k))
            {
                Console.WriteLine($"Failed to get key from azure. \nCheck if this command works:\n az servicebus topic authorization-rule keys list -g <rg name> --namespace-name <ns> --name {a.KeyName} --topic-name {a.TopicName} --query primaryKey ");
                throw new Exception("Failed to get key from azure");
            }
            var connectionString = $"Endpoint=sb://{a.NamespaceName}.servicebus.windows.net/;SharedAccessKeyName={a.KeyName};SharedAccessKey={k}";

            var subscriptionPath = EntityNameHelper.FormatSubscriptionPath(a.TopicName, a.Name);

            Receiver = new MessageReceiver(connectionString, subscriptionPath); 
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