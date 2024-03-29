﻿using coo.Services;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using System.Text.RegularExpressions;

namespace coo
{
    class Program
    {
        static void Main(string[] args)
        {
            string githubWorkspace = Utils.GitHubActionsUtil.GitHubEnv(Utils.GitHubActionsUtil.GitHubEnvKeyEnum.GITHUB_WORKSPACE);
            if (!string.IsNullOrEmpty(githubWorkspace))
            {
                // GITHUB_ACTION_PATH: /home/runner/work/clear-image-action/clear-image-action/./
                // GITHUB_WORKSPACE: /home/runner/work/clear-image-action/clear-image-action
                // CurrentDirectory: /home/runner/work/clear-image-action/clear-image-action
                Console.WriteLine("------------------------------------------------------------------------");
                bool debug = Convert.ToBoolean(Utils.GitHubActionsUtil.GetEnv("cia_debug"));
                Console.WriteLine($"cia_debug: {debug}");
                Console.WriteLine($"GITHUB_ACTION_PATH: {Utils.GitHubActionsUtil.GitHubEnv(Utils.GitHubActionsUtil.GitHubEnvKeyEnum.GITHUB_ACTION_PATH)}");
                Console.WriteLine($"GITHUB_REPOSITORY: {Utils.GitHubActionsUtil.GitHubEnv(Utils.GitHubActionsUtil.GitHubEnvKeyEnum.GITHUB_REPOSITORY)}");
                Console.WriteLine($"GITHUB_REPOSITORY_OWNER: {Utils.GitHubActionsUtil.GitHubEnv(Utils.GitHubActionsUtil.GitHubEnvKeyEnum.GITHUB_REPOSITORY_OWNER)}");
                Console.WriteLine($"GITHUB_WORKFLOW: {Utils.GitHubActionsUtil.GitHubEnv(Utils.GitHubActionsUtil.GitHubEnvKeyEnum.GITHUB_WORKFLOW)}");
                Console.WriteLine($"GITHUB_ACTION_REPOSITORY: {Utils.GitHubActionsUtil.GitHubEnv(Utils.GitHubActionsUtil.GitHubEnvKeyEnum.GITHUB_ACTION_REPOSITORY)}");
                Console.WriteLine($"GITHUB_WORKSPACE: {githubWorkspace}");
                Console.WriteLine($"CurrentDirectory: {System.IO.Directory.GetCurrentDirectory()}");
                Console.WriteLine("------------------------------------------------------------------------");
            }

            #region CLI

            #region Hello
            if (args.Length == 0)
            {
                var versionString = Assembly.GetEntryAssembly()
                                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                        .InformationalVersion
                                        .ToString();

                Console.WriteLine($"coo v{versionString}");
                Console.WriteLine("-------------");
                //Console.WriteLine("\nUsage:");
                //Console.WriteLine("  coo <message>");
                return;
            }
            #endregion

            #region RootCommand
            var rootCommand = new RootCommand("coo tool cli")
            {
                //new Argument<string>("url","web site url"),
                //new Option<bool>(new string[]{ "--getimage" ,"-image"},"Get images"),
                //new Option<bool>(new string[]{ "--regex-option" ,"-regex"},"Use regex"),
                //new Option<bool>(new string[]{ "--htmlagilitypack-option", "-agpack"},"Use HtmlAgilityPack"),
                //new Option<bool>(new string[]{ "--anglesharp-option", "-agsharp"},"Use AngleSharp"),
                //new Option<string>(new string[]{ "--download-path" ,"-path"},"Designate download path"),
            };
            #endregion

            #region mdimg
            var mdimgCommand = new Command("mdimg", "md and image");
            mdimgCommand.AddArgument(new Argument<string>("dir", "Set post (md and images) dir"));
            mdimgCommand.AddOption(new Option<bool>(new string[] { "--delete", "-d" }, "Delete unreferenced images"));
            mdimgCommand.Handler = CommandHandler.Create((string dir, bool delete) =>
            {
                MdImgService mdImgService = new MdImgService();
                //string postDir = @"F:\Com\me\Repos\notebook\source\_posts";
                List<string> unReferencedImgList = mdImgService.MdImgStat(dir);
                if (delete)
                {
                    int deleteSuccessCount = 0;
                    foreach (var item in unReferencedImgList)
                    {
                        try
                        {
                            System.IO.File.Delete(item);
                            deleteSuccessCount++;
                            Console.WriteLine($"{deleteSuccessCount}: 删除 未引用图片: {item}");
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            });
            rootCommand.AddCommand(mdimgCommand);
            #endregion

            #region cimg
            var cimgCommand = new Command("cimg", "Clean up unreferenced images");
            cimgCommand.AddArgument(new Argument<string>("dir", "Set post (md, html, htm, images) dir"));
            cimgCommand.AddOption(new Option<bool>(new string[] { "--delete", "-d" }, "Delete unreferenced images"));
            cimgCommand.AddOption(new Option<bool>(new string[] { "--github-action", "-ga" }, "outputs for GitHub Action"));
            cimgCommand.AddOption(new Option<string>(new string[] { "--ignore-paths", "-ip" }, "ignore paths"));
            cimgCommand.Handler = CommandHandler.Create((string dir, bool delete, bool githubAction, string ignorePaths) =>
            {
                CImgService cImgService = new CImgService();
                //string postDir = @"F:\Com\me\Repos\notebook\source\_posts";
                var resModel = cImgService.CImgStat(postDir: dir, githubAction: githubAction, ignorePathsStr: ignorePaths, delete: delete);
            });
            rootCommand.AddCommand(cimgCommand);
            #endregion

            #region fimg
            var fimgCommand = new Command("fimg", "check referenced images");
            fimgCommand.AddArgument(new Argument<string>("dir", "Set post (md, html, htm, images) dir"));
            fimgCommand.AddOption(new Option<bool>(new string[] { "--github-action", "-ga" }, "outputs for GitHub Action"));
            fimgCommand.AddOption(new Option<string>(new string[] { "--ignore-paths", "-ip" }, "ignore paths"));
            fimgCommand.Handler = CommandHandler.Create((string dir, bool githubAction, string ignorePaths) =>
            {
                FImgService fImgService = new FImgService();
                var resModel = fImgService.Stat(postDir: dir, githubAction: githubAction, ignorePathsStr: ignorePaths);
            });
            rootCommand.AddCommand(fimgCommand);
            #endregion

            #region ustar
            var ustarCommand = new Command("ustar", "you star this repo?");
            ustarCommand.AddArgument(new Argument<string>("userName", "your GitHub userName"));
            ustarCommand.AddArgument(new Argument<string>("repoFullName", "GitHub repo fullname: yiyungent/coo"));
            ustarCommand.AddArgument(new Argument<string>("githubToken", "GitHub token"));
            ustarCommand.AddOption(new Option<bool>(new string[] { "--github-action", "-ga" }, "outputs for GitHub Action"));
            ustarCommand.Handler = CommandHandler.Create((string userName, string repoFullName, string githubToken, bool githubAction) =>
            {
                UStarService uStarService = new UStarService();
                var resModel = uStarService.Star(userName, repoFullName, githubToken, githubAction);
            });
            rootCommand.AddCommand(ustarCommand);
            #endregion

            #region enex2md
            var enex2MdCommand = new Command("enex2md", "enex to md");
            enex2MdCommand.AddArgument(new Argument<string>("inputDir", "input enex dir"));
            enex2MdCommand.AddArgument(new Argument<string>("outputDir", "output md dir"));
            enex2MdCommand.AddOption(new Option<string>(new string[] { "--template", "-t" }, "note markdown template file path"));
            enex2MdCommand.AddOption(new Option<bool>(new string[] { "--use-id", "-u" }, "whether to use unique identifier as md filename"));
            enex2MdCommand.Handler = CommandHandler.Create((string inputDir, string outputDir, string template, bool useId) =>
            {
                Enex2MdService enex2MdService = new Enex2MdService();
                inputDir = inputDir.Replace('/', System.IO.Path.DirectorySeparatorChar).Replace('\\', System.IO.Path.DirectorySeparatorChar);
                outputDir = outputDir.Replace('/', System.IO.Path.DirectorySeparatorChar).Replace('\\', System.IO.Path.DirectorySeparatorChar);
                template = template.Replace('/', System.IO.Path.DirectorySeparatorChar).Replace('\\', System.IO.Path.DirectorySeparatorChar);
                var resModel = enex2MdService.Dump(inputDir, outputDir, template, useId);
            });
            rootCommand.AddCommand(enex2MdCommand);
            #endregion

            #region rimg
            var rimgCommand = new Command("rimg", "rename image file");
            rimgCommand.AddArgument(new Argument<string>("dir", "Set post (md, images) dir"));
            rimgCommand.Handler = CommandHandler.Create((string dir) =>
            {
                RImgService rImgService = new RImgService();
                rImgService.RenameImg(dir);
            });
            rootCommand.AddCommand(rimgCommand);
            #endregion

            rootCommand.InvokeAsync(args);

            #endregion


            //ShowBot(string.Join(' ', args));
        }

        static void ShowBot(string message)
        {
            string bot = $"\n        {message}";
            bot += @"
            __________________
                            \
                            \
                                ....
                                ....'
                                ....
                                ..........
                            .............'..'..
                        ................'..'.....
                    .......'..........'..'..'....
                    ........'..........'..'..'.....
                    .'....'..'..........'..'.......'.
                    .'..................'...   ......
                    .  ......'.........         .....
                    .    _            __        ......
                    ..    #            ##        ......
                ....       .                 .......
                ......  .......          ............
                    ................  ......................
                    ........................'................
                ......................'..'......    .......
                .........................'..'.....       .......
            ........    ..'.............'..'....      ..........
        ..'..'...      ...............'.......      ..........
        ...'......     ...... ..........  ......         .......
        ...........   .......              ........        ......
        .......        '...'.'.              '.'.'.'         ....
        .......       .....'..               ..'.....
        ..       ..........               ..'........
                ............               ..............
                .............               '..............
                ...........'..              .'.'............
            ...............              .'.'.............
            .............'..               ..'..'...........
            ...............                 .'..............
            .........                        ..............
                .....
        ";
            Console.WriteLine(bot);
        }

    }
}
