using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace MessageManager
{
    public class DeleterArguments
    {
        public string NamespaceName { get; set; }
        public string KeyName { get; set; }
        public string Key { get; set; }
        public string TopicName { get; set; }
        public string Name { get; set; }
        public bool Dead { get; set; }
        public string Id { get; set; }
    } 

    public class Deleter
    {
        private MessageReceiver Receiver { get; }

        public Deleter(KeyFetcher keyFetcher, DeleterArguments a)
        {
            var k = string.IsNullOrEmpty(a.Key) ? keyFetcher.Getkey(a.NamespaceName, a.KeyName, a.TopicName).Replace("\"", "") : a.Key;
            if (string.IsNullOrEmpty(k))
            {
                Console.WriteLine($"Failed to get key from azure. \nCheck if this command works:\n az servicebus topic authorization-rule keys list -g <rg name> --namespace-name <ns> --name {a.KeyName} --topic-name {a.TopicName} --query primaryKey ");
                throw new Exception("Failed to get key from azure");
            }
            var connectionString = $"Endpoint=sb://{a.NamespaceName}.servicebus.windows.net/;SharedAccessKeyName={a.KeyName};SharedAccessKey={k}";

            Receiver = new MessageReceiver(connectionString, EntityNameHelper.FormatSubscriptionPath(a.TopicName, a.Name + (a.Dead ? "/$DeadLetterQueue" : "")));   
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