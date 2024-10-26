# 语言
- [English](README.md)
- [日本語](README_JPN.md)
- 简体中文（您在此）
- [한국어](README_KOR.md)

# 目录
  - [描述](#描述)
  - [使用条款](#使用条款)
  - [功能](#功能)
  - [下载和安装](#下载和安装)
  - [使用教程](#使用教程)
  - [支持](#支持)
  - [相关项目](#相关项目)
  - [项目贡献者](#项目贡献者)

# 描述
`VVVF-Simulator` 是用于在PC上模拟VVVF逆变器声音的应用程序。<br>
该软件适用于C# WPF应用程序。<br>

# 使用条款
您可以**自由**使用该程序中的代码。<br>
对于您使用该应用程序进行的任何操作，我们**不承担任何责任**。<br>

我们请您:<br>
- 发布此 GitHub 页面的 URL<br>

我们不允许:<br>
- 发布修改后的代码而不引用此页面<br>

# 功能
## VVVF音频生成
此应用程序将以 `.wav` 扩展名导出模拟的VVVF逆变器声音。<br>
音频采样频率为192kHz。<br>

## 生成波形视频
视频将以`.avi`的文件扩展名导出
![2022-02-14](https://user-images.githubusercontent.com/77259842/153803020-6615bcce-22a6-4839-b919-ea114dc12d03.png)

## 生成电压矢量视频（磁链）
视频将以`.avi`的文件扩展名导出

## 生成调制状态信息视频
该应用程序可以导出调制状态信息的视频  <br>
视频将以`.avi`的文件扩展名导出 <br>
![2022-06-11 (1)](https://user-images.githubusercontent.com/77259842/173188884-72a1290a-6d7b-4354-88e4-cecfa5d0d424.png)

## 实时音频模拟
您可以实时生成音频，并控制声音频率是否增加或减少以及频率增加或减少的速率。<br>

# 下载和安装
您可以在自己的电脑上编译该项目的源代码，也可以直接下载`.exe`文件。<br>
## 下载EXE文件
请前往[Releases](https://github.com/VvvfGeeks/VVVF-Simulator/releases)页面，下载`VVVF-SIM.zip`.<br>
解压`VVVF-SIM.zip`，找到`VvvfSimulator.exe`并点击运行<br>

## 在 Visual Studio 上使用源代码进行设置
首先，下载 Visual Studio然后运行安装程序，并确保在安装程序中选择了 .NET 桌面开发。安装完成后，打开 Visual Studio 并单击“克隆存储库”。并且复制 VVVF-Simulator 页面的 URL：
<br>
https://github.com/VvvfGeeks/VVVF-Simulator
<br>
之后粘贴网址并点击克隆。单击“解决方案 ‘VVVF-Simulator’ ”，然后单击绿色箭头以编译并运行该程序。现在您应该会看到一个窗口打开。
<br>

# 使用教程

要加载或保存文件，请单击`文件`选项卡并从那里选择要执行的操作。<br>

要实时生成声音，请单击 `实时模拟` 选项卡。`VVVF`是通过音频播放的生成的PWM，而`Train`是模拟火车上的走行音。<br>

通过单击其中任一设置，您可以更改音频缓冲区大小、显示控制变量、显示电压矢量图（磁链）、显示波形、启用实时编辑或显示 FFT。请注意，启用的设置越多，所需的CPU处理能力就越强。

## 创建声音教程
由于原文与现版本的某些选项不通用，已略去。

# 支持
- 加入我们的[Discord](https://discord.gg/SQr2tXJgVq)服务器！您可以经常询问有关VVVF-SIM的问题！
- [示例文件](https://github.com/VvvfGeeks/VVVF-Simulator/releases/download/v1.9.0.1/yaml_samples.zip)

# 相关项目
 - [Raspberry Pi Zero Vvvf](https://github.com/VvvfGeeks/RPi-Zero-VVVF)
 - [Raspberry Pi 3 Vvvf](https://github.com/VvvfGeeks/RPi-3-VVVF)
 - [Youtube](https://www.youtube.com/channel/UCdo7fDodYWO29-Q_0G1S59g)

# 项目贡献者
## 早期贡献者
 - [Thunderfeng](https://github.com/Leifengfengfeng)
 - [Geek of the Week](https://github.com/geekotw)
## 语言翻译
 - turtle713 (Korean)
 - [02001](https://github.com/Jerethon) (Simplified Chinese)
