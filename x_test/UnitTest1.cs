using NUnit.Framework;
using synczfs.CommonObjects;
using synczfs.processhelper;
using synczfs.ZFS.Objects;
using synczfs;
using synczfs.Syncer;

namespace x_test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Pool pool = new Pool();
            Assert.Pass();
        }

        [Test]
        public void TestShellError()
        {
            Target target = new Target("root@localhost");
            var result = target.Shell.Run("df xxx");
        }

        [Test]
        public void TestSyncingOnTestVM()
        {
            string[] args = new string[4];
            args[0] = "dev";
            args[1] = "root@localhost://tank/vm";
            args[2] = "root@localhost://tank_slave/vm";
            args[3] = "-r";

            CliArguments myArgs = new CliArguments(args);
            SyncerProcess.CreateInstance(myArgs).Run();
        }

        [Test]
        public void EnduranceTest()
        {
            for (int i = 0; i < 1000; i++)
            {
                TestSyncingOnTestVM();
            }
        }

        [Test]
        public void NewParse()
        {
            string testStr = "root:fooooo@127.0.0.1://tank_slave/vm";

            string user = null;
            string pass = null;
            int endPos = Target.ParseOut(testStr, 0, null, new string[] { "@", ":" }, false, out user);
            try
            {
                endPos = Target.ParseOut(testStr, endPos, new string[] { ":" }, new string[] { "@" }, true, out pass);
            }
            catch (System.Exception)
            {
                // No Password defined!
            }
            string address = null;
            endPos = Target.ParseOut(testStr, endPos, new string[] { "@" }, new string[] { ":", "://" }, false, out address);
            
            string port = null;
            try
            {
                endPos = Target.ParseOut(testStr, endPos, new string[] { ":" }, new string[] { "://" }, false, out port);
            }
            catch (System.Exception)
            {
                // Kein Port
            }
            
            

            string zfsPath = null;
            endPos = Target.ParseOut(testStr, endPos, new string[] { "://" }, null, false, out zfsPath);
        }
    }
}