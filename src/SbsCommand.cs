using System;
using MessageManager.Key;
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

        public SbsCommand(string connectionString, string entityPath)
        {
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
