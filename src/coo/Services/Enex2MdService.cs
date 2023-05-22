using coo.Models.UStar;
using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace coo.Services
{
    public class Enex2MdService
    {
        public async Task<bool> Dump(string inputDir, string outputDir, string template)
        {
            bool rtn = false;

            try
            {
                string[] enexFilePathArray = Directory.GetFiles(inputDir, "*.enex", SearchOption.AllDirectories);
                for (int i = 0; i < enexFilePathArray.Length; i++)
                {
                    try
                    {
                        string enexFilePath = enexFilePathArray[i];
                        string relativeFilePath = enexFilePath.Replace(new DirectoryInfo(inputDir).FullName, "").Trim(Path.DirectorySeparatorChar);

                        Console.WriteLine(relativeFilePath);
                        string mdDir = string.Empty;
                        List<string> catNameList = new List<string>();
                        if (relativeFilePath.Contains(Path.DirectorySeparatorChar))
                        {
                            string relativeDirPath = relativeFilePath.Substring(0, relativeFilePath.LastIndexOf(Path.DirectorySeparatorChar));
                            string[] splitFolderNames = relativeDirPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string folderName in splitFolderNames)
                            {
                                mdDir = Path.Combine(mdDir, $"分类-{folderName}");
                                catNameList.Add(folderName);
                            }
                        }
                        catNameList.Add(Path.GetFileNameWithoutExtension(enexFilePath));
                        string fileOutputDir = Path.Combine(outputDir, mdDir, $"分类-{Path.GetFileNameWithoutExtension(enexFilePath)}");
                        EnexLib.EnexLib enexLib = new EnexLib.EnexLib();
                        if (!string.IsNullOrEmpty(template))
                        {
                            enexLib.MdTemplateFilePath = template;
                        }
                        enexLib.CatNameList = catNameList;
                        enexLib.Load(enexFilePath);
                        enexLib.DumpAll(outputDir: fileOutputDir, new EnexLib.MarkdownConfig
                        {
                            InlineImage = false,
                            GuidFileName = false
                        });
                    }
                    catch (Exception ex)
                    {
                        rtn = false;
                        Utils.LogUtil.Exception(ex);
                    }
                }

                rtn = true;
            }
            catch (Exception ex)
            {
                rtn = false;
                Utils.LogUtil.Exception(ex);
            }

            return await Task.FromResult(rtn);
        }
    }
}
