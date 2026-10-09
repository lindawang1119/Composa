# Composa 1.4.0 中文语言扩展

这是基于 [dvdstelt/Composa](https://github.com/dvdstelt/Composa) 的 `v1.4.0` 源码制作的非官方简体中文语言扩展。

Composa 原作者为 Dennis van der Stelt，原项目许可证为 MIT。本分支保留原项目版权、许可证和第三方声明，新增中英文界面切换。

## 使用

下载 Windows x64 中文语言扩展便携包，解压后运行 `composa.exe`。便携版已包含运行环境，不需要另外安装 .NET。

首次使用仍默认英文。在菜单 **Help → Language / 语言 → 简体中文** 中选择中文并重启 Composa。中文界面下，该选项位于 **帮助(H)** 菜单；选择 **English** 可以切回英文。选择会自动保存，关闭前先保存正在编辑的作品。

菜单、工具栏、状态提示、图层面板、历史记录和主要对话框均已加入简体中文。图层名称、作品文字、文件名、字体名称及 RGB/HSV 等技术标记保留其原始内容。来自操作系统或第三方组件的异常详情可能仍使用原来的语言。

## 偏好设置

Windows 下，偏好设置保存在 `%APPDATA%\Composa\settings.json`。

中文版与原版沿用同一份偏好设置。原版文件和作品无需替换。语言选项写在偏好设置中，作品格式不变。

此语言扩展尚未合入官方版本；官方更新包可能不包含这些中文界面改动。保留本中文便携版文件夹即可继续使用。

程序内的更新检查仍查询原项目的官方版本。它不会自动提供本语言扩展的更新。

## 验证

已在 Windows x64 上构建并通过启动检查。测试通过 669 项（Core 386 项、App 283 项），包括中英文界面、语言选择、快捷键和导入报告等检查。

## 重新构建

需要 .NET 10 SDK 和 Git Bash。生成便携包还需要 `7z` 或 `zip` 压缩工具。先在源码目录运行测试，再在 Git Bash 中运行打包脚本：

```bash
dotnet test
bash scripts/package/windows.sh win-x64 dist --no-installer
```

打包脚本会下载并校验未随源码提交的 MODNet 模型，复制许可证与第三方声明，并在 `dist/` 中生成 Windows x64 便携压缩包。仅运行 `dotnet publish` 不等同于完成可分发包的制作。

翻译表位于 `src/Composa.App/Localization/*.zh-CN.json`。英文原文作为键，缺少词条时回退到英文。语言仅影响界面显示，不改变快捷键标识、选项值或数字解析。

请同时保留 `LICENSE`、`THIRD-PARTY-NOTICES.txt`、`ImageMagick-NOTICE.txt` 和完整的 `models` 文件夹。
