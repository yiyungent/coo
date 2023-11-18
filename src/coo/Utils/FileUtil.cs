using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace coo.Utils
{
    public class FileUtil
    {
        /// <summary>
        /// 程序一启动为 根目录
        /// </summary>
        public static readonly string AppDir;

        static FileUtil()
        {
            AppDir = Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// 获取文件夹下所有文件
        /// </summary>
        /// <param name="directory">文件夹路径</param>
        /// <param name="pattern">文件类型</param>
        /// <param name="list">集合</param>
        public static void GetFiles(string directory, string pattern, ref List<string> list)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            foreach (FileInfo info in directoryInfo.GetFiles(pattern))
            {
                list.Add(info.FullName);
            }
            foreach (DirectoryInfo info in directoryInfo.GetDirectories())
            {
                GetFiles(info.FullName, pattern, ref list);
            }
        }


        public static async Task<string> ReadStringAsync(string filePath)
        {
            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }


        /// <summary>
        /// 相对路径 转 绝对路径
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <param name="currentDirectory">(要比较的) 当前路径</param>
        /// <returns></returns>
        public static string RelativePathToAbsolutePath(string relativePath, string currentDirectory)
        {
            string originDir = AppDir;
            //Console.WriteLine($"currentDirectory: {currentDirectory}");
            Directory.SetCurrentDirectory(currentDirectory);
            string absolutePath = Path.GetFullPath(relativePath);
            //Console.WriteLine($"absolutePath: {absolutePath}");

            // Fixed: TODO: 这样做有弊端, 导致最后一次执行后，currentDirectory 变化
            // 执行完成后, 重新设置回原来 dir, 防止污染
            Directory.SetCurrentDirectory(originDir);

            return absolutePath;
        }

        public static void GetLocalImageInfo(string imagePath, out long byteSize, out long width, out long height)
        {
            try
            {
                using (FileStream imageStream = File.Open(imagePath, FileMode.Open))
                {
                    byteSize = imageStream.Length;
                }
                //System.Drawing.Image mImage = System.Drawing.Image.FromFile(imagePath);
                var mImage = Image.Load(imagePath);
                width = mImage.Width;
                height = mImage.Height;
            }
            catch (Exception ex)
            {
                byteSize = 0;
                width = 0;
                height = 0;
            }
        }


        /// <summary> 
        /// 获取网络图片的大小和尺寸 
        /// </summary> 
        /// <param name="imageUrl">图片url</param> 
        /// <param name="byteSize">图片大小 (Byte)</param> 
        /// <param name="widthxHeight">图片尺寸（WidthxHeight）</param> 
        public static void GetRemoteImageInfo(string imageUrl, out long byteSize, out long width, out long height)
        {
            try
            {
                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(200);
                var result = httpClient.GetAsync(imageUrl).Result;
                if (result.IsSuccessStatusCode)
                {
                    using (var stream = result.Content.ReadAsStreamAsync().Result)
                    {
                        byteSize = (stream.Length / 1024);
                        var mImage = Image.Load(stream);
                        width = mImage.Width;
                        height = mImage.Height;
                    }
                }
                else
                {
                    byteSize = 0;
                    width = 0;
                    height = 0;
                }
            }
            catch (Exception ex)
            {
                byteSize = 0;
                width = 0;
                height = 0;
            }
        }

        #region 计算MD5
        /// <summary>
        /// 获取文件的MD5码
        /// </summary>
        /// <param name="fileName">文件的完整绝对路径: 传入的文件名（含路径及后缀名）</param>
        /// <returns></returns>
        public static string GetMD5HashFromFile(string filePath)
        {
            try
            {
                FileStream file = new FileStream(filePath, System.IO.FileMode.Open);
                //System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        public static string GetFileMD5(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            int bufferSize = 1048576;
            byte[] buff = new byte[bufferSize];
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            md5.Initialize();
            long offset = 0;
            while (offset < fs.Length)
            {
                long readSize = bufferSize;
                if (offset + readSize > fs.Length)
                    readSize = fs.Length - offset;
                fs.Read(buff, 0, Convert.ToInt32(readSize));
                if (offset + readSize < fs.Length)
                    md5.TransformBlock(buff, 0, Convert.ToInt32(readSize), buff, 0);
                else
                    md5.TransformFinalBlock(buff, 0, Convert.ToInt32(readSize));
                offset += bufferSize;
            }
            if (offset >= fs.Length)
            {
                fs.Close();
                byte[] result = md5.Hash;
                md5.Clear();
                StringBuilder sb = new StringBuilder(32);
                for (int i = 0; i < result.Length; i++)
                    sb.Append(result[i].ToString("X2"));
                return sb.ToString();
            }
            else
            {
                fs.Close();
                return null;
            }
        }
        #endregion
    }
}
