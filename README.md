<p align="center">
<!-- <img src="docs/_images/coo.png" alt="coo"> -->
</p>
<h1 align="center">coo</h1>

> :cake: 自用CLI

[![repo size](https://img.shields.io/github/repo-size/yiyungent/coo.svg?style=flat)]()
[![LICENSE](https://img.shields.io/github/license/yiyungent/coo.svg?style=flat)](https://github.com/yiyungent/coo/blob/master/LICENSE)
[![nuget](https://img.shields.io/nuget/v/coo.svg?style=flat)](https://www.nuget.org/packages/coo/)
[![downloads](https://img.shields.io/nuget/dt/coo.svg?style=flat)](https://www.nuget.org/packages/coo/)



## 介绍

coo: 自用CLI，工具集.   
 

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

> 1. 清理 md文件的未引用图片: 例如使用 `Hexo`, 采用本地图片文件 (图片放在md同名文件夹下) 等    

`"F:\Com\me\Repos\notebook\source\_posts"` 为 `Hexo文章目录`, 本人图片与文章处于同一目录下

```bash
# 统计分析
coo mdimg "F:\Com\me\Repos\notebook\source\_posts"
# 统计分析 并删除未引用图片
coo mdimg -d "F:\Com\me\Repos\notebook\source\_posts"
```


> 补充:

卸载 coo

```bash
dotnet tool uninstall -g coo
```

## Donate

coo is an MIT licensed open source project and completely free to use. However, the amount of effort needed to maintain and develop new features for the project is not sustainable without proper financial backing.

We accept donations through these channels:
- <a href="https://afdian.net/@yiyun" target="_blank">爱发电</a>

## Author

**coo** © [yiyun](https://github.com/yiyungent), Released under the [MIT](./LICENSE) License.<br>
Authored and maintained by yiyun with help from contributors ([list](https://github.com/yiyungent/coo/contributors)).

> GitHub [@yiyungent](https://github.com/yiyungent) Gitee [@yiyungent](https://gitee.com/yiyungent)

