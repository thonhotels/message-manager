using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MessageManager.Key
{
    public class KeyFetcher
    {
        private const string Cmd = "cmd.exe";
        private const string Bash = "/bin/bash";

        private string ResourceGroupName { get; }
        private string PrimaryOrSecondary { get; }
        private string PythonPath { get; }
        private string AzureCliDefaultPathWindows { get; }
        private const string AzureCliDefaultPath = "/usr/bin:/usr/local/bin";

        private Dictionary<string, string> Cache { get; }

        private bool IsWindows() => System.Runtime.InteropServices.RuntimeInformation
                                               .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

        public KeyFetcher(string resourceGroupName, bool primaryKey)
        {
            ResourceGroupName = resourceGroupName;
            PrimaryOrSecondary = primaryKey ? "primaryKey" : "secondaryKey";
            Cache = new Dictionary<string, string>();
            AzureCliDefaultPathWindows =
                $"{Environment.GetEnvironmentVariable("ProgramFiles(x86)")}\\Microsoft SDKs\\Azure\\CLI2\\wbin; {Environment.GetEnvironmentVariable("ProgramFiles")}\\Microsoft SDKs\\Azure\\CLI2\\wbin";
        }

        private string GetKey(string namespaceName, string keyName, string name, BusType type) =>
            $"{namespaceName}-{keyName}-{name}-{type.ToString()}";

        public async Task<string> Getkey(string namespaceName, string keyName, string topicName, BusType type)
        {
            var key = GetKey(namespaceName, keyName, topicName, type);
            if (Cache.ContainsKey(key))
                return Cache[key];
            var process = new Process();
            var processManager = new Microsoft.Azure.Services.AppAuthentication.ProcessManager();
            var result = await processManager.ExecuteAsync(
                    new Process 
                    { 
                        StartInfo = GetProcessStartInfo(namespaceName, keyName, topicName, type)
                    });
            if (!string.IsNullOrEmpty(result))
                Cache.Add(key, result);
            return result;
        }

        private ProcessStartInfo GetProcessStartInfo(string namespaceName, string keyName, string topicName, BusType type) 
        {
            var args = $"{BuildArguments(namespaceName, keyName, topicName, type)}";
            return IsWindows() ? 
                GetWindowsProcessStartInfo("/c " + args) :
                GetProcessStartInfo(args);
        }
            
        private ProcessStartInfo GetWindowsProcessStartInfo(string args)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.SystemDirectory, Cmd),
                Arguments = args
            };

            startInfo.Environment["PATH"] = $"{Environment.GetEnvironmentVariable("AzureCLIPath")};{AzureCliDefaultPathWindows}";
            return startInfo;
        }

        private ProcessStartInfo GetProcessStartInfo(string args)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Bash,
                Arguments = args
            };

            startInfo.Environment["PATH"] = $"{Environment.GetEnvironmentVariable("AzureCLIPath")}:{AzureCliDefaultPath}";
            return startInfo;
        }

        private string BuildArguments(string namespaceName, string keyName, string name, BusType type) => 
            type == BusType.Queue ?
                $"az servicebus topic authorization-rule keys list -g {ResourceGroupName} --namespace-name {namespaceName} --name {keyName} --topic-name {name} --query {PrimaryOrSecondary}" :
                $"az servicebus queue authorization-rule keys list -g {ResourceGroupName} --namespace-name {namespaceName} --name {keyName} --queue-name {name} --query {PrimaryOrSecondary}";

        private string GetWindowsPath() => 
            string.IsNullOrEmpty(PythonPath) ? @"C:\Program Files (x86)\Microsoft SDKs\Azure\CLI2\python.exe" : PythonPath;
    }
}