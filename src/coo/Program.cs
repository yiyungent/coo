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
            #region CLI
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

            var rootCommand = new RootCommand("coo tool cli")
            {
                //new Argument<string>("url","web site url"),
                //new Option<bool>(new string[]{ "--getimage" ,"-image"},"Get images"),
                //new Option<bool>(new string[]{ "--regex-option" ,"-regex"},"Use regex"),
                //new Option<bool>(new string[]{ "--htmlagilitypack-option", "-agpack"},"Use HtmlAgilityPack"),
                //new Option<bool>(new string[]{ "--anglesharp-option", "-agsharp"},"Use AngleSharp"),
                //new Option<string>(new string[]{ "--download-path" ,"-path"},"Designate download path"),
            };

            var mdimgCommand = new Command("mdimg", "md and image");
            mdimgCommand.AddArgument(new Argument<string>("dir", "Set post (md and images) dir"));
            mdimgCommand.AddOption(new Option<bool>(new string[] { "--delete", "-d" }, "Delete unreferenced images"));
            mdimgCommand.Handler = CommandHandler.Create((string dir, bool delete) =>
            {
                MdService mdService = new MdService();
                //string postDir = @"F:\Com\me\Repos\notebook\source\_posts";
                List<string> unReferencedImgList = mdService.MdImgStat(dir);
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
