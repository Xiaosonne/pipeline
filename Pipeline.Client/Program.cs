using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PipelineNotification;

namespace Pipeline.Client
{
    class Program : INotificationHandler
    {
        static void Main(string[] args)
        {
            while (true)
            {
                string[] cmd = Console.ReadLine().Trim().Split(' ');
                if (cmd[0] == "t")
                {
                    cmd.Skip(1).ToList().ForEach(p =>
                    {
                        NotificationManager.ShareNotificationManager.AddClient(p, new Program());
                    });
                } 
            }
        }

        public void NotifiedBy(object message, string topic = "")
        {
            Console.WriteLine(topic + " " + message);
        }
    }
}
