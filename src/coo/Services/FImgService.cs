using coo.Models.FImg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace coo.Services
{
    /// <summary>
    /// 检查 引用的本地图片 是否存在     
    /// 检查 引用的网络图片 是否有效 (200 非 404)
    /// </summary>
    public class FImgService
    {
        public FImgStatModel Stat(string postDir, bool githubAction = false, string ignorePathsStr = null)
        {
            bool debug = Convert.ToBoolean(Utils.GitHubActionsUtil.GetEnv("cia_debug"));
            FImgStatModel rtnModel = null;
            List<string> ignorePaths = new List<string>();
            if (!string.IsNullOrEmpty(ignorePathsStr))
            {
                ignorePaths = ignorePathsStr.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            #region 扫描文件
            // key: referencedImgAbsolutePath or referencedImgUrl ; value: filePath
            // 一个图片路径 可能被多处引用
            Dictionary<string, List<string>> referencedImgAndFileDic = new Dictionary<string, List<string>>();

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

            foreach (var file in allList)
            {
                string fileContent = Utils.FileUtil.ReadStringAsync(file).Result;

                // 注意: 对所有文件都要执行两种匹配, 因为在 md 文件中也有可能存在 HTML 形式的 <img> 引用
                // 宁可漏删, 不可误删

                #region 任务1: 匹配 Markdown 图片标记
                // md文件: xxx/_posts/dotnet-cli-coo.md
                // 匹配图片标记: ![描述](图片url)
                // ![image-20210205221642687](dotnet-cli-coo/image-20210205221642687.png)
                // 正则:   \!\[(?<desc>.*?)\]\((?<url>.+?)\)

                // Bug:  [![爱发电](https://afdian.moeci.com/1/badge.svg)](https://afdian.net/@yiyun)
                // 这种情况下, 匹配出错, 匹配到了 https://afdian.net/@yiyun
                // fixed: 使用 ? 启用懒惰模式, 而不是贪婪导致匹配到了后面的(https://afdian.net/@yiyun)

                Regex mdImgRegex = new Regex(@"\!\[(?<desc>.*?)\]\((?<url>.+?)\)");
                // 利用 (?<xxx>子表达式) 定义分组别名，这样就可以利用 Groups["xxx"] 进行访问分组/子表达式内容。
                MatchCollection mdImgMatches = mdImgRegex.Matches(fileContent);
                for (int i = 1; i <= mdImgMatches.Count; i++)
                {
                    string imgUrl = mdImgMatches[i - 1]?.Groups["url"]?.Value;
                    if (string.IsNullOrEmpty(imgUrl))
                    {
                        continue;
                    }
                    // 检测是否是网络图片
                    if (imgUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                        || imgUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // TODO: // 开头也可以引用图片
                        referencedImgUrlList.Add(imgUrl);

                        if (!referencedImgAndFileDic.ContainsKey(imgUrl))
                        {
                            List<string> tempList = new List<string>();
                            tempList.Add(file);
                            referencedImgAndFileDic.Add(imgUrl, tempList);
                        }
                        else
                        {
                            // 注意: 可能存在 在此文件中, 多次引用同一路径图片
                            if (!referencedImgAndFileDic[imgUrl].Contains(file))
                            {
                                referencedImgAndFileDic[imgUrl].Add(file);
                            }
                        }

                        continue;
                    }

                    string imgRelativePath = imgUrl;
                    // 此 文件 所在目录
                    string mdDir = System.IO.Path.GetDirectoryName(file);
                    // 根据当前文件路径: 引用图片相对路径 转 绝对路径
                    try
                    {
                        string imgAbsolutePath = Utils.FileUtil.RelativePathToAbsolutePath(imgRelativePath, mdDir);
                        referencedImgAbsolutePathList.Add(imgAbsolutePath);

                        if (!referencedImgAndFileDic.ContainsKey(imgAbsolutePath))
                        {
                            List<string> tempList = new List<string>();
                            tempList.Add(file);
                            referencedImgAndFileDic.Add(imgAbsolutePath, tempList);
                        }
                        else
                        {
                            // 注意: 可能存在 在此文件中, 多次引用同一路径图片
                            if (!referencedImgAndFileDic[imgAbsolutePath].Contains(file))
                            {
                                referencedImgAndFileDic[imgAbsolutePath].Add(file);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 可能路径不存在, 或不合法, 导致无法转换为 AbsolutePath
                    }
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
                    string imgUrl = htmlImgMatches[i - 1]?.Groups["imgUrl"]?.Value;
                    if (string.IsNullOrEmpty(imgUrl))
                    {
                        continue;
                    }
                    // 检测是否是网络图片
                    if (imgUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                        || imgUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                    {
                        referencedImgUrlList.Add(imgUrl);

                        if (!referencedImgAndFileDic.ContainsKey(imgUrl))
                        {
                            List<string> tempList = new List<string>();
                            tempList.Add(file);
                            referencedImgAndFileDic.Add(imgUrl, tempList);
                        }
                        else
                        {
                            // 注意: 可能存在 在此文件中, 多次引用同一路径图片
                            if (!referencedImgAndFileDic[imgUrl].Contains(file))
                            {
                                referencedImgAndFileDic[imgUrl].Add(file);
                            }
                        }

                        continue;
                    }

                    string imgRelativePath = imgUrl;
                    //Console.WriteLine($"file: {file}");
                    // 此 文件 所在目录
                    string fileDir = System.IO.Path.GetDirectoryName(file);
                    //Console.WriteLine($"fileDir: {fileDir}");
                    // 根据当前文件路径: 引用图片相对路径 转 绝对路径
                    try
                    {
                        string imgAbsolutePath = Utils.FileUtil.RelativePathToAbsolutePath(imgRelativePath, fileDir);
                        referencedImgAbsolutePathList.Add(imgAbsolutePath);

                        if (!referencedImgAndFileDic.ContainsKey(imgAbsolutePath))
                        {
                            List<string> tempList = new List<string>();
                            tempList.Add(file);
                            referencedImgAndFileDic.Add(imgAbsolutePath, tempList);
                        }
                        else
                        {
                            // 注意: 可能存在 在此文件中, 多次引用同一路径图片
                            if (!referencedImgAndFileDic[imgAbsolutePath].Contains(file))
                            {
                                referencedImgAndFileDic[imgAbsolutePath].Add(file);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 可能路径不存在, 或不合法, 导致无法转换为 AbsolutePath
                    }
                }
                #endregion

            }

            #endregion

            // 去重
            referencedImgAbsolutePathList = referencedImgAbsolutePathList.Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            referencedImgUrlList = referencedImgUrlList.Distinct().ToList();

            #region GitHub Action
            string reportGitHubAction = $"---  \\n" +
                                         $"### Update report  \\n" +
                                         $"- Updated with {DateTime.Now.ToString()} \\n" +
                                         $"- Auto-generated by [clear-image-action](https://github.com/yiyungent/clear-image-action)  \\n" +
                                         $"---  \\n" +
                                         $"### `(md,html,htm)` 文件 共: {allList.Count}  \\n" +
                                         $"### `(md,html,htm)` 文件 引用本地图片 共: {referencedImgAbsolutePathList.Count}  \\n" +
                                         $"### `(md,html,htm)` 文件 引用网络图片 共: {referencedImgUrlList.Count}  \\n" +
                                         $"---  \\n";
            StringBuilder sbTempReportGitHubAction = new StringBuilder();
            string githubWorkSpace = "";
            if (githubAction)
            {
                githubWorkSpace = Utils.GitHubActionsUtil.GitHubEnv(Utils.GitHubActionsUtil.GitHubEnvKeyEnum.GITHUB_WORKSPACE);
            }
            #endregion

            #region 检查 引用的本地图片 是否存在
            Console.WriteLine("引用本地图片:");
            sbTempReportGitHubAction.Append("### 引用本地图片:  \\n");
            int notExistLocalImgCount = 0;
            for (int i = 0; i < referencedImgAbsolutePathList.Count; i++)
            {
                string imgAbsolutePath = referencedImgAbsolutePathList[i];
                bool existImgFile = false;
                try
                {
                    existImgFile = File.Exists(imgAbsolutePath);
                }
                catch (Exception ex)
                {
                    existImgFile = false;
                }
                if (existImgFile)
                {
                    #region 由于输出太多, 因此暂时仅输出不存在的
                    //Utils.FileUtil.GetLocalImageInfo(imgAbsolutePath, out long byteSize, out long width, out long height);
                    //string imgSize = Utils.CommonUtil.PrettyFileSize(byteSize);

                    //Console.WriteLine($"{i + 1}. {imgAbsolutePath} - 存在 - {imgSize}");

                    //if (githubAction)
                    //{
                    //    sbTempReportGitHubAction.Append($"{i + 1}. {imgAbsolutePath.Replace($"{githubWorkSpace}/", "")} - 存在 - {imgSize}  \\n");
                    //} 
                    #endregion
                }
                else
                {
                    // TODO: 由于 可能存在多个文件 引用同一路径图片, 并且做了路径去重处理, 因此无法找到 是哪些文件引用了此图片

                    Console.WriteLine($"{notExistLocalImgCount + 1}. {imgAbsolutePath} - 不存在");
                    Console.WriteLine("引用自:");

                    if (githubAction)
                    {
                        sbTempReportGitHubAction.Append($"{notExistLocalImgCount + 1}. {imgAbsolutePath.Replace($"{githubWorkSpace}/", "")} - 不存在  \\n");
                        sbTempReportGitHubAction.Append("引用自:  \\n");
                    }

                    List<string> fromFileList = referencedImgAndFileDic.First(m => m.Key == imgAbsolutePath).Value;

                    for (int j = 0; j < fromFileList.Count; j++)
                    {
                        Console.WriteLine($"{notExistLocalImgCount + 1}-{j + 1}. {fromFileList[j]}");
                    }

                    if (githubAction)
                    {
                        for (int j = 0; j < fromFileList.Count; j++)
                        {
                            sbTempReportGitHubAction.Append($"{notExistLocalImgCount + 1}-{j + 1}. {fromFileList[j].Replace($"{githubWorkSpace}/", "")}  \\n");
                        }
                    }

                    notExistLocalImgCount++;
                }
            }
            if (notExistLocalImgCount == 0)
            {
                Console.WriteLine("祝贺: 没有 引用的本地图片 不存在的 情况");
                if (githubAction)
                {
                    sbTempReportGitHubAction.Append("祝贺: 没有 引用的本地图片 不存在的 情况");
                }
            }
            #endregion

            #region 检查 引用的网络图片 是否有效
            Console.WriteLine("引用网络图片:");
            sbTempReportGitHubAction.Append("### 引用网络图片:  \\n");
            int successRemoteImgCount = 0;
            for (int i = 0; i < referencedImgUrlList.Count; i++)
            {
                string imgUrl = referencedImgUrlList[i];
                bool successStatusCode = false;
                try
                {
                    // 测试网络图片是否有效
                    successStatusCode = Utils.HttpUtil.TestSuccess(imgUrl);
                }
                catch (Exception ex)
                {
                    successStatusCode = false;
                }
                if (successStatusCode)
                {
                    #region 由于输出太多, 因此暂时仅输出不存在的

                    #endregion
                }
                else
                {
                    // TODO: 由于 可能存在多个文件 引用同一路径图片, 并且做了路径去重处理, 因此无法找到 是哪些文件引用了此图片

                    Console.WriteLine($"{successRemoteImgCount + 1}. {imgUrl} - 无效");
                    Console.WriteLine("引用自:");

                    if (githubAction)
                    {
                        sbTempReportGitHubAction.Append($"{successRemoteImgCount + 1}. {imgUrl} - 无效  \\n");
                        sbTempReportGitHubAction.Append("引用自:  \\n");
                    }

                    List<string> fromFileList = referencedImgAndFileDic.First(m => m.Key == imgUrl).Value;

                    for (int j = 0; j < fromFileList.Count; j++)
                    {
                        Console.WriteLine($"{successRemoteImgCount + 1}-{j + 1}. {fromFileList[j]}");
                    }

                    if (githubAction)
                    {
                        for (int j = 0; j < fromFileList.Count; j++)
                        {
                            sbTempReportGitHubAction.Append($"{successRemoteImgCount + 1}-{j + 1}. {fromFileList[j].Replace($"{githubWorkSpace}/", "")}  \\n");
                        }
                    }

                    successRemoteImgCount++;
                }
            }
            if (successRemoteImgCount == 0)
            {
                Console.WriteLine("祝贺: 没有 引用的网络图片 无效的 情况");
                if (githubAction)
                {
                    sbTempReportGitHubAction.Append("祝贺: 没有 引用的网络图片 无效的 情况");
                }
            }
            #endregion

            #region Report GitHub Action
            if (githubAction)
            {
                reportGitHubAction += sbTempReportGitHubAction.ToString();
                // fixed: md,html,htm: command not found
                // ` 符号在 bash 中造成了歧义
                reportGitHubAction = reportGitHubAction.Replace("`", "");

                Utils.GitHubActionsUtil.SetOutput("image_report", reportGitHubAction);
            }
            #endregion

            return rtnModel;
        }
    }
}
