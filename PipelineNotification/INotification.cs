namespace PipelineNotification
{
    public interface INotification
    {
        /// <summary>
        /// 不断地以此话题监听连接
        /// </summary>
        /// <param name="topic"></param>
        void Listen(string topic);
        /// <summary>
        /// 通知此话题下的所有连接
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        void Notify(string topic, object message);
    }
}