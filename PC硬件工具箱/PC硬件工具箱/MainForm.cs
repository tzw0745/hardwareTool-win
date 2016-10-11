using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace PC硬件工具箱
{
    public partial class MainForm : Form
    {
        //png文件头为137 80 78 71 13 10 26 10
        private byte[] pngHead = new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };

        //判断系统为32bit or 64bit，系统为win10 or win7
        private int systemBit = Environment.Is64BitOperatingSystem ? 64 : 32;
        private bool aboveWin8 = false;

        string cachePath = @"hardwareTool\.cache";
        string cacheFilePath = @"hardwareTool\.cache\imageCache";
        string iniFilePath = @"hardwareTool.ini";
        string currentDir = Environment.CurrentDirectory;

        private string[] pathArr = new string[20];
        //path的路径部分
        private string[] pathDirArr = new string[20];
        //path的文件名部分
        private string[] pathFileArr = new string[20];

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //获取系统信息
            Version currentVersion = Environment.OSVersion.Version;
            Version win8Version = new Version("6.2");
            if (currentVersion.CompareTo(win8Version) >= 0)
                this.aboveWin8 = true;

            //载入配置文件
            string result = this.loadIniFile();
            //载入ini文件中是否发生了错误
            if (result.Length > 0)
            {
                string[] temp = new string[3];
                temp[0] = "载入配置文件时发生错误，是否恢复默认配置文件？";
                temp[1] = "\n\n错误提示：\n-- ";
                temp[2] = result;
                string showText = String.Join("", temp);
                DialogResult r = MessageBox.Show(showText, "错误",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (r == DialogResult.Yes) this.setDefaultConfig();
                if (r == DialogResult.No) System.Environment.Exit(0);
            }

            this.loadIconCache();

            //初始化时间lable
            this.refreshTime_Tick(sender, e);

            //保证cachePath的存在
            if (!Directory.Exists(this.cachePath))
                Directory.CreateDirectory(this.cachePath);

            //载入图片缓存
            this.loadIconCache();
        }

        #region 图片处理
        //从filePath中提取icon，filePath一般为exe,ico文件
        private Bitmap GetIcon(string filePath, bool large=false)
        {
            IntPtr[] largeIcons, smallIcons;
            largeIcons = new IntPtr[1];
            smallIcons = new IntPtr[1];

            ExtractIconEx(filePath, 0, largeIcons, smallIcons, 1);

            if (large)
                return Icon.FromHandle(largeIcons[0]).ToBitmap();
            else
                return Icon.FromHandle(smallIcons[0]).ToBitmap();
        }
        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        private static extern int ExtractIconEx(string lpszFile, int niconIndex,
            IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

        //将bmp压缩或放大到newW*newH的Bitmap
        public Bitmap KiResizeImage(Bitmap bmp, int newW, int newH)
        {
            Bitmap b = new Bitmap(newW, newH);
            Graphics g = Graphics.FromImage(b);
            // 插值算法的质量
            g.InterpolationMode = InterpolationMode.Default;
            g.DrawImage(bmp, new Rectangle(0, 0, newW, newH),
                new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
            g.Dispose();
            return b;
        }

        //讲Image转换成一维数组
        private byte[] ImgToByt(Image img)
        {
            MemoryStream ms = new MemoryStream();
            bool temp = img == null;
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] imagedata = null;
            imagedata = ms.GetBuffer();
            return imagedata;
        }
        //将一维数组转化成Image
        private Bitmap BytToImg(byte[] byt)
        {
            Stream ms = new MemoryStream(byt);
            Bitmap img = new Bitmap(ms, true);
            return img;
        }
        #endregion

        #region 杂项
        //计时器自动刷新时间
        private void refreshTime_Tick(object sender, EventArgs e)
        {
            this.timeText.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        }

        //打开作者网站链接
        private void authorLink_LinkClicked(object sender,
            LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://tzw0745.cn");
        }

        //刷新图片缓存，lable的单击事件
        private void refreshPicLable_LinkClicked(object sender,
            LinkLabelLinkClickedEventArgs e)
        {
            //20个icon
            Bitmap[] icons = new Bitmap[20];
            Bitmap imageTemp;

            byte[][] imageBytes = new byte[20][];
            //下面这个循环给imageBytes数组的每一个元素预分配一个pngHead的空间
            //假设icon获取失败，则只将文件头写入，而只有文件头就相当于无图片
            for (int offset = 0; offset < imageBytes.Length; ++offset)
            {
                imageBytes[offset] = new byte[this.pngHead.Length];
            }
            
            //以下四个部分将从pathArr中的20个路径获取icon
            #region group0 0-4
            try
            {
                imageTemp = this.GetIcon(this.pathArr[0], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[0] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[0], this.pngHead.Length);
            }
            icons[0] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[1], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[1] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[1], this.pngHead.Length);
            }
            icons[1] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[2], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[2] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[2], this.pngHead.Length);
            }
            icons[2] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[3], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[3] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[3], this.pngHead.Length);
            }
            icons[3] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[4], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[4] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[4], this.pngHead.Length);
            }
            icons[4] = imageTemp;
            #endregion

            #region group1 5-9
            try
            {
                imageTemp = this.GetIcon(this.pathArr[5], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[5] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[5], this.pngHead.Length);
            }
            icons[5] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[6], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[6] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[6], this.pngHead.Length);
            }
            icons[6] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[7], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[7] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[7], this.pngHead.Length);
            }
            icons[7] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[8], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[8] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[8], this.pngHead.Length);
            }
            icons[8] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[9], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[9] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[9], this.pngHead.Length);
            }
            icons[9] = imageTemp;
            #endregion

            #region group2 10-14
            try
            {
                imageTemp = this.GetIcon(this.pathArr[10], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[10] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[10], this.pngHead.Length);
            }
            icons[10] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[11], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[11] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[11], this.pngHead.Length);
            }
            icons[11] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[12], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[12] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[12], this.pngHead.Length);
            }
            icons[12] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[13], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[13] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[13], this.pngHead.Length);
            }
            icons[13] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[14], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[14] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[14], this.pngHead.Length);
            }
            icons[14] = imageTemp;
            #endregion

            #region group3 15-19
            try
            {
                imageTemp = this.GetIcon(this.pathArr[15], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[15] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[15], this.pngHead.Length);
            }
            icons[15] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[16], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[16] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[16], this.pngHead.Length);
            }
            icons[16] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[17], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[17] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[17], this.pngHead.Length);
            }
            icons[17] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[18], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[18] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[18], this.pngHead.Length);
            }
            icons[18] = imageTemp;

            try
            {
                imageTemp = this.GetIcon(this.pathArr[19], large: true);
                imageTemp = this.KiResizeImage(imageTemp, 64, 64);
                imageBytes[19] = this.ImgToByt(imageTemp);
            }
            catch
            {
                imageTemp = null;
                Array.Copy(this.pngHead, imageBytes[19], this.pngHead.Length);
            }
            icons[19] = imageTemp;
            #endregion

            //将获取到的20个icon显示在20个button中
            this.showIconOnButton(icons);
            
            #region 保存图片缓存至cacheFilePath
            if (File.Exists(this.cacheFilePath))
            {
                File.Delete(this.cacheFilePath);
            }
            if (!Directory.Exists(this.cachePath))
            {
                Directory.CreateDirectory(this.cachePath);
            }

            FileStream fileStream = new FileStream(this.cacheFilePath,
                FileMode.Append, FileAccess.Write);
            foreach (byte[] imageByte in imageBytes)
            {
                fileStream.Write(imageByte, 0, imageByte.Length);
            }
            fileStream.Close();
            #endregion
        }

        //从缓存文件的二进制流中载入图片并显示在对应的button中
        private void loadIconCache()
        {
            //尝试从cache文件中读取icon数据
            byte[] bytes1d;
            try
            {
                bytes1d = File.ReadAllBytes(this.cacheFilePath);
            }
            catch
            {
                //文件不存在或其它错误
                this.refreshPicLable_LinkClicked(null, null);
                return;
            }

            byte[][] imageBytes2d = new byte[20][];

            #region 缓存文件切分成多个png文件流
            int[] pngBeginNumArr = new int[20];
            int n = 0;

            for (int offset = 0; offset < bytes1d.Length; ++offset)
            {
                bool isPngBegin = true;
                for (int pngHeadNum = 0; pngHeadNum < this.pngHead.Length; ++pngHeadNum)
                {
                    if (offset + pngHeadNum >= bytes1d.Length) break;
                    if (bytes1d[offset + pngHeadNum] != this.pngHead[pngHeadNum])
                    {
                        isPngBegin = false;
                    }
                }
                if (isPngBegin) pngBeginNumArr[n++] = offset;
            }

            for (int offset = 0; offset < 20; ++offset)
            {
                if (offset < 19)
                {
                    int size = pngBeginNumArr[offset + 1] - pngBeginNumArr[offset];
                    imageBytes2d[offset] = new byte[size];
                    Array.Copy(bytes1d, pngBeginNumArr[offset], imageBytes2d[offset], 0,
                        pngBeginNumArr[offset + 1] - pngBeginNumArr[offset]);
                }
                else
                {
                    imageBytes2d[offset] = new byte[bytes1d.Length - pngBeginNumArr[offset]];
                    Array.Copy(bytes1d, pngBeginNumArr[offset], imageBytes2d[offset], 0,
                        bytes1d.Length - pngBeginNumArr[offset]);
                }
            }
            #endregion

            Bitmap[] icons = new Bitmap[20];
            for (int offset = 0; offset < icons.Length; ++offset)
            {
                try
                {
                    icons[offset] = this.BytToImg(imageBytes2d[offset]);
                }
                catch
                {
                    icons[offset] = null;
                }
            }

            this.showIconOnButton(icons);
        }

        //将20个icon显示在20个button上
        private void showIconOnButton(Bitmap[] icons)
        {
            #region group0 0-4
            if (icons[0] != null)
            {
                this.button00.Image = icons[0];
                this.button00.Text = null;
                this.button00.Enabled = true;

            }
            else
            {
                this.button00.Image = null;
                this.button00.Text = "未检测到文件！";
                this.button00.Enabled = false;

            }

            if (icons[1] != null)
            {
                this.button01.Image = icons[1];
                this.button01.Text = null;
                this.button01.Enabled = true;

            }
            else
            {
                this.button01.Image = null;
                this.button01.Text = "未检测到文件！";
                this.button01.Enabled = false;

            }

            if (icons[2] != null)
            {
                this.button02.Image = icons[2];
                this.button02.Text = null;
                this.button02.Enabled = true;

            }
            else
            {
                this.button02.Image = null;
                this.button02.Text = "未检测到文件！";
                this.button02.Enabled = false;

            }

            if (icons[3] != null)
            {
                this.button03.Image = icons[3];
                this.button03.Text = null;
                this.button03.Enabled = true;

            }
            else
            {
                this.button03.Image = null;
                this.button03.Text = "未检测到文件！";
                this.button03.Enabled = false;

            }

            if (icons[4] != null)
            {
                this.button04.Image = icons[4];
                this.button04.Text = null;
                this.button04.Enabled = true;

            }
            else
            {
                this.button04.Image = null;
                this.button04.Text = "未检测到文件！";
                this.button04.Enabled = false;

            }
            #endregion

            #region group1 5-9
            if (icons[5] != null)
            {
                this.button10.Image = icons[5];
                this.button10.Text = null;
                this.button10.Enabled = true;

            }
            else
            {
                this.button10.Image = null;
                this.button10.Text = "未检测到文件！";
                this.button10.Enabled = false;

            }

            if (icons[6] != null)
            {
                this.button11.Image = icons[6];
                this.button11.Text = null;
                this.button11.Enabled = true;

            }
            else
            {
                this.button11.Image = null;
                this.button11.Text = "未检测到文件！";
                this.button11.Enabled = false;

            }

            if (icons[7] != null)
            {
                this.button12.Image = icons[7];
                this.button12.Text = null;
                this.button12.Enabled = true;

            }
            else
            {
                this.button12.Image = null;
                this.button12.Text = "未检测到文件！";
                this.button12.Enabled = false;

            }

            if (icons[8] != null)
            {
                this.button13.Image = icons[8];
                this.button13.Text = null;
                this.button13.Enabled = true;

            }
            else
            {
                this.button13.Image = null;
                this.button13.Text = "未检测到文件！";
                this.button13.Enabled = false;

            }

            if (icons[9] != null)
            {
                this.button14.Image = icons[9];
                this.button14.Text = null;
                this.button14.Enabled = true;

            }
            else
            {
                this.button14.Image = null;
                this.button14.Text = "未检测到文件！";
                this.button14.Enabled = false;

            }
            #endregion

            #region group1 10-14
            if (icons[10] != null)
            {
                this.button20.Image = icons[10];
                this.button20.Text = null;
                this.button20.Enabled = true;

            }
            else
            {
                this.button20.Image = null;
                this.button20.Text = "未检测到文件！";
                this.button20.Enabled = false;

            }

            if (icons[11] != null)
            {
                this.button21.Image = icons[11];
                this.button21.Text = null;
                this.button21.Enabled = true;

            }
            else
            {
                this.button21.Image = null;
                this.button21.Text = "未检测到文件！";
                this.button21.Enabled = false;

            }

            if (icons[12] != null)
            {
                this.button22.Image = icons[12];
                this.button22.Text = null;
                this.button22.Enabled = true;

            }
            else
            {
                this.button22.Image = null;
                this.button22.Text = "未检测到文件！";
                this.button22.Enabled = false;

            }

            if (icons[13] != null)
            {
                this.button23.Image = icons[13];
                this.button23.Text = null;
                this.button23.Enabled = true;

            }
            else
            {
                this.button23.Image = null;
                this.button23.Text = "未检测到文件！";
                this.button23.Enabled = false;

            }

            if (icons[14] != null)
            {
                this.button24.Image = icons[14];
                this.button24.Text = null;
                this.button24.Enabled = true;

            }
            else
            {
                this.button24.Image = null;
                this.button24.Text = "未检测到文件！";
                this.button24.Enabled = false;

            }
            #endregion

            #region group 2 15-19
            if (icons[15] != null)
            {
                this.button30.Image = icons[15];
                this.button30.Text = null;
                this.button30.Enabled = true;

            }
            else
            {
                this.button30.Image = null;
                this.button30.Text = "未检测到文件！";
                this.button30.Enabled = false;

            }

            if (icons[16] != null)
            {
                this.button31.Image = icons[16];
                this.button31.Text = null;
                this.button31.Enabled = true;

            }
            else
            {
                this.button31.Image = null;
                this.button31.Text = "未检测到文件！";
                this.button31.Enabled = false;

            }

            if (icons[17] != null)
            {
                this.button32.Image = icons[17];
                this.button32.Text = null;
                this.button32.Enabled = true;

            }
            else
            {
                this.button32.Image = null;
                this.button32.Text = "未检测到文件！";
                this.button32.Enabled = false;

            }

            if (icons[18] != null)
            {
                this.button33.Image = icons[18];
                this.button33.Text = null;
                this.button33.Enabled = true;

            }
            else
            {
                this.button33.Image = null;
                this.button33.Text = "未检测到文件！";
                this.button33.Enabled = false;

            }

            if (icons[19] != null)
            {
                this.button34.Image = icons[19];
                this.button34.Text = null;
                this.button34.Enabled = true;

            }
            else
            {
                this.button34.Image = null;
                this.button34.Text = "未检测到文件！";
                this.button34.Enabled = false;

            }
            #endregion
        }

        //载入配置文件
        private string loadIniFile()
        {
            // http://stackoverflow.com/questions/217902/reading-writing-an-ini-file
            if (!File.Exists(this.iniFilePath))
                return "配置文件不存在！";

            //读取配置文件
            IniFile configFile = new IniFile(this.iniFilePath);
            this.Text = configFile.Read("title", "main");
            if (this.Text.Length <= 0) return "main的title不存在！";

            #region groupbox的文本
            this.groupBox0.Text = configFile.Read("text", "group0");
            this.groupBox1.Text = configFile.Read("text", "group1");
            this.groupBox2.Text = configFile.Read("text", "group2");
            this.groupBox3.Text = configFile.Read("text", "group3");
            if (this.groupBox0.Text.Length <= 0) return "group0的text不存在！";
            if (this.groupBox1.Text.Length <= 0) return "group1的text不存在！";
            if (this.groupBox2.Text.Length <= 0) return "group2的text不存在！";
            if (this.groupBox3.Text.Length <= 0) return "group3的text不存在！";
            #endregion
            #region lable的文本&设定lable的新坐标
            this.lable00.Text = configFile.Read("text", "00");
            this.lable01.Text = configFile.Read("text", "01");
            this.lable02.Text = configFile.Read("text", "02");
            this.lable03.Text = configFile.Read("text", "03");
            this.lable04.Text = configFile.Read("text", "04");
            this.lable10.Text = configFile.Read("text", "10");
            this.lable11.Text = configFile.Read("text", "11");
            this.lable12.Text = configFile.Read("text", "12");
            this.lable13.Text = configFile.Read("text", "13");
            this.lable14.Text = configFile.Read("text", "14");
            this.lable20.Text = configFile.Read("text", "20");
            this.lable21.Text = configFile.Read("text", "21");
            this.lable22.Text = configFile.Read("text", "22");
            this.lable23.Text = configFile.Read("text", "23");
            this.lable24.Text = configFile.Read("text", "24");
            this.lable30.Text = configFile.Read("text", "30");
            this.lable31.Text = configFile.Read("text", "31");
            this.lable32.Text = configFile.Read("text", "32");
            this.lable33.Text = configFile.Read("text", "33");
            this.lable34.Text = configFile.Read("text", "34");

            if (this.lable00.Text.Length <= 0) return "00的text不存在！";
            if (this.lable01.Text.Length <= 0) return "01的text不存在！";
            if (this.lable02.Text.Length <= 0) return "02的text不存在！";
            if (this.lable03.Text.Length <= 0) return "03的text不存在！";
            if (this.lable04.Text.Length <= 0) return "04的text不存在！";
            if (this.lable10.Text.Length <= 0) return "10的text不存在！";
            if (this.lable11.Text.Length <= 0) return "11的text不存在！";
            if (this.lable12.Text.Length <= 0) return "12的text不存在！";
            if (this.lable13.Text.Length <= 0) return "13的text不存在！";
            if (this.lable14.Text.Length <= 0) return "14的text不存在！";
            if (this.lable20.Text.Length <= 0) return "20的text不存在！";
            if (this.lable21.Text.Length <= 0) return "21的text不存在！";
            if (this.lable22.Text.Length <= 0) return "22的text不存在！";
            if (this.lable23.Text.Length <= 0) return "23的text不存在！";
            if (this.lable24.Text.Length <= 0) return "24的text不存在！";
            if (this.lable30.Text.Length <= 0) return "30的text不存在！";
            if (this.lable31.Text.Length <= 0) return "31的text不存在！";
            if (this.lable32.Text.Length <= 0) return "32的text不存在！";
            if (this.lable33.Text.Length <= 0) return "33的text不存在！";
            if (this.lable34.Text.Length <= 0) return "34的text不存在！";

            int newX;

            newX = this.button00.Location.X + this.button00.Size.Width / 2;
            newX = newX - this.lable00.Size.Width / 2;
            this.lable00.Location = new Point(newX, this.lable00.Location.Y);

            newX = this.button01.Location.X + this.button01.Size.Width / 2;
            newX = newX - this.lable01.Size.Width / 2;
            this.lable01.Location = new Point(newX, this.lable01.Location.Y);

            newX = this.button02.Location.X + this.button02.Size.Width / 2;
            newX = newX - this.lable02.Size.Width / 2;
            this.lable02.Location = new Point(newX, this.lable02.Location.Y);

            newX = this.button03.Location.X + this.button03.Size.Width / 2;
            newX = newX - this.lable03.Size.Width / 2;
            this.lable03.Location = new Point(newX, this.lable03.Location.Y);

            newX = this.button04.Location.X + this.button04.Size.Width / 2;
            newX = newX - this.lable04.Size.Width / 2;
            this.lable04.Location = new Point(newX, this.lable04.Location.Y);

            newX = this.button10.Location.X + this.button10.Size.Width / 2;
            newX = newX - this.lable10.Size.Width / 2;
            this.lable10.Location = new Point(newX, this.lable10.Location.Y);

            newX = this.button11.Location.X + this.button11.Size.Width / 2;
            newX = newX - this.lable11.Size.Width / 2;
            this.lable11.Location = new Point(newX, this.lable11.Location.Y);

            newX = this.button12.Location.X + this.button12.Size.Width / 2;
            newX = newX - this.lable12.Size.Width / 2;
            this.lable12.Location = new Point(newX, this.lable12.Location.Y);

            newX = this.button13.Location.X + this.button13.Size.Width / 2;
            newX = newX - this.lable13.Size.Width / 2;
            this.lable13.Location = new Point(newX, this.lable13.Location.Y);

            newX = this.button14.Location.X + this.button14.Size.Width / 2;
            newX = newX - this.lable14.Size.Width / 2;
            this.lable14.Location = new Point(newX, this.lable14.Location.Y);

            newX = this.button20.Location.X + this.button20.Size.Width / 2;
            newX = newX - this.lable20.Size.Width / 2;
            this.lable20.Location = new Point(newX, this.lable20.Location.Y);

            newX = this.button21.Location.X + this.button21.Size.Width / 2;
            newX = newX - this.lable21.Size.Width / 2;
            this.lable21.Location = new Point(newX, this.lable21.Location.Y);

            newX = this.button22.Location.X + this.button22.Size.Width / 2;
            newX = newX - this.lable22.Size.Width / 2;
            this.lable22.Location = new Point(newX, this.lable22.Location.Y);

            newX = this.button23.Location.X + this.button23.Size.Width / 2;
            newX = newX - this.lable23.Size.Width / 2;
            this.lable23.Location = new Point(newX, this.lable23.Location.Y);

            newX = this.button24.Location.X + this.button24.Size.Width / 2;
            newX = newX - this.lable24.Size.Width / 2;
            this.lable24.Location = new Point(newX, this.lable24.Location.Y);

            newX = this.button30.Location.X + this.button30.Size.Width / 2;
            newX = newX - this.lable30.Size.Width / 2;
            this.lable30.Location = new Point(newX, this.lable30.Location.Y);

            newX = this.button31.Location.X + this.button31.Size.Width / 2;
            newX = newX - this.lable31.Size.Width / 2;
            this.lable31.Location = new Point(newX, this.lable31.Location.Y);

            newX = this.button32.Location.X + this.button32.Size.Width / 2;
            newX = newX - this.lable32.Size.Width / 2;
            this.lable32.Location = new Point(newX, this.lable32.Location.Y);

            newX = this.button33.Location.X + this.button33.Size.Width / 2;
            newX = newX - this.lable33.Size.Width / 2;
            this.lable33.Location = new Point(newX, this.lable33.Location.Y);

            newX = this.button34.Location.X + this.button34.Size.Width / 2;
            newX = newX - this.lable34.Size.Width / 2;
            this.lable34.Location = new Point(newX, this.lable34.Location.Y);
            #endregion
            #region path
            this.pathArr[0] = configFile.Read("path", "00");
            this.pathArr[1] = configFile.Read("path", "01");
            this.pathArr[2] = configFile.Read("path", "02");
            this.pathArr[3] = configFile.Read("path", "03");
            this.pathArr[4] = configFile.Read("path", "04");
            this.pathArr[5] = configFile.Read("path", "10");
            this.pathArr[6] = configFile.Read("path", "11");
            this.pathArr[7] = configFile.Read("path", "12");
            this.pathArr[8] = configFile.Read("path", "13");
            this.pathArr[9] = configFile.Read("path", "14");
            this.pathArr[10] = configFile.Read("path", "20");
            this.pathArr[11] = configFile.Read("path", "21");
            this.pathArr[12] = configFile.Read("path", "22");
            this.pathArr[13] = configFile.Read("path", "23");
            this.pathArr[14] = configFile.Read("path", "24");
            this.pathArr[15] = configFile.Read("path", "30");
            this.pathArr[16] = configFile.Read("path", "31");
            this.pathArr[17] = configFile.Read("path", "32");
            this.pathArr[18] = configFile.Read("path", "33");
            this.pathArr[19] = configFile.Read("path", "34");
            #endregion

            //格式化path，比如win7 or win10，64bit or 32bit
            int osVersion = this.aboveWin8 ? 10 : 7;
            for (int offset = 0; offset < pathArr.Length; ++offset)
            {
                if (this.pathArr[offset].Length <= 0)
                {
                    //将10进制的offset转换成5进制以对应ini文件内的内容
                    int system5 = (offset / 5) * 10 + offset % 5;
                    return String.Format("{0:00}的path不存在！", system5);
                }

                //format
                if(this.pathArr[offset].Contains("win{0:D}"))
                    this.pathArr[offset] = string.Format(this.pathArr[offset],
                        osVersion);
                else if(this.pathArr[offset].Contains("{0:D}"))
                    this.pathArr[offset] = string.Format(this.pathArr[offset],
                        this.systemBit);

                //获取format后path中的文件夹路径
                this.pathDirArr[offset] = Path.GetDirectoryName(this.pathArr[offset]);
                //获取format后path中的文件名
                this.pathFileArr[offset] = Path.GetFileName(this.pathArr[offset]);
            }

            //配置文件没有错误
            return "";
        }

        //恢复默认设置
        private void setDefaultConfig()
        {
            IniFile configFile = new IniFile(this.iniFilePath);
            #region 写入title
            configFile.Write("title", "PC硬件工具箱@tzw0745", "main");
            #endregion
            #region 写入4个group
            configFile.Write("text", "硬件信息", "group0");
            configFile.Write("text", "硬件评测", "group1");
            configFile.Write("text", "烤机软件", "group2");
            configFile.Write("text", "其他工具", "group3");
            #endregion
            #region 写入path
            configFile.Write("path", @"hardwareTool\cpuz\cpuz_x{0:D}.exe", "00");
            configFile.Write("path", @"hardwareTool\FurMark\gpuz.exe", "01");
            configFile.Write("path", @"hardwareTool\HDTune\HDTunePro_win{0:D}.exe", "02");
            configFile.Write("path", @"hardwareTool\HWiNFO\HWiNFO{0:D}.exe", "03");
            configFile.Write("path", @"hardwareTool\aida64\aida64.exe", "04");
            configFile.Write("path", @"hardwareTool\chess\chess.exe", "10");
            configFile.Write("path", @"hardwareTool\DisplayX\DisplayX.exe", "11");
            configFile.Write("path", @"hardwareTool\HDBenchmark\AS SSD Benchmark.exe", "12");
            configFile.Write("path", @"hardwareTool\HDBenchmark\DiskMark{0:D}.exe", "13");
            configFile.Write("path", @"hardwareTool\KeyboardTest\KeyboardTest.exe", "14");
            configFile.Write("path", @"hardwareTool\\LinX\\LinX.exe", "20");
            configFile.Write("path", @"hardwareTool\\ORTHOS\\ORTHOS.exe", "21");
            configFile.Write("path", @"hardwareTool\\FurMark\\FurMark.exe", "22");
            configFile.Write("path", @"hardwareTool\\MemTest\\MemTest.exe", "23");
            configFile.Write("path", @"hardwareTool\\CoreTemp\\CoreTemp_x{0:D}.exe", "24");
            configFile.Write("path", @"hardwareTool\\ATKKPING\\ATKKPING.exe", "30");
            configFile.Write("path", @"hardwareTool\\install\\lantern-installer-beta.exe", "31");
            configFile.Write("path", @"hardwareTool\\install\\Fraps.exe", "32");
            configFile.Write("path", @"hardwareTool\\MenuMgr.exe", "33");
            configFile.Write("path", @"hardwareTool\\ChipEasy.exe", "34");
            #endregion
            #region 写入lable(text)
            configFile.Write("text", "CPU-Z", "00");
            configFile.Write("text", "GPU-Z", "01");
            configFile.Write("text", "HDTune", "02");
            configFile.Write("text", "HWiNFO", "03");
            configFile.Write("text", "AIDA64", "04");
            configFile.Write("text", "国际象棋", "10");
            configFile.Write("text", "DisplayX", "11");
            configFile.Write("text", "AS SDD", "12");
            configFile.Write("text", "DiskMark", "13");
            configFile.Write("text", "KBTest", "14");
            configFile.Write("text", "LinX", "20");
            configFile.Write("text", "ORTHOS", "21");
            configFile.Write("text", "FurMark", "22");
            configFile.Write("text", "MemTest", "23");
            configFile.Write("text", "CoreTemp", "24");
            configFile.Write("text", "ATKKPING", "30");
            configFile.Write("text", "lantern", "31");
            configFile.Write("text", "Fraps", "32");
            configFile.Write("text", "MenuMgr", "33");
            configFile.Write("text", "ChipEasy", "34");
            #endregion
            this.loadIniFile();
        }
        #endregion

        #region group0的button单击事件 0-4
        private void button00_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[0]);
                System.Diagnostics.Process.Start(this.pathFileArr[0]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button01_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[1]);
                System.Diagnostics.Process.Start(this.pathFileArr[1]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button02_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[2]);
                System.Diagnostics.Process.Start(this.pathFileArr[2]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button03_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[3]);
                System.Diagnostics.Process.Start(this.pathFileArr[3]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button04_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[4]);
                System.Diagnostics.Process.Start(this.pathFileArr[4]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }
        #endregion

        #region group1的button单击事件 5-9
        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[5]);
                System.Diagnostics.Process.Start(this.pathFileArr[5]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[6]);
                System.Diagnostics.Process.Start(this.pathFileArr[6]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[7]);
                System.Diagnostics.Process.Start(this.pathFileArr[7]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[8]);
                System.Diagnostics.Process.Start(this.pathFileArr[8]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[9]);
                System.Diagnostics.Process.Start(this.pathFileArr[9]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }
        #endregion

        #region group2的button单击事件 10-14
        private void button20_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[10]);
                System.Diagnostics.Process.Start(this.pathFileArr[10]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[11]);
                System.Diagnostics.Process.Start(this.pathFileArr[11]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button22_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[12]);
                System.Diagnostics.Process.Start(this.pathFileArr[12]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button23_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[13]);
                System.Diagnostics.Process.Start(this.pathFileArr[13]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button24_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[14]);
                System.Diagnostics.Process.Start(this.pathFileArr[14]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }
        #endregion

        #region group3的button单击事件 15-19
        private void button30_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[15]);
                System.Diagnostics.Process.Start(this.pathFileArr[15]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button31_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[16]);
                System.Diagnostics.Process.Start(this.pathFileArr[16]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button32_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[17]);
                System.Diagnostics.Process.Start(this.pathFileArr[17]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button33_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[18]);
                System.Diagnostics.Process.Start(this.pathFileArr[18]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }

        private void button34_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(this.pathDirArr[19]);
                System.Diagnostics.Process.Start(this.pathFileArr[19]);
            }
            catch
            {

            }
            System.IO.Directory.SetCurrentDirectory(this.currentDir);
        }
        #endregion

    }
}
