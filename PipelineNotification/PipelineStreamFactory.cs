using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PipelineNotification
{
    public static class PipelineStreamFactory
    {
        public static T GetPipelineStream<T>(string tpc, bool isServer) where T : PipeStream
        {
            T ret = null;
            if (isServer)
            {
                var pipe = new NamedPipeServerStream(tpc, PipeDirection.InOut, 200, PipeTransmissionMode.Message);
                PipeSecurity ps = pipe.GetAccessControl();
                PipeAccessRule clientRule = new PipeAccessRule(
                    //"Authenticated Users"
                    new SecurityIdentifier(WellKnownSidType.AnonymousSid, null),
                    PipeAccessRights.ReadWrite, AccessControlType.Allow);
                PipeAccessRule ownerRule = new PipeAccessRule(
                    WindowsIdentity.GetCurrent().Owner,
                    PipeAccessRights.FullControl,
                    AccessControlType.Allow);
                ps.AddAccessRule(clientRule);
                ps.AddAccessRule(ownerRule);
                ret = (T)(PipeStream)pipe;
            }
            else
            {
                string ip = "localhost";
                var topic = tpc.ToString();
                if (topic.IndexOf("@") >= 0)
                {
                    ip = topic.Split('@')[0];
                    topic = topic.Split('@')[1];
                }
                NamedPipeClientStream client = new NamedPipeClientStream(ip, topic);
                //var access = client.GetAccessControl();
                //var secIden = new SecurityIdentifier(WellKnownSidType.AnonymousSid, null);
                //access.AddAccessRule(new PipeAccessRule(secIden, PipeAccessRights.ReadWrite, AccessControlType.Allow));
                ret = (T)(PipeStream)client;
            }
            return ret;
        }
    }
}