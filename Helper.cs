using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BakaPrince
{
    class Helper
    {
        public static bool Caching = false;

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            foreach (byte t in hash)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }

        public static string GetTemp(string tempName)
        {
            return Path.GetTempPath() + tempName;
        }

        public static Stream GetFile(Uri path, string tempName)
        {

            string p = GetTemp(tempName);

            if (!Caching || !File.Exists(p))
            {
                try
                {
                    WebRequest req = WebRequest.Create(path);
                    using (FileStream fileStream = File.Create(p))
                    using (Stream responseStream = req.GetResponse().GetResponseStream())
                    {
                        if (responseStream != null) responseStream.CopyTo(fileStream);
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
                var reader = new StreamReader(stream);
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


        public static Uri Cwd
        {
            get
            {
                return new Uri(Directory.GetCurrentDirectory() + "\\");
            }
        }
    }
}
