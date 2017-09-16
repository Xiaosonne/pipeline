using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PipelineNotification
{
    static class run
    {
        public static void Main(string[] args)
        {
            TestServer ts = new TestServer();
            ts.Run();
            NotificationManager.ShareNotificationManager.AddClient("fuckfuck", new TestServer());
            NotificationManager.ShareNotificationManager.AddClient("fuckfuck", new TestServer());
            while (Console.ReadLine() != "quit")
            {
                ts.Send();
            }
            Console.ReadLine();
        }
    }

}
