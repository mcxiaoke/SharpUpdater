using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Security.Policy;
using Semver;
using System.IO.Compression;

namespace SharpUpdater {
    public enum UpdateStatus {
        NONE,
        READY,
        DONE,
        ERROR
    }

    public partial class UpdateDialog : Form {
        private string CmdName;
        private string CmdURL;
        private VersionInfo UpdateVersionInfo;
        private UpdateStatus CurrentUpdateStatus = UpdateStatus.NONE;

        public UpdateDialog(CommandOptions options) {
            InitializeComponent();
            ParseOptions(options);
        }

        private void ParseOptions(CommandOptions options) {
            var (name, url) = (options.Name, options.URL);
            Console.WriteLine($"UpdateDialog option={name} {url}");
            if (name == null && url == null && !string.IsNullOrWhiteSpace(options.ConfigFile)) {
                (name, url) = SharpUtils.ReadConfig(options.ConfigFile);
                Console.WriteLine($"UpdateDialog config={name} {url}");
            }
            if (name == null && url == null) {
                (name, url) = SharpUtils.ReadConfig();
                Console.WriteLine($"UpdateDialog default={name} {url}");
            }
            CmdName = name;
            CmdURL = url;
            Console.WriteLine($"UpdateDialog final={CmdName} {CmdURL}");
        }

        private async void UpdateDialog_Load(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(CmdURL)) {
                SetFatalStatusInfo("启动参数错误：" +
                    $"\n\n使用命令行参数：" +
                    $"\n-u/--url version-info-url" +
                    "\n-n/--name application-name"+
                    "\n-c/--config local-config-file"+
                    "\n\n使用配置文件: \nSharpUpdater.json file in application directory.");
            } else {
                await CheckUpdate();
            }
        }

        private void UpdateDialog_FormClosing(object sender, FormClosingEventArgs e) {

        }

        private void UpdateDialog_Shown(object sender, EventArgs e) {
            if (!string.IsNullOrWhiteSpace(CmdName)) {
                this.Text = $"{CmdName} 更新工具";
            } else {
                this.Text = $"{Application.ProductName} {Application.ProductVersion}";
            }
        }


        private static FileVersionInfo ReadFileVersion(string path) {
            try {
                return FileVersionInfo.GetVersionInfo(path);
            } catch (Exception ex) {
                Console.WriteLine(ex);
                return null;
            }
        }

        private void SetFatalStatusInfo(string errorText) {
            CurrentUpdateStatus = UpdateStatus.NONE;
            Invoke(new Action(() => {
                BigTextBox.Text = errorText;
                BigTextBox.ForeColor = Color.Blue;
                BigTextBox.Font = new System.Drawing.Font(BigTextBox.Font.Name, 8F);
                BigButton.Enabled = true;
                BigButton.Text = "退出";
            }));
        }

        private void SetRetryStatusInfo(string errorText) {
            CurrentUpdateStatus = UpdateStatus.ERROR;
            Invoke(new Action(() => {
                BigTextBox.Text = errorText;
                BigTextBox.ForeColor = Color.Blue;
                BigTextBox.Font = new System.Drawing.Font(BigTextBox.Font.Name, 8F);
                BigButton.Enabled = true;
                BigButton.Text = "点击重试";
            }));
        }

        private void SetVersionInfoTextBox(string localVer, VersionInfo info) {
            var infoStr = "";
            infoStr += $"★ 应用名称：{info.Name}\n";
            infoStr += $"★ 应用版本：{localVer} => {info.Version}\n";
            infoStr += $"★ 文件大小：{SharpUtils.FormatFileSize(info.DownloadSize)}\n";
            infoStr += $"★ 发布时间：{info.CreatedAt}\n";
            infoStr += $"★ 项目地址：{info.ProjectUrl}\n";
            infoStr += $"★ 更新说明：{info.Changelog}";
            BigTextBox.Text = infoStr;
            BigTextBox.Font = new System.Drawing.Font(BigTextBox.Font.Name, 9F);
            BigTextBox.ForeColor = Control.DefaultForeColor;
        }

