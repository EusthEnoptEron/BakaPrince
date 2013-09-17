using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BakaPrince
{
    class Helper
    {
        public static bool Caching = false;

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string GetTemp(string tempName)
        {
            return System.IO.Path.GetTempPath() + tempName; ;       
        }

        public static Stream GetFile(Uri path, string tempName)
        {

            string p = GetTemp(tempName);

            if (!Caching || !File.Exists(p))
            {
                try
                {
                    WebRequest req = HttpWebRequest.Create(path);
                    using (FileStream fileStream = File.Create(p))
                    using (Stream responseStream = req.GetResponse().GetResponseStream())
                    {
                        responseStream.CopyTo(fileStream);
                    }
                }
                catch (WebException e)
                {
                    Console.WriteLine("WARNING: [{0}] {1}", path, e.Message);
                    return Stream.Null;
                }

            }

            return File.OpenRead(p);
        }

        public static string GetString(Uri path)
        {
            string result;
            using (Stream stream = GetFile(path, CalculateMD5Hash(path.ToString()) + ".string"))
            {
                StreamReader reader = new StreamReader(stream);
                result = reader.ReadToEnd();
            }

            return result;
        }


        public static string GetExePath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetAssetsPath()
        {
            return GetExePath() + "\\assets\\";
        }

    }
}
