using System;
using System.Security.Cryptography;

namespace synczfs.CommonObjects
{
    class ToolBox
    {
        public static string HashStringSha256(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new Exception("Text is null or empty!!!");
            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
    }
}