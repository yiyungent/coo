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
        public async Task<bool> Dump(string inputDir, string outputDir)
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
                        string relativeFilePath = enexFilePath.Replace(new DirectoryInfo(inputDir).FullName, "");
                        string relativeDirPath = new DirectoryInfo(relativeFilePath).Parent.FullName;
                        string[] splitFolderNames = relativeDirPath.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
                        string mdDir = string.Empty;
                        foreach (string folderName in splitFolderNames)
                        {
                            mdDir = Path.Combine(mdDir, $"分类-{folderName}");
                        }
                        string fileOutputDir = Path.Combine(outputDir, mdDir, $"分类-{Path.GetFileNameWithoutExtension(enexFilePath)}");
                        EnexLib.EnexLib enexLib = new EnexLib.EnexLib();
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
