# ����
1. �������vs2010����
2. ���б������Ҫ.net frameworks4.0������win7���Դ�

# ����
1. һ�������棬4�����飬ÿ������5����ť����������⡢�����ı����������ļ���
2. �Զ��������ļ���path��text��ͨ��path��ȡ����ͼ����ʾ�ڰ�ť�У�text��ʾ�ڰ�ť�¡�
    ![](http://p1.bqimg.com/567571/bc006565de2e26e5.png)
3. ��ȡ�����ļ����ִ���ʱ��ʾ�ָ���Ĭ�����á�
4. ʵʱ�л���path�Ĺ���Ŀ¼��
5. �����ļ�(hardwareTool.ini)˵����
    ```config
    [main]
    title=abc
    : ��������ı�����Ϊabc

    [group0]
    text=abc
    : ����һ��������ı���Ϊabc
    : ���ĸ����飬group0--group3

    [00]
    path=d:\abc.exe
    text=abc
    : ����һ�е�һ����ť������ĳ���·����Ϊd:\abc.exe���ı���Ϊabc
    : ��20����ť��00��01��02��03��04Ϊ��һ���飬10-14Ϊ�ڶ����飬20-24Ϊ�������飬30-34Ϊ���ķ���
    ```
6. ��[00]--[34]��path���Դ���"win{0:D}"����"{0:D}"����ѡһ�������path����"win{0:D}"����ô������Զ�ѡ���жϲ���ϵͳ�汾���ֱ�ִ��"win7"��"win10"�����pathֻ����"{0:D}"����ô������Զ��жϲ���ϵͳλ�����ֱ�ִ��"64"��"32"��
    ```config
    [00]
    path=hardwareTool\cpuz\cpuz_x{0:D}.exe
    text=CPU-Z
    [02]
    path=hardwareTool\HDTune\HDTunePro_win{0:D}.exe
    text=HDTune
    [00]���Զ�ѡ��cpuz_x32.exe �� cpuz_x64.exe
    [02]���Զ�ѡ��HDTunePro_win7.exe �� HDTunePro_win10.exe
    
    ```

# ����
1. �����ʼ������
    ![](http://p1.bqimg.com/567571/75e1955a325daab9.png)