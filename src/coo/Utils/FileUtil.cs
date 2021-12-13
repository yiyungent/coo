using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Directory.SetCurrentDirectory(currentDirectory);
            string absolutePath = Path.GetFullPath(relativePath);

            // Fixed: TODO: 这样做有弊端, 导致最后一次执行后，currentDirectory 变化
            // 执行完成后, 重新设置回原来 dir, 防止污染
            Directory.SetCurrentDirectory(originDir);

            return absolutePath;
        }


    }
}
