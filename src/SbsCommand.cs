using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace MessageManager
{
    public class CommandArguments
    {
        public BusType Type { get; set; }
        public string NamespaceName { get; set; }
        public string KeyName { get; set; }
        public string Key { get; set; }
        public string TopicQueueName { get; set; }
        public string TopicName { get => TopicQueueName; set { TopicQueueName = value; } }
        public string QueueName { get => TopicQueueName; set { TopicQueueName = value; } }
        public string Name { get; set; }        
    }

    public abstract class SbsCommand
    {
        protected MessageReceiver Receiver { get; }

        public SbsCommand(KeyFetcher keyFetcher, CommandArguments a, string entityPath)
        {
            var k = string.IsNullOrEmpty(a.Key) ? keyFetcher.Getkey(a.NamespaceName, a.KeyName, a.TopicQueueName, a.Type).Replace("\"", "") : a.Key;
            if (string.IsNullOrEmpty(k))
            {
                Console.WriteLine($"Failed to get key from azure. \nCheck if this command works:\n az servicebus topic authorization-rule keys list -g <rg name> --namespace-name <ns> --name {a.KeyName} --topic-name {a.TopicQueueName} --query primaryKey ");
                throw new Exception("Failed to get key from azure");
            }
            var connectionString = $"Endpoint=sb://{a.NamespaceName}.servicebus.windows.net/;SharedAccessKeyName={a.KeyName};SharedAccessKey={k}";

            Receiver = new MessageReceiver(connectionString, entityPath);   
        }

        protected static string GetEntityPath(BusType type, string topicQueueName, string subscriptionName, bool dead) =>
            type == BusType.Queue ?
                dead ?  EntityNameHelper.FormatDeadLetterPath(topicQueueName) : topicQueueName :
                EntityNameHelper.FormatSubscriptionPath(topicQueueName, subscriptionName + (dead ? "/$DeadLetterQueue" : ""));

        protected static string GetEntityPath(BusType type, string topicQueueName, string subscriptionName) =>
            type == BusType.Queue ? topicQueueName : EntityNameHelper.FormatSubscriptionPath(topicQueueName, subscriptionName);
    }
}
