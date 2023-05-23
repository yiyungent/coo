using HtmlAgilityPack;
using Scriban;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EnexLib
{
    public class EnexLib
    {
        #region Properties
        public XmlEntity.EnExport EnexExport { get; private set; }

        public string MdTemplateFilePath { get; set; }

        public List<string> CatNameList { get; set; }
        #endregion

        #region Ctor
        public EnexLib()
        {
            this.MdTemplateFilePath = Path.Combine(Directory.GetCurrentDirectory(), "templates", "note.md");
        }
        #endregion

        #region Methods
        public XmlEntity.EnExport Load(string enexFilePath)
        {
            using (var stream = new StreamReader(enexFilePath))
            {
                var xmlSerializer = new XmlSerializer(typeof(XmlEntity.EnExport));
                EnexExport = xmlSerializer.Deserialize(stream) as XmlEntity.EnExport;
                return EnexExport;
            }
        }

        public void DumpSingle(XmlEntity.Note note, string outputDir, MarkdownConfig config = null)
        {
            #region 准备
            if (config == null)
            {
                config = new MarkdownConfig();
            }
            // 注意: 不能使用 Guid 会导致每次文件名不同
            // string mdFileName = config.GuidFileName ? $"evernote-{note.Created}-{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5)}" : Utility.SafeFileName(note.Title);
            // 有时会生成两个 -, 即 --, 这是因为 GetHashCode 可能为负数
            // evernote-20180524T153937Z--144251972
            // evernote-20180529T113033Z-1196295610
            // 发现 note.Title.GetHashCode() 对于相同字符串也会生成不同 HashCode, 于是也弃用, 很奇怪, 但直接使用 "Hello".GetHashCode() 却一致
            // string mdFileName = config.GuidFileName ? $"evernote-{note.Created}-{note.Title.GetHashCode().ToString()}" : Utility.SafeFileName(note.Title);
            // 保险起见, 使用 md5
            string createdStr = DateTime.ParseExact(note.Created, "yyyyMMddTHHmmssZ", null).ToString("yyyy-MM-dd-HH-mm-ss");
            string mdFileName = config.UseUniqueIdFileName ? $"evernote-{createdStr}-{Utils.Md5Util.MD5Encrypt16(note.Title ?? "null title").Substring(0, 5)}-note".ToLower() : Utility.SafeFileName(note.Title);
            mdFileName = Utility.SafeFileName($"{mdFileName}.md");
            config.AttachmentPath = Path.GetFileNameWithoutExtension(mdFileName);
            Directory.CreateDirectory(Path.Combine(outputDir, config.AttachmentPath));
            #endregion

            #region 解析内容
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(note.Content);
            var html = doc.DocumentNode.SelectSingleNode("/en-note").InnerHtml;
            var converter = new ReverseMarkdown.Converter(config);
            converter.Register("en-media", new MarkdownConverters.EnMedia(note));
            converter.Register("en-todo", new MarkdownConverters.EnTodo());
            string mdFileContent = System.Web.HttpUtility.HtmlDecode(converter.Convert(html));
            #endregion

            #region 资源
            if (note.Resource != null)
            {
                foreach (var item in note.Resource)
                {
                    if (config.UseInlineImage && item.Mime.ToLower().StartsWith("image/"))
                    {
                        // 内联 base64 图片
                        string attachmentFileName = $"image-{item.Data.Hash}{Path.GetExtension(item.FileName)}";
                        //mdFileContentSb.AppendLine($"[{item.Data.Hash}]: data:{item.Mime};base64,{item.Data.Base64}");
                        mdFileContent = mdFileContent.Replace($"[{item.FileName}][{item.Data.Hash}]", $"[{Path.GetFileNameWithoutExtension(attachmentFileName)}](data:{item.Mime};base64,{item.Data.Base64})");
                    }
                    else if (!config.UseInlineImage && item.Mime.ToLower().StartsWith("image/"))
                    {
                        // 外联 图片文件
                        string attachmentFileName = $"image-{item.Data.Hash}{Path.GetExtension(item.FileName)}";
                        if (File.Exists(Path.Combine(outputDir, config.AttachmentPath, attachmentFileName)))
                        {
                            var ext = Path.GetExtension(attachmentFileName);
                            var name = Path.GetFileNameWithoutExtension(attachmentFileName);
                            var n = 1;
                            while (File.Exists(Path.Combine(outputDir, config.AttachmentPath, $"{name}_{n}{ext}")))
                            {
                                ++n;
                            }
                            attachmentFileName = $"{name}_{n}{ext}";
                        }
                        //sb.AppendLine($"![]({Path.GetFileNameWithoutExtension(mdFileName)}/{attachmentFileName})");
                        mdFileContent = mdFileContent.Replace($"[{item.FileName}][{item.Data.Hash}]", $"[{Path.GetFileNameWithoutExtension(attachmentFileName)}]({Path.GetFileNameWithoutExtension(mdFileName)}/{attachmentFileName})");
                        File.WriteAllBytes(Path.Combine(outputDir, config.AttachmentPath, attachmentFileName), item.Data.Content);
                    }
                    else
                    {
                        // 其它 二进制文件
                        string attachmentFileName = $"attachment-{item.Data.Hash}{Path.GetExtension(item.FileName)}";
                        if (File.Exists(Path.Combine(outputDir, config.AttachmentPath, attachmentFileName)))
                        {
                            var ext = Path.GetExtension(attachmentFileName);
                            var name = Path.GetFileNameWithoutExtension(attachmentFileName);
                            var n = 1;
                            while (File.Exists(Path.Combine(outputDir, config.AttachmentPath, $"{name}_{n}{ext}")))
                            {
                                ++n;
                            }
                            attachmentFileName = $"{name}_{n}{ext}";
                        }
                        //sb.AppendLine($"[{attachmentFileName}]({Path.GetFileNameWithoutExtension(mdFileName)}/{attachmentFileName})");
                        mdFileContent = mdFileContent.Replace($"[{item.FileName}][{item.Data.Hash}]", $"[{Path.GetFileNameWithoutExtension(attachmentFileName)}]({Path.GetFileNameWithoutExtension(mdFileName)}/{attachmentFileName})");
                        File.WriteAllBytes(Path.Combine(outputDir, config.AttachmentPath, attachmentFileName), item.Data.Content);
                    }
                }
            }
            #endregion

            mdFileContent = Regex.Replace(mdFileContent, "(?:" + Environment.NewLine + "){3,}", $"{Environment.NewLine}{Environment.NewLine}");
            mdFileContent = mdFileContent.Trim();

            #region 模版解析
            string mdTemplateFilePath = this.MdTemplateFilePath;
            if (!File.Exists(mdTemplateFilePath))
            {
                throw new FileNotFoundException(message: null, fileName: mdTemplateFilePath);
            }
            // string mdTemplateContent = File.ReadAllText(mdTemplateFilePath, Encoding.UTF8);
            string mdTemplateContent = File.ReadAllText(mdTemplateFilePath);
            var mdTemplate = Template.Parse(mdTemplateContent);
            string mdTemplateRenderResult = mdTemplate.Render(new
            {
                Note = note,
                MdFileContent = mdFileContent,
                CatNameList = CatNameList,
                AttachmentPath = config.AttachmentPath,
                MdFileName = mdFileName,
                MdFileNameWithoutExtension = Path.GetFileNameWithoutExtension(mdFileName),
                MarkdownConfig = config
            });
            #endregion

            #region 写入内容
            // File.WriteAllText(Path.Combine(outputDir, mdFileName), mdTemplateRenderResult, Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputDir, mdFileName), mdTemplateRenderResult);
            #endregion
        }

        public void DumpAll(string outputDir, MarkdownConfig config = null)
        {
            StringBuilder errorSb = new StringBuilder();
            for (int i = 0; i < EnexExport.Notes.Length; i++)
            {
                var item = EnexExport.Notes[i];
                try
                {
                    DumpSingle(item, outputDir, config);
                    //Console.Clear();
                    //Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{(i + 1)}/{EnexExport.Notes.Length} Dump: {item.Title}");
                }
                catch (Exception ex)
                {
                    errorSb.AppendLine($"Error: {item.Title}");
                    errorSb.AppendLine(ex.ToString());
                }
            }
            //Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorSb.ToString());
        }
        #endregion
    }
}
