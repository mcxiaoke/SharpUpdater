using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO.Compression;

namespace SharpUpdater {
    internal static class SharpUtils {

        public static string SimpleRelativePath(string relativeTo, string path) {
            return Path.GetFullPath(path).Substring(Path.GetFullPath(relativeTo).Length + 1);
        }

        public static void CheckOrCreateDirectory(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        public static void UnzipFile(string zipSource,
            string zipDest,
            bool backupOld = false,
            bool stripPrefix = false, string prefixStr = null) {
            var zipPath = Path.GetFullPath(zipSource);
            var destPath = Path.GetFullPath(zipDest);
            if (!File.Exists(zipPath)) { return; }
            var backupPath = backupOld ? Path.Combine(destPath, "backups", DateTime.Now.ToString("yyyyMMdd_HHmmss")) : null;
            Console.WriteLine($"UnzipFile \nSRC={zipPath} \nDST={destPath} \nbackup={backupPath} " +
                $"\nstrip={stripPrefix} prefix={prefixStr}");

            CheckOrCreateDirectory(destPath);
            if (!string.IsNullOrWhiteSpace(backupPath)) {
                CheckOrCreateDirectory(backupPath);
            }
            using (ZipArchive archive = ZipFile.OpenRead(zipPath)) {
                var entries = archive.Entries;
                string prefix = string.Empty;
                if (stripPrefix) {
                    if (!string.IsNullOrWhiteSpace(prefixStr) && prefixStr.EndsWith("/")) {
                        prefix = prefixStr;
                    } else {
                        var fileNames = entries.Select(e => e.FullName);
                        //Console.WriteLine(string.Join("\n", fileNames.ToArray()));
                        var prefixFound = GetCommonStringPrefix(fileNames);
                        if (!string.IsNullOrWhiteSpace(prefixFound) && prefixFound.EndsWith("/")) {
                            prefix = prefixFound;
                        }
                    }
                }
                Console.WriteLine($"UnzipFile prefix={prefix}");
                foreach (ZipArchiveEntry entry in entries) {
                    //Console.WriteLine(entry.FullName);
                    if (entry.Length == 0) {
                        Console.WriteLine("skip=" + entry.FullName);
                        continue;
                    }
                    var fullName = entry.FullName;
                    //Console.WriteLine($"fullName old={fullName}");
                    if (!string.IsNullOrWhiteSpace(prefix)) {
                        if (fullName.StartsWith(prefix)) {
                            fullName = fullName.Remove(0, prefix.Length);
                        }
                        //Console.WriteLine($"fullName new={fullName}");
                    }
                    string destinationPath = Path.GetFullPath(Path.Combine(destPath, fullName));
                    //Console.WriteLine($"dest={destinationPath}");
                    if (File.Exists(destinationPath)) {
                        var rp = SimpleRelativePath(destPath, destinationPath);
                        //Console.WriteLine($"relative={rp}");
                        var destinationBackupPath = Path.Combine(backupPath, rp);
                        var destinationBackupDir = Path.GetDirectoryName(destinationBackupPath);
                        if (!Directory.Exists(destinationBackupDir)) {
                            Directory.CreateDirectory(destinationBackupDir);
                        }
                        //Console.WriteLine($"backup={destinationBackupPath}");
                        File.Copy(destinationPath, destinationBackupPath);
                    }

                    Console.WriteLine("UnzipFile unzip " + destinationPath);
                    FileInfo fileInfo = new FileInfo(destinationPath);
                    fileInfo.Directory.Create();
                    entry.ExtractToFile(destinationPath, true);
                }
            }
            Console.WriteLine($"UnzipFile done.");
        }

        public static string FormatFileSize(long lSize) {
            double size = lSize;
            int index = 0;
            for (; size > 1024; index++)
                size /= 1024;
            return size.ToString("0.00 " + new[] { "B", "KB", "MB", "GB", "TB" }[index]);
        }

        // slow
        public static string GetCommonStringPrefix2(IEnumerable<string> strings) {
            var commonPrefix = strings.FirstOrDefault() ?? "";
            foreach (var s in strings) {
                var potentialMatchLength = Math.Min(s.Length, commonPrefix.Length);

                if (potentialMatchLength < commonPrefix.Length)
                    commonPrefix = commonPrefix.Substring(0, potentialMatchLength);

                for (var i = 0; i < potentialMatchLength; i++) {
                    if (s[i] != commonPrefix[i]) {
                        commonPrefix = commonPrefix.Substring(0, i);
                        break;
                    }
                }
            }
            return commonPrefix;
        }

        // https://stackoverflow.com/questions/2070356 fast
        public static string GetCommonStringPrefix(IEnumerable<string> strings) {
            var keys = strings.ToArray();
            Array.Sort(keys, StringComparer.InvariantCulture);
            string a1 = keys[0], a2 = keys[keys.Length - 1];
            int L = a1.Length, i = 0;
            while (i < L && a1[i] == a2[i]) {
                i++;
            }
            return a1.Substring(0, i);
        }

        public static List<string> GetFilesInFolder(string path) {
            return Directory.GetFiles(path).Select(it => Path.GetFileName(it)).ToList();
        }

        public static Exception StopProcessByPath(string fullpath) {
            Console.WriteLine($"StopProcessByPath fullpath={fullpath}");
            var fileName = Path.GetFileName(fullpath);
            var moduleName = Path.GetFileNameWithoutExtension(fileName);
            try {
                Process[] existing = Process.GetProcessesByName(moduleName);
                foreach (Process p in existing) {
                    Console.WriteLine($"StopProcessByName process={p.ProcessName} {p.Id} {p.MainModule.FileName} {p.MainModule.ModuleName}");
                    string path = p.MainModule.FileName;
                    if (path == fullpath) {
                        p.Kill();
                        p.WaitForExit(100);
                    }
                }
                return null;
            } catch (Exception ex) {
                Console.WriteLine($"StopProcessByPath error={ex}");
                return ex;
            }
        }

        public static (string program, string url) ReadConfig(string filename = "SharpUpdater.json") {
            var filepath = Path.Combine(Application.StartupPath, filename);
            if (!File.Exists(filepath)) {
                return default;
            }
            try {
                var content = File.ReadAllText(filepath, Encoding.UTF8);
                dynamic o = JsonConvert.DeserializeObject(content);
                var p = (string)o.program;
                var u = (string)o.url;
                Console.WriteLine($"ReadConfig program={p} url={u}");
                return (p, u);
            } catch (Exception ex) {
                Console.WriteLine($"ReadConfig error={ex}");
                return default;
            }
        }

        public static bool WriteConfig(string p, string u, string filename = "SharpUpdater.json") {
            var filepath = Path.Combine(Application.StartupPath, filename);
            try {
                var obj = new {
                    program = p,
                    url = u
                };
                var content = JsonConvert.SerializeObject(obj);
                Console.WriteLine($"WriteConfig content={content}");
                File.WriteAllText(filepath, content, Encoding.UTF8);
                return true;
            } catch (Exception ex) {
                Console.WriteLine($"WriteConfig error={ex}");
                return false;
            }
        }

    }
}
