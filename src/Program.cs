﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MessageManager
{
    class Program
    {
        private static IConfiguration CreateConfiguration()
        {
            var builder = new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile("appsettings.json");

            builder = builder.AddEnvironmentVariables();
            return builder.Build();
        }

        static int Main(string[] args)
        {        
            var resourceGroupOpt = new Option(
                "--resource-group",
                "Name of resource group.",
                new Argument<string>() { Arity = ArgumentArity.ExactlyOne }
            );   
            resourceGroupOpt.AddAlias("-g");

            var primaryOpt = new Option(
                "--primaryKey",
                "Use primary or secondary key",
                new Argument<bool>(defaultValue:true)
            );            
            var namespaceOpt = new Option(
                "--namespace-name",
                "servicebus namespace name",
                new Argument<string>()
            );
            var keyNameOpt = new Option(
                "--keyName",
                "Name of access policy",
                new Argument<string>()
            );
            var keyOpt = new Option(
                "--key",
                "SharedAccessKey",
                new Argument<string>()
            );
            var topicNameOpt = new Option(
                "--topic-name",
                "servicebus topic name",
                new Argument<string>()
            );
            var subscriptionNameOpt = new Option(
                "--name",
                "servicebus subscription name",
                new Argument<string>()
            );
            var deadOpt = new Option(
                "--dead",
                "use deadletter queue",
                new Argument<bool>(defaultValue:false)
            );
            var detailsOpt = new Option(
                "--details",
                "display details",
                new Argument<bool>(defaultValue:false)
            );

            var list = new Command("list")
            {
                resourceGroupOpt,primaryOpt,namespaceOpt,keyNameOpt,keyOpt,topicNameOpt,subscriptionNameOpt,deadOpt,detailsOpt
            };
            var command = new RootCommand()
            {
                list
            };

            list.Handler = CommandHandler.Create<string, bool, ReceiverArguments>((resourceGroup, primaryKey, a) =>
            {
                new Receiver(new KeyFetcher(resourceGroup, primaryKey), a)
                    .Peek(a.Details).GetAwaiter().GetResult();

            });

            var configuration = CreateConfiguration();

            return command.InvokeAsync(AddRequiredListOptions(args)).Result;
        }

        private static string[] AddRequiredListOptions(string[] args)
        {
            var result = new List<string>(args);
            if (!result.Contains("--resource-group") && !result.Contains("-g"))
                result.Add("--resource-group");
            if (!result.Contains("--namespace-name"))
                result.Add("--namespace-name");
            if (!result.Contains("--keyName"))
                result.Add("--keyName");   
            if (!result.Contains("--topic-name"))
                result.Add("--topic-name");        
            if (!result.Contains("--name"))
                result.Add("--name"); 
            return result.ToArray();
        }

        private static bool CheckRequiredOptions(string resourceGroup, ReceiverArguments a)
        {
            if (string.IsNullOrEmpty(resourceGroup) )
            {
                Console.WriteLine("Resource group name is required");
                return false;
            }
            if (string.IsNullOrEmpty(a.NamespaceName) )
            {
                Console.WriteLine("Namespace is required");
                return false;
            }
            if (string.IsNullOrEmpty(a.KeyName) )
            {
                Console.WriteLine("Key name is required");
                return false;
            }
            if (string.IsNullOrEmpty(a.TopicName) )
            {
                Console.WriteLine("Topic name is required");
                return false;
            }
            if (string.IsNullOrEmpty(a.Name) )
            {
                Console.WriteLine("Subscription name is required");
                return false;
            }

            return true;
        }
    }
}
