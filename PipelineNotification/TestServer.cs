using System;
using System.Collections.Generic;

namespace PipelineNotification
{
    class TestServer : INotificationHandler
    {
        private INotification notity;
        public void Run()
        {
            notity = new Notification();
            notity.Listen("fuckfuck");
            NotificationManager.ShareNotificationManager.AddClient("fuckfuck", this);
            notity.Notify("fuckfuck", new Dictionary<string, string>()
            {
                { "test","test" }
            });
        }

        public void Send()
        {
            notity.Notify("fuckfuck", new Dictionary<string, string>()
            {
                { "test","test" }
            });
        }
        public void NotifiedBy(object message, string topic = "")
        {
            Console.WriteLine(this.GetHashCode() + " get message " + message);
        }
    }
}