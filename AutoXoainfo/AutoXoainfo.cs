using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// Dùng thêm
using AutoXoainfo.Models;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace AutoXoainfo
{
    public partial class AutoXoainfo : Form
    {
        public AutoXoainfo()
        {
            InitializeComponent();
        }
        #region Các Biến Dùng Chung
        public bool isStop;
        public string appPath = Directory.GetCurrentDirectory();
        public string ip, username = "root", pass = "alpine",XoaInfoExe = null;
        public string[] ips;
        //List<Thread> listtask = new List<Thread>();
        Dictionary<string,Thread> listtask = new Dictionary<string, Thread>();
        public List<string> ipss = new List<string>();
        public List<string> ports = new List<string>();
        public List<string> membsid = new List<string>();
        public List<string> listport=new List<string>();
        public int port = 22, numdevices, resping=1;
        public long ketquacheckproxytimer;
        ContextMenu cm = new ContextMenu();

        #endregion

        private void AutoXoainfo_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            cm.MenuItems.Add(new MenuItem("&Start", new System.EventHandler(this.PreStartPerThread)));
            cm.MenuItems.Add(new MenuItem("&Stop", new System.EventHandler(this.PreStopPerThread)));
            cm.MenuItems.Add(new MenuItem("Bii"));
           

            var MyIni = new IniFile(appPath + "\\CONFIG\\Setting.ini");

            if (!File.Exists(appPath + "\\CONFIG\\ListDevices.txt"))
            {
                Directory.CreateDirectory(appPath + "\\CONFIG");
                System.IO.File.WriteAllText(appPath + "\\CONFIG\\ListDevices.txt", "");
            }

            if (!File.Exists(appPath + "\\CONFIG\\ListMembID.txt"))
            {
                Directory.CreateDirectory(appPath + "\\CONFIG");
                System.IO.File.WriteAllText(appPath + "\\CONFIG\\ListMembID.txt", "");
            }

            if (!File.Exists(appPath + "\\CONFIG\\ListPortFake.txt"))
            {
                Directory.CreateDirectory(appPath + "\\CONFIG");
                System.IO.File.WriteAllText(appPath + "\\CONFIG\\ListPortFake.txt", "");
            }

            if (!File.Exists(appPath + "\\CONFIG\\Setting.ini"))
            {
                Directory.CreateDirectory(appPath + "\\CONFIG");
                System.IO.File.WriteAllText(appPath + "\\CONFIG\\Setting.ini", "");
            }

            if (File.Exists(appPath + "\\log\\log.txt"))
            {
                Directory.Delete(appPath + "\\log", true);
            }

            richTextBoxListIP.Text = File.ReadAllText(appPath + "\\CONFIG\\ListDevices.txt");
            richTextBoxMembID.Text = File.ReadAllText(appPath + "\\CONFIG\\ListMembID.txt");
            richTextBoxPortFake.Text = File.ReadAllText(appPath + "\\CONFIG\\ListPortFake.txt");

            if (MyIni.Read("RadioLuuRRS") == "True") radioButtonLuuRRS.Checked = true;
            if (MyIni.Read("RadioResetData") == "True") radioButtonResetData.Checked = true;
            if (MyIni.Read("RadioRestoreRRS") == "True") radioButtonRestoreRRS.Checked = true;
            if (MyIni.Read("RadioMicro") == "True") radioButtonMicro.Checked = true;
            if (MyIni.Read("RadioLumi") == "True") radioButtonLumi.Checked = true;
            if (MyIni.Read("RadioSSH") == "True") radioButtonSSH.Checked = true;
            if (MyIni.Read("RadioGift") == "True") radioButtonGift.Checked = true;
            if (MyIni.Read("RadioCheckFake") == "True") radioButtonCheckFake.Checked = true;
            if (MyIni.Read("RadioXXT") == "True") radioButtonXXT.Checked = true;
            if (MyIni.Read("RadioTS") == "True") radioButtonTS.Checked = true;
            if (MyIni.Read("RadioATT") == "True") radioButtonATT.Checked = true;
            if (MyIni.Read("RadioSock5") == "True") radioButtonSocks5.Checked = true;
            if (MyIni.Read("RadioProxy") == "True") radioButtonProxy.Checked = true;
            if (MyIni.Read("RadioDirectAppStore") == "True") radioButtonDirectAppstore.Checked = true;
            if (MyIni.Read("RadioProxyAppStore") == "True") radioButtonProxyAppStore.Checked = true;

            textBoxLinkOffer.Text = MyIni.Read("LinkOffer");
            textBoxIphoneTimer.Text = MyIni.Read("IphoneTimer");
            textBoxPortIphoneTimer.Text = MyIni.Read("PortIphoneTimer");
            textBoxServerData.Text = MyIni.Read("ServerData");
            textBoxTimeXoaInfo.Text = MyIni.Read("TimeXoaInfo");
            textBoxTimeDiLink.Text = MyIni.Read("TimeDiLink");
            textBoxTimeCheckFake.Text = MyIni.Read("TimeCheckFake");
            textBoxTimeAppStore.Text = MyIni.Read("TimeAppStore");
            textBoxTimeRandomTouch.Text = MyIni.Read("TimeRandomTouch");
            textBoxThoiGianVang.Text = MyIni.Read("ThoiGianVang");
            textBoxIpFake.Text = MyIni.Read("IpAddressSock");
            textBoxProxyAppstore.Text = MyIni.Read("ProxyAppStore");

            clearspace();

            Thread.Sleep(200);
            if (!File.Exists(appPath + "\\log\\log.txt"))
            {
                Directory.CreateDirectory(appPath + "\\log");
                System.IO.File.WriteAllText(appPath + "\\log\\log.txt", "");
            }


        }

        private void clearspace()
        {
            richTextBoxListIP.Text = Regex.Replace(richTextBoxListIP.Text, @"^\s*$(\n|\r|\r\n)", "", RegexOptions.Multiline).TrimEnd();
            richTextBoxMembID.Text = Regex.Replace(richTextBoxMembID.Text, @"^\s*$(\n|\r|\r\n)", "", RegexOptions.Multiline).TrimEnd();
            richTextBoxPortFake.Text = Regex.Replace(richTextBoxPortFake.Text, @"^\s*$(\n|\r|\r\n)", "", RegexOptions.Multiline).TrimEnd();
        }
        
        private void save()
        {
            clearspace();

            File.WriteAllText(appPath + "\\CONFIG\\ListDevices.txt", richTextBoxListIP.Text);
            File.WriteAllText(appPath + "\\CONFIG\\ListMembID.txt", richTextBoxMembID.Text);
            File.WriteAllText(appPath + "\\CONFIG\\ListPortFake.txt", richTextBoxPortFake.Text);
            // Creates or loads an INI file in the same directory as your executable
            // Or specify a specific name in a specific dir
            var MyIni = new IniFile(appPath + "\\CONFIG\\Setting.ini");

            MyIni.Write("LinkOffer", textBoxLinkOffer.Text);
            MyIni.Write("IphoneTimer", textBoxIphoneTimer.Text);
            MyIni.Write("PortIphoneTimer", textBoxPortIphoneTimer.Text);
            MyIni.Write("ServerData", textBoxServerData.Text);
            MyIni.Write("TimeXoaInfo", textBoxTimeXoaInfo.Text);
            MyIni.Write("TimeDiLink", textBoxTimeDiLink.Text);
            MyIni.Write("TimeCheckFake", textBoxTimeCheckFake.Text);
            MyIni.Write("TimeAppStore", textBoxTimeAppStore.Text);
            MyIni.Write("TimeRandomTouch", textBoxTimeRandomTouch.Text);
            MyIni.Write("ThoiGianVang", textBoxThoiGianVang.Text);
            MyIni.Write("IpAddressSock", textBoxIpFake.Text);
            MyIni.Write("ProxyAppStore", textBoxProxyAppstore.Text);

            if (radioButtonLuuRRS.Checked == true) MyIni.Write("RadioLuuRRS", "True"); else MyIni.Write("RadioLuuRRS", "False");
            if (radioButtonResetData.Checked == true) MyIni.Write("RadioResetData", "True"); else MyIni.Write("RadioResetData", "False");
            if (radioButtonRestoreRRS.Checked == true) MyIni.Write("RadioRestoreRRS", "True"); else MyIni.Write("RadioRestoreRRS", "False");
            if (radioButtonMicro.Checked == true) MyIni.Write("RadioMicro", "True"); else MyIni.Write("RadioMicro", "False");
            if (radioButtonLumi.Checked == true) MyIni.Write("RadioLumi", "True"); else MyIni.Write("RadioLumi", "False");
            if (radioButtonSSH.Checked == true) MyIni.Write("RadioSSH", "True"); else MyIni.Write("RadioSSH", "False");
            if (radioButtonGift.Checked == true) MyIni.Write("RadioGift", "True"); else MyIni.Write("RadioGift", "False");
            if (radioButtonCheckFake.Checked == true) MyIni.Write("RadioCheckFake", "True"); else MyIni.Write("RadioCheckFake", "False");
            if (radioButtonXXT.Checked == true) MyIni.Write("RadioXXT", "True"); else MyIni.Write("RadioXXT", "False");
            if (radioButtonTS.Checked == true) MyIni.Write("RadioTS", "True"); else MyIni.Write("RadioTS", "False");
            if (radioButtonATT.Checked == true) MyIni.Write("RadioATT", "True"); else MyIni.Write("RadioATT", "False");
            if (radioButtonSocks5.Checked == true) MyIni.Write("RadioSock5", "True"); else MyIni.Write("RadioSock5", "False");
            if (radioButtonProxy.Checked == true) MyIni.Write("RadioProxy", "True"); else MyIni.Write("RadioProxy", "False");
            if (radioButtonDirectAppstore.Checked == true) MyIni.Write("RadioDirectAppStore", "True"); else MyIni.Write("RadioDirectStore", "False");
            if (radioButtonProxyAppStore.Checked == true) MyIni.Write("RadioProxyAppStore", "True"); else MyIni.Write("RadioProxyAppStore", "False");
        }

        private void writestatus(int rows, string ip, string messeage)
        {
            Action action1 = () => dataGridViewSTTIphone.Rows[rows].SetValues(ip);
            Action action2 = () => dataGridViewSTTIphone.Rows[rows].Cells[1].Value = messeage;
            Action action3 = () => dataGridViewSTTIphone.Rows[rows].DefaultCellStyle.BackColor = Color.Empty;
            this.Invoke(action1);
            this.Invoke(action2);
            this.Invoke(action3);
        }
        
        private void WriteSttHL(int rows, string ip, string messeage)
        {
            Action action1 = () => dataGridViewSTTIphone.Rows[rows].SetValues(ip);
            Action action2 = () => dataGridViewSTTIphone.Rows[rows].Cells[1].Value = messeage;
            Action action3 = () => dataGridViewSTTIphone.Rows[rows].DefaultCellStyle.BackColor = Color.Red;
            this.Invoke(action1);
            this.Invoke(action2);
            this.Invoke(action3);
        }

   
        private bool sendcmd(string ip, string cmd, int row)
        {
            try
            {
                SshClient cSSH = new SshClient(ip, port, username, pass);
                writestatus(row, ip, "Đang Kết Nối");
                cSSH.Connect();
                writestatus(row, ip, "Chuẩn Bị Gửi Lệnh");
                SshCommand x = cSSH.RunCommand(cmd);
                writestatus(row, ip, cmd);
                cSSH.Disconnect();
                cSSH.Dispose();
                writestatus(row, ip, "Xong");
                return true;
            }
            catch (Exception ex)
            {
                WriteSttHL(row, ip, $"Lỗi Send Cmd : {ex.Message}");
                return false;
            }
        }

        private bool sendcmdrsync(int row,string ip,string cmd)
        {
            try
            {
                SshClient ssh = new SshClient(ip, 22, username, pass);
                ssh.Connect();
                if (ssh.IsConnected)
                {
                    var promptRegex = new Regex(@"\][#$>]"); // regular expression for matching terminal prompt
                    var modes = new Dictionary<TerminalModes, uint>();
                    using (var stream = ssh.CreateShellStream("xterm", 255, 50, 800, 600, 1024, modes))
                    {
                        TimeSpan timeout = new TimeSpan(0, 0, 2, 0);// (ngày,giờ,phút,giây)

                        stream.Write(cmd + "\n");
                        Thread.Sleep(2000);
                        var a = stream.Read();
                        var b = a.Contains("yes");
                        var c = a.Contains("password");
                        if (b == true)
                        {
                            stream.Write("yes\n");
                            Thread.Sleep(2000);
                            stream.Write("X0@inf0\n");
                        }
                        if (c == true) stream.Write("X0@inf0\n");

                        // var output = stream.Expect(promptRegex, timeout);
                        ssh.Disconnect();
                        ssh.Dispose();
                    }
                }
            }//try to open connection
            catch (Exception ex)
            {
                WriteSttHL(row,ip,ex.ToString());
                throw ex;
            }
            return false;
        }

        private void CountThread()
        {
            try
            {
                ipss.Clear();
                ports.Clear();
                numdevices = 0;
                string[] ips = { };
                Action action1 = () => dataGridViewSTTIphone.ClearSelection();
                this.Invoke(action1);

                Action action2 = () => richTextBoxListIP.Text = Regex.Replace(richTextBoxListIP.Text, @"^\s*$(\n|\r|\r\n)", "", RegexOptions.Multiline).TrimEnd();
                this.Invoke(action2);

                ipss.AddRange(richTextBoxListIP.Lines);
                ports.AddRange(richTextBoxPortFake.Lines);
                membsid.AddRange(richTextBoxMembID.Lines);
                listport.AddRange(richTextBoxPortFake.Lines);
                numdevices = richTextBoxListIP.Lines.Count();
                ips = richTextBoxListIP.Lines.ToArray();

                Action action3 = () => dataGridViewSTTIphone.RowCount = numdevices;
                this.Invoke(action3);
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi checkthread rồi gái. Lý do : {ex.Message}"); }
        }

        private void clearAuto(int row,string ip)
        {
            try
            {
                writestatus(row, ip, "Chuẩn bị chạy");
                stopfilelua(ip);

                if (checkBoxDeleteRRS.Checked == true) sendCommandToiOSWeb(ip, "PCAutoD -command find /private/var/root/Library/XoaInfo/ -empty -type d -delete");

                if (checkBoxAutoRespring.Checked == true)
                {
                    int looptime = 20;
                    if (resping == 4)
                    {
                        sendCommandToiOSWeb(ip, "PCAutoD -k TSDaemon backboardd");
                        Thread.Sleep(10000);
                        resping = 1;
                        do
                        {
                            sendCommandToiOSWeb(ip, "PCAutoD -unlock");
                            looptime = looptime - 1;
                            Thread.Sleep(1000);
                        } while (looptime > 1);
                    }
                    else resping = resping + 1;
                }

                if (Checkport(ip, 34789) == true)
                {
                    sendCommandToiOSWeb(ip, "PCAutoD -rm /private/var/mobile/Media/1ferver/lua/scripts/AutoTouchDone");
                    sendCommandToiOSWeb(ip, "PCAutoD -rm /private/var/mobile/Media/1ferver/lua/scripts/AutoTouchDontLead");
                    sendCommandToiOSWeb(ip, "PCAutoD -rm /private/var/mobile/Media/1ferver/lua/scripts/AutoTouchError");
                    sendCommandToiOSWeb(ip, "PCAutoD -rm /private/var/mobile/Media/1ferver/lua/scripts/AutoTouchOpenApp");
                    sendCommandToiOSWeb(ip, "PCAutoD -rm /private/var/mobile/Media/1ferver/lua/scripts/CheckFakeSaiTT");
                    sendCommandToiOSWeb(ip, "PCAutoD -rm /private/var/mobile/Media/1ferver/lua/scripts/CheckFakeQuaStore");
                    sendCommandToiOSWeb(ip, "PCAutoD -rm /private/var/mobile/Media/1ferver/lua/scripts/AutoDownloadNotYet");
                    sendCommandToiOSWeb(ip, "PCAutoD -rm /private/var/mobile/Media/1ferver/lua/scripts/AutoDownloadDone");
                    sendCommandToiOSWeb(ip, "PCAutoD -rm /private/var/mobile/Media/1ferver/lua/scripts/AutoDownloading");
                    sendCommandToiOSWeb(ip, "PCAutoD -rm /private/var/mobile/Media/1ferver/lua/scripts/AutoDownloadNOFree");
                }
            }
            catch (ThreadAbortException ex)
            {
                WriteSttHL(row, ip, $"Lỗi clearAuto : {ex.Message}");
            }
            catch (Exception ex)
            {
                WriteSttHL(row, ip, $"Lỗi clearAuto : {ex.Message}");
            }
        }

        //public string HttpSendData(int row, string webserver, int port, string method, string pathURL, string data)
        //{
        //    string URL = $"http://{webserver}:{port}";
        //    writestatus(row, webserver, "Requestting");
        //    try
        //    {
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
        //        request.Method = method;
        //        request.ContentType = "application/json";
        //        request.ContentLength = data.Length;
        //        StreamWriter requestWriter = new StreamWriter(request.GetRequestStream(),System.Text.Encoding.ASCII);
        //        requestWriter.Write(data);
        //        requestWriter.Close();
        //
        //
        //        // get the response
        //        WebResponse webResponse = request.GetResponse();
        //        Stream webStream = webResponse.GetResponseStream();
        //        StreamReader responseReader = new StreamReader(webStream);
        //        string response = responseReader.ReadToEnd();
        //        responseReader.Close();
        //        writestatus(row, webserver, "Request Done");
        //    }
        //    catch (WebException we)
        //    {
        //        string webExceptionMessage = we.Message;
        //        writestatus(row, webserver, webExceptionMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        // no need to do anything special here....
        //        writestatus(row, webserver, ex.Message);
        //    }
        //    return null;
        //}
        
        private string httpsend(string address, string method, string myJson)
        {
            try
            {
                if (method=="GET")
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    request.Timeout = 6000;

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var result = reader.ReadToEnd();
                        reader.Dispose();
                        reader.Close();
                        stream.Dispose();
                        stream.Close();
                        response.Dispose();
                        response.Close();
                        request.Abort();
                        return result;
                    }
                }

                if (method == "POST")
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(address);
                    //httpWebRequest.Headers.Add("application/json");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.KeepAlive = false;
                    httpWebRequest.Method = method;
                    httpWebRequest.Timeout = 6000;

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        var json = myJson;
                        streamWriter.Write(json);
                        streamWriter.Dispose();
                        streamWriter.Close();
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        string result = streamReader.ReadToEnd();
                        streamReader.Dispose();
                        streamReader.Close();
                        httpResponse.Dispose();
                        httpResponse.Close();
                        httpWebRequest.Abort();
                        //string ketQua = JsonConvert.DeserializeObject<string>(result);
                        //Login = ketqua;
                        Console.Write($"{myJson} {result}\n");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Lỗi Lý do:{ex.Message}");
                Console.Write($"Lỗi lý do : {ex.Message}" + "\n");
                return "false";
            }
            return "false";
        }

        private string httpsend2(string address, string method, string myJson)
        {
            try
            {
                if (method == "GET")
                {
                    string url = $"";
                    string jsonstr = myJson;
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(address);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = client.GetAsync(url).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            jsonstr = response.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            DebugLog(address,"Kết nối đến service thất bại");
                        }
                    }
                    if (jsonstr != "")
                    {
                        return jsonstr;
                    }
                }

                if (method == "POST")
                {
                    string url = "";
                    string ketQua = null;
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(address);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
                        var response = client.PostAsJsonAsync(url, myJson).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            ketQua = response.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            DebugLog(address, "Kết nối đến service thất bại");
                        }
                    }
                    if (ketQua != null)
                    {
                            return ketQua;
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Lỗi Lý do:{ex.Message}");
                Console.Write($"Lỗi lý do : {ex.Message}" + "\n");
                return "false";
            }
            return "false";
        }

        private string httpsend3(string address, string method, string myJson)
        {
            try
            {
                if (method == "GET")
                {
                    string url = $"";
                    string jsonstr = myJson;
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(address);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Add("Connection", "close");
                        var response = client.GetAsync(url).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            jsonstr = response.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            DebugLog(address, "Kết nối đến service thất bại");
                        }
                    }
                    if (jsonstr != "")
                    {
                        return jsonstr;
                    }
                }

                if (method == "POST")
                {
                    string url = "";
                    string ketQua = null;
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri(address);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
                    client.DefaultRequestHeaders.Add("Connection", "close");

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Content = new StringContent(myJson, Encoding.UTF8,"application/json");//CONTENT-TYPE header

                    //client.SendAsync(request)
                    //      .ContinueWith(responseTask =>
                    //      {
                    //          Console.WriteLine("Response: {0}", responseTask.Result);
                    //      });

                    ketQua = client.SendAsync(request).Result.ReasonPhrase.ToString();
                    if (ketQua != null) return ketQua;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Lỗi Lý do:{ex.Message}");
                Console.Write($"Lỗi lý do : {ex.Message}" + "\n");
                return "false";
            }
            return "false";
        }

        private string httpsend4(string address, string method, string myJson)
        {
            try
            {
                if (method == "GET")
                {
                    xNet.HttpRequest client = new xNet.HttpRequest();
                    string respone = client.Get(address).ToString();
                    if (!string.IsNullOrEmpty(respone)) return respone; else return "false";
                }


                if (method == "POST")
                {
                    xNet.HttpRequest client = new xNet.HttpRequest();
                    
                    //client.AddHeader("Connection", "close");
                    string respone = client.Post(address,myJson, "application/x-www-form-urlencoded; charset=UTF-8").ToString();
                    return respone;
                }

                }
            catch (Exception ex)
            {
                //MessageBox.Show($"Lỗi Lý do:{ex.Message}");
                Console.Write($"Lỗi lý do : {ex.Message}" + "\n");
                return "false";
            }
            return "false";
        }
        
        

        private void DebugLog(string ip, string content)
        {
            var datetime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            Console.Write($"{datetime} : {content}\n");
            using (StreamWriter sw = File.AppendText(appPath + $"\\log\\{ip}_log.txt"))
            {
                sw.WriteLine($"{datetime} : {content}\n");
            }
        }

        private string RunLuaWebserver(string server,string luafile)
        {
            string pathTS = @"{" + (char)34 + "path" + (char)34 + ':' + (char)34 + "var/mobile/Media/TouchSprite/lua/" + luafile + (char)34 + @"}";

            if (radioButtonTS.Checked==true)
            {
                DebugLog(server, $"Set TS Lua path {luafile}");
                //MessageBox.Show("Cpi Set lua path");
                if (httpsend($"http://{server}:50005/setLuaPath", "POST", pathTS) != "false")
                {
                    DebugLog(server, $"Run TS Lua {luafile}");
                    return httpsend($"http://{server}:50005/runLua", "GET", null);
                }
            }
            if (radioButtonATT.Checked==true)
            {
                DebugLog(server, "Run Lua XXT");
                return httpsend($"http://{server}:8080/control/start_playing?path={luafile}", "GET", null);
            }
            return false.ToString();
        }

        private string GetBundleID(int row, string address)
        {
            if(CHECKCWebServer(row,address) == true)
            {
                string Response = sendCommandToiOSWeb(address, $"PCAutoD -bundledownload");
                return Response;
            }
            return false.ToString();
        }

        private void CheckXoaInfoExe(int row, string ip)
        {
            //if (XoaInfoExe == null)
            //{
            //    if(CheckFileiOSExist(row, ip,"/Applications/XoaInfo.app/XoaInfo_") == true)
            //    {
            //        XoaInfoExe = "XoaInfo_";
            //    }
            //    else
            //    {
            //        XoaInfoExe = "XoaInfo";
            //    }
            //}
            XoaInfoExe = "XoaInfo";
            DebugLog(ip, $"file chạy : {XoaInfoExe}");
        }

        private bool CheckFileiOSExist(int row, string ip, string fileName)
        {
            if (CHECKCWebServer(row,ip) == true)
            {
                string Response = sendCommandToiOSWeb(ip, $"PCAutoD -ef {fileName}");
                return bool.Parse(Response);
            }
            return false;
        }

        private string DeleteiOSFile(int row, string ip, string fileName)
        {
            if (CHECKCWebServer(row,ip) == true)
            {
                string Response = sendCommandToiOSWeb(ip, $"PCAutoD -rm {fileName}");
                return Response;
            }
            return false.ToString();
        }

        private string KilliOSProcess(int row, string ip, string fileName)
        {
            if (CHECKCWebServer(row,ip) == true)
            {
                string Response = sendCommandToiOSWeb(ip, $"PCAutoD -k {fileName}");
                //return Response;
                if (Response == "true") return true.ToString();
            }
            return false.ToString();
        }

        private bool CheckRunningProcess(int row, string ip, string processName)
        {
            if (CHECKCWebServer(row,ip) == true)
            {
                string Response = sendCommandToiOSWeb(ip, $"PCAutoD -c {processName}");
                Thread.Sleep(1000);
                int kq = int.Parse(Response);               
                if (kq >= 1 ) return true;
            }
            DebugLog(ip, $"Process {processName} not run");
            return false;
        }
        
        private bool _PingPort(string ip,int port)
        {
            // TcpClient client = new TcpClient();

            try
            {
                using (var client = new TcpClient(ip, port))
                    return true;
            }
            catch (SocketException ex)
            {
                DebugLog(ip, $"port {port} closed : {ex.Message}");
                return false;
            }
        }
        
        private bool Checkport(string ip,int port)
        {
            int looptime = 30;
            do
            {
                looptime = looptime - 1;
                if (isStop == true) return false;
                if (_PingPort(ip, port) == true) return true;
                Thread.Sleep(1000);
            }
            while (looptime > 1);
            return false;
        }

        private bool CHECKCWebServer(int row, string ip)
        {
            int looptime = 1;
            do
            {
                if (isStop == true) return false;// buộc dừng stop

                if (radioButtonXXT.Checked == true)
                {
                    var checkloop = _PingPort(ip, 22);
                    if (checkloop == true )
                    {
                        //DebugLog(ip, "Ping port 22 Ok");
                        return true;
                    }
                    else
                    {
                        DebugLog(ip, "ping port 22 error ");
                        looptime += 1;
                        Thread.Sleep(1000);
                    }
                    if (looptime > 30)
                    {

                        WriteSttHL(row, ip, "Kiểm tra lại XXT");
                        looptime = 1;
                    }
                }

                if (radioButtonTS.Checked == true)
                {
                    var checkloop = _PingPort(ip, 50005);
                    if (checkloop == true)
                    {
                        //DebugLog(ip, "Ping port 50005 Ok");
                        return true;
                    }
                    else
                    {
                        DebugLog(ip, "ping port 50005 error ");
                        looptime += 1;
                        Thread.Sleep(1000);
                    }
                    if (looptime > 30) 
                    {
                        sendCommandToiOSWeb(ip, "PCAutoD -uiopen touchsprite://");
                        WriteSttHL(row, ip, "Kiểm tra lại TS"); 
                        looptime = 1; 
                    }
                }

                if (radioButtonATT.Checked == true)
                {
                    var checkloop = _PingPort(ip, 8080);
                    if (checkloop == true)
                    {
                        //DebugLog(ip, "Ping port 8080 Ok");
                        return true;
                    }
                    else
                    {
                        DebugLog(ip, "ping port 8080 error ");
                        looptime += 1;
                        Thread.Sleep(1000);
                    }
                    if (looptime > 30)
                    {
                        WriteSttHL(row, ip, "Kiểm tra lại ATT");
                        looptime = 1;
                    }
                }

                Thread.Sleep(1000);
            }
            while (1 == 1);
            return false;
        }

        private string sendCommandToiOSWeb(string address, string command)
        {
            OutputPcAutoD ketqua = new OutputPcAutoD();
            var result = httpsend($"http://{address}:34789", "POST", $"command={command}");
            if (!string.IsNullOrEmpty(result) && result != "false")
            {
                ketqua = JsonConvert.DeserializeObject<OutputPcAutoD>(result); ;
                return ketqua.status.ToString();
            }
            else return "false";
        }

        private bool waitProxy(int row, string ip, long goodTime = 60)
        {
            if (radioButtonGift.Checked == true) return true;// AppGift
            if (radioButtonLink.Checked == true) return true;// AppGift

            if (radioButtonLumi.Checked == true)
            {
                writestatus(row, ip, "Changing Lumi ...");
                do
                {
                    if (isStop == true) return false; // ràng buộc bấm STOP
                    Thread.Sleep(1500);
                    writestatus(row, ip, "Changing Lumi ...");
                    string ketquatravelumi = httpsend($"http://{ip}:22999//api/refresh_sessions/{ports[row]}", "POST", null);
                    if (ketquatravelumi == "0") writestatus(row, ip, "Change Lumi Done"); return true;
                }
                while (1 == 1);
            }

            string sServer = textBoxIphoneTimer.Text;
            int sPort = int.Parse(textBoxPortIphoneTimer.Text);

            int looptime = 10;
            do
            {
                if (isStop == true) return false; // ràng buộc bấm STOP

                if (looptime <= 0)
                {
                    WriteSttHL(row, ip, "IOS đếm giờ chưa chạy");
                    //return false;
                }
                looptime = looptime - 1;
                var checkLoop = _PingPort(textBoxIphoneTimer.Text, int.Parse(textBoxPortIphoneTimer.Text));
                DebugLog(ip, $"check port : {textBoxPortIphoneTimer.Text} ==> {checkLoop.ToString()}");
                if (checkLoop == true) break;
                Thread.Sleep(500);
            }
            while (1 == 1);

            do
            {
                if (isStop == true) return false; // ràng buộc bấm STOP

                ketquacheckproxytimer = checkProxyTimer(row);
                DebugLog(ip, $"Proxy : {ketquacheckproxytimer} - {goodTime}");
                Thread.Sleep(1500);
            }
            while (ketquacheckproxytimer > goodTime);
            return true;
        }

        private long checkProxyTimer(int row)
        {
            OutputProxyTimer ketqua = new OutputProxyTimer();
            var result = httpsend($"http://{textBoxIphoneTimer.Text}:{textBoxPortIphoneTimer.Text}", "POST", $"IPProxy={textBoxIpFake}&port={listport[row]}");
            if (!string.IsNullOrEmpty(result)&& result!="false") ketqua = JsonConvert.DeserializeObject<OutputProxyTimer>(result);
            else return 9999999999;
            if (ketqua.Amount > 0)
            {
                return ketqua.Amount;
            }
            else
            {
                return 9999999999;
            }
        }

        // các hàm điều khiển chạy file autotouch .lua trên iphone
        #region các hàm điều khiển chạy file autotouch .lua trên iphone
        private bool RunAutoTouch(int row,string ip)
        {
            if(CHECKCWebServer(row,ip))
            {
                writeTimeRandomTouch(row,ip);
                writestatus(row, ip, "Finding BundleID");
                var bundleID = GetBundleID(row, ip);
                if (bundleID == "false")
                {
                    DebugLog(ip, "Ko tim ra bundleID");
                    writestatus(row,ip, "Ko tim ra bundleID");
                    return false;
                }
                string luaFile = "";
                writestatus(row, ip, bundleID);
                if (radioButtonXXT.Checked == true) luaFile = "XXT-randomTouch.lua";
                if (radioButtonTS.Checked == true) luaFile = "TS-randomTouch.lua";
                if (radioButtonATT.Checked == true) luaFile = "ATT-randomTouch.lua";

                
                DebugLog(ip, $"Tên File Lua : {luaFile}");
                DeleteiOSFile(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoTouchDone");
                DeleteiOSFile(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoTouchError");
                //KilliOSProcess(row, ip, "AppStore");

                if (RunLuaWebserver(ip, luaFile)=="false") return false;
            }
            return WaitAutoTouchDone(row, ip);
        }

        private bool WaitAutoTouchDone(int row, string ip)
        {
            if (CHECKCWebServer(row,ip))
            {
                var timeout = int.Parse(textBoxTimeRandomTouch.Text);

                do
                {
                    if (isStop == true) return false; // ràng buộc bấm STOP
                    timeout = timeout - 1;
                    if (timeout <= 1) return false;
                    writestatus(row, ip, $"Đang Chạy Random Touch {timeout}");
                    Thread.Sleep(1200);
                }
                while (CheckFileiOSExist(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoTouchDone") == false);
                DeleteiOSFile(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoTouchDone");
                stopfilelua(ip);
                return true;
            }
            return true;
        }

        private bool DeleteDontLeadRRS(int row, string ip)
        {
            if(CHECKCWebServer(row,ip)==true)
            {
                DeleteiOSFile(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoTouchDontLead");
                string axz = sendCommandToiOSWeb(ip, "PCAutoD -rmRRS");
                DebugLog(ip, $"KQ Del RRS k lead : {axz}");
                writestatus(row, ip, "Đã gửi lệnh xáo rrs ko lead");
                return true;
            }
            return false;
        }

        private bool CheckDownloading(int row,string ip)
        {
            if(CHECKCWebServer(row,ip)==true)
            {
                stopfilelua(ip);
                int timeout = 150;
                do
                {
                    if (isStop == true) return false; // ràng buộc bấm STOP

                    if (CheckFileiOSExist(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoDownloadDone") == true) return true;

                    if (CheckFileiOSExist(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoDownloadNOFree") == true) return false;

                    timeout = timeout - 1;
                    if (timeout < 1) return false;

                    Thread.Sleep(1000);

                } while (CheckFileiOSExist(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoDownloading") == false);
                DeleteiOSFile(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoDownloading");
                Thread.Sleep(2000);
                return true;
            }
            return true;
        }

        private int DelayAppstore(int row,string ip)
        {
            writestatus(row, ip, "Đang Kiểm Tra AppStore");
            int timeout = int.Parse(textBoxTimeAppStore.Text);
            if (CheckDownloading(row,ip)==true)
            {
                do
                {
                    if (isStop == true) return 0; // ràng buộc bấm STOP

                    if (CheckFileiOSExist(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoDownloadDone") == true)
                    {
                        writestatus(row, ip, "Download App Done");
                        return 1;
                    }

                    if (CheckFileiOSExist(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoDownloadNOFree") == true)
                    {
                        writestatus(row, ip, "Not Download");
                        return 0;
                    }

                    writestatus(row, ip, $"Downloading App {timeout}");
                    Thread.Sleep(1000);
                    timeout = timeout - 1;

                    if (timeout <= 1) return 0;

                }
                while (timeout > 1);
            }
            return 0;

        }
        #endregion

        private bool OpenLink(int row, string ip)
        {
            if (CHECKCWebServer(row,ip) == true)
            {
                KilliOSProcess(row, ip, "MobileSafari");
                KilliOSProcess(row, ip, "AppStore");
                Thread.Sleep(1000);

                sendCommandToiOSWeb(ip, $"PCAutoD -uiopen '{textBoxLinkOffer.Text}'");
                writestatus(row, ip, "Openning Link ....");
                Thread.Sleep(1000);
                return WaitSafariLoadLink(row, ip);
            }
            return true;
        }

        private bool killTS(int row,string ip)
        {
            sendCommandToiOSWeb(ip, "PCAutoD -k TouchSprite TSDaemon itunesstored appstored");
            Thread.Sleep(1000);
            DebugLog(ip, $"Open TS {sendCommandToiOSWeb(ip, "PCAutoD -uiopen touchsprite://")}");
            Thread.Sleep(2000);
            if (CHECKCWebServer(row, ip) == true) return true;
            return true;
        }

        private bool stopfilelua(string ip)
        {
            try
            {
                DebugLog(ip, "Bat Dau Stop File Lua");
                if (radioButtonXXT.Checked == true) httpsend($"http://{ip}:46952/recycle", "POST", "");
                if (radioButtonTS.Checked == true) httpsend($"http://{ip}:50005/stopLua", "GET", "");
                if (radioButtonATT.Checked == true)
                {
                    httpsend($"http://{ip}:8080/control/stop_playing?path=ATT-autoLeadCheckFake.lua", "GET", "");
                    httpsend($"http://{ip}:8080/control/stop_playing?path=ATT-autoRRSCheckFake.lua", "GET", "");
                    httpsend($"http://{ip}:8080/control/stop_playing?path=ATT-smlgame.lua", "GET", "");
                    httpsend($"http://{ip}:8080/control/stop_playing?path=ATT-RRSsmlgame.lua", "GET", "");
                    httpsend($"http://{ip}:8080/control/stop_playing?path=ATT-randomTouch.lua", "GET", "");
                }
                DebugLog(ip, "Stop File Xong");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ip} { ex.Message}");
                return false;
            }
            return false;
        }

        private bool WaitSafariLoadLink(int row, string ip)
        {
            if (CHECKCWebServer(row, ip) == true)
            {
                int timeout = int.Parse(textBoxTimeDiLink.Text);
                do
                {
                    if (isStop == true) return false; // ràng buộc bấm STOP

                    timeout = timeout - 1;
                    if (timeout < 0) return false;

                    writestatus(row, ip, $"Link Openning ... {timeout}");
                    Thread.Sleep(1000);

                }
                while (CheckRunningProcess(row, ip, "AppStore") == false);
            }
            return true;
        }

        private bool OpenCheckFake(int row,string ip)
        {
            KilliOSProcess(row, ip, "Check Fake");
            KilliOSProcess(row, ip, "MobileSafari");
            KilliOSProcess(row, ip, "AppStore");

            if(radioButtonGift.Checked == true) // Gift
            {
                writestatus(row, ip, "Preparing AppGift");
                string luasmlgame = "smlgame.lua";
                if (radioButtonLuuRRS.Checked == true) luasmlgame = "RetenSmlgame.lua";
                if(radioButtonXXT.Checked==true) //xxt
                {
                    RunLuaWebserver(ip, $"XXT-{luasmlgame}");
                    writestatus(row, ip, "Open AppGift By XXT ...");
                }

                if (radioButtonTS.Checked == true) //ts
                {
                    RunLuaWebserver(ip, $"TS-{luasmlgame}");
                    writestatus(row, ip, "Open AppGift By TS ...");
                }

                if (radioButtonATT.Checked == true) //att
                {
                    RunLuaWebserver(ip, $"ATT-{luasmlgame}");
                    writestatus(row, ip, "Open AppGift By ATT ...");
                }

            }

            if (radioButtonCheckFake.Checked == true) // CF
            {
                writestatus(row, ip, "Preparing Check Fake");
                string luafile = "autoLeadCheckFake.lua";
                if (radioButtonXXT.Checked == true) //xxt
                {
                    writestatus(row, ip, "Open CheckFake By XXT ...");
                    RunLuaWebserver(ip, $"XXT-{luafile}");
                   
                }

                if (radioButtonTS.Checked == true) //ts
                {
                    writestatus(row, ip, "Open CheckFake By TS ...");
                    RunLuaWebserver(ip, $"TS-{luafile}");
                    
                }

                if (radioButtonATT.Checked == true) //att
                {
                    writestatus(row, ip, "Open CheckFake By ATT ...");
                    RunLuaWebserver(ip, $"ATT-{luafile}");
                    
                }

            }

            return WaitCheckFake(row,ip);
        }

        private bool OpenCheckFakeRRS(int row, string ip)
        {
            KilliOSProcess(row, ip, "Check Fake");
            KilliOSProcess(row, ip, "MobileSafari");
            KilliOSProcess(row, ip, "AppStore");

            if (radioButtonGift.Checked == true) // Gift
            {
                writestatus(row, ip, "Preparing AppGift");
                string luasmlgame = "RRSsmlgame.lua";
                if (radioButtonLuuRRS.Checked == true) luasmlgame = "RetenSmlgame.lua";
                if (radioButtonXXT.Checked == true) //xxt
                {
                    RunLuaWebserver(ip, $"XXT-{luasmlgame}");
                    writestatus(row, ip, "Open RRS AppGift By XXT ...");
                }

                if (radioButtonTS.Checked == true) //ts
                {
                    RunLuaWebserver(ip, $"TS-{luasmlgame}");
                    writestatus(row, ip, "Open RRS AppGift By TS ...");
                }

                if (radioButtonATT.Checked == true) //att
                {
                    RunLuaWebserver(ip, $"ATT-{luasmlgame}");
                    writestatus(row, ip, "Open RRS AppGift By ATT ...");
                }

            }

            if (radioButtonCheckFake.Checked == true) // CheckFake
            {
                writestatus(row, ip, "Preparing Check Fake");
                string luafile = "autoRRSCheckFake.lua";
                if (radioButtonXXT.Checked == true) //xxt
                {
                    
                    writestatus(row, ip, "Open RRS CheckFake By XXT ...");
                    RunLuaWebserver(ip, $"XXT-{luafile}");
                }

                if (radioButtonTS.Checked == true) //ts
                {
                    
                    writestatus(row, ip, "Open RRS CheckFake By TS ...");
                    RunLuaWebserver(ip, $"TS-{luafile}");
                }

                if (radioButtonATT.Checked == true) //att
                {
                    
                    writestatus(row, ip, "Open RRS CheckFake By ATT ...");
                    RunLuaWebserver(ip, $"ATT-{luafile}");
                }

            }

            return WaitCheckFakeRRS(row, ip);
        }

        private bool WaitCheckFake(int row,string ip)
        {
            if(CHECKCWebServer(row,ip)==true)
            {
                int timeout = int.Parse(textBoxTimeCheckFake.Text);

                do
                {
                    if (isStop == true) return false; // ràng buộc bấm STOP

                    if(CheckFileiOSExist(row,ip, "/private/var/mobile/Media/1ferver/lua/scripts/CheckFakeSaiTT")==true) return false;

                    timeout = timeout - 1;
                    if (timeout <= 1)
                    {
                        stopfilelua(ip);
                        return false;
                    }

                    writestatus(row, ip, $"Waitting CheckFake {timeout}");
                    Thread.Sleep(1000);
                }
                while (CheckFileiOSExist(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/CheckFakeQuaStore") == false);
                return true;
            }
            return true;
        }

        private bool WaitCheckFakeRRS(int row, string ip)
        {
            if (CHECKCWebServer(row, ip) == true)
            {
                int timeout = int.Parse(textBoxTimeCheckFake.Text);

                do
                {
                    if (isStop == true) return false; // ràng buộc bấm STOP

                    if (CheckFileiOSExist(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoTouchDone") == true) return false;

                    if (CheckFileiOSExist(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoTouchError") == true) return false;

                    if (CheckFileiOSExist(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoTouchDontLead") == true) return false;

                    timeout = timeout - 1;
                    if (timeout <= 1)
                    {
                        stopfilelua(ip);
                        return false;
                    }

                    writestatus(row, ip, $"Waitting CheckFake RRS {timeout}");
                    Thread.Sleep(1000);
                }
                while (CheckFileiOSExist(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoTouchOpenApp") == false);
            }
            return WaitCheckAppStoreRRS(row, ip);
        }

        private bool WaitCheckAppStoreRRS(int row,string ip)
        {
            if (CHECKCWebServer(row, ip) == true)
            {
                int timeout = int.Parse(textBoxTimeAppStore.Text);

                do
                {
                    if (isStop == true) return false; // ràng buộc bấm STOP

                    if (CheckFileiOSExist(row, ip, "/private/var/mobile/Media/1ferver/lua/scripts/AutoDownloadDone") == true) return true;

                    timeout = timeout - 1;
                    if (timeout <= 1)
                    {
                        stopfilelua(ip);
                        return false;
                    }

                    writestatus(row, ip, $"Wait Appstore RRS {timeout}");
                    Thread.Sleep(1000);
                }
                while (timeout > 1);
                return false;
            }
            return false;
        }

        private bool writeDownMemCode(int row, string ip)
        {
            if (CHECKCWebServer(row, ip) == true)
            {
                var Response = sendCommandToiOSWeb(ip, $"PCAutoD -writememcode {richTextBoxMembID.Text}");
                if (Response == "false") return false;
                return bool.Parse(Response);
            }
            return false;
        }

        private bool writeTimeRandomTouch(int row, string ip)
        {
            if (CHECKCWebServer(row, ip) == true)
            {
                var Response = sendCommandToiOSWeb(ip, $"PCAutoD -writememcode {textBoxTimeRandomTouch.Text}");
                if (Response == "false") return false;
                return bool.Parse(Response);
            }
            return false;
        }

        #region Các Phương thức điều khiển XoaInfo
        private bool XoaInfo(int row,string ip)
        {
            if(CHECKCWebServer(row,ip)==true)
            {
                KilliOSProcess(row, ip, "XoaInfo");
                Thread.Sleep(1000);
                do
                {
                    if (isStop == true) return false; // ràng buộc bấm STOP
                    sendCommandToiOSWeb(ip, "PCAutoD -uiopen 'XoaInfo://'");
                    Thread.Sleep(2000);
                }
                while (CheckRunningProcess(row, ip, "XoaInfo") == false); // đợi XoaInfo mở thành công

                sendCommandToiOSWeb(ip, "PCAutoD -uiopen 'XoaInfo://Reset'");

                writestatus(row, ip, "XoaInfo Reset Data ...");

                if (isStop == true) return false; // ràng buộc bấm STOP
            }
            return WaitXoaInfoDone(row,ip);
        }

        private bool SaveRRS(int row, string ip)
        {
            if (CHECKCWebServer(row, ip) == true)
            {
                KilliOSProcess(row, ip, "XoaInfo");
                Thread.Sleep(1000);

                writestatus(row, ip, "XoaInfo Reset Data ...");
                do
                {
                    if (isStop == true) return false; // ràng buộc bấm STOP
                    sendCommandToiOSWeb(ip, "PCAutoD -uiopen 'XoaInfo://'");
                    Thread.Sleep(2000);
                }
                while (CheckRunningProcess(row, ip, "XoaInfo") == false); // đợi XoaInfo mở thành công

                sendCommandToiOSWeb(ip, "PCAutoD -uiopen 'XoaInfo://?RRS'");

                writestatus(row, ip, "XoaInfo Lưu RRS ... ");
                if (isStop == true) return false; // ràng buộc bấm STOP
            }
            return WaitXoaInfoDone(row, ip);
        }

        private bool WaitXoaInfoDone(int row, string ip)
        {
            if(CHECKCWebServer(row,ip)==true)
            {
                int timeout = int.Parse(textBoxTimeXoaInfo.Text);
                do
                {
                    if (isStop == true) return false; // ràng buộc bấm STOP

                    timeout = timeout - 1;
                    if (timeout <= 1) return false;
                    writestatus(row, ip, $"Wait XoaInfo {timeout}");
                    Thread.Sleep(1000);
                }
                while (CheckRunningProcess(row, ip, "XoaInfo") == true);
            }
            Thread.Sleep(10000);
            return true;
        }

        private bool RestoreRRS(int row, string ip)
        {
            if(CHECKCWebServer(row,ip)==true)
            {
                KilliOSProcess(row, ip, "XoaInfo");
                Thread.Sleep(1000);
                writestatus(row, ip, "XoaInfo Restore RRS ...");
                sendCommandToiOSWeb(ip, "PCAutoD -uiopen 'XoaInfo://Restore'");

                if (isStop == true) return false; // ràng buộc bấm STOP

            }
            return WaitXoaInfoDone(row, ip);
        }

        private void backupRRS(int row, string ip)
        {
            if (checkBoxRRStoServer.Checked == true) // chọn rrs to sv
            {
                string foldertype=null;
                string date = DateTime.Today.ToString(("dd-MM-yyyy"));
                if (radioButtonGift.Checked) foldertype = "svRetention";
                if (radioButtonCheckFake.Checked) foldertype = "svRRS";
                string Cmdcreatefolder = $"rsync -a --rsync-path={'"'}mkdir -p /cygdrive/d/{foldertype}/{date}/{ip}/ && rsync{'"'} $source rsync@{textBoxServerData.Text}:/cygdrive/d/{foldertype}/{date}/{ip}/";

                string cmdRsync = $"rsync -avzP /private/var/root/Library/XoaInfo/ rsync@{textBoxServerData.Text}:/cygdrive/d/{foldertype}/{date}/{ip}/";

                string cmdRsyncRemove = $"rsync --remove-source-files -avzP /private/var/root/Library/XoaInfo/ rsync@{textBoxServerData.Text}:/cygdrive/d/{foldertype}/{date}/{ip}/";

                writestatus(row, ip, "Bắt đầu backup rrs to sv");
                if (Checkport(ip, 22) == true)
                {
                    if (checkBoxDeleteRRS.Checked == true) // Nếu chọn xóa rrs
                    {
                        DebugLog(ip, sendcmdrsync(row, ip, Cmdcreatefolder).ToString());
                        Thread.Sleep(2000);
                        DebugLog(ip, sendcmdrsync(row, ip, cmdRsyncRemove).ToString());
                    }
                    else // ko chọn xóa rrs
                    {
                        DebugLog(ip, sendcmdrsync(row, ip, Cmdcreatefolder).ToString());
                        Thread.Sleep(2000);
                        DebugLog(ip, sendcmdrsync(row, ip, cmdRsync).ToString());
                    }
                }
            }

        }


        private void Respring(int row, string ip)
        {
            if (CHECKCWebServer(row,ip)==true)
            {
                sendCommandToiOSWeb(ip, "PCAutoD -respring");
                writestatus(row, ip, "Respring");
            }
        }
        #endregion

        private void dataGridViewSTTIphone_MouseClick(Object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int currentMouseOverRow = dataGridViewSTTIphone.HitTest(e.X, e.Y).RowIndex;
                //ContextMenu cm = new ContextMenu();
                //this.ContextMenu = cm;
                //cm.MenuItems.Add(new MenuItem(string.Format("&Stop", currentMouseOverRow.ToString()), new System.EventHandler(this.StopPerThread)));
                //cm.MenuItems.Add(new MenuItem(string.Format("Do something to row {0}", currentMouseOverRow.ToString())));

                cm.Show(this, new Point(e.X, e.Y));
            }

        }

        private void PreStart()
        {
            for (int i = 0; i < numdevices; i++)
            {
                //listtask.Add(new Thread(() => start(i, ipss[i])));
                //listtask[i].Name=$"Luồng Thứ {i}";
                //listtask[i].Start();
                //Thread.Sleep(100);

                listtask.Add(ipss[i], new Thread(() => start(i, ipss[i])));
                listtask[ipss[i]].IsBackground = true;
                listtask[ipss[i]].Name = ipss[i];
                listtask[ipss[i]].Start();
                Thread.Sleep(100);
            }
        }

        private void start(int row, string ip)
        {
            try
            {
                isStop = false;
                if (radioButtonTS.Checked == true) sendCommandToiOSWeb(ip, "PCAutoD -uiopen touchsprite://");
                backupRRS(row, ip);
                CheckXoaInfoExe(row, ip);

                while (1 == 1)
                {
                    //CountThread();
                    if (isStop == true) return; // ràng buộc bấm STOP
                    writestatus(row, ip, "Bắt đầu từ đầu");
                    DebugLog(ip, "Bat dau tu dau");
                    clearAuto(row, ip);

                    if (WipeData(row, ip) == false) continue;
                    if (LoadOff(row, ip) == false) continue;
                    if (waitAppStore(row, ip) == false) continue;
                    if (OpenOff(row, ip) == false) continue;
                    writestatus(row, ip, "Xong 1 Tua");
                }
            }
            catch (ThreadAbortException ex)
            {
                WriteSttHL(row, ip, $"Trong start : {ex.Message}");
            }
            // 1. Mở TS, Chạy backup to sv
            // 2. Chạy XoaInfo(Lưu RRS or Reset or Restore RRS
            // 3. Đi link (Mở Check Fake or Gift or Link safari
            // 4. Đợi Kết Quả Đi Link
            // 5. Nếu Ra off thì đợi tải , ko thì reset data
            // 6. Tải xong Mở random touch

        }

        private bool WipeData(int row, string ip) //  Chạy XoaInfo(Lưu RRS or Reset or Restore RRS)
        {
            try
            {
                if (radioButtonResetData.Checked == true)
                {
                    if (waitProxy(row, ip, long.Parse(textBoxThoiGianVang.Text)) == false) return false;
                    do
                    {
                        if (isStop == true) return false; // ràng buộc bấm STOP
                    } while (XoaInfo(row, ip) == false);
                }

                if (radioButtonLuuRRS.Checked == true)
                {
                    if (waitProxy(row, ip, int.Parse(textBoxThoiGianVang.Text)) == false) return false;
                    do
                    {
                        if (isStop == true) return false; // ràng buộc bấm STOP
                    } while (SaveRRS(row, ip) == false);
                    backupRRS(row, ip);
                }

                if (radioButtonRestoreRRS.Checked == true)
                {
                    if (waitProxy(row, ip, 900) == false) return false;
                    do
                    {
                        if (isStop == true) return false; // ràng buộc bấm STOP
                    } while (RestoreRRS(row, ip) == false);
                }
            }
            catch (Exception ex)
            {
                WriteSttHL(row, ip, $"Error : {ex.Message}");
                DebugLog(ip, $"Error : {ex.Message}");
                return false;
            }
            return true;
        }

        private bool LoadOff(int row, string ip) // Đi link (Mở Check Fake or Gift or Link safari)
        {
            try
            {
                writeDownMemCode(row, ip);
                if (radioButtonLink.Checked == true)
                {

                }

                if (radioButtonResetData.Checked == true)
                {
                    if (OpenCheckFake(row, ip) == true) return true;
                }

                if (radioButtonLuuRRS.Checked == true)
                {
                    if (OpenCheckFake(row, ip) == true) return true;
                }

                if (radioButtonRestoreRRS.Checked == true)
                {
                    if (OpenCheckFakeRRS(row, ip) == true) return true;
                }
            }
            catch (Exception ex)
            {
                WriteSttHL(row, ip, $"Error : {ex.Message}");
                DebugLog(ip, $"Error : {ex.Message}");
                return false;
            }
            return false;
        }

        private bool waitAppStore(int row, string ip) // Đợi tải off
        {
            try
            {
                if (DelayAppstore(row, ip) == 1) return true;
                else return false;
            }
            catch (Exception ex)
            {
                WriteSttHL(row, ip, $"Error : {ex.Message}");
                DebugLog(ip, $"Error : {ex.Message}");
                return false;
            }

        }

        private bool OpenOff(int row, string ip) // Mở off
        {
            try
            {
                writeTimeRandomTouch(row, ip);
                if (RunAutoTouch(row, ip) == true) return true;
                else return false;
            }
            catch (Exception ex)
            {
                WriteSttHL(row, ip, $"Error : {ex.Message}");
                DebugLog(ip, $"Error : {ex.Message}");
                return false;
            }
        }

        private void PreStartPerThread(Object sender, EventArgs e)
        {
            var a = StartPerThread();
        }

        async Task<int> StartPerThread()
        {
            for (int i = 0; i < dataGridViewSTTIphone.Rows.Count; i++)
            {
                if (dataGridViewSTTIphone.Rows[i].Selected == true)
                {
                    listtask.Add(dataGridViewSTTIphone.Rows[i].Cells[0].Value.ToString(), new Thread(() => start(i, dataGridViewSTTIphone.Rows[i].Cells[0].Value.ToString())));
                    listtask[dataGridViewSTTIphone.Rows[i].Cells[0].Value.ToString()].IsBackground = true;
                    listtask[dataGridViewSTTIphone.Rows[i].Cells[0].Value.ToString()].Name = dataGridViewSTTIphone.Rows[i].Cells[0].Value.ToString();
                    listtask[dataGridViewSTTIphone.Rows[i].Cells[0].Value.ToString()].Start();
                    Thread.Sleep(100);
                }
            }
            return 0;
        }

        private void PreStopPerThread(Object sender, EventArgs e)
        {
            var a = StopPerThread();
        }

        async Task<int> StopPerThread()
        {
            for (int i = 0; i <= dataGridViewSTTIphone.Rows.Count; i++)
            {
                if (dataGridViewSTTIphone.Rows[i].Selected == true)
                {
                    listtask[dataGridViewSTTIphone.Rows[i].Cells[0].Value.ToString()].Abort();
                    listtask.Remove(dataGridViewSTTIphone.Rows[i].Cells[0].Value.ToString());
                    Thread.Sleep(100);
                }
            }
            return 0;
        }

        private void test()
        {
            string luafile = "TS-autoLeadCheckFake.lua";
            string pathTS = @"{" + (char)34 + "path" + (char)34 + ':' + (char)34 + "var/mobile/Media/TouchSprite/lua/" + luafile + (char)34 + @"}";
            string pathTS2 = @"{" + (char)34 + "path" + (char)34;

            httpsend($"http://192.168.88.98:50005/stopLua", "GET", "");

            //pathTS.Replace(@"\", "");
            // pathTS2 = Regex.Replace(pathTS, @"(\[|""|\])", "");
            // pathTS3 = Regex.Replace(pathTS, @"(\[|\])", "");

            //while (1==1)
            //{
            //    var kq = httpsend2($"http://{server}:50005/stopLua", "GET", null);
            //    Thread.Sleep(1000);
            //    DebugLog(server, "Set Lua path");
            //   var kq2 = httpsend4($"http://{server}:50005/setLuaPath", "POST", pathTS);
            //   DebugLog(server, "Run Lua TS");
            Thread.Sleep(1000);
            //  var kq3 = httpsend4($"http://{server}:50005/runLua", "GET", null);
            //    //kqThread.Sleep(15000);
            //
            //    //var result = httpsend3($"http://192.168.88.97:34789", "POST", $"command=PCAutoD -uiopen  //XoaInfo://");
            //    //Console.WriteLine($"Chuoi tra ve {result}");
            //    //Thread.Sleep(7000);
            //    //OutputPcAutoD ketqua = JsonConvert.DeserializeObject<OutputPcAutoD>(result);
            //    //Console.WriteLine($"Ket Qua la : {ketqua}");
            //}
            //while (1 == 1)
            //{
            //    CHECKCWebServer(0, "192.168.88.97");
            //    Thread.Sleep(1000);
            //}

           
        }

        private void buttonStartAuto_Click(object sender, EventArgs e)
        {
            buttonStartAuto.Enabled = false;
            save();
            CountThread();


            new Thread(() => PreStart()).Start();

            //new Thread(() => test()).Start();


        }

        private void buttonStopAuto_Click(object sender, EventArgs e)
        {
            //Environment.Exit(Environment.ExitCode);
            var a = new Thread(StopAll);
            a.Start();
            //StopAll();
            //foreach (KeyValuePair<string, Thread> entry in listtask) entry.Value.Abort();
            //listtask.Clear();
            a.Join();
            buttonStartAuto.Enabled = true;
        }

        private void StopAll()
        {
            isStop = true;         
            foreach (KeyValuePair<string, Thread> entry in listtask) entry.Value.Abort();
            listtask.Clear();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            save();
        }
    }
}
