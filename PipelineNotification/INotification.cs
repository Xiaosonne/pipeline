namespace PipelineNotification
{
    public interface INotification
    {
        /// <summary>
        /// ���ϵ��Դ˻����������
        /// </summary>
        /// <param name="topic"></param>
        void Listen(string topic);
        /// <summary>
        /// ֪ͨ�˻����µ���������
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        void Notify(string topic, object message);
    }
}