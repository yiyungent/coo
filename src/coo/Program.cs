using coo.Services;
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
