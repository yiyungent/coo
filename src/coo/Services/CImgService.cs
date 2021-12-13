using coo.Models.CImg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace coo.Services
{
    public class CImgService
    {

        /// <summary>
        /// 文件 引用图片统计
        /// </summary>
        /// <param name="postDir">文件与图片文件 所在目录</param>
        /// <param name="ignorePathsStr">忽略删除这些路径</param>
        /// <returns>未引用图片路径</returns>
        public CImgStatModel CImgStat(string postDir, bool githubAction = false, string ignorePathsStr = null, bool delete = false)
        {
            bool debug = Convert.ToBoolean(Utils.GitHubActionsUtil.GetEnv("cia_debug"));
            CImgStatModel rtnModel = null;
            List<string> ignorePaths = new List<string>();
            if (!string.IsNullOrEmpty(ignorePathsStr))
            {
                ignorePaths = ignorePathsStr.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            #region 扫描图片 - 本地图片
            List<string> allImageList = new List<string>();
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

            #region 扫描文件
            // 所有引用的本地图片 绝对路径
            List<string> referencedImgAbsolutePathList = new List<string>();
            // 所有引用的网络图片 URL
            List<string> referencedImgUrlList = new List<string>();

            List<string> allList = new List<string>();

            List<string> allMdList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.md", ref allMdList);
            allList.AddRange(allMdList);
            List<string> allHtmlList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.html", ref allHtmlList);
            allList.AddRange(allHtmlList);
            List<string> allHtmList = new List<string>();
            Utils.FileUtil.GetFiles(postDir, "*.htm", ref allHtmList);
            allList.AddRange(allHtmList);

            // TODO: 匹配 css

            int fileCount = 0;
            foreach (var file in allList)
            {
                string fileContent = Utils.FileUtil.ReadStringAsync(file).Result;

                // 注意: 对所有文件都要执行两种匹配, 因为在 md 文件中也有可能存在 HTML 形式的 <img> 引用
                // 宁可漏删, 不可误删

                #region 任务1: 匹配 Markdown 图片标记
                // md文件: xxx/_posts/dotnet-cli-coo.md
                // 匹配图片标记: ![描述](图片url)
                // ![image-20210205221642687](dotnet-cli-coo/image-20210205221642687.png)
                // 正则:   \!\[(?<desc>.*)\]\((?<url>.+)\)
                Regex mdImgRegex = new Regex(@"\!\[(?<desc>.*)\]\((?<url>.+)\)");
                // 利用 (?<xxx>子表达式) 定义分组别名，这样就可以利用 Groups["xxx"] 进行访问分组/子表达式内容。
                MatchCollection mdImgMatches = mdImgRegex.Matches(fileContent);
                for (int i = 1; i <= mdImgMatches.Count; i++)
                {
                    string imgUrl = mdImgMatches[i - 1].Groups["url"].Value;
                    // 检测是否是网络图片
                    if (imgUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                        || imgUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // TODO: // 开头也可以引用图片
                        referencedImgUrlList.Add(imgUrl);

                        continue;
                    }

                    string imgRelativePath = imgUrl;
                    // 此 文件 所在目录
                    string mdDir = System.IO.Path.GetDirectoryName(file);
                    // 根据当前文件路径: 引用图片相对路径 转 绝对路径
                    string imgAbsolutePath = Utils.FileUtil.RelativePathToAbsolutePath(imgRelativePath, mdDir);
                    //Console.WriteLine($"{i} - {imgRelativePath} - {imgAbsolutePath}");

                    //referencedImgAbsolutePathList.Add(imgAbsolutePath);
                    referencedImgAbsolutePathList.Add(imgAbsolutePath.ToLower());
                }
                #endregion

                #region 任务2: 匹配 HTML 图片标记
                // 匹配图片标记: ![描述](图片url)
                // <img src="">
                // 正则:   
                // Regex htmlImgRegex = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
                // 去掉分组中的 \s 防止图片的链接中含有空格导致匹配的url不全的问题
                Regex htmlImgRegex = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
                // 利用 (?<xxx>子表达式) 定义分组别名，这样就可以利用 Groups["xxx"] 进行访问分组/子表达式内容。
                MatchCollection htmlImgMatches = htmlImgRegex.Matches(fileContent);
                for (int i = 1; i <= htmlImgMatches.Count; i++)
                {
                    string imgUrl = htmlImgMatches[i - 1].Groups["imgUrl"].Value;
                    // 检测是否是网络图片
                    if (imgUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                        || imgUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                    {
                        referencedImgUrlList.Add(imgUrl);

                        continue;
                    }

                    string imgRelativePath = imgUrl;
                    //Console.WriteLine($"file: {file}");
                    // 此 文件 所在目录
                    string fileDir = System.IO.Path.GetDirectoryName(file);
                    //Console.WriteLine($"fileDir: {fileDir}");
                    // 根据当前文件路径: 引用图片相对路径 转 绝对路径
                    string imgAbsolutePath = Utils.FileUtil.RelativePathToAbsolutePath(imgRelativePath, fileDir);
                    //Console.WriteLine($"{i} - {imgRelativePath} - {imgAbsolutePath}");

                    //referencedImgAbsolutePathList.Add(imgAbsolutePath);
                    referencedImgAbsolutePathList.Add(imgAbsolutePath.ToLower());
                }
                #endregion

                fileCount++;
            }

            List<string> unReferencedImgAbsolutePathList = new List<string>();
            foreach (var imgPath in allImageList)
            {
                // TODO: 这里有个小bug, 在 Linux 下路径区分大小写, 而 Windows 不区分, 因此若大小写不同, 则若在 Windows 正常, 在 Linux 下就无效引用
                // 在 Windows 下, 大小写应当不区分, 但这里 Contains 是区分的
                // 为解决这个问题, 干脆大小写不敏感, 统一转小写, 宁可漏未引用的图片, 也不能删除了引用的图片
                //if (!referencedImgAbsolutePathList.Contains(imgPath))
                if (!referencedImgAbsolutePathList.Contains(imgPath.ToLower()))
                {
                    // 未引用图片
                    unReferencedImgAbsolutePathList.Add(imgPath);
                }
            }
            #endregion

            // 去重
            referencedImgAbsolutePathList = referencedImgAbsolutePathList.Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            unReferencedImgAbsolutePathList = unReferencedImgAbsolutePathList.Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            rtnModel = new CImgStatModel()
            {
                FileCount = fileCount,
                AllImageList = allImageList,
                ReferencedImgAbsolutePathList = referencedImgAbsolutePathList,
                UnReferencedImgAbsolutePathList = unReferencedImgAbsolutePathList
            };

            #region 统计输出
            Console.WriteLine("------------------------------------------------------------------------");
            Console.WriteLine($"(md,html,htm) 文件 共: {fileCount}");
            Console.WriteLine($"本地图片 共: {allImageList.Count}");
            Console.WriteLine($"(md,html,htm) 文件 引用本地图片 共: {referencedImgAbsolutePathList.Count}");
            Console.WriteLine($"(md,html,htm) 文件 未引用本地图片 共: {unReferencedImgAbsolutePathList.Count}");
            Console.WriteLine("------------------------------------------------------------------------");

            Console.WriteLine("未引用本地图片:");
            List<UnReferencedImgDeleteModel> deleteModels = new List<UnReferencedImgDeleteModel>();
            for (int i = 0; i < unReferencedImgAbsolutePathList.Count; i++)
            {
                var imgAbsolutePath = unReferencedImgAbsolutePathList[i];
                bool ignore = false;
                foreach (var ignorePath in ignorePaths)
                {
                    string ignoreAbsolutePath = ignorePath;
                    if (githubAction)
                    {
                        string githubWorkspace = Utils.GitHubActionsUtil.GitHubEnv(Utils.GitHubActionsUtil.GitHubEnvKeyEnum.GITHUB_WORKSPACE);
                        // 注意: 若是 GitHub Action , 则 ignorePath 存储的是相对路径
                        ignoreAbsolutePath = Utils.FileUtil.RelativePathToAbsolutePath(ignorePath, currentDirectory: githubWorkspace);
                    }
                    else
                    {
                        // 确保万一: 可能用 CLI 输入的 --ignore-paths 也是相对路径, 相对于 发起 CLI 所在目录, 例如仓库根目录
                        ignoreAbsolutePath = System.IO.Path.GetFullPath(ignorePath);
                    }
                    //Console.WriteLine($"ignoreAbsolutePath: {ignoreAbsolutePath}");
                    if (imgAbsolutePath.StartsWith(ignoreAbsolutePath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // 忽略
                        ignore = true;
                        break;
                    }
                }
                if (ignore)
                {
                    deleteModels.Add(new UnReferencedImgDeleteModel
                    {
                        DeleteStatus = UnReferencedImgDeleteModel.DeleteStatusEnum.DeleteIgnore,
                        ImgAbsolutePath = imgAbsolutePath
                    });
                }
                else
                {
                    deleteModels.Add(new UnReferencedImgDeleteModel
                    {
                        DeleteStatus = UnReferencedImgDeleteModel.DeleteStatusEnum.NeedDelete,
                        ImgAbsolutePath = imgAbsolutePath
                    });
                }

                if (!delete)
                {
                    string deleteStatus = ignore ? "忽略" : "需删除";
                    Console.WriteLine($"{i + 1}: {imgAbsolutePath} - {deleteStatus}");
                }
            }
            if (!delete)
            {
                Console.WriteLine("------------------------------------------------------------------------");
            }
            #endregion

            #region 删除 未引用本地图片
            if (delete)
            {
                for (int i = 0; i < deleteModels.Count; i++)
                {
                    var model = deleteModels[i];
                    try
                    {
                        if (model.DeleteStatus == UnReferencedImgDeleteModel.DeleteStatusEnum.DeleteIgnore)
                        {
                            // 忽略
                            Console.WriteLine($"{i + 1}: {model.ImgAbsolutePath} - 忽略");
                        }
                        else
                        {
                            // 删除
                            try
                            {
                                System.IO.File.Delete(model.ImgAbsolutePath);

                                Console.WriteLine($"{i + 1}: {model.ImgAbsolutePath} - 删除成功");

                                model.DeleteStatus = UnReferencedImgDeleteModel.DeleteStatusEnum.DeleteSuccess;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"{i + 1}: {model.ImgAbsolutePath} - 删除失败");

                                Utils.LogUtil.Exception(ex);

                                model.DeleteStatus = UnReferencedImgDeleteModel.DeleteStatusEnum.DeleteFailure;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.LogUtil.Exception(ex);
                    }
                }
                Console.WriteLine("------------------------------------------------------------------------");
            }
            #endregion

            #region GitHub Action 统计输出
            if (githubAction)
            {
                string githubWorkSpace = Utils.GitHubActionsUtil.GitHubEnv(Utils.GitHubActionsUtil.GitHubEnvKeyEnum.GITHUB_WORKSPACE);
                // imageReport 使用 Markdown
                string imageReport = $"---  \\n" +
                                     $"### Update report  \\n" +
                                     $"- Updated with {DateTime.Now.ToString()} \\n" +
                                     $"- Auto-generated by [clear-image-action](https://github.com/yiyungent/clear-image-action)  \\n" +
                                     $"---  \\n" +
                                     $"### `(md,html,htm)` 文件 共: {fileCount}  \\n" +
                                     $"### 本地图片 共: {allImageList.Count}  \\n" +
                                     $"### `(md,html,htm)` 文件 引用本地图片 共: {referencedImgAbsolutePathList.Count}  \\n" +
                                     $"### `(md,html,htm)` 文件 未引用本地图片 共: {unReferencedImgAbsolutePathList.Count}  \\n" +
                                     $"---  \\n";

                StringBuilder sbTemp = new StringBuilder();
                sbTemp.Append($"### 未引用本地图片:  \\n");
                if (delete)
                {
                    for (int i = 0; i < deleteModels.Count; i++)
                    {
                        var item = deleteModels[i];
                        string deleteResult = "";
                        switch (item.DeleteStatus)
                        {
                            case UnReferencedImgDeleteModel.DeleteStatusEnum.DeleteSuccess:
                                deleteResult = "删除成功";
                                break;
                            case UnReferencedImgDeleteModel.DeleteStatusEnum.DeleteFailure:
                                deleteResult = "删除失败";
                                break;
                            case UnReferencedImgDeleteModel.DeleteStatusEnum.DeleteIgnore:
                                deleteResult = "忽略";
                                break;
                            default:
                                break;
                        }
                        sbTemp.Append($"{i + 1}. `{item.ImgAbsolutePath.Replace($"{githubWorkSpace}/", "")}` - `{deleteResult}`  \\n");
                    }
                }
                else
                {
                    for (int i = 0; i < deleteModels.Count; i++)
                    {
                        var item = deleteModels[i];
                        // 注意: 非 忽略 即为 需删除
                        string deleteStatus = item.DeleteStatus != UnReferencedImgDeleteModel.DeleteStatusEnum.DeleteIgnore ? "需删除" : "忽略";
                        sbTemp.Append($"{i + 1}. `{item.ImgAbsolutePath.Replace($"{githubWorkSpace}/", "")}` - `{deleteStatus}`  \\n");
                    }
                }
                imageReport += $"{sbTemp.ToString()}  \\n";
                imageReport += $"---  \\n";

                imageReport += $"### `(md,html,htm)` 文件 引用网络图片 共:{referencedImgUrlList.Count}  \\n";
                sbTemp = new StringBuilder();
                sbTemp.Append($"### 引用网络图片:  \\n");
                for (int i = 0; i < referencedImgUrlList.Count; i++)
                {
                    var item = referencedImgUrlList[i];
                    sbTemp.Append($"{i + 1}. <{item}>  \\n");
                }

                imageReport += $"---  \\n";

                // fixed: md,html,htm: command not found
                // ` 符号在 bash 中造成了歧义
                imageReport = imageReport.Replace("`", "");

                Utils.GitHubActionsUtil.SetOutput("image_report", imageReport);
            }
            #endregion

            return rtnModel;
        }

    }
}
