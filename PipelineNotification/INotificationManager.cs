namespace PipelineNotification
{
    internal interface INotificationManager
    {
        void AddClient(string topic, INotificationHandler client);
    }
}