        private async Task CheckUpdate(string versionUrl = null) {
            BigTextBox.Text = string.Empty;
            using (var client = new WebClient()) {
                var url = versionUrl ?? CmdURL;
                Console.WriteLine($"CheckUpdate url={url} ");
                try {
                    var text = await client.DownloadStringTaskAsync(new Uri(url));
                    var info = JsonConvert.DeserializeObject<VersionInfo>(text);
                    if (VersionInfo.DataInValid(info)) {
                        SetRetryStatusInfo($"配置错误：配置无效或缺少必须字段！\n\n{text}");
                        return;
                    }
                    UpdateVersionInfo = info;
                    Console.WriteLine($"CheckUpdate info={info}");
                    var exePath = Path.Combine(Application.StartupPath, info.Program);
                    if (!File.Exists(exePath)) {
                        SetRetryStatusInfo($"文件错误：可执行文件 [{info.Program}] 不存在！\n\n" +
                        $"当前目录 {Application.StartupPath} 未找到文件名为 {info.Program} 的可执行文件，" +
                        $"如果你曾经给文件更名，请改回 {info.Program} 后重试");
                        return;
                    }
                    Console.WriteLine($"CheckUpdate exePath={exePath}");
                    var localFile = ReadFileVersion(exePath);
                    SharpUtils.WriteConfig(localFile.ProductName, url);
                    var localVer = SemVersion.Parse(localFile.ProductVersion, SemVersionStyles.Any);
                    var remoteVer = SemVersion.Parse(info.Version, SemVersionStyles.Any);
                    bool shouldUpdate = localVer < remoteVer;
                    var titleStr = shouldUpdate ? $"{localFile.ProductName ?? info.Name} 发现新版本！" :
                        $"{localFile.ProductName} 当前已经是最新版！";

                    Console.WriteLine($"CheckUpdate end {DateTime.Now}");
                    CurrentUpdateStatus = UpdateStatus.READY;
                    Invoke(new Action(() => {
                        this.Text = titleStr;
                        SetVersionInfoTextBox(localVer.ToString(), info);
                        BigButton.Enabled = shouldUpdate;
                        BigButton.Text = shouldUpdate ? "开始更新" : "不需要更新";
                    }));
                } catch (Exception ex) {
                    Console.WriteLine($"CheckUpdate failed error={ex.Message}");
                    SetRetryStatusInfo($"遇到错误：{ex.Message}\n\n{url}\n{ex}");
                }
            }
        }

        private void BigTextBox_LinkClicked(object sender, LinkClickedEventArgs e) {
            Console.WriteLine($"InfoTextBox_LinkClicked {e.LinkText}");
            Process.Start(e.LinkText);
        }

        // http://simplygenius.net/Article/AncillaryAsyncProgress
        // https://devblogs.microsoft.com/dotnet/async-in-4-5-enabling-progress-and-cancellation-in-async-apis/
        private async Task<(string, Exception)> DownloadFileAsync(VersionInfo info, IProgress<int> progress) {
            // url for test
            Uri uri = new Uri((info.DownloadUrl));
            Console.WriteLine($"DownloadFileAsync url={uri}");
            var downloadPath = Path.Combine(Application.StartupPath, ".temp");
            //var filename = Path.GetFileName(uri.LocalPath);
            var filename = "Release.zip";
            string filepath = Path.Combine(downloadPath, filename);
            var tempfile = Path.Combine(downloadPath, filename + ".tmp");
            // make io operations async
            try {
                await Task.Run(() => {
                    if (!Directory.Exists(downloadPath)) {
                        Directory.CreateDirectory(downloadPath);
                        File.SetAttributes(downloadPath, FileAttributes.Hidden);
                    }
                    var filepathOld = filepath + ".old";
                    if (File.Exists(filepathOld)) {
                        File.Delete(filepathOld);
                    }
                    if (File.Exists(filepath)) {
                        File.Move(filepath, filepathOld);
                    }
                    if (File.Exists(tempfile)) {
                        File.Delete(tempfile);
                    }
                });
            } catch (Exception ex) {
                Console.WriteLine($"DownloadFileAsync error1={ex}");
                return (null, ex);
            }
            using (var client = new WebClient()) {
                try {
                    client.DownloadProgressChanged += (o, e) => {
                        progress?.Report(e.ProgressPercentage);
                    };
                    await client.DownloadFileTaskAsync(uri, tempfile);
                    Console.WriteLine($"DownloadFileAsync file={tempfile}");
                    File.Move(tempfile, filepath);
                    return (filepath, null);
                } catch (Exception ex) {
                    Console.WriteLine($"DownloadFileAsync error2={ex}");
                    return (null, ex);
                }
            }

        }

