using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PipelineNotification;

namespace Pipeline
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("input pipe name");
            while (true)
            {
                string[] cmd = Console.ReadLine().Trim().Split(' ');
                if (cmd[0] == "l")
                {
                    cmd.Skip(1).ToList().ForEach(p =>
                    {
                        Notification.ShareNotification.Listen(p);
                    });
                }
                if (cmd[0] == "s")
                {
                    try
                    {
                        Notification.ShareNotification.Notify(cmd[1], cmd[2]);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
