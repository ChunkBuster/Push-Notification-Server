﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Mono.Options;
using Newtonsoft.Json;
using PushNotificationServer.Server;
using PushNotificationServer.Services;
using PushNotificationServer.UserIOData;

namespace PushNotificationServer {
    internal class Program {
        private static bool _running = true;
        private static string _url = "http://+:3010/";
        private static bool _help;
        private static bool _writeToDisk = true;
        private static int _threads = 4;
        private static bool _clientTest;

        private static int Main(string[] args) {
            var p = new OptionSet {
                {"u|url=", $"The {{url}} to bind to, including port. (Default: {_url})", v => _url = v}, {
                    "w", $"Flag to indicate that logs should not be written to disk (Default: {_writeToDisk})",
                    v => _writeToDisk = v == null
                }, {
                    "t|threads=", $"The max number of {{threads}} the server will run on. (Default: {_threads})",
                    v => _threads = int.Parse(v)
                },{
                    "v|verbosity=", $"The {{verbosity}} of the server. (Default: {Logger.VerbosityThreshhold})",
                    v => Logger.VerbosityThreshhold = int.Parse(v)
                },
                {"c|clienttest", $"Run a stress test on the server, posing as a client.", v => _clientTest = v != null},
                {"h|?|help", "Show this dialog", v => _help = v != null}
            };

            List<string> unknownCommands;
            try {
                unknownCommands = p.Parse(args);
            }
            catch (Exception e) {
                Logger.Log($"Unable to parse commands:{e.Message}");
                return 1;
            }

            if (unknownCommands.Count > 0)
                Console.WriteLine($"Unknown commands: {string.Join(", ", unknownCommands)}");

            if (_help) {
                p.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            if (_clientTest) {
                RunClientTest(_threads);
                return 0;
            }

            var server = new NotificationServer(_url, _threads, _writeToDisk);

            #region commands

            var commands = new Dictionary<string, Action>();
            commands.Add("list",
                () => Console.WriteLine($"Active Notifications: " +
                                        $"{Environment.NewLine}{NotificationInfoLoader.ToString()}"));
            commands.Add("restart", server.Restart);
            commands.Add("reload", NotificationInfoLoader.Reload);
            commands.Add("exit", () => { _running = false; });
#if DEBUG
            commands.Add("help", () => Console.WriteLine($"Commands: {string.Join(", ", commands.Keys)}"));
            commands.Add("crashall", () => server.CrashServer());
            commands.Add("testlog", () => Logger.Log("Test log"));
            commands.Add("testlogwarning", () => Logger.LogWarning("Test warning log"));
            commands.Add("testlogerror", () => Logger.LogError("Test error log"));
#endif

#endregion

            server.Start();
            Thread.Sleep(100);
            //touchey touchey (Forces static ctor to trigger)
            NotificationInfoLoader.ToString();
            Console.WriteLine(Environment.NewLine + "Type 'help' for a list of commands");
            while (_running) {
                Console.Write('>');
                var command = Console.ReadLine()?.ToLower();
                if (command == null) continue;
                Console.WriteLine();
                if (!commands.TryGetValue(command, out var action))
                    action = () => Console.WriteLine("Invalid command. Type 'help' for a list of commands.");
                action();
                Thread.Sleep(300);
            }

            server.Stop();
            Logger.Log("Server shut down successfully.");
            return 0;
        }

        private static void RunClientTest(int threads) {
            var workers = new Thread[threads];
            for (var i = 0; i < workers.Length; i++) {
                workers[i] = new Thread(ClientTest);
                workers[i].Start();
            }
        }

        private static void ClientTest() {
            var ensureUse = "";
            var cInfo = new ClientInfo();
            cInfo.Version = "6.2.0";
            var clientInfo = JsonConvert.SerializeObject(cInfo);
            while (true) {
                var request = WebRequest.Create(_url);
                request.Method = WebRequestMethods.Http.Post;
                try {
                    var encodedData = Encoding.ASCII.GetBytes(clientInfo.ToCharArray());
                    request.ContentLength = encodedData.Length;
                    request.ContentType = "application/json";
                    var dataStream = request.GetRequestStream();
                    dataStream.Write(encodedData, 0, encodedData.Length);
                    dataStream.Close();
                    using (var response = request.GetResponse()) {
                        var stream = response.GetResponseStream();
                        if (stream == null) return;
                        using (var sr = new StreamReader(stream)) {
                            var notificationString = sr.ReadToEnd();
                            Console.WriteLine(notificationString);
                        }
                    }
                }
                catch { }
            }
        }
    }
}