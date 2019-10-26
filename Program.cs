using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading;
using WinSCP;

namespace updater {

    class Program {

        private static string _lastFileName = "";
        static string directory = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
        static string dir_terminal = directory + @"\terminal\";

        [DllImport ("kernel32.dll")]
        static extern IntPtr GetConsoleWindow ();

        [DllImport ("user32.dll")]
        static extern bool ShowWindow (IntPtr hWnd, int nCmdShow);

        const int SwHide = 0;
        const int SwShow = 5;
        private static void SessionFileTransferProgress (
            object sender, FileTransferProgressEventArgs e) {
            if ((_lastFileName != null) && (_lastFileName != e.FileName)) {
                Console.WriteLine ();
            }

            Console.Write ("\r{0} ({1:P0})", e.FileName, e.FileProgress);

            _lastFileName = e.FileName;
        }
        public static void Copy (string sourceDirectory, string targetDirectory) {
            var diSource = new DirectoryInfo (sourceDirectory);
            var diTarget = new DirectoryInfo (targetDirectory);
            CopyAll (diSource, diTarget);
        }

        public static void CopyAll (DirectoryInfo source, DirectoryInfo target) {
            Directory.CreateDirectory (target.FullName);

            foreach (FileInfo fi in source.GetFiles ()) {
                fi.CopyTo (Path.Combine (target.FullName, fi.Name), true);
                Console.WriteLine ("Идет копирование файл: " + fi.ToString ());
            }
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories ()) {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory (diSourceSubDir.Name);
                CopyAll (diSourceSubDir, nextTargetSubDir);
            }
        }
        public static void Download () {
            try {
                SessionOptions sessionOptions = new SessionOptions {
                    Protocol = Protocol.Ftp,
                    HostName = "",
                    UserName = "",
                    Password = "",

                };
                using (Session session = new Session ()) {
                    session.FileTransferProgress += SessionFileTransferProgress;
                    session.FileTransferred += FileTransferred;

                    TransferOptions transferOptions = new TransferOptions ();

                    transferOptions.FileMask = "*<=7D";

                    
                    session.Open (sessionOptions);
                    try {
                        session.GetFiles ("htdocs.zip", @"C:\temp\").Check ();
                        session.GetFiles ("terminal.zip", @"C:\temp\terminal\").Check ();
                    } finally {
                        if (_lastFileName != null) {
                            Console.WriteLine ();
                        }
                    }

                    
                    Console.WriteLine (@"Удаляется предыдущая версия продукта htdocs");
                    DirectoryInfo old = new DirectoryInfo (@"C:\xampp\htdocs\");
                    foreach (FileInfo file in old.EnumerateFiles ()) {
                        file.Delete ();
                    }
                    foreach (DirectoryInfo dir in old.EnumerateDirectories ()) {
                        dir.Delete (true);
                    }

                    Console.WriteLine (@"Удаляется предыдущая версия продукта terminal");
                    DirectoryInfo oldTerminal = new DirectoryInfo (dir_terminal);
                    foreach (FileInfo file in oldTerminal.EnumerateFiles ()) {
                        file.Delete ();
                    }
                    foreach (DirectoryInfo dir in oldTerminal.EnumerateDirectories ()) {
                        dir.Delete (true);
                    }
                }
            } catch (Exception ex) {
                Console.Write (ex.ToString ());
            }
        }
        private static void FileTransferred (object sender, TransferEventArgs e) {
            if (e.Error == null) {
                Console.WriteLine ("\n" + @"Загружен {0} успешно", e.FileName);
            } else {
                Console.WriteLine ("\n" + @"Не загружен {0} ошибка: {1}", e.FileName, e.Error);
            }
            if (e.Chmod != null) {
                if (e.Chmod.Error == null) {
                    Console.WriteLine (
                        "Разрешения {0} установить {1}", e.Chmod.FileName, e.Chmod.FilePermissions);
                } else {
                    Console.WriteLine (
                        "Установка разрешения  {0} неудача: {1}", e.Chmod.FileName, e.Chmod.Error);
                }
            } else {
                Console.WriteLine ("Разрешения {0} по умолчанию", e.Destination);
            }
            if (e.Touch != null) {
                if (e.Touch.Error == null) {
                    Console.WriteLine (
                        "Временная метка {0} установлена в {1}", e.Touch.FileName, e.Touch.LastWriteTime);
                } else {
                    Console.WriteLine (
                        "Временная метка {0} не установлена в {1}", e.Touch.FileName, e.Touch.Error);
                }
            } else {
                Console.WriteLine (
                    "Временная метка {0} сохраняется по умолчанию (текущее время)", e.Destination);
            }
        }
        static void Main (string[] args) {

            Process.Start ("taskkill", "/F /IM HtmlUsingRuntimeT4.exe");

            var handle = GetConsoleWindow ();
            
            
            
            ShowWindow (handle, SwShow);
            try {
                ProcessStartInfo proc2 = new ProcessStartInfo {
                    UseShellExecute = false,
                    FileName = @"C:\windows\system32\cmd.exe",
                    Arguments = @"/c mkdir C:\temp"
                };
                Process.Start (proc2);
                string directoryPrgFile = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
                
                ProcessStartInfo schedulerKill = new ProcessStartInfo ("at.exe");
                schedulerKill.UseShellExecute = false;
                
                schedulerKill.RedirectStandardInput = true;
                schedulerKill.RedirectStandardOutput = true;
                schedulerKill.Arguments = "/delete /yes";
                Process procKill = new Process { StartInfo = schedulerKill };
                procKill.Start ();
                
                ProcessStartInfo scheduler = new ProcessStartInfo ("at.exe") {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    Arguments = "3:00 /next:wednesday " + Environment.CurrentDirectory +
                    @"\updater.exe"
                };
                
                Process proc = new Process { StartInfo = scheduler };
                proc.Start ();
                ProcessStartInfo proc1 = new ProcessStartInfo {
                    UseShellExecute = false,
                    FileName = @"C:\windows\system32\cmd.exe",
                    Arguments = @"/c rmdir /s/q C:\temp"
                };
                try {
                    Process.Start (proc1);
                } catch (Exception exception) {
                    Console.WriteLine (exception.ToString ());
                }
                ProcessStartInfo proc4 = new ProcessStartInfo {
                    UseShellExecute = false,
                    FileName = @"C:\windows\system32\cmd.exe",
                    Arguments = @"/c mkdir C:\temp"
                };

                Process.Start (proc4);
                Download ();
                Thread.Sleep (1000);
                string zipPath = @"c:/temp/htdocs.zip";
                string extractPath = @"c:/xampp/";
                Console.WriteLine (@"Идет распаковка архива");
                ZipFile.ExtractToDirectory (zipPath, extractPath);
                string zipPathTerminal = @"c:/temp/terminal/terminal.zip";
                Console.WriteLine (@"Идет распаковка архива");
                directoryPrgFile += @"\terminal\";
                ZipFile.ExtractToDirectory (zipPathTerminal, directoryPrgFile);

                DirectoryInfo temp = new DirectoryInfo (@"C:/temp/");
                foreach (FileInfo file in temp.EnumerateFiles ()) {
                    file.Delete ();
                }
                foreach (DirectoryInfo dir in temp.EnumerateDirectories ()) {
                    dir.Delete (true);
                }
                Console.WriteLine (@"Обновление завершено");

                Process.Start (dir_terminal + @"\HtmlUsingRuntimeT4\", "");
            } catch (Exception exception) {
                Console.Write (exception.ToString ());
                Console.ReadKey ();
            }
        }
    }
}