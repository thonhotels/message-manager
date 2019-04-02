using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MessageManager
{
    public class KeyFetcher
    {
        private string ResourceGroupName { get; }
        private string PrimaryOrSecondary { get; }
        private string PythonPath { get; }

        private Dictionary<string, string> Cache { get; }

        private bool IsWindows() => System.Runtime.InteropServices.RuntimeInformation
                                               .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

        public KeyFetcher(string resourceGroupName, bool primaryKey, string pythonPath)
        {
            ResourceGroupName = resourceGroupName;
            PrimaryOrSecondary = primaryKey ? "primaryKey" : "secondaryKey";
            Cache = new Dictionary<string, string>();
        }

        private string GetKey(string namespaceName, string keyName, string name, BusType type) =>
            $"{namespaceName}-{keyName}-{name}-{type.ToString()}";

        public string Getkey(string namespaceName, string keyName, string topicName, BusType type)
        {
            var key = GetKey(namespaceName, keyName, topicName, type);
            if (Cache.ContainsKey(key))
                return Cache[key];
            var process = new Process();
            process.StartInfo.FileName = IsWindows() ? GetWindowsPath() : "az";
            process.StartInfo.Arguments = IsWindows() ? "-IBm azure.cli " : "" + BuildArguments(namespaceName, keyName, topicName, type);
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

        private string GetWindowsPath() => 
            string.IsNullOrEmpty(PythonPath) ? @"C:\Program Files (x86)\Microsoft SDKs\Azure\CLI2\python.exe" : PythonPath;
    }
}