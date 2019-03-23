using System;
using System.Diagnostics;

namespace MessageManager
{
    public class KeyFetcher
    {
        private string ResourceGroupName { get; }
        private string PrimaryOrSecondary { get; }

        public KeyFetcher(string resourceGroupName, bool primaryKey)
        {
            ResourceGroupName = resourceGroupName;
            PrimaryOrSecondary = primaryKey ? "primaryKey" : "secondaryKey";
        }

        public string Getkey(string namespaceName, string keyName, string topicName)
        {
            var process = new Process();
            process.StartInfo.FileName = "az";
            process.StartInfo.Arguments = $"servicebus topic authorization-rule keys list -g {ResourceGroupName} --namespace-name {namespaceName} --name {keyName} --topic-name {topicName} --query {PrimaryOrSecondary}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                return process.StandardOutput.ReadLine();
            }
            return "";
        }
    }
}