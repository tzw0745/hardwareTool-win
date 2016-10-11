# 声明
1. 本软件由vs2010开发
2. 运行本软件需要.net frameworks4.0环境，win7不自带

# 特性
1. 一个主界面，4个分组，每个分组5个按钮。主界面标题、分组文本来自配置文件。
2. 自动从配置文件读path和text，通过path提取程序图标显示在按钮中，text显示在按钮下。
    ![](http://p1.bqimg.com/567571/bc006565de2e26e5.png)
3. 读取配置文件出现错误时提示恢复到默认配置。
4. 实时切换到path的工作目录。
5. 配置文件(hardwareTool.ini)说明：
    ```config
    [main]
    title=abc
    : 将主程序的标题设为abc

    [group0]
    text=abc
    : 将第一个分组的文本设为abc
    : 共四个分组，group0--group3

    [00]
    path=d:\abc.exe
    text=abc
    : 将第一行第一个按钮所代表的程序路径设为d:\abc.exe，文本设为abc
    : 共20个按钮，00，01，02，03，04为第一分组，10-14为第二分组，20-24为第三分组，30-34为第四分组
    ```
6. 在[00]--[34]的path可以带有"win{0:D}"或者"{0:D}"（二选一），如果path带有"win{0:D}"，那么程序会自动选择判断操作系统版本，分别执行"win7"或"win10"；如果path只带有"{0:D}"，那么程序会自动判断操作系统位数，分别执行"64"和"32"。
    ```config
    [00]
    path=hardwareTool\cpuz\cpuz_x{0:D}.exe
    text=CPU-Z
    [02]
    path=hardwareTool\HDTune\HDTunePro_win{0:D}.exe
    text=HDTune
    [00]会自动选择cpuz_x32.exe 和 cpuz_x64.exe
    [02]会自动选择HDTunePro_win7.exe 和 HDTunePro_win10.exe
    
    ```

# 其它
1. 程序初始化流程
    ![](http://p1.bqimg.com/567571/75e1955a325daab9.png)