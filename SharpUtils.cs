using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Diagnostics;

namespace SharpUpdater {
    internal static class SharpUtils {

        public static bool StopProcessByPath(string fullpath) {
            Console.WriteLine($"StopProcessByPath fullpath={fullpath}");
            var fileName = Path.GetFileName(fullpath);
            var moduleName = Path.GetFileNameWithoutExtension(fileName);
            try {
                Process[] existing = Process.GetProcessesByName("v2rayn");
                foreach (Process p in existing) {
                    Console.WriteLine($"StopProcessByName process={p.ProcessName} {p.Id} {p.MainModule.FileName} {p.MainModule.ModuleName}");
                    string path = p.MainModule.FileName;
                    if (path == fullpath) {
                        //p.Kill();
                        //p.WaitForExit(100);
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"StopProcessByPath {ex}");
            }
            return true;
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