        private async Task<Exception> InstallUpdateAsync(VersionInfo info, string filepath) {
            // Normalizes the path.
            var program = info.Program;
            var zipPath = Path.GetFullPath(filepath);
            var destPath = Path.GetFullPath(Application.StartupPath);
            Console.WriteLine($"InstallUpdateAsync file={filepath}");
            return await Task.Run(() => {
                try {
                    var hasProgram = SharpUtils.ZipFileContains(zipPath, program);
                    if (!hasProgram) {
                        throw new NullReferenceException($"升级包损坏或错误：可执行文件 [{program}] 不存在");
                    }
                    SharpUtils.UnzipFile(zipPath, destPath, true, true, "Release/");
                    return null;
                } catch (Exception ex) {
                    Console.WriteLine($"InstallUpdateAsync error={ex.Message}");
                    return ex;
                }
            });
        }

        private Exception StopRunningProgram(VersionInfo info) {
            var fullpath = Path.Combine(Application.StartupPath, info.Program);
            Console.WriteLine($"StopRunningProgram fullpath={fullpath}");
            return SharpUtils.StopProcessByPath(fullpath);
        }

        private async void UpdateButton_Click(object sender, EventArgs e) {
            if (CurrentUpdateStatus == UpdateStatus.ERROR) {
                BigTextBox.Text = string.Empty;
                await CheckUpdate();
                return;
            }
            if (CurrentUpdateStatus == UpdateStatus.DONE) {
                Close();
                Process.Start(Path.Combine(Application.StartupPath, UpdateVersionInfo.Program));
                return;
            }
            if (CurrentUpdateStatus != UpdateStatus.READY) {
                Close();
                return;
            }

            var p = new Progress<int>(value => {
                AProgressBar.Value = value;
            });
            BigButton.Enabled = false;
            BigButton.Text = "正在下载...";
            AProgressBar.Visible = true;
            AProgressBar.Value = 0;
            var (filepath, dError) = await DownloadFileAsync(UpdateVersionInfo, p);
            if (dError != null) {
                CurrentUpdateStatus = UpdateStatus.READY;
                BigButton.Enabled = true;
                BigButton.Text = "下载失败，点击重试";
                MessageBox.Show($"网址：{UpdateVersionInfo.DownloadUrl}\n\n{dError}", $"更新包下载失败 {dError.GetType()}", MessageBoxButtons.OK);
                return;
            }
            BigButton.Text = "正在安装...";
            var sError = StopRunningProgram(UpdateVersionInfo);
            if (sError != null) {
                CurrentUpdateStatus = UpdateStatus.READY;
                BigButton.Enabled = true;
                BigButton.Text = "安装失败，点击重试";
                MessageBox.Show($"待更新的应用正在运行，请退出后重试\n程序：{UpdateVersionInfo.Program}\n\n{sError}", $"无法结束进程 {sError.GetType()}", MessageBoxButtons.OK);
                return;
            }
            var iError = await InstallUpdateAsync(UpdateVersionInfo, filepath);
            if (iError != null) {
                CurrentUpdateStatus = UpdateStatus.READY;
                BigButton.Enabled = true;
                BigButton.Text = "安装失败，点击重试";
                MessageBox.Show($"文件：{filepath}\n\n{iError}", $"更新包安装失败 {iError.GetType()}", MessageBoxButtons.OK);
                return;
            }
            CurrentUpdateStatus = UpdateStatus.DONE;
            BigButton.Enabled = true;
            BigButton.Text = "已安装更新，启动应用";
        }
    }
}
