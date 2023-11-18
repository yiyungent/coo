using coo.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace coo.Services
{
    public class RImgService
    {
        /// <summary>
        /// 重命名图片文件名, 并且将图片文件对应的 md 文件的引用替换为新文件名
        /// 目的: 大多数粘贴的图片文件名带有创建时间, 容易泄露隐私
        /// </summary>
        /// <param name="postDir"></param>
        public void RenameImg(string postDir)
        {
            // md 文件:          _posts/2023/02/nps-notebook.md
            // 图片文件夹:        _posts/2023/02/nps-notebook
            // 图片文件:          _posts/2023/02/nps-notebook/image-2023-02-25-16-40-33.png
            // md 文件中图片引用: ![image-2023-02-25-16-40-33](nps-notebook/image-2023-02-25-16-40-33.png)
            // 注意: 路径比较都要忽略大小写

            List<string> allImageList = new List<string>();
            #region 扫描图片
            List<string> allImagePngList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.png", ref allImagePngList);
            List<string> allImageJpgList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.jpg", ref allImageJpgList);
            List<string> allImageJpegList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.jpeg", ref allImageJpegList);
            List<string> allImageGifList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.gif", ref allImageGifList);
            List<string> allImagWebpList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.webp", ref allImagWebpList);

            allImageList.AddRange(allImagePngList);
            allImageList.AddRange(allImageJpgList);
            allImageList.AddRange(allImageJpegList);
            allImageList.AddRange(allImageGifList);
            allImageList.AddRange(allImagWebpList);
            #endregion

            List<string> allMdList = new List<string>();
            #region 扫描md文件
            Utils.FileUtil.GetFiles(postDir, "*.md", ref allMdList);
            #endregion

            #region 重命名图片文件名并将对应的md文件中引用替换
            int i = 0;
            foreach (var imageFilePath in allImageList)
            {
                try
                {
                    // 图片文件重命名
                    #region 图片文件重命名
                    string oldFileName = Path.GetFileName(imageFilePath);
                    string oldFileNameWithoutExt = Path.GetFileNameWithoutExtension(oldFileName);
                    string fileMd5 = FileUtil.GetMD5HashFromFile(imageFilePath);
                    string newFileName = $"image-{fileMd5}{Path.GetExtension(imageFilePath)}";
                    string newFileNameWithoutExt = Path.GetFileNameWithoutExtension(newFileName);
                    string imageFileDir = Path.GetDirectoryName(imageFilePath);
                    string newImageFilePath = Path.Combine(imageFileDir, newFileName);
                    if (File.Exists(newImageFilePath))
                    {
                        // 已存在: 重复: 由于是从文件内容计算 md5, 因此甚至是图片文件重复
                        Console.WriteLine($"{(i + 1)}. 已存在: {newImageFilePath}");
                        continue;
                    }
                    else
                    {
                        File.Move(imageFilePath, newImageFilePath);

                        Console.WriteLine($"{(i + 1)}.");
                        Console.WriteLine(imageFilePath);
                        Console.WriteLine("->");
                        Console.WriteLine(newImageFilePath);
                    }
                    #endregion
                    // 对应的 md 文件中图片引用替换
                    #region 图片引用替换
                    string mdFilePath = allMdList.FirstOrDefault(m => m.ToLower() == $"{imageFileDir}.md".ToLower());
                    if (!string.IsNullOrEmpty(mdFilePath))
                    {
                        string mdFileContent = File.ReadAllText(mdFilePath, System.Text.Encoding.UTF8);

                        #region 替换
                        // md 文件中图片引用:
                        // ![image-2023-02-25-16-40-33](nps-notebook/image-2023-02-25-16-40-33.png)
                        string mdFileNameWithoutExt = Path.GetFileNameWithoutExtension(mdFilePath);
                        // 注意: 尽量匹配精确一点,
                        // 有部分图片为 1.png, 2.png 等,
                        // 以及部分引用为 ![image-2023-02-25-16-40-33](./nps-notebook/image-2023-02-25-16-40-33.png)
                        string newMdFileContent = mdFileContent
                            .Replace($"{mdFileNameWithoutExt}/{oldFileName}", $"{mdFileNameWithoutExt}/{newFileName}", StringComparison.InvariantCultureIgnoreCase);
                        //newMdFileContent = mdFileContent.Replace($"![{oldFileNameWithoutExt}](", $"![{newFileNameWithoutExt}](");
                        newMdFileContent = newMdFileContent
                            .Replace($"![{oldFileNameWithoutExt}](", $"![](", StringComparison.InvariantCultureIgnoreCase)
                            .Replace($"![{oldFileName}](", $"![](", StringComparison.InvariantCultureIgnoreCase);
                        #endregion

                        File.WriteAllText(mdFilePath, newMdFileContent, System.Text.Encoding.UTF8);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{(i + 1)}.");
                    LogUtil.Exception(ex);
                }
                i++;
            }
            #endregion

        }
    }
}
