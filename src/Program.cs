using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MessageManager
{
    public enum BusType { Queue, Topic } 

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

            var typeOpt = new Option(
                "--type",
                "Queue or Topic.",
                new Argument<BusType>() { Arity = ArgumentArity.ExactlyOne }
            );   
            typeOpt.AddAlias("-t");

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
            var queueNameOpt = new Option(
                "--queue-name",
                "servicebus queue name",
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

            var idOpt = new Option(
                "--id",
                "message id",
                new Argument<string>()
            );

            var labelOpt = new Option(
                "--label",
                "message label",
                new Argument<string>()
            );

            var pathOpt = new Option(
                "--path",
                "path to file with message content to send",
                new Argument<string>()
            );

            var list = new Command("list")
            {
                resourceGroupOpt,typeOpt,primaryOpt,namespaceOpt,keyNameOpt,keyOpt,topicNameOpt,queueNameOpt,subscriptionNameOpt,deadOpt,detailsOpt
            };

            var resend = new Command("resend")
            {
                resourceGroupOpt,typeOpt,primaryOpt,namespaceOpt,keyNameOpt,keyOpt,topicNameOpt,queueNameOpt,subscriptionNameOpt
            };

            var kill = new Command("kill")
            {
                resourceGroupOpt,typeOpt,primaryOpt,namespaceOpt,keyNameOpt,keyOpt,topicNameOpt,queueNameOpt,subscriptionNameOpt,idOpt
            };

            var delete = new Command("delete")
            {
                resourceGroupOpt,typeOpt,primaryOpt,namespaceOpt,keyNameOpt,keyOpt,topicNameOpt,queueNameOpt,subscriptionNameOpt,deadOpt,idOpt
            };

            var send = new Command("send")
            {
                resourceGroupOpt,typeOpt,primaryOpt,namespaceOpt,keyNameOpt,keyOpt,topicNameOpt,queueNameOpt,subscriptionNameOpt,idOpt,labelOpt,pathOpt
            };

            var command = new RootCommand()
            {
                list, resend, kill, delete, send
            };

            list.Handler = CommandHandler.Create<string, bool, ListerArguments>((resourceGroup, primaryKey, a) =>
            {
                // if (!System.Diagnostics.Debugger.IsAttached) 
                //     Console.WriteLine($"Please attach a debugger, PID: {System.Diagnostics.Process.GetCurrentProcess().Id}");
                // while (!System.Diagnostics.Debugger.IsAttached) 
                //     System.Threading.Thread.Sleep(100);
                // System.Diagnostics.Debugger.Break();                 
                new Lister(new KeyFetcher(resourceGroup, primaryKey), a)
                    .Execute(a.Details).GetAwaiter().GetResult();
            });

            resend.Handler = CommandHandler.Create<string, bool, ResenderArguments>((resourceGroup, primaryKey, a) =>
            {           
                new Resender(new KeyFetcher(resourceGroup, primaryKey), a)
                    .Execute().GetAwaiter().GetResult();
            });

            kill.Handler = CommandHandler.Create<string, bool, KillerArguments>((resourceGroup, primaryKey, a) =>
            {               
                new Killer(new KeyFetcher(resourceGroup, primaryKey), a)
                    .Execute(a.Id).GetAwaiter().GetResult();
            });

            delete.Handler = CommandHandler.Create<string, bool, DeleterArguments>((resourceGroup, primaryKey, a) =>
            {               
                new Deleter(new KeyFetcher(resourceGroup, primaryKey), a)
                    .Execute(a.Id).GetAwaiter().GetResult();
            });

            send.Handler = CommandHandler.Create<string, bool, SenderArguments>((resourceGroup, primaryKey, a) =>
            {               
                new Sender(new KeyFetcher(resourceGroup, primaryKey), a)
                    .Execute(a.Path, a.Id, a.Label).GetAwaiter().GetResult();
            });

            var configuration = CreateConfiguration();  

            return command.InvokeAsync(AddRequiredListOptions(args)).Result;
        }

        private static string[] AddRequiredListOptions(string[] args)
        {
            var result = new List<string>(args);
            if (!result.Contains("--resource-group") && !result.Contains("-g"))
                result.Add("--resource-group");
            if (!result.Contains("--type") && !result.Contains("-t"))
                result.Add("--type Queue");
            if (!result.Contains("--namespace-name"))
                result.Add("--namespace-name");
            if (!result.Contains("--keyName"))
                result.Add("--keyName");   
            if (!result.Contains("--topic-name") && !result.Contains("--queue-name"))
                result.Add("--topic-name");  
            // if (!result.Contains("--name"))
            //     result.Add("--name"); 
            return result.ToArray();
        }

        private static bool CheckRequiredOptions(string resourceGroup, ListerArguments a)
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
            if (string.IsNullOrEmpty(a.TopicQueueName) )
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

// copy this into a handler to break when starting from command line
                // if (!System.Diagnostics.Debugger.IsAttached) 
                //     Console.WriteLine($"Please attach a debugger, PID: {System.Diagnostics.Process.GetCurrentProcess().Id}");
                // while (!System.Diagnostics.Debugger.IsAttached) 
                //     System.Threading.Thread.Sleep(100);
                // System.Diagnostics.Debugger.Break();         
    }
}
