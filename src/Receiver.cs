
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace MessageManager
{
    public class ReceiverArguments
    {
        public string NamespaceName { get; set; }
        public string KeyName { get; set; }
        public string Key { get; set; }
        public string TopicName { get; set; }
        public string Name { get; set; }
        public bool Dead { get; set; }
        public bool Details { get; set; }
    }
    
    public class Receiver
    {
        private MessageReceiver Client { get; set; }

        public Receiver(KeyFetcher keyFetcher, ReceiverArguments a)
        {
            var k = string.IsNullOrEmpty(a.Key) ? keyFetcher.Getkey(a.NamespaceName, a.KeyName, a.TopicName).Replace("\"", "") : a.Key;
            if (string.IsNullOrEmpty(k))
            {
                Console.WriteLine($"Failed to get key from azure. \nCheck if this command works:\n az servicebus topic authorization-rule keys list -g <rg name> --namespace-name <ns> --name {a.KeyName} --topic-name {a.TopicName} --query primaryKey ");
                throw new Exception("Failed to get key from azure");
            }
            var connectionString = $"Endpoint=sb://{a.NamespaceName}.servicebus.windows.net/;SharedAccessKeyName={a.KeyName};SharedAccessKey={k}";

            Client = new MessageReceiver(connectionString, EntityNameHelper.FormatSubscriptionPath(a.TopicName, a.Name + (a.Dead ? "/$DeadLetterQueue" : "")));   
        }

        public async Task Peek(bool detailed)
        {
            Message message;
            do
            {
                message = await Client.PeekAsync();

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
