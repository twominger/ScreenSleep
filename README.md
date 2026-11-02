# ScreenSleep

一个轻量的 Windows 息屏托盘工具。左键点击托盘图标即可关闭显示器，同时保持电脑和后台任务继续运行。

## 下载

从 [Releases](https://github.com/twominger/ScreenSleep/releases/latest) 下载 `ScreenSleep.exe`。这是包含 .NET 运行环境的 Windows x64 独立文件，无需安装 .NET。把它放在固定位置再设置开机启动；首次运行可能因为尚未签名而显示 Windows SmartScreen 提示。

## 使用方法

1. 运行 `ScreenSleep.exe`，程序会常驻系统托盘。
2. 左键点击托盘图标，或者右键选择“立即息屏”。
3. 按任意键或移动鼠标即可解除息屏。
4. 右键托盘图标可以设置开机自动启动或退出程序。

如果已经运行旧版，请先从旧版托盘菜单选择“退出”，再启动新版。程序采用单实例运行，直接双击新版不会替换已经运行的旧进程。

## 工作原理

- 通过 `WM_SYSCOMMAND / SC_MONITORPOWER` 请求 Windows 关闭显示器，而不是让电脑睡眠。
- 息屏期间通过 `SetThreadExecutionState(ES_SYSTEM_REQUIRED)` 防止系统进入睡眠，后台下载、播放、计算等任务可以继续运行。
- 使用低级键盘和鼠标钩子检测唤醒意图，但不记录按键内容。
- 进入息屏后有约 650 毫秒的输入保护时间，用来忽略点击托盘图标时产生的鼠标释放事件；保护时间结束后，按键、移动鼠标或点击鼠标都会立即退出息屏模式。
- Windows 会在收到鼠标输入时自动点亮显示器，因此程序不会为了判断“连续移动”而再次关闭显示器，避免出现亮灭闪烁。
- 息屏前会在每个显示器上创建不可激活的纯黑保护层。只有能进入 Windows 正常输入链的鼠标或键盘才会解除息屏；已经在 PnP 设备层禁用的内置键盘不会解除。
- 程序同时订阅 Windows 的 `GUID_SESSION_DISPLAY_STATUS` 电源通知。如果被禁用的内置键盘仍在固件层短暂给显示器上电，保护层会遮住桌面，程序随后再次关闭显示器，避免桌面闪现。
- 退出息屏后恢复系统原有的电源管理行为，不修改 Windows 电源计划。

## 构建

需要 .NET 8 SDK：

```powershell
dotnet build .\ScreenSleep.csproj -c Release
```

发布独立单文件版本：

```powershell
dotnet publish .\ScreenSleep.csproj -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -p:DebugType=None -p:DebugSymbols=false `
  -o .\dist
```

## 注意事项

- 全屏独占应用、远程桌面或部分显示器驱动可能以不同方式处理显示器电源命令。
- 某些笔记本固件会在内置键盘被 Windows 禁用后仍短暂开启屏幕背光。应用可以用纯黑保护层避免显示桌面并再次关屏，但只有 BIOS/EC 提供相应选项时才能从硬件层完全阻止背光通电。
- 开机启动项保存 EXE 的完整路径，移动程序后需要重新关闭并开启一次开机启动。
- 程序不会阻止人为关机、重启或休眠命令。

## 隐私与许可

程序仅在本机处理键盘和鼠标事件，不保存按键内容，不联网，也不收集使用数据。项目以 [MIT License](LICENSE) 开源。欢迎提交 Issue 或 Pull Request。
