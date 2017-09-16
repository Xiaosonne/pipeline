namespace PipelineNotification
{
    public interface INotificationHandler
    {
        void NotifiedBy(object message, string topic = "");
    }
}