using System;
using System.Threading.Tasks;

namespace MessageManager.Key
{
    public static class ConnectionStringHelper
    {
        public static async Task<string> Get(KeyFetcher keyFetcher, CommandArguments a)
        {
            var k = string.IsNullOrEmpty(a.Key) ? (await keyFetcher.Getkey(a.NamespaceName, a.KeyName, a.TopicQueueName, a.Type)).Replace("\"", "") : a.Key;
            if (string.IsNullOrEmpty(k))
            {
                Console.WriteLine($"Failed to get key from azure. \nCheck if this command works:\n az servicebus topic authorization-rule keys list -g <rg name> --namespace-name <ns> --name {a.KeyName} --topic-name {a.TopicQueueName} --query primaryKey ");
                throw new Exception("Failed to get key from azure");
            }

            return $"Endpoint=sb://{a.NamespaceName}.servicebus.windows.net/;SharedAccessKeyName={a.KeyName};SharedAccessKey={k}";
        }
    }
}