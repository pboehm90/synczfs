using System.Collections.Generic;
using System.Text;
using synczfs.processhelper;

namespace synczfs.CommonObjects
{
    public class Target
    {
        string TargetString { get; }
        public bool UseSsh { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set;}
        public string Host { get; private set; }
        public string ZfsPath { get; private set; }
        public ushort SshPort {get; private set;} = 22;
        public SimpleProcessBase Shell { get; private set; }
        public Target(string target)
        {
            TargetString = target;

            UseSsh = ParseSSH();
            if (UseSsh && Password != null)
            {
                // Using SSH.NET Library
                Shell = new SimpleSshConnection(this);
            }
            else
            {
                // If Password not specified and SSH is used so SSH is done by the shell command
                Shell = new SimpleShellProcess(this);
            }
        }

        private bool ParseSSH()
        {
            try
            {
                string user = null;
                string pass = null;
                int endPos = Target.ParseOut(TargetString, 0, null, new string[] { "@", ":" }, false, out user);
                try
                {
                    endPos = Target.ParseOut(TargetString, endPos, new string[] { ":" }, new string[] { "@" }, true, out pass);
                    Password = pass;
                }
                catch (System.Exception)
                {
                    // No Password defined!
                }
                string host = null;
                endPos = Target.ParseOut(TargetString, endPos, new string[] { "@" }, new string[] { ":", "://" }, false, out host);
                
                string port = null;
                try
                {
                    endPos = Target.ParseOut(TargetString, endPos, new string[] { ":" }, new string[] { "://" }, false, out port);
                    SshPort = ushort.Parse(port);
                }
                catch (System.Exception)
                {
                    // No Port specified!
                }

                string zfsPath = null;
                endPos = Target.ParseOut(TargetString, endPos, new string[] { "://" }, null, false, out zfsPath);

                Username = user;
                Host = host;
                ZfsPath = zfsPath;
            }
            catch (System.Exception ex)
            {
                ZfsPath = TargetString;
            }

            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Host);
        }
        
        public static int ParseOut(string inputString, int startPos, string[] left, string[] right, bool beginSearchRight, out string result)
        {
            int beginLeft = SearchFromLeft(inputString, startPos, left, true);
            int endRight;

            if (right == null)
                endRight = inputString.Length;
            else
            {
                if (beginSearchRight)
                    endRight = SearchFromRight(inputString, beginLeft, right);
                else
                    endRight = SearchFromLeft(inputString, beginLeft, right, false);
            }

            result = inputString.Substring(beginLeft, endRight - beginLeft);
            return endRight;
        }

        private static int SearchFromLeft(string inputString, int startPos, string[] left, bool leftSide)
        {
            int beginLeft = -1;

            if (left == null)
                beginLeft = startPos;
            else
            {
                StringBuilder strBuf = new StringBuilder();
                for (int i = startPos; i < inputString.Length; i++)
                {
                    strBuf.Append(inputString[i]);

                    foreach (string cmpLeft in left)
                    {
                        if (strBuf.ToString().EndsWith(cmpLeft))
                        {
                            beginLeft = i;
                            if (leftSide)
                                beginLeft += 1;
                            else
                                beginLeft -= (cmpLeft.Length - 1);
                            break;
                        }
                    }

                    if (beginLeft >= 0)
                        break;
                }
            }

            if (beginLeft < 0)
                throw new System.Exception("Left Side not found!");

            return beginLeft;
        }

        private static int SearchFromRight(string inputString, int minimalPosition, string[] right)
        {
            int beginRight = -1;

            if (right == null)
                beginRight = right.Length;
            else
            {
                List<char> chars = new List<char>();
                for (int i = inputString.Length - 1; i >= 0; i--)
                {
                    if (i == minimalPosition)
                        throw new System.Exception("Minimal Position reached!");
                    chars.Add(inputString[i]);

                    foreach (string cmpRight in right)
                    {
                        if (FindStringInReverseCharList(chars, cmpRight))
                        {
                            beginRight = i;
                            break;
                        }
                    }

                    if (beginRight >= 0)
                        break;
                }
            }

            if (beginRight < 0)
                throw new System.Exception("Right Side not found!");

            return beginRight;
        }

        private static bool FindStringInReverseCharList(List<char> reverseChars, string compare)
        {
            int matching = 0;

            for (int i = reverseChars.Count - 1; i >= 0; i--)
            {
                if (matching == compare.Length)
                    return true;

                if (reverseChars[i] == compare[matching])
                    matching++;
                else
                    matching = 0;
            }

            return false;
        }

        private void ParseHost(string hostString)
        {
            string[] split = hostString.Split(':');
            Host = split[0];
            if (split.Length == 2)
                SshPort = ushort.Parse(split[1]);
        }
    }
}