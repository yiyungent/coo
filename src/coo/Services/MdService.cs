using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace coo.Services
{
    public class MdService
    {

        /// <summary>
        /// md文件 引用图片统计
        /// </summary>
        /// <param name="postDir">md文件与图片文件 所在目录</param>
        /// <returns>未引用图片路径</returns>
        public List<string> MdImgStat(string postDir)
        {
            #region 扫描图片
            List<string> allImageList = new List<string>();
            List<string> allImagePngList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.png", ref allImagePngList);
            List<string> allImageJpgList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.jpg", ref allImageJpgList);
            List<string> allImageJpegList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.jpeg", ref allImageJpegList);
            List<string> allImageGifList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.gif", ref allImageJpegList);

            allImageList.AddRange(allImagePngList);
            allImageList.AddRange(allImageJpgList);
            allImageList.AddRange(allImageJpegList);
            allImageList.AddRange(allImageGifList);
            #endregion

            #region 扫描md文件
            List<string> referencedImgAbsolutePathList = new List<string>();
            List<string> allMdList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.md", ref allMdList);

            // test
            //allMdList.Add(@"F:\Com\me\Repos\notebook\source\_posts\dotnet-cli-coo.md");
            int mdFileCount = 0;
            foreach (var mdFile in allMdList)
            {
                string mdContent = Utils.FileUtil.ReadStringAsync(mdFile).Result;
                // md文件: xxx/_posts/dotnet-cli-coo.md
                // 匹配图片标记: ![描述](图片url)
                // ![image-20210205221642687](dotnet-cli-coo/image-20210205221642687.png)
                // 正则:   \!\[(?<desc>.*)\]\((?<url>.+)\)
                Regex regex = new Regex(@"\!\[(?<desc>.*)\]\((?<url>.+)\)");
                // 利用 (?<xxx>子表达式) 定义分组别名，这样就可以利用 Groups["xxx"] 进行访问分组/子表达式内容。
                MatchCollection matches = regex.Matches(mdContent);
                for (int i = 1; i <= matches.Count; i++)
                {
                    string imgRelativePath = matches[i - 1].Groups["url"].Value;
                    // 此 md文件 所在目录
                    string mdDir = System.IO.Path.GetDirectoryName(mdFile);
                    // 根据当前md文件路径: 引用图片相对路径 转 绝对路径
                    string imgAbsolutePath = Utils.FileUtil.RelativePathToAbsolutePath(imgRelativePath, mdDir);
                    //Console.WriteLine($"{i} - {imgRelativePath} - {imgAbsolutePath}");

                    //referencedImgAbsolutePathList.Add(imgAbsolutePath);
                    referencedImgAbsolutePathList.Add(imgAbsolutePath.ToLower());
                }

                mdFileCount++;
            }

            List<string> unReferencedImgList = new List<string>();
            foreach (var imgPath in allImageList)
            {
                // TODO: 这里有个小bug, 在 Linux 下路径区分大小写, 而 Windows 不区分, 因此若大小写不同, 则若在 Windows 正常, 在 Linux 下就无效引用
                // 在 Windows 下, 大小写应当不区分, 但这里 Contains 是区分的
                // 为解决这个问题, 干脆大小写不敏感, 统一转小写, 宁可漏未引用的图片, 也不能删除了引用的图片
                //if (!referencedImgAbsolutePathList.Contains(imgPath))
                if (!referencedImgAbsolutePathList.Contains(imgPath.ToLower()))
                {
                    // 未引用图片
                    unReferencedImgList.Add(imgPath);
                }
            }
            #endregion

            #region 统计输出
            Console.WriteLine($"md文件 共: {mdFileCount}");
            Console.WriteLine($"图片 共: {allImageList.Count}");
            Console.WriteLine($"md文件 引用图片 共: {referencedImgAbsolutePathList.Count}");
            Console.WriteLine($"md文件 未引用图片 共: {unReferencedImgList.Count}");
            Console.WriteLine("------------------------------------------------------------------------");
            Console.WriteLine("未引用图片:");
            for (int i = 0; i < unReferencedImgList.Count; i++)
            {
                Console.WriteLine($"{i + 1}: {unReferencedImgList[i]}");
            }
            #endregion

            return unReferencedImgList;
        }

    }
}
