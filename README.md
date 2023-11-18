<p align="center">
<!-- <img src="docs/_images/coo.png" alt="coo"> -->
</p>
<h1 align="center">coo</h1>

> 🧰 .NET 自用 CLI | 工具集

[![repo size](https://img.shields.io/github/repo-size/yiyungent/coo.svg?style=flat)]()
[![LICENSE](https://img.shields.io/github/license/yiyungent/coo.svg?style=flat)](https://github.com/yiyungent/coo/blob/master/LICENSE)
[![nuget](https://img.shields.io/nuget/v/coo.svg?style=flat)](https://www.nuget.org/packages/coo/)
[![downloads](https://img.shields.io/nuget/dt/coo.svg?style=flat)](https://www.nuget.org/packages/coo/)
[![QQ Group](https://img.shields.io/badge/QQ%20Group-894031109-deepgreen)](https://jq.qq.com/?_wv=1027&k=q5R82fYN)



## 介绍

🧰 .NET 自用 CLI | 工具集
 

## 使用

> 需要先在本地安装 `.NET 6 SDK`

- [Windows | .NET 6 SDK | 下载](https://dotnet.microsoft.com/download/dotnet/thank-you/sdk-6.0.100-windows-x64-installer)
- [macOS | .NET 6 SDK | 下载](https://dotnet.microsoft.com/download/dotnet/thank-you/sdk-6.0.100-macos-x64-installer)
- [Linux | .NET 6 SDK](https://docs.microsoft.com/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website)

然后 安装 `coo` 工具集

```bash
dotnet tool install -g coo
```

### 功能

#### 1. mdimg

> 1. 清理 md文件的未引用图片: 例如使用 `Hexo`, 采用本地图片文件 (图片放在md同名文件夹下) 等    

`"F:\Com\me\Repos\notebook\source\_posts"` 为 `Hexo文章目录`, 本人图片与文章处于同一目录下

```bash
# 统计分析
coo mdimg "F:\Com\me\Repos\notebook\source\_posts"
# 统计分析 并删除未引用图片
coo mdimg -d "F:\Com\me\Repos\notebook\source\_posts"
```

> 补充:  
> 1. `所有图片数量=引用图片数+未引用图片数`      
> 这个等式匹配不上 `属于正常现象`, 因为 匹配 所有图片 时, 目前仅匹配了 `png,jpg,jpeg,gif`, 而 你在md中引用的图片可能还有其它       
> 因此 `所有图片数量<=引用图片数+未引用图片数`
>      
> 2. Windows, macOS 不区分路径大小写, 但 Linux 区分大小写      
> 本工具匹配时, 忽略大小写, 因此不用担心 引用时, 大小写不一致 而导致工具以为 未引用此图片 而误删,       
> 因此工具 可能存在漏删, 但不会误删除引用图片
> 
> 3. 本工具通过检测 `文章.md` 中图片的相对路径引用, 并转换为绝对路径, 与目标目录的所有存在图片的绝对路径进行匹配, 来找出哪些图片未引用

#### 2. cimg

> 2. 清理 未引用图片  (`mdimg` 升级版)     
> 支持识别 `md,html,htm` 

> 支持 `相对路径`: 相对于当前命令行执行所在路径

```bash
coo cimg -d --ignore-paths="IgnoreDir1,IgnoreDir2,images/1.png" "source/_posts"
```

> `--ignore-paths="IgnoreDir1,IgnoreDir2,images/1.png"` 这些图片地址忽略, 不会被删除



### 补充

卸载 coo

```bash
dotnet tool uninstall -g coo
```

## Related Projects

- [yiyungent/hexo-asset-img: 🍰 Hexo 本地图片插件](https://github.com/yiyungent/hexo-asset-img)
- [yiyungent/clear-image-action: 🔧 自动清理未引用图片 | GitHub Action](https://github.com/yiyungent/clear-image-action)

## Donate

coo is an MIT licensed open source project and completely free to use. However, the amount of effort needed to maintain and develop new features for the project is not sustainable without proper financial backing.

We accept donations through these channels:
- <a href="https://afdian.net/@yiyun" target="_blank">爱发电</a>

## Author

**coo** © [yiyun](https://github.com/yiyungent), Released under the [MIT](./LICENSE) License.<br>
Authored and maintained by yiyun with help from contributors ([list](https://github.com/yiyungent/coo/contributors)).

> GitHub [@yiyungent](https://github.com/yiyungent) Gitee [@yiyungent](https://gitee.com/yiyungent)

