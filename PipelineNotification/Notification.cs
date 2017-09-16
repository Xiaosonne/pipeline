using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using Newtonsoft.Json;

namespace PipelineNotification
{
    public class Notification : INotification
    {
        private ConcurrentDictionary<string, List<NamedPipeServerStream>> _servers;
        private ConcurrentDictionary<string, Queue<object>> _topicMessageCache;
        private Dictionary<string, Thread> _connectionThreads;
        private Dictionary<string, Thread> _handlerThreads;
        private HashSet<NamedPipeServerStream> usedPipeline;
        internal Notification()
        {
            _servers = new ConcurrentDictionary<string, List<NamedPipeServerStream>>();
            _topicMessageCache = new ConcurrentDictionary<string, Queue<object>>();
            _connectionThreads = new Dictionary<string, Thread>();
            _handlerThreads = new Dictionary<string, Thread>();
            usedPipeline = new HashSet<NamedPipeServerStream>();
        }

        static Notification()
        {
            shareNotification = new Notification();
        }
        static private Notification shareNotification;
        public static Notification ShareNotification
        {
            get { return shareNotification; }
        }
        public void Listen(string topic)
        {
            List<NamedPipeServerStream> addServer(string p)
            {
                var pipepool = new List<NamedPipeServerStream>();
                var pipe = PipelineStreamFactory.GetPipelineStream<NamedPipeServerStream>(topic, true);
                pipepool.Add(pipe);
                var thread = new Thread(new ParameterizedThreadStart(tpc =>
                {
                    while (true)
                    {
                        //try get  server connections
                        if (_servers.TryGetValue((string)tpc, out var pipepoolInstance))
                        {
                            bool noNewConnection = true;
                            foreach (var connectedPipe in usedPipeline)
                            {
                                if (!connectedPipe.IsConnected)
                                {
                                    pipepoolInstance.Remove(connectedPipe);
                                }
                            }
                            foreach (var namedPipeServerStream in pipepoolInstance)
                            {
                                if (!usedPipeline.Contains(namedPipeServerStream) && !namedPipeServerStream.IsConnected)
                                {
                                    noNewConnection = false;
                                    namedPipeServerStream.WaitForConnection();
                                    usedPipeline.Add(namedPipeServerStream);
                                }
                            }
                            if (noNewConnection)
                            {
                                pipepoolInstance.Add(PipelineStreamFactory.GetPipelineStream<NamedPipeServerStream>(topic, true));
                                Thread.Sleep(10);
                            }
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                    }
                }));
                _connectionThreads.Add(p, thread);
                thread.Start(p);
                return pipepool;
            }

            List<NamedPipeServerStream> updateServer(string p, List<NamedPipeServerStream> s)
            {
                return s;
            }
            _servers.AddOrUpdate(topic, addServer, updateServer);
        }

        public void Notify(string topic, object message)
        {

            Queue<object> addCache(string p)
            {
                Queue<object> que = new Queue<object>();
                que.Enqueue(message);
                var thread = new Thread(new ParameterizedThreadStart(tpc =>
                {
                    Console.WriteLine("read topic " + tpc);
                    while (true)
                    {
                        if (que.Count > 0)
                        {
                            //retrieve one message 
                            var message1 = que.Dequeue();
                            //broadcast to all server connections
                            if (_servers.TryGetValue((string)tpc, out var serverStreams))
                            {
                                bool noNewMessageSend = true;
                                foreach (var con in serverStreams)
                                {
                                    //if server is connected send message
                                    if (con.IsConnected)
                                    {
                                        try
                                        {
                                            StreamWriter sw = new StreamWriter(con);
                                            sw.WriteLine(JsonConvert.SerializeObject(message1));
                                            sw.Flush();
                                            Console.WriteLine("send message");
                                            noNewMessageSend = false;
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                        }
                                    }
                                }
                                if (noNewMessageSend)
                                {
                                    Thread.Sleep(1);
                                }
                            }
                            else
                            {
                                Thread.Sleep(1);
                            }
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                    }
                }));
                _handlerThreads.Add(p, thread);
                thread.Start(p);
                return que;
            }

            Queue<object> updateCache(string p, Queue<object> s)
            {
                s.Enqueue(message);
                return s;
            }

            _topicMessageCache.AddOrUpdate(topic, addCache, updateCache);
        }
    }
}