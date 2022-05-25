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
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Security.Policy;
using Semver;
using System.IO.Compression;

namespace SharpUpdater {
    public partial class UpdateDialog : Form {
        private readonly string VersionUrl;
        private VersionInfo NewVersion;

        public UpdateDialog(string[] args) {
            InitializeComponent();
            if (args != null && args.Length >= 1) {
                string url = args[0];
                try {
                    VersionUrl = new Uri(url).ToString();
                } catch (Exception) {
                    VersionUrl = null;
                }
            } else {
                var (_, url) = SharpUtils.ReadConfig();
                VersionUrl = url;
            }
        }

        private async void UpdateDialog_Load(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(VersionUrl)) {
                InfoTextBox.Text = "参数错误：" +
                    "\n请传递下列启动参数之一：" +
                    "\n配置URL: -u/--url version-config-url" +
                    "\n配置文件  -c/--config SharpUpdater.json";
                InfoTextBox.ForeColor = Color.Red;
            } else {
                await CheckUpdate();
            }
        }

        private void UpdateDialog_FormClosing(object sender, FormClosingEventArgs e) {

        }

        private void UpdateDialog_Shown(object sender, EventArgs e) {

        }


        private static FileVersionInfo ReadFileVersion(string path) {
            try {
                return FileVersionInfo.GetVersionInfo(path);
            } catch (Exception ex) {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task CheckUpdate() {
            using (var client = new WebClient()) {
                var url = VersionUrl;
                Console.WriteLine($"CheckUpdate {DateTime.Now} url={url} ");
                try {
                    var text = await client.DownloadStringTaskAsync(new Uri(url));
                    var info = JsonConvert.DeserializeObject<VersionInfo>(text);
                    if (info == null || !info.HasUpdate) {
                        Invoke(new Action(() => {
                            InfoTextBox.Text = "没有发现新版本！";
                        }));
                        return;
                    }
                    Console.WriteLine($"CheckUpdate {DateTime.Now} {info?.Version}");
                    NewVersion = info;
                    var exePath = Path.Combine(Application.StartupPath, $"{info.Name}.exe");
                    if (!File.Exists(exePath)) {
                        Invoke(new Action(() => {
                            InfoTextBox.Text = "本地可执行文件不存在！";
                        }));
                        return;
                    }
                    var localFile = ReadFileVersion(exePath);
                    SharpUtils.WriteConfig(localFile.InternalName, url);
                    var localVer = SemVersion.Parse(localFile.ProductVersion, SemVersionStyles.Any);
                    info.Version = "0.6.2";
                    var remoteVer = SemVersion.Parse(info.Version, SemVersionStyles.Any);
                    bool shouldUpdate = localVer < remoteVer;
                    var titleStr = shouldUpdate ? $"{localFile.ProductName} 发现新版本！" :
                        $"{localFile.ProductName} 当前已经是最新版！";
                    var infoStr = "";
                    infoStr += $"★ 应用名称：{info.Name}\n";
                    infoStr += $"★ 本地版本：{localVer}\n";
                    infoStr += $"★ 最新版本：{remoteVer}\n";
                    infoStr += $"★ 发布时间：{info.CreatedAt}\n";
                    infoStr += $"★ 项目地址：{info.ProjectUrl}\n";
                    infoStr += $"★ 更新说明：{info.Title} {info.Changelog}";

                    Console.WriteLine($"CheckUpdate end {DateTime.Now}");
                    Invoke(new Action(() => {
                        this.Text = titleStr;
                        InfoTextBox.Text = infoStr;
                        UpdateButton.Enabled = shouldUpdate;
                        UpdateButton.Text = shouldUpdate ? "开始更新" : "不需要更新";
                    }));
                    SharpUtils.StopProcessByPath(Path.Combine(Application.StartupPath, "SharpUpdater.exe"));
                } catch (Exception ex) {
                    Console.WriteLine($"CheckUpdate failed error={ex.Message}");
                    Invoke(new Action(() => {
                        InfoTextBox.Text = $"未知错误：URL={url} ERROR={ex}";
                        InfoTextBox.ForeColor = Color.Red;
                        InfoTextBox.Font = new System.Drawing.Font(InfoTextBox.Font.Name, 8F);
                    }));
                }
            }
        }

        private void InfoTextBox_LinkClicked(object sender, LinkClickedEventArgs e) {
            Console.WriteLine($"InfoTextBox_LinkClicked {e.LinkText}");
            Process.Start(e.LinkText);
        }

        // http://simplygenius.net/Article/AncillaryAsyncProgress
        // https://devblogs.microsoft.com/dotnet/async-in-4-5-enabling-progress-and-cancellation-in-async-apis/
        private async Task<string> DownloadFileAsync(VersionInfo info, IProgress<int> progress) {
            // url for test
            Uri uri = new Uri(("https://download.fastgit.org/Grasscutters/Grasscutter/releases/download/v1.1.0/grasscutter-1.1.0.jar"));
            Console.WriteLine($"DownloadFileAsync url={uri}");
            var downloadPath = Path.Combine(Application.StartupPath, ".temp");
            //var filename = Path.GetFileName(uri.LocalPath);
            var filename = "Update.zip";
            string filepath = Path.Combine(downloadPath, filename);
            var tempfile = Path.Combine(downloadPath, filename + ".tmp");
            // make io operations async
            await Task.Run(() => {
                if (!Directory.Exists(downloadPath)) {
                    Directory.CreateDirectory(downloadPath);
                    File.SetAttributes(downloadPath, FileAttributes.Hidden);
                }
                if (File.Exists(filepath)) {
                    File.Delete(filepath);
                }
                if (File.Exists(tempfile)) {
                    File.Delete(tempfile);
                }
            });
            using (var client = new WebClient()) {
                try {
                    client.DownloadProgressChanged += (o, e) => {
                        progress?.Report(e.ProgressPercentage);
                    };
                    await client.DownloadFileTaskAsync(uri, tempfile);
                    Console.WriteLine($"DownloadFileAsync file={tempfile}");
                    File.Move(tempfile, filepath);
                    return filepath;
                } catch (Exception ex) {
                    Console.WriteLine($"DownloadFileAsync error={ex}");
                    return null;
                }
            }

        }

        private void InstallUpdate(VersionInfo info, string filepath) {
            // Normalizes the path.
            var extractPath = Path.GetFullPath(Application.StartupPath);

            // Ensures that the last character on the extraction path
            // is the directory separator char.
            // Without this, a malicious zip file could try to traverse outside of the expected
            // extraction path.
            if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                extractPath += Path.DirectorySeparatorChar;
            if (!Directory.Exists(extractPath)) {
                Directory.CreateDirectory(extractPath);
            }
            extractPath = Path.GetFullPath(extractPath);
            Console.WriteLine(extractPath);
            using (ZipArchive archive = ZipFile.OpenRead(filepath)) {
                foreach (ZipArchiveEntry entry in archive.Entries) {
                    Console.WriteLine("Processing: " + entry.FullName);
                    if (entry.Length == 0) {
                        continue;
                    }
                    string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));
                    if (File.Exists(destinationPath)) {
                        continue;
                    }
                    Console.WriteLine("Extractint to " + destinationPath);
                    FileInfo fileInfo = new FileInfo(destinationPath);
                    fileInfo.Directory.Create();
                    entry.ExtractToFile(destinationPath, false);
                }
            }
        }

        private async void UpdateButton_Click(object sender, EventArgs e) {
            if (NewVersion == null) {
                return;
            }
            var p = new Progress<int>(value => {
                AProgressBar.Value = value;
            });
            UpdateButton.Enabled = false;
            UpdateButton.Text = "正在下载...";
            AProgressBar.Visible = true;
            AProgressBar.Value = 0;
            var filepath = await DownloadFileAsync(NewVersion, p);
            UpdateButton.Text = "正在安装...";
            InstallUpdate(NewVersion, filepath);
        }
    }
}
