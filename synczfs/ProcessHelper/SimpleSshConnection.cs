using System.IO;
using System.Text;
using Renci.SshNet;
using synczfs.CommonObjects;

namespace synczfs.processhelper
{
    public class SimpleSshConnection : SimpleProcessBase
    {
        SshClient Client { get; set;}

        public SimpleSshConnection(Target target) : base(target)
        {
            InitializeSshConnection();
        }

        private void InitializeSshConnection()
        {
            ConnectionInfo ci = new ConnectionInfo(CurrentTarget.Host, CurrentTarget.Username, new AuthenticationMethod[] { new PasswordAuthenticationMethod(CurrentTarget.Username, "linux")});

            Client = new SshClient(ci);
            Client.Connect();
        }

        internal override ProcessResult ExecInternal(string command)
        {
            SshCommand cmdResult = Client.RunCommand(command);
            ProcessResult procResult = new ProcessResult(cmdResult.ExitStatus, cmdResult.Result, cmdResult.Error);
            cmdResult.Dispose();
            return procResult;
        }

        private string ReadStringFromStream(System.IO.Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public override void Dispose()
        {
            Client?.Disconnect();
            Client?.Dispose();
        }
    }
}