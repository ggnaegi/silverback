﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Silverback.Messaging;
using Silverback.Messaging.Adapters;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Configuration;
using Silverback.Messaging.Messages;

namespace Silverback.Integration.FileSystem.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Execute();
        }

        public Program()
        {
        }

        private void Execute()
        {
            PrintLogo();

            Console.WriteLine("USAGE");
            Console.WriteLine();
            Console.WriteLine("produce <topic> <message>");
            Console.WriteLine("consume <topic>");
            Console.WriteLine();

            while (true)
            {
                Console.Write("? ");

                var command = Console.ReadLine().Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    switch (command[0].ToLower())
                    {
                        case "produce":
                            Produce(command[1], command[2]);
                            break;
                        case "consume":
                            Consume(command[1]);
                            break;
                        default:
                            WriteError("Unknown command!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    WriteError(ex.Message);
                }
            }
        }

        private FileSystemBroker GetBroker()
        {
            // TODO: An helper like Broker.Create<T>().OnPath would be nice
            return new FileSystemBroker().OnPath(@"D:\Temp\Broker");
        }

        private void Produce(string topicName, string messageContent)
        {
            var message = new TestMessage { Content = messageContent };
            var endpoint = BasicEndpoint.Create(topicName);

            using (var broker = GetBroker())
            {
                broker.GetProducer(endpoint).Produce(Envelope.Create(message));
            }

            WriteSuccess($"Successfully produced message {message.Id} to topic '{topicName}'.");
        }

        private void Consume(string topicName)
        {
            var endpoint = BasicEndpoint.Create(topicName);

            using (var broker = GetBroker())
            {
                var consumer = broker.GetConsumer(endpoint);

                broker.Connect();

                consumer.Received += (_, e) =>
                {
                    var message = (TestMessage)e.Message;
                    WriteLine($"Received message {message.Id} from topic '{topicName}' with content '{message.Content}'.", ConsoleColor.Yellow);
                };

                while (Console.ReadKey(true).Key != ConsoleKey.Q)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void WriteError(string message)
            => WriteLine(message, ConsoleColor.Red);

        private void WriteSuccess(string message)
            => WriteLine(message, ConsoleColor.Green);

        private void WriteLine(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        private void PrintLogo()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("   _|_|_|  _|  _|                                  _|                            _|        ");
            Console.WriteLine(" _|            _|  _|      _|    _|_|    _|  _|_|  _|_|_|      _|_|_|    _|_|_|  _|  _|    ");
            Console.WriteLine("   _|_|    _|  _|  _|      _|  _|_|_|_|  _|_|      _|    _|  _|    _|  _|        _|_|      ");
            Console.WriteLine("       _|  _|  _|    _|  _|    _|        _|        _|    _|  _|    _|  _|        _|  _|    ");
            Console.WriteLine(" _|_|_|    _|  _|      _|        _|_|_|  _|        _|_|_|      _|_|_|    _|_|_|  _|    _|  ");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.ResetColor();
        }
    }
}
