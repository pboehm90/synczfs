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
    }
}