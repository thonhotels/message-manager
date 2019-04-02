using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MessageManager
{
    public class KeyFetcher
    {
        private string ResourceGroupName { get; }
        private string PrimaryOrSecondary { get; }
        private Dictionary<string, string> Cache { get; }

        public KeyFetcher(string resourceGroupName, bool primaryKey)
        {
            ResourceGroupName = resourceGroupName;
            PrimaryOrSecondary = primaryKey ? "primaryKey" : "secondaryKey";
            Cache = new Dictionary<string, string>();
        }

        private string GetKey(string namespaceName, string keyName, string name, BusType type) =>
            $"{namespaceName}-{keyName}-{name}-{type.ToString()}";

        public string Getkey(string namespaceName, string keyName, string name, BusType type)
        {
            var key = GetKey(namespaceName, keyName, name, type);
            if (Cache.ContainsKey(key))
                return Cache[key];
            var process = new Process();
            process.StartInfo.FileName = "az";
            process.StartInfo.Arguments = BuildArguments(namespaceName, keyName, name, type);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                var result = process.StandardOutput.ReadLine();
                Cache.Add(key, result);
                return result;
            }
            return "";
        }

        private string BuildArguments(string namespaceName, string keyName, string name, BusType type) => 
            type == BusType.Queue ?
                $"servicebus topic authorization-rule keys list -g {ResourceGroupName} --namespace-name {namespaceName} --name {keyName} --topic-name {name} --query {PrimaryOrSecondary}" :
                $"servicebus queue authorization-rule keys list -g {ResourceGroupName} --namespace-name {namespaceName} --name {keyName} --queue-name {name} --query {PrimaryOrSecondary}";
    }
}