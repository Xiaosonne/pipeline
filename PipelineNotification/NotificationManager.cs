using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using Newtonsoft.Json;

namespace PipelineNotification
{
    public class NotificationManager : INotificationManager
    {
        private Thread _threadForConnection;
        private Thread _threadForTopic;
        private ConcurrentDictionary<string, HashSet<INotificationHandler>> _topicHandlers;
        private Dictionary<string, Thread> _topicMessageSendThreads;

        NotificationManager()
        {
            _topicHandlers = new ConcurrentDictionary<string, HashSet<INotificationHandler>>();
            _topicMessageSendThreads = new Dictionary<string, Thread>();
        }

        static NotificationManager()
        {
            shareNotificationManager = new NotificationManager();
        }

        static private NotificationManager shareNotificationManager;
        public static NotificationManager ShareNotificationManager
        {
            get { return shareNotificationManager; }
        }

        public void AddClient(string topic, INotificationHandler client)
        {
            HashSet<INotificationHandler> AddValue(string p)
            {
                var lis = new HashSet<INotificationHandler>();
                lis.Add(client);
                Thread thread = new Thread(new ParameterizedThreadStart(runForTopic));
                _topicMessageSendThreads.Add(p, thread);
                thread.Start(p);
                return lis;
            }

            HashSet<INotificationHandler> UpdateValue(string p, HashSet<INotificationHandler> q)
            {
                q.Add(client);
                return q;
            }

            _topicHandlers.AddOrUpdate(topic, AddValue, UpdateValue);
        }
        /// <summary>
        /// 为每个话题单开一个线程 和一个 管道来传送数据
        /// </summary>
        /// <param name="topicObj"></param>
        private void runForTopic(object topicObj)
        {
            string topic = topicObj.ToString();
            NamedPipeClientStream client = PipelineStreamFactory.GetPipelineStream<NamedPipeClientStream>(topic, false);

            Console.WriteLine("connecting the server " + topic);
            client.Connect();
            Console.WriteLine("connected the server " + topic);
            while (true)
            {
                if (!client.IsConnected)
                {
                    client = PipelineStreamFactory.GetPipelineStream<NamedPipeClientStream>(topic, false);
                    client.Connect();
                }
                StreamReader sr = new StreamReader(client);
                string message = "";
                try
                {
                    message = sr.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Console.WriteLine("recieve message from " + topic + " " + message);
                if (message != null)
                {
                    var msg = JsonConvert.DeserializeObject<dynamic>(message);
                    if (_topicHandlers.TryGetValue(topic, out var value))
                    {
                        foreach (var notificationHandler in value)
                        {
                            notificationHandler.NotifiedBy(msg, topic);
                        }
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }

            }
        }
    }
}