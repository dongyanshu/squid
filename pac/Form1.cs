using System.Windows.Forms;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using BadouSquidClient;
using System.Text.RegularExpressions;
using System.Collections;

//using System.Management;
namespace pac
{
    public partial class Form1 : Form
    {

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("Kernel32.dll")]
        private extern static int GetPrivateProfileSectionNamesA(byte[] buffer, int iLen, string fileName);

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
        static bool _settingsReturn, _refreshReturn;
        string globalURL;
        public string inipath;
        string PacURL;

 

        public static void NotifyIE()
        {
            // These lines implement the Interface in the beginning of program 
            // They cause the OS to refresh the settings, causing IP to realy update
            _settingsReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            _refreshReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }

        public Form1()
        {
            InitializeComponent();
        }
        private void MarkStartup()
        {

            string path = Application.ExecutablePath;
            RegistryKey runKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            string[] keyValueNames = runKey.GetValueNames();

            foreach (string keyValueName in keyValueNames)

            {

                try
                {
                    if (keyValueName == "GGA")

                    {
                        RebootSystem.Checked = true;
                        // toolStripMenuItem2.Text = "取消随系统启动";
                        //  runKey.Close();
                        // runKey.DeleteValue("GGA");
                    }


                    else if (keyValueName != "GGA")

                    {
                        RebootSystem.Checked = false;
                        //  toolStripMenuItem2.Text = "随系统启动";
                        //  runKey.Close();
                        //  runKey.SetValue("GGA", path);

                    }
                }



                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
            runKey.Close();
        }




        private void SetStartup()
        {

            string path = Application.ExecutablePath;
            RegistryKey runKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            string[] keyValueNames = runKey.GetValueNames();

            foreach (string keyValueName in keyValueNames)

            {

                try
                {
                    if (keyValueName == "GGA")

                    {
                        //  runKey.Close();
                        runKey.DeleteValue("GGA");
                        //  toolStripMenuItem2.Text = "随系统启动";
                        RebootSystem.Checked = false;


                    }


                    else if (keyValueName != "GGA")

                    {
                        //  runKey.Close();
                        runKey.SetValue("GGA", path);
                        //   toolStripMenuItem2.Text = "取消随系统启动";
                        RebootSystem.Checked = true;

                    }
                }



                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
            runKey.Close();
        }

        private void RegularURL(string PacURL)


        {

            string p = @"(http|https)://(?<domain>[^(:|/]*)";
            Regex reg = new Regex(p, RegexOptions.IgnoreCase);
            Match m = reg.Match(PacURL);
            globalURL = m.Groups["domain"].Value;

            //  MessageBox.Show(Result);
        }



        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            // MessageBox.Show("Download completed!");

            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(3000, "更新提示:", "下载完成,请重启客户端", ToolTipIcon.Info);
        }
        private void CheckUpdate()

        {

            // version info from xml file  
            Version newVersion = null;
            // and in this variable we will put the url we  
            // would like to open so that the user can  
            // download the new version  
            // it can be a homepage or a direct  
            // link to zip/exe file  
            string url = "";
            XmlTextReader reader = null;
            try
            {
                // provide the XmlTextReader with the URL of  
                // our xml document  
                var StrxmlURL = BadouSquidClient.Properties.Resources.xmlURL;
                // MessageBox.Show(StrxmlURL);

                reader = new XmlTextReader(StrxmlURL);
                // simply (and easily) skip the junk at the beginning  
                reader.MoveToContent();
                // internal - as the XmlTextReader moves only  
                // forward, we save current xml element name  
                // in elementName variable. When we parse a  
                // text node, we refer to elementName to check  
                // what was the node name  
                string elementName = "";
                // we check if the xml starts with a proper  
                // "ourfancyapp" element node  
                if ((reader.NodeType == XmlNodeType.Element) &&
                    (reader.Name == "BadouAPP"))
                {
                    while (reader.Read())
                    {
                        // when we find an element node,  
                        // we remember its name  
                        if (reader.NodeType == XmlNodeType.Element)
                            elementName = reader.Name;
                        else
                        {
                            // for text nodes...  
                            if ((reader.NodeType == XmlNodeType.Text) &&
                                (reader.HasValue))
                            {
                                // we check what the name of the node was  
                                switch (elementName)
                                {
                                    case "version":
                                        // thats why we keep the version info  
                                        // in xxx.xxx.xxx.xxx format  
                                        // the Version class does the  
                                        // parsing for us  
                                        newVersion = new Version(reader.Value);
                                        break;
                                    case "url":
                                        url = reader.Value;
                                        break;

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            // compare the versions  
            if (curVersion.CompareTo(newVersion) < 0)
            {

                string message = "已经存在一个新的版本" + newVersion + ", 是否更新?";
                string caption = "更新提示";
                DialogResult result;
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                //  MessageBox.Show("new version is available");
                result = MessageBox.Show(message, caption, buttons);


                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    string path = System.Environment.CurrentDirectory;
                    // MessageBox.Show("为你下载新版");
                    string Downloadfilename = "巴豆Squid" + newVersion + ".exe";
                    var myStringWebResource = BadouSquidClient.Properties.Resources.myStringWebResource;

                    //System.Net.WebClient unreasonably slow 
                    //http://stackoverflow.com/questions/4415443/system-net-webclient-unreasonably-slow

                    WebClient webClient = new WebClient();
                    webClient.Proxy = null;
                    ServicePointManager.DefaultConnectionLimit = 25;
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    //  webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    webClient.DownloadFileAsync(new Uri(myStringWebResource), @path + "/" + Downloadfilename);


                }

                else

                {

                    Console.ReadLine();


                    //     MessageBox.Show(path);
                }


            }
            else

            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(3000, "版本提示:", "已经是最新版本", ToolTipIcon.Info);
            }



        }

        private void DeleteAdslSettting()
        {

            string starupPath = Application.ExecutablePath;
            //class Micosoft.Win32.RegistryKey. 表示Window注册表中项级节点,此类是注册表装.
            RegistryKey loca1 = Registry.CurrentUser;
            RegistryKey Adslrun = loca1.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Connections");

            //  RegistryKey subKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            string[] keyValueNames = Adslrun.GetValueNames();

            foreach (string keyValueName in keyValueNames)

            {

                try
                {
                    Adslrun.DeleteValue(keyValueName);

                }

                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void DeleteLANSettting()
        {
            RegistryKey loca = Registry.CurrentUser;
            RegistryKey run = loca.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            //  Software\Microsoft\\Windows\CurrentVersion\Internet Settings\Connections",



            string[] keyValueNames = run.GetValueNames();

            // run.Close();

            //bool result = false;

            foreach (string keyValueName in keyValueNames)

            {

                if (keyValueName == "ProxyServer")

                {
                    run.DeleteValue("ProxyServer");
                    // result = true;

                    break;

                }

                else

                {
                    Console.WriteLine("Test");

                }
                if (keyValueName == "AutoConfigURL")

                {
                    run.DeleteValue("AutoConfigURL");
                    break;

                }

                else

                {
                    Console.WriteLine("Test");
                }

            }


            try

            {
                run.SetValue("ProxyEnable", 0);
                loca.Close();
            }

            catch (Exception ee)

            {
                MessageBox.Show(ee.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private static void IEAutoDetectProxy(bool set)
        {
            RegistryKey registry =
                Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings\\Connections",
                    true);
            byte[] defConnection = (byte[])registry.GetValue("DefaultConnectionSettings");
            byte[] savedLegacySetting = (byte[])registry.GetValue("SavedLegacySettings");
            if (set)
            {
                defConnection[8] = Convert.ToByte(defConnection[8] & 8);
                savedLegacySetting[8] = Convert.ToByte(savedLegacySetting[8] & 8);
            }
            else
            {
                defConnection[8] = Convert.ToByte(defConnection[8] & ~8);
                savedLegacySetting[8] = Convert.ToByte(savedLegacySetting[8] & ~8);
            }
            registry.SetValue("DefaultConnectionSettings", defConnection);
            registry.SetValue("SavedLegacySettings", savedLegacySetting);
        }

        private void AdslSetSquidProxy(string SquidGobal)

        {
            RegistryKey loca = Registry.CurrentUser;
            RegistryKey run = loca.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            //  Software\Microsoft\\Windows\CurrentVersion\Internet Settings\Connections",


            try

            {
                run.SetValue("ProxyEnable", 1);
                run.SetValue("ProxyServer", SquidGobal + ":25");
                run.SetValue("AutoConfigURL", "");

                NotifyIE();
                //Must Notify IE first, or the connections do not chanage

                IEAutoDetectProxy(false);
                //Must Notify IE first, or the connections do not chanage
                CopyProxySettingFromLan();

            }

            catch (Exception ee)

            {
                MessageBox.Show(ee.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



        }


        private void SetSquidProxy()
        {
            RegistryKey loca = Registry.CurrentUser;
            RegistryKey run = loca.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            //  Software\Microsoft\\Windows\CurrentVersion\Internet Settings\Connections",


            try

            {
                var StrSquidServer1 = BadouSquidClient.Properties.Resources.SquidServer1;


                var StrxmlURL = BadouSquidClient.Properties.Resources.xmlURL;
                run.SetValue("ProxyEnable", 0);
                run.SetValue("ProxyServer", "");
                run.SetValue("AutoConfigURL", StrSquidServer1);

                NotifyIE();
                //Must Notify IE first, or the connections do not chanage

                IEAutoDetectProxy(false);
                //Must Notify IE first, or the connections do not chanage
                CopyProxySettingFromLan();

            }

            catch (Exception ee)

            {
                MessageBox.Show(ee.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



        }




        private static void CopyProxySettingFromLan()
        {
            RegistryKey registry =
                Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings\\Connections",
                    true);
            var defaultValue = registry.GetValue("DefaultConnectionSettings");
            try
            {
                var connections = registry.GetValueNames();
                foreach (String each in connections)
                {
                    if (!(each.Equals("DefaultConnectionSettings")
                        || each.Equals("LAN Connection")
                        || each.Equals("SavedLegacySettings")))
                    {
                        //set all the connections's proxy as the lan
                        registry.SetValue(each, defaultValue);
                    }
                }
                NotifyIE();
                //Must Notify IE first, or the connections do not chanage

            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetSquidProxy1(string ServerAdress)
        {
            RegistryKey loca = Registry.CurrentUser;
            RegistryKey run = loca.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings");
            //  Software\Microsoft\\Windows\CurrentVersion\Internet Settings\Connections",


            try

            {

                run.SetValue("ProxyEnable", 0);
                run.SetValue("ProxyServer", "");
                run.SetValue("AutoConfigURL", ServerAdress);

                NotifyIE();
                //Must Notify IE first, or the connections do not chanage

                IEAutoDetectProxy(false);
                //Must Notify IE first, or the connections do not chanage
                CopyProxySettingFromLan();

                //Must Notify IE first, or the connections do not chanage

                loca.Close();
            }

            catch (Exception ee)

            {
                MessageBox.Show(ee.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        public string ReadINI(string section, string key, string fileName)
        {
            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(section, key, "NullValue", temp, 1024, fileName);
            return temp.ToString();
        }



        void MenuClicked(object sender, EventArgs e)
        {

            //    MessageBox.Show(((sender as ToolStripMenuItem).Text));
            //   SetSquidProxy1((sender as ToolStripMenuItem).Text);
            string path = System.Environment.CurrentDirectory;
            string GlobalStatus = ReadINI("ProxyMode", "Global", @path + "\\" + "SquidConfig.ini");
            string SmartStatus = ReadINI("ProxyMode", "Smart", @path + "\\" + "SquidConfig.ini");

            if (GlobalStatus == "1")

            {
                PacURL = ((sender as ToolStripMenuItem).Text);

                RegularURL(PacURL);

                AdslSetSquidProxy(globalURL);

                notifyIcon1.Text = "巴豆 V2.0.0.11" + Environment.NewLine + "代理模式: 全局代理" + Environment.NewLine + "服务器:" + globalURL;
            }
            else if (SmartStatus == "1")

            {
            //    MessageBox.Show(((sender as ToolStripMenuItem).Text));
                SetSquidProxy1((sender as ToolStripMenuItem).Text);


                notifyIcon1.Text = "巴豆 V2.0.0.11" + Environment.NewLine + "代理模式: 智能代理" + Environment.NewLine + "服务器:" + ((sender as ToolStripMenuItem).Text); 
            }

            IniWriteValue("DefaultStartup", "Server", ((sender as ToolStripMenuItem).Text));


   

        }

        void GlobalWriteInIClicked(object sender, EventArgs e)

        {
            IniWriteValue("ProxyMode", "Global", "1");
            IniWriteValue("ProxyMode", "Smart", "0");

            //   SetSquidProxy1((sender as ToolStripMenuItem).Text);
            string path = System.Environment.CurrentDirectory;
            string GlobalStatus = ReadINI("ProxyMode", "Global", @path + "\\" + "SquidConfig.ini");
            string SmartStatus = ReadINI("ProxyMode", "Smart", @path + "\\" + "SquidConfig.ini");

            if (GlobalStatus == "1")

            {
                string DefaultServer = ReadINI("DefaultStartup", "Server", @path + "\\" + "SquidConfig.ini");

                string PacURL = DefaultServer;

                RegularURL(PacURL);

                AdslSetSquidProxy(globalURL);

                notifyIcon1.Text = "巴豆 V2.0.0.11" + Environment.NewLine + "代理模式: 全局" + Environment.NewLine + "服务器:" + globalURL;

            }
            else if (SmartStatus == "1")

            {
                string DefaultServer = ReadINI("DefaultStartup", "Server", @path + "\\" + "SquidConfig.ini");

                SetSquidProxy1(DefaultServer);

                notifyIcon1.Text = "巴豆 V2.0.0.11" + Environment.NewLine + "代理模式: 智能" + Environment.NewLine + "服务器:" + DefaultServer;
            }

       
        }

        void SmartWriteINIClicked(object sender, EventArgs e)

        {

            IniWriteValue("ProxyMode", "Global", "0");
            IniWriteValue("ProxyMode", "Smart", "1");

            //   SetSquidProxy1((sender as ToolStripMenuItem).Text);
            string path = System.Environment.CurrentDirectory;
            string GlobalStatus = ReadINI("ProxyMode", "Global", @path + "\\" + "SquidConfig.ini");
            string SmartStatus = ReadINI("ProxyMode", "Smart", @path + "\\" + "SquidConfig.ini");

            if (GlobalStatus == "1")

            {
                string DefaultServer = ReadINI("DefaultStartup", "Server", @path + "\\" + "SquidConfig.ini");

                string PacURL = DefaultServer;

                RegularURL(PacURL);

                AdslSetSquidProxy(globalURL);
                notifyIcon1.Text = "巴豆 V2.0.0.11" + Environment.NewLine + "代理模式: 全局" + Environment.NewLine + "服务器:" + globalURL;

            }
            else if (SmartStatus == "1")

            {

                string DefaultServer = ReadINI("DefaultStartup", "Server", @path + "\\" + "SquidConfig.ini");

                SetSquidProxy1(DefaultServer);

                notifyIcon1.Text = "巴豆 V2.0.0.11" + Environment.NewLine + "代理模式: 智能" + Environment.NewLine + "服务器:" + DefaultServer;
            }


        }



        void LoadSetting(object sender, EventArgs e)
        {


            contextMenuStrip1.Items.Clear();


            //添加菜单一 
            ToolStripMenuItem AboutME;
            AboutME = AddContextMenu("关于", contextMenuStrip1.Items, null);

            AboutME.Click += AboutMe_Click;






            ToolStripMenuItem StartupSystem;
            StartupSystem = AddContextMenu("随系统启动", contextMenuStrip1.Items, null);

            StartupSystem.Click += RebootSystem_Click;


            ToolStripMenuItem CheckedUpdate;
            CheckedUpdate = AddContextMenu("检查更新", contextMenuStrip1.Items, null);


            CheckedUpdate.Click += CheckUpdate_Click;


            ToolStripMenuItem ProxyMode;
            ProxyMode = AddContextMenu("代理模式", contextMenuStrip1.Items, null);
            AddContextMenu("全局代理", ProxyMode.DropDownItems, new EventHandler(GlobalWriteInIClicked));
            AddContextMenu("智能代理", ProxyMode.DropDownItems, new EventHandler(SmartWriteINIClicked));



            //添加菜单一 
            ToolStripMenuItem subItem;
            subItem = AddContextMenu("服务器", contextMenuStrip1.Items, null);



            //ToolStripMenuItem subItem2;
            //subItem2 = AddContextMenu("重新加载配置", contextMenuStrip1.Items, new EventHandler(LoadSetting));



            ToolStripMenuItem subItem1;
            subItem1 = AddContextMenu("-", contextMenuStrip1.Items, null);

            subItem1 = AddContextMenu("退出", contextMenuStrip1.Items, null);



            subItem1.Click += tested;




            string path = System.Environment.CurrentDirectory;

            string ServerindexString = ReadINI("DefaultStartup", "index", path + "\\" + "SquidConfig.ini");

            //  MessageBox.Show(ServerindexString);

            int ServerindexInt = int.Parse(ServerindexString);

            int j = 0;

            for (int i = 0; i < ServerindexInt; i++)
            {


                j = j + 1;
                string ServerAddress = ReadINI("User", "Server" + j.ToString(), path + "\\" + "SquidConfig.ini");

                //   MessageBox.Show(ServerAddress);

                //   EditServer.listBox1.Items.Add(ServerAddress);

                AddContextMenu(ServerAddress, subItem.DropDownItems, new EventHandler(MenuClicked));




            }

            AddContextMenu("编辑服务器", subItem.DropDownItems, new EventHandler(EditServer));

            //   contextMenuStrip1.Items.Clear();
            //  contextMenuStrip1.Items.Clear();

            // subItem.DropDownItems.Clear();
            //   SetSquidProxy1((sender as ToolStripMenuItem).Text);

            string GlobalStatus = ReadINI("ProxyMode", "Global", @path + "\\" + "SquidConfig.ini");
            string SmartStatus = ReadINI("ProxyMode", "Smart", @path + "\\" + "SquidConfig.ini");

            if (GlobalStatus == "1")

            {
                string DefaultServer = ReadINI("DefaultStartup", "Server", @path + "\\" + "SquidConfig.ini");

                string PacURL = DefaultServer;

                RegularURL(PacURL);

                AdslSetSquidProxy(globalURL);

                notifyIcon1.Text = "巴豆 V2.0.0.11" + Environment.NewLine + "代理模式: 全局" + Environment.NewLine + "服务器:" + globalURL;

            }
            else if (SmartStatus == "1")

            {

                string DefaultServer = ReadINI("DefaultStartup", "Server", @path + "\\" + "SquidConfig.ini");

                SetSquidProxy1(DefaultServer);

                notifyIcon1.Text = "巴豆 V2.0.0.11" + Environment.NewLine + "代理模式: 智能" + Environment.NewLine + "服务器:" + DefaultServer;
            }




        }

        public void IniWriteValue(string Section, string Key, string Value)
        {


            //  MessageBox.Show(inipath);
            string path = System.Environment.CurrentDirectory;
            inipath = path + "/" + "SquidConfig.ini";
            WritePrivateProfileString(Section, Key, Value, inipath);
        }





        public void prikazi()
        {
            MessageBox.Show("ok");
        }


        public void LoadServerMenu()

        {



            ToolStripMenuItem ProxyMode;
            ProxyMode = AddContextMenu("代理模式", contextMenuStrip1.Items, null);
            AddContextMenu("全局代理", ProxyMode.DropDownItems, new EventHandler(GlobalWriteInIClicked));
            AddContextMenu("智能代理", ProxyMode.DropDownItems, new EventHandler(SmartWriteINIClicked));


            //添加菜单一 
            ToolStripMenuItem subItem;
            subItem = AddContextMenu("服务器", contextMenuStrip1.Items, null);





            string path = System.Environment.CurrentDirectory;

            string ServerindexString = ReadINI("DefaultStartup", "index", path + "\\" + "SquidConfig.ini");

            //  MessageBox.Show(ServerindexString);

            int ServerindexInt = int.Parse(ServerindexString);

            int j = 0;

            for (int i = 0; i < ServerindexInt; i++)
            {


                j = j + 1;
                string ServerAddress = ReadINI("User", "Server" + j.ToString(), path + "\\" + "SquidConfig.ini");

                //   MessageBox.Show(ServerAddress);

                //   EditServer.listBox1.Items.Add(ServerAddress);

                AddContextMenu(ServerAddress, subItem.DropDownItems, new EventHandler(MenuClicked));




            }

            AddContextMenu("编辑服务器", subItem.DropDownItems, new EventHandler(EditServer));

            //   contextMenuStrip1.Items.Clear();
            //  contextMenuStrip1.Items.Clear();

            // subItem.DropDownItems.Clear();

            string GlobalStatus = ReadINI("ProxyMode", "Global", @path + "\\" + "SquidConfig.ini");
            string SmartStatus = ReadINI("ProxyMode", "Smart", @path + "\\" + "SquidConfig.ini");

            if (GlobalStatus == "1")

            {
                string DefaultServer = ReadINI("DefaultStartup", "Server", @path + "\\" + "SquidConfig.ini");

                string PacURL = DefaultServer;

                RegularURL(PacURL);

                AdslSetSquidProxy(globalURL);

                notifyIcon1.Text = "巴豆 V2.0.0.11" + Environment.NewLine + "代理模式: 全局" + Environment.NewLine + "服务器:"+globalURL;
            }
            else if (SmartStatus == "1")

            {

                string DefaultServer = ReadINI("DefaultStartup", "Server", @path + "\\" + "SquidConfig.ini");

                SetSquidProxy1(DefaultServer);

                notifyIcon1.Text = "巴豆 V2.0.0.11" + Environment.NewLine + "代理模式: 智能" + Environment.NewLine + "服务器:" +  DefaultServer;
            }



        }

        void EditServer(object sender, EventArgs e)
        {
            //EditServer form = new EditServer();
            //form.Show();

            EditServer form2 = new EditServer();

         //   form2.DisableButton += new EventHandler(form2_DisableButton);

            form2.DisableButton += new EventHandler(LoadSetting);

            form2.ShowDialog();

        }


        void tested(object sender, EventArgs e)
        {

            DeleteAdslSettting();
            notifyIcon1.Icon = null;
            notifyIcon1.Dispose();
            Application.DoEvents();
            System.Environment.Exit(0);

        }


        ToolStripMenuItem AddContextMenu(string text, ToolStripItemCollection cms, EventHandler callback)
        {
            if (text == "-")
            {
                ToolStripSeparator tsp = new ToolStripSeparator();
                cms.Add(tsp);

                return null;
            }
            else if (!string.IsNullOrEmpty(text))
            {
                ToolStripMenuItem tsmi = new ToolStripMenuItem(text);
                if (callback != null) tsmi.Click += callback;
                cms.Add(tsmi);

                return tsmi;
            }

            return null;
        }

        public void ReadSections()
        {

            string path = System.Environment.CurrentDirectory;

            string ServerindexString = @path + "\\" + "SquidConfig.ini";


            if (File.Exists(ServerindexString))

            {
             //   MessageBox.Show("存在ini");

                byte[] buffer = new byte[65535];
                int rel = GetPrivateProfileSectionNamesA(buffer, buffer.GetUpperBound(0), ServerindexString);
                int iCnt, iPos;
                System.Collections.ArrayList arrayList = new ArrayList();
                string tmp;
                if (rel > 0)
                {
                    iCnt = 0; iPos = 0;
                    for (iCnt = 0; iCnt < rel; iCnt++)
                    {
                        if (buffer[iCnt] == 0x00)
                        {
                            tmp = System.Text.ASCIIEncoding.Default.GetString(buffer, iPos, iCnt - iPos).Trim();
                            iPos = iCnt + 1;
                            if (tmp != "")
                                arrayList.Add(tmp);
                            // MessageBox.Show(tmp);
                        }
                    }
                }


                //foreach (string test in arrayList)
                //{

                //        MessageBox.Show(test);

                //}

                bool exists = ((IList)arrayList).Contains("User");
                if (exists)
                {
                   // MessageBox.Show("exists");

                }
                // 存在
                else
                // 不存在
                {
                    //  MessageBox.Show("no exists");


                    EditServer form2 = new EditServer();

                    //   form2.DisableButton += new EventHandler(form2_DisableButton);

                    form2.DisableButton += new EventHandler(LoadSetting);

                    form2.ShowDialog();
                }

            }

            else

            {
              //  MessageBox.Show("不存在ini");

                IniWriteValue("DefaultStartup", "index", "0");
                IniWriteValue("ProxyMode", "Global", "0");
                IniWriteValue("ProxyMode", "Smart", "1");
                

                EditServer form2 = new EditServer();

                //   form2.DisableButton += new EventHandler(form2_DisableButton);

                form2.DisableButton += new EventHandler(LoadSetting);

                form2.ShowDialog();

            }

          
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            ReadSections();

            LoadServerMenu();// LoadServerMenu


            //ToolStripMenuItem subItem2;
            //subItem2 = AddContextMenu("重新加载配置", contextMenuStrip1.Items, new EventHandler(LoadSetting));


            ToolStripMenuItem subItem1;
            subItem1 = AddContextMenu("-", contextMenuStrip1.Items, null);

            subItem1 = AddContextMenu("退出", contextMenuStrip1.Items, null);



            subItem1.Click += tested;

            // setting up proxy
            //  SetSquidProxy1();
            //  Server2.Checked = true;


            // setting up proxy

            //  Server1.Checked = false;



            this.Hide();
            this.ShowInTaskbar = false;

            //Check Latest Version 
            CheckUpdate();
            //Check Latest Version 

            FormBorderStyle = FormBorderStyle.FixedDialog;

            //mark the Menu while reboot status
            MarkStartup();


            //loading string which intrduce squid Technology
            var squid = BadouSquidClient.Properties.Resources.test;
            richTextBox1.Text = squid;



        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteAdslSettting();
            DeleteLANSettting();
            notifyIcon1.Icon = null;
            notifyIcon1.Dispose();
            Application.DoEvents();
            System.Environment.Exit(0);
        }

        private void RebootSystem_Click(object sender, EventArgs e)
        {
            SetStartup();
        }



        private void CheckUpdate_Click(object sender, EventArgs e)
        {
            CheckUpdate();
        }



        private void AboutMe_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.notifyIcon1.Visible = true;

            // FadeOut(this, 0);
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
        }


        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText); // call default browser  
        }

        private void button1_Click(object sender, EventArgs e)
        {
            EditServer frm = new EditServer();
            frm.Show();

        }

    

        private void button1_Click_2(object sender, EventArgs e)
        {

        }

        private void button1_Click_3(object sender, EventArgs e)
        {
      

         }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }





    }



}

