using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NyrionShell
{
    class Program
    {
        // Constants and global state
        static readonly string NYRION_VERSION = "Atlas";
        static readonly string BUILD = "26S1.0";
        static readonly string PREFIX = "QopWarren Build 6018 - Prefix QWE";

        static readonly List<string> CommandHistory = new List<string>();
        static bool EchoOn = true;
        static readonly DateTime StartTime = DateTime.Now;
        static readonly Random Rng = new Random();

        static HttpListener CurrentHttpListener = null;
        static bool HttpServerRunning = false;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            Console.WriteLine(NYRION_VERSION);
            Console.WriteLine(BUILD);
            Console.WriteLine(PREFIX);

            MainLoop();
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;

            if (HttpServerRunning && CurrentHttpListener != null)
            {
                try { CurrentHttpListener.Stop(); } catch { }
                HttpServerRunning = false;
                Console.WriteLine();
                Console.WriteLine("HTTP server stopped.");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Use 'qwe exit' to quit.");
            }
        }

        static void MainLoop()
        {
            while (true)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Nyrion:" + Directory.GetCurrentDirectory() + "> ");
                    Console.ResetColor();

                    string input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    ExecuteCommand(input);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unexpected error: " + ex.Message);
                    Console.ResetColor();
                }
            }
        }

        static void ExecuteCommand(string cmd, bool echo = true)
        {
            if (echo && EchoOn)
            {
                Console.WriteLine(cmd);
            }

            CommandHistory.Add(cmd);
            if (CommandHistory.Count > 100)
                CommandHistory.RemoveAt(0);

            string[] parts = cmd.Trim()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                Console.WriteLine("Command must start with 'qwe' and have a subcommand.");
                return;
            }

            if (!parts[0].Equals("qwe", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Commands must start with 'qwe'");
                return;
            }

            string command = parts[1].ToLowerInvariant();
            string[] args = parts.Skip(2).ToArray();

            try
            {
                switch (command)
                {
                    case "help":
                        QweShowHelp();
                        break;
                    case "time":
                        QweShowTime();
                        break;
                    case "date":
                        QweShowDate();
                        break;
                    case "cls":
                        Console.Clear();
                        break;
                    case "dir":
                        QweListDir();
                        break;
                    case "cd":
                        if (args.Length > 0) QweChangeDir(args[0]);
                        else Console.WriteLine("Missing directory.");
                        break;
                    case "type":
                        if (args.Length > 0) QweTypeFile(args[0]);
                        else Console.WriteLine("Missing filename.");
                        break;
                    case "copy":
                        if (args.Length == 2) QweCopyFile(args[0], args[1]);
                        else Console.WriteLine("Syntax: qwe copy <src> <dst>");
                        break;
                    case "move":
                        if (args.Length == 2) QweMoveFile(args[0], args[1]);
                        else Console.WriteLine("Syntax: qwe move <src> <dst>");
                        break;
                    case "del":
                        if (args.Length > 0) QweDeleteFile(args[0]);
                        else Console.WriteLine("Missing filename.");
                        break;
                    case "ren":
                        if (args.Length == 2) QweRenameFile(args[0], args[1]);
                        else Console.WriteLine("Syntax: qwe ren <old> <new>");
                        break;
                    case "mkdir":
                        if (args.Length > 0) QweMkdir(args[0]);
                        else Console.WriteLine("Missing directory name.");
                        break;
                    case "rmdir":
                        if (args.Length > 0) QweRmdir(args[0]);
                        else Console.WriteLine("Missing directory name.");
                        break;
                    case "echo":
                        QweEcho(args);
                        break;
                    case "pause":
                        QwePause();
                        break;
                    case "whoami":
                        QweWhoAmI();
                        break;
                    case "hostname":
                        QweHostname();
                        break;
                    case "ls":
                        QweLs(args);
                        break;
                    case "cp":
                        QweCp(args);
                        break;
                    case "mv":
                        QweMv(args);
                        break;
                    case "rm":
                        QweRm(args);
                        break;
                    case "chmod":
                        QweChmod(args);
                        break;
                    case "stat":
                        QweStat(args);
                        break;
                    case "find":
                        QweFind(args);
                        break;
                    case "open":
                        QweOpen(args);
                        break;
                    case "edit":
                        QweEdit(args);
                        break;
                    case "base64":
                        QweBase64(args);
                        break;
                    case "hex":
                        QweHex(args);
                        break;
                    case "tar":
                        QweTar(args);
                        break;
                    case "untar":
                        QweUntar(args);
                        break;
                    case "gzip":
                        QweGzip(args);
                        break;
                    case "gunzip":
                        QweGunzip(args);
                        break;
                    case "http":
                        QweHttp(args);
                        break;
                    case "curl":
                        QweCurl(args);
                        break;
                    case "ip":
                        QweIp(args);
                        break;
                    case "ping":
                        QwePing(args);
                        break;
                    case "kill":
                        QweKill(args);
                        break;
                    case "ps":
                        QwePs(args);
                        break;
                    case "df":
                        QweDf(args);
                        break;
                    case "du":
                        QweDu(args);
                        break;
                    case "calc":
                        QweCalculator();
                        break;
                    case "specs":
                        QweSpecs();
                        break;
                    case "nyrver":
                        QweNyrver();
                        break;
                    case "base":
                        QweBase();
                        break;
                    case "script":
                        QweScript();
                        break;
                    case "wmip":
                        QweWmip();
                        break;
                    case "tree":
                        QweTree(args);
                        break;
                    case "touch":
                        QweTouch(args);
                        break;
                    case "head":
                        QweHead(args);
                        break;
                    case "tail":
                        QweTail(args);
                        break;
                    case "info":
                        QweInfo();
                        break;
                    case "sysinfo":
                        QweSysinfo();
                        break;
                    case "history":
                        QweHistory();
                        break;
                    case "joke":
                        QweJoke();
                        break;
                    case "crashtest":
                        QweCrashtest();
                        break;
                    case "reboot":
                        QweReboot();
                        break;
                    case "uptime":
                        QweUptime();
                        break;
                    case "exit":
                        Console.WriteLine("Exiting Nyrion shell...");
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("'" + command + "' is not recognized as a QopWarren command.");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error executing command: " + e.Message);
            }
        }

        // ==== Core helpers ====

        static void WriteColor(ConsoleColor color, string text, bool newLine = true)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            if (newLine) Console.WriteLine(text);
            else Console.Write(text);
            Console.ForegroundColor = old;
        }

        static bool Confirm(string prompt)
        {
            Console.Write(prompt);
            string ans = Console.ReadLine();
            if (ans == null) return false;
            ans = ans.Trim();
            return ans.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                   ans.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        static string FormatSize(long n, bool human)
        {
            if (!human) return n.ToString();
            string[] units = { "B", "K", "M", "G", "T", "P" };
            double v = n;
            int i = 0;
            while (v >= 1024 && i < units.Length - 1)
            {
                v /= 1024;
                i++;
            }
            return string.Format("{0:0}{1}", v, units[i]);
        }

        static long DirSize(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    return new FileInfo(path).Length;
                }

                long size = 0;
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        size += new FileInfo(file).Length;
                    }
                    catch
                    {
                    }
                }
                return size;
            }
            catch
            {
                return 0;
            }
        }

        static int RunExternal(string fileName, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = false
            };
            using (var proc = Process.Start(psi))
            {
                if (proc == null) return -1;
                proc.WaitForExit();
                return proc.ExitCode;
            }
        }

        // ==== Commands ====

        static void QweShowHelp()
        {
            WriteColor(ConsoleColor.Cyan, "Available QopWarren commands:");
            Console.WriteLine(" qwe help          - Show this help list");
            Console.WriteLine(" qwe time          - Show current time");
            Console.WriteLine(" qwe date          - Show current date");
            Console.WriteLine(" qwe cls           - Clear screen");
            Console.WriteLine(" qwe dir           - List directory files");
            Console.WriteLine(" qwe cd <dir>      - Change directory");
            Console.WriteLine(" qwe type <file>   - Display file contents");
            Console.WriteLine(" qwe copy <src> <dst> - Copy file");
            Console.WriteLine(" qwe move <src> <dst> - Move file");
            Console.WriteLine(" qwe del <file>    - Delete file");
            Console.WriteLine(" qwe ren <old> <new>  - Rename file");
            Console.WriteLine(" qwe mkdir <dir>   - Make directory");
            Console.WriteLine(" qwe rmdir <dir>   - Remove directory");
            Console.WriteLine(" qwe echo <text>   - Print text / toggle echo");
            Console.WriteLine(" qwe pause         - Pause for keypress");
            Console.WriteLine(" qwe calc          - Calculator");
            Console.WriteLine(" qwe specs         - Show system specs");
            Console.WriteLine(" qwe nyrver        - Show Nyrion version");
            Console.WriteLine(" qwe base          - Show underlying OS");
            Console.WriteLine(" qwe script        - Simple script editor");
            Console.WriteLine(" qwe find <pattern> [path] - Find file");
            Console.WriteLine(" qwe edit <file>   - Edit a text file");
            Console.WriteLine(" qwe sysinfo       - Show system info");
            Console.WriteLine(" qwe history       - Shows last 5 commands");
            Console.WriteLine(" qwe joke          - Tell a joke");
            Console.WriteLine(" qwe reboot        - Restart shell");
            Console.WriteLine(" qwe exit          - Exit shell");
            Console.WriteLine(" qwe whoami        - Who are you?");
            Console.WriteLine(" qwe hostname      - What's the hostname?");
            Console.WriteLine(" qwe ls            - List directories");
            Console.WriteLine(" qwe cp            - Copy (Unix-style)");
            Console.WriteLine(" qwe mv            - Move (Unix-style)");
            Console.WriteLine(" qwe rm            - Remove file(s)");
            Console.WriteLine(" qwe chmod         - Change file permissions");
            Console.WriteLine(" qwe stat          - Check file info");
            Console.WriteLine(" qwe open <file>   - Open a file with default app");
            Console.WriteLine(" qwe base64        - Base64 encode/decode");
            Console.WriteLine(" qwe hex           - Hex dump");
            Console.WriteLine(" qwe tar           - Tar a file/dir (uses external 'tar')");
            Console.WriteLine(" qwe untar         - Untar a file (uses external 'tar')");
            Console.WriteLine(" qwe gzip          - Gzip a file");
            Console.WriteLine(" qwe gunzip        - Gunzip a file");
            Console.WriteLine(" qwe http [port]   - Start a simple HTTP server");
            Console.WriteLine(" qwe curl URL [outfile] - Download a URL");
            Console.WriteLine(" qwe ip            - Check your IP address");
            Console.WriteLine(" qwe ping HOST [count] - Ping a host");
            Console.WriteLine(" qwe kill PID      - Kill a process");
            Console.WriteLine(" qwe ps            - List processes");
            Console.WriteLine(" qwe df [path]     - Disk usage");
            Console.WriteLine(" qwe du PATH       - Directory size");
            Console.WriteLine(" qwe wmip          - Show IP address (simple)");
            Console.WriteLine(" qwe tree [path]   - Directory tree");
            Console.WriteLine(" qwe touch <file>  - Create/update file timestamp");
            Console.WriteLine(" qwe head <file>   - First 10 lines");
            Console.WriteLine(" qwe tail <file>   - Last 10 lines");
            Console.WriteLine(" qwe info          - Secret info");
            Console.WriteLine(" qwe crashtest     - Simulated crash + reboot");
            Console.WriteLine(" qwe uptime        - Show shell uptime");
        }

        static void QweShowTime()
        {
            Console.WriteLine(DateTime.Now.ToString("ddd MMM dd HH:mm:ss yyyy"));
        }

        static void QweShowDate()
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        static void QweListDir()
        {
            try
            {
                foreach (var entry in Directory.GetFileSystemEntries(Directory.GetCurrentDirectory()))
                {
                    string name = Path.GetFileName(entry);
                    if (Directory.Exists(entry))
                    {
                        WriteColor(ConsoleColor.Cyan, name + "\\");
                    }
                    else
                    {
                        Console.WriteLine(name);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error listing directory: " + e.Message);
            }
        }

        static void QweChangeDir(string path)
        {
            try
            {
                Directory.SetCurrentDirectory(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("Bad command or filename: " + e.Message);
            }
        }

        static void QweTypeFile(string filename)
        {
            try
            {
                Console.WriteLine(File.ReadAllText(filename));
            }
            catch (Exception e)
            {
                Console.WriteLine("File not found: " + e.Message);
            }
        }

        static void QweEcho(string[] args)
        {
            if (args.Length == 0)
            {
                WriteColor(ConsoleColor.Red, "The syntax of the command is incorrect.");
                return;
            }

            string arg0 = args[0].ToLowerInvariant();
            if (arg0 == "off")
            {
                EchoOn = false;
            }
            else if (arg0 == "on")
            {
                EchoOn = true;
            }
            else
            {
                Console.WriteLine(string.Join(" ", args));
            }
        }

        static void QweHostname()
        {
            Console.WriteLine(Dns.GetHostName());
        }

        static void QweWhoAmI()
        {
            Console.WriteLine(Environment.UserName);
        }

        static string FormatPermissions(FileSystemInfo fsi)
        {
            bool isDir = (fsi.Attributes & FileAttributes.Directory) != 0;
            char typeChar = isDir ? 'd' : '-';
            // Simple, fixed perms since Windows doesn't have Unix perms
            string perms = isDir ? "rwxr-xr-x" : "rw-r--r--";
            return typeChar + perms;
        }

        static void QweLs(string[] args)
        {
            bool showAll = args.Contains("-a");
            bool @long = args.Contains("-l");
            bool human = args.Contains("-h");

            var paths = args.Where(a => !a.StartsWith("-"))
                            .DefaultIfEmpty(".")
                            .ToArray();

            foreach (var path in paths)
            {
                string p = path;
                try
                {
                    if (Directory.Exists(p))
                    {
                        var dir = new DirectoryInfo(p);
                        var entries = dir.GetFileSystemInfos();

                        if (!showAll)
                        {
                            entries = entries
                                .Where(e => !e.Name.StartsWith("."))
                                .ToArray();
                        }

                        if (paths.Length > 1)
                        {
                            WriteColor(ConsoleColor.Cyan, p + ":");
                        }

                        foreach (var e in entries.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase))
                        {
                            string name = e.Name + ((e.Attributes & FileAttributes.Directory) != 0 ? Path.DirectorySeparatorChar.ToString() : "");
                            if (@long)
                            {
                                long size = 0;
                                if ((e.Attributes & FileAttributes.Directory) == 0)
                                {
                                    size = ((FileInfo)e).Length;
                                }
                                string perms = FormatPermissions(e);
                                string mtime = e.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                                Console.WriteLine("{0} {1,8} {2} {3}", perms, FormatSize(size, human), mtime, name);
                            }
                            else
                            {
                                Console.WriteLine(name);
                            }
                        }
                    }
                    else if (File.Exists(p))
                    {
                        Console.WriteLine(Path.GetFileName(p));
                    }
                    else
                    {
                        WriteColor(ConsoleColor.Red, "ls error: path not found: " + p);
                    }
                }
                catch (Exception e)
                {
                    WriteColor(ConsoleColor.Red, "ls error: " + e.Message);
                }
            }
        }

        static void QweCp(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: cp [-r] SRC DST");
                return;
            }

            bool recursive = args.Contains("-r") || args.Contains("/s");
            var srcDst = args.Where(a => !a.StartsWith("-")).ToArray();

            if (srcDst.Length != 2)
            {
                Console.WriteLine("Usage: cp [-r] SRC DST");
                return;
            }

            string src = srcDst[0];
            string dst = srcDst[1];

            try
            {
                if (Directory.Exists(src))
                {
                    if (!recursive)
                    {
                        WriteColor(ConsoleColor.Red, "cp: -r required for directories");
                        return;
                    }
                    DirectoryCopy(src, dst, true);
                }
                else
                {
                    File.Copy(src, dst, true);
                }
                Console.WriteLine("Copied.");
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "cp error: " + e.Message);
            }
        }

        static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists) throw new DirectoryNotFoundException("Source dir not found: " + sourceDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destDirName);

            foreach (FileInfo file in dir.GetFiles())
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, true);
                }
            }
        }

        static void QweStat(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: stat FILE");
                return;
            }

            string p = args[0];
            try
            {
                if (File.Exists(p))
                {
                    var st = new FileInfo(p);
                    Console.WriteLine("Size: {0} bytes", st.Length);
                    Console.WriteLine("Mode: {0}", FormatPermissions(st));
                    Console.WriteLine("Modified: {0}", st.LastWriteTime);
                    Console.WriteLine("Created:  {0}", st.CreationTime);
                    Console.WriteLine("Inode: n/a");
                }
                else if (Directory.Exists(p))
                {
                    var st = new DirectoryInfo(p);
                    Console.WriteLine("Size: {0} bytes (files only)", DirSize(p));
                    Console.WriteLine("Mode: {0}", FormatPermissions(st));
                    Console.WriteLine("Modified: {0}", st.LastWriteTime);
                    Console.WriteLine("Created:  {0}", st.CreationTime);
                    Console.WriteLine("Inode: n/a");
                }
                else
                {
                    WriteColor(ConsoleColor.Red, "stat error: path does not exist");
                }
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "stat error: " + e.Message);
            }
        }

        static void QweRm(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: rm [-r] [-f] TARGET...");
                return;
            }

            bool recursive = args.Contains("-r") || args.Contains("/s");
            bool force = args.Contains("-f") || args.Contains("/f");
            var targets = args.Where(a => !a.StartsWith("-")).ToArray();

            foreach (var t in targets)
            {
                try
                {
                    if (Directory.Exists(t))
                    {
                        if (!recursive)
                        {
                            WriteColor(ConsoleColor.Red, "rm: '" + t + "' is a directory (use -r)");
                            continue;
                        }
                        if (!force && !Confirm("Recursively delete '" + t + "'? [y/N]: "))
                        {
                            Console.WriteLine("Skipped.");
                            continue;
                        }
                        Directory.Delete(t, true);
                    }
                    else if (File.Exists(t))
                    {
                        if (!force && !Confirm("Delete '" + t + "'? [y/N]: "))
                        {
                            Console.WriteLine("Skipped.");
                            continue;
                        }
                        File.Delete(t);
                    }
                    else
                    {
                        WriteColor(ConsoleColor.Red, "rm error: path not found: " + t);
                        continue;
                    }
                    Console.WriteLine("Deleted: " + t);
                }
                catch (Exception e)
                {
                    WriteColor(ConsoleColor.Red, "rm error: " + e.Message);
                }
            }
        }

        static void QweChmod(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: chmod MODE FILE  (MODE numeric, e.g., 755)");
                return;
            }

            string modeStr = args[0];
            string path = args[1];

            try
            {
                int mode = Convert.ToInt32(modeStr, 8);

                if (!File.Exists(path))
                {
                    WriteColor(ConsoleColor.Red, "chmod error: file not found");
                    return;
                }

                var fi = new FileInfo(path);
                // Simple mapping: if owner write bit not set, mark read-only.
                bool ownerWrite = (mode & 0x80) != 0;
                fi.IsReadOnly = !ownerWrite;

                Console.WriteLine("Mode set (approx).");
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "chmod error: " + e.Message);
            }
        }

        static void QweMv(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: mv SRC DST");
                return;
            }

            string src = args[0];
            string dst = args[1];

            try
            {
                if (Directory.Exists(src))
                {
                    Directory.Move(src, dst);
                }
                else
                {
                    File.Move(src, dst);
                }
                Console.WriteLine("Moved.");
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "mv error: " + e.Message);
            }
        }

        static void QweCopyFile(string src, string dst)
        {
            try
            {
                if (Directory.Exists(src))
                {
                    DirectoryCopy(src, dst, true);
                }
                else
                {
                    File.Copy(src, dst, true);
                }
                Console.WriteLine("Copied.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Copy error: " + e.Message);
            }
        }

        static void QweMoveFile(string src, string dst)
        {
            try
            {
                if (Directory.Exists(src))
                {
                    Directory.Move(src, dst);
                }
                else
                {
                    File.Move(src, dst);
                }
                Console.WriteLine("Moved.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Move error: " + e.Message);
            }
        }

        static void QweDeleteFile(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, false);
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else
                {
                    Console.WriteLine("File/directory not found.");
                    return;
                }
                Console.WriteLine("Deleted.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Delete error: " + e.Message);
            }
        }

        static void QweRenameFile(string oldName, string newName)
        {
            try
            {
                if (Directory.Exists(oldName))
                {
                    Directory.Move(oldName, newName);
                }
                else if (File.Exists(oldName))
                {
                    File.Move(oldName, newName);
                }
                else
                {
                    Console.WriteLine("File/directory not found.");
                    return;
                }
                Console.WriteLine("Renamed.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Rename error: " + e.Message);
            }
        }

        static void QweMkdir(string dirname)
        {
            try
            {
                Directory.CreateDirectory(dirname);
                Console.WriteLine("Directory created: " + dirname);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot create directory: " + e.Message);
            }
        }

        static void QweRmdir(string dirname)
        {
            try
            {
                Directory.Delete(dirname);
                Console.WriteLine("Directory removed: " + dirname);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot remove directory: " + e.Message);
            }
        }

        static void QwePause()
        {
            Console.Write("Press Enter to continue...");
            Console.ReadLine();
        }

        static void QweCalculator()
        {
            Console.WriteLine("Calculator started. Type 'exit' to quit.");
            while (true)
            {
                Console.Write("calc> ");
                string expr = Console.ReadLine();
                if (expr == null) break;
                expr = expr.Trim();
                if (expr.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    expr.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    break;

                try
                {
                    string allowedChars = "0123456789+-*/(). ";
                    if (!expr.All(c => allowedChars.IndexOf(c) >= 0))
                    {
                        Console.WriteLine("Invalid characters in expression.");
                        continue;
                    }

                    var dt = new DataTable();
                    object result = dt.Compute(expr, null);
                    Console.WriteLine(result);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }

        static void QweOpen(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: open FILE");
                return;
            }

            string path = args[0];
            try
            {
                var psi = new ProcessStartInfo(path)
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "open error: " + e.Message);
            }
        }

        static void QweEdit(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: edit FILE");
                return;
            }

            string filename = args[0];
            Console.WriteLine("Editing " + filename + ". Type 'SAVE' alone on a line to save and exit.");
            List<string> lines = new List<string>();

            if (File.Exists(filename))
            {
                try
                {
                    lines = File.ReadAllLines(filename).ToList();
                    Console.WriteLine(string.Join(Environment.NewLine, lines));
                }
                catch
                {
                    Console.WriteLine("Existing file could not be read, editing as new.");
                }
            }
            else
            {
                Console.WriteLine("New file.");
            }

            List<string> newLines = new List<string>();
            while (true)
            {
                string line = Console.ReadLine();
                if (line == null) break;
                if (line.Trim().ToUpperInvariant() == "SAVE")
                    break;
                newLines.Add(line);
            }

            try
            {
                File.WriteAllLines(filename, newLines);
                Console.WriteLine(filename + " saved.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to save file: " + e.Message);
            }
        }

        static void QweBase64(string[] args)
        {
            if (args.Length == 0 || (args[0] != "encode" && args[0] != "decode"))
            {
                Console.WriteLine("Usage: base64 encode|decode <infile|-t TEXT> [outfile]");
                return;
            }

            string mode = args[0];
            byte[] data = null;
            string outfile = null;

            try
            {
                if (args.Length >= 2 && args[1] == "-t")
                {
                    string text = args.Length > 2 ? string.Join(" ", args.Skip(2)) : "";
                    data = Encoding.UTF8.GetBytes(text);
                }
                else if (args.Length >= 2)
                {
                    string infile = args[1];
                    data = File.ReadAllBytes(infile);
                    if (args.Length >= 3)
                    {
                        outfile = args[2];
                    }
                }
                else
                {
                    Console.WriteLine("Usage: base64 encode|decode <infile|-t TEXT> [outfile]");
                    return;
                }

                if (mode == "encode")
                {
                    string encoded = Convert.ToBase64String(data);
                    if (!string.IsNullOrEmpty(outfile))
                    {
                        File.WriteAllText(outfile, encoded);
                        Console.WriteLine("Wrote " + outfile);
                    }
                    else
                    {
                        Console.WriteLine(encoded);
                    }
                }
                else
                {
                    string sData = Encoding.ASCII.GetString(data);
                    byte[] decoded = Convert.FromBase64String(sData);
                    if (!string.IsNullOrEmpty(outfile))
                    {
                        File.WriteAllBytes(outfile, decoded);
                        Console.WriteLine("Wrote " + outfile);
                    }
                    else
                    {
                        try
                        {
                            string txt = Encoding.UTF8.GetString(decoded);
                            Console.WriteLine(txt);
                        }
                        catch
                        {
                            Console.WriteLine(BitConverter.ToString(decoded).Replace("-", ""));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "base64 error: " + e.Message);
            }
        }

        static void QweHex(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: hex FILE [bytes]");
                return;
            }

            string fname = args[0];
            int n = 256;
            if (args.Length > 1)
            {
                int.TryParse(args[1], out n);
            }

            try
            {
                using (var fs = new FileStream(fname, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[n];
                    int read = fs.Read(buffer, 0, n);
                    int offset = 0;

                    while (offset < read)
                    {
                        int len = Math.Min(16, read - offset);
                        byte[] chunk = new byte[len];
                        Array.Copy(buffer, offset, chunk, 0, len);

                        string hexs = string.Join(" ", chunk.Select(b => b.ToString("x2")).ToArray());
                        string text = new string(chunk.Select(b => (b >= 32 && b < 127) ? (char)b : '.').ToArray());

                        Console.WriteLine("{0:x8}  {1,-47}  {2}", offset, hexs, text);
                        offset += len;
                    }
                }
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "hex error: " + e.Message);
            }
        }

        static void QweTar(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: tar SRC DEST.tar|.tar.gz");
                return;
            }

            string src = args[0];
            string dest = args[1];

            try
            {
                string options = (dest.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase) ||
                                  dest.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase))
                                 ? "-czf"
                                 : "-cf";

                string tarArgs = string.Format("{0} \"{1}\" \"{2}\"", options, dest, src);
                int exit = RunExternal("tar", tarArgs);
                if (exit == 0)
                {
                    Console.WriteLine("Created " + dest);
                }
                else
                {
                    WriteColor(ConsoleColor.Red, "tar error: exit code " + exit);
                }
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "tar error: " + e.Message);
            }
        }

        static void QweUntar(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: untar FILE.tar[.gz] [DEST]");
                return;
            }

            string src = args[0];
            string dest = args.Length > 1 ? args[1] : ".";

            try
            {
                string tarArgs = string.Format("-xf \"{0}\" -C \"{1}\"", src, dest);
                int exit = RunExternal("tar", tarArgs);
                if (exit == 0)
                {
                    Console.WriteLine("Extracted to " + dest);
                }
                else
                {
                    WriteColor(ConsoleColor.Red, "untar error: exit code " + exit);
                }
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "untar error: " + e.Message);
            }
        }

        static void QweGzip(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: gzip FILE");
                return;
            }

            string src = args[0];
            try
            {
                string dst = src + ".gz";
                using (FileStream fIn = new FileStream(src, FileMode.Open, FileAccess.Read))
                using (FileStream fOut = new FileStream(dst, FileMode.Create, FileAccess.Write))
                using (GZipStream gz = new GZipStream(fOut, CompressionMode.Compress))
                {
                    fIn.CopyTo(gz);
                }
                Console.WriteLine("Created " + dst);
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "gzip error: " + e.Message);
            }
        }

        static void QweGunzip(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: gunzip FILE.gz");
                return;
            }

            string src = args[0];
            try
            {
                if (!src.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("gunzip expects a .gz file");
                    return;
                }

                string dst = src.Substring(0, src.Length - 3);
                using (FileStream fIn = new FileStream(src, FileMode.Open, FileAccess.Read))
                using (GZipStream gz = new GZipStream(fIn, CompressionMode.Decompress))
                using (FileStream fOut = new FileStream(dst, FileMode.Create, FileAccess.Write))
                {
                    gz.CopyTo(fOut);
                }
                Console.WriteLine("Created " + dst);
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "gunzip error: " + e.Message);
            }
        }

        static void QweHttp(string[] args)
        {
            int port = 8000;
            if (args.Length > 0)
            {
                int.TryParse(args[0], out port);
                if (port <= 0) port = 8000;
            }

            if (HttpServerRunning)
            {
                Console.WriteLine("HTTP server already running.");
                return;
            }

            try
            {
                var listener = new HttpListener();
                string prefix = "http://localhost:" + port + "/";
                listener.Prefixes.Add(prefix);
                listener.Start();

                CurrentHttpListener = listener;
                HttpServerRunning = true;

                WriteColor(ConsoleColor.Green, "Serving HTTP on " + prefix + " (Ctrl+C to stop)");

                while (HttpServerRunning)
                {
                    HttpListenerContext ctx;
                    try
                    {
                        ctx = listener.GetContext();
                    }
                    catch
                    {
                        break;
                    }

                    try
                    {
                        HandleHttpRequest(ctx);
                    }
                    catch
                    {
                        try { ctx.Response.StatusCode = 500; ctx.Response.Close(); } catch { }
                    }
                }

                listener.Close();
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "http error: " + e.Message);
            }
            finally
            {
                HttpServerRunning = false;
                CurrentHttpListener = null;
                Console.WriteLine("Server stopped.");
            }
        }

        static void HandleHttpRequest(HttpListenerContext ctx)
        {
            string relPath = ctx.Request.Url.AbsolutePath.TrimStart('/');
            if (string.IsNullOrEmpty(relPath))
            {
                relPath = "index.html";
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), relPath);

            if (!File.Exists(filePath))
            {
                ctx.Response.StatusCode = 404;
                byte[] notFound = Encoding.UTF8.GetBytes("404 Not Found");
                ctx.Response.OutputStream.Write(notFound, 0, notFound.Length);
                ctx.Response.OutputStream.Close();
                return;
            }

            byte[] data = File.ReadAllBytes(filePath);
            ctx.Response.ContentType = "application/octet-stream";
            ctx.Response.ContentLength64 = data.Length;
            ctx.Response.OutputStream.Write(data, 0, data.Length);
            ctx.Response.OutputStream.Close();
        }

        static void QweCurl(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: curl URL [outfile]");
                return;
            }

            string url = args[0];
            string outFile;

            if (args.Length > 1)
            {
                outFile = args[1];
            }
            else
            {
                Uri uri = new Uri(url);
                outFile = Path.GetFileName(uri.LocalPath);
                if (string.IsNullOrEmpty(outFile))
                    outFile = "downloaded.file";
            }

            try
            {
                using (var wc = new WebClient())
                {
                    wc.DownloadFile(url, outFile);
                }
                Console.WriteLine("Saved to " + outFile);
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "curl error: " + e.Message);
            }
        }

        static void QweIp(string[] args)
        {
            try
            {
                string host = Dns.GetHostName();
                Console.WriteLine("Host: " + host);
                var addrs = new HashSet<string>();

                foreach (var ip in Dns.GetHostAddresses(host))
                {
                    addrs.Add(ip.ToString());
                }

                foreach (var a in addrs.OrderBy(x => x))
                {
                    Console.WriteLine(a);
                }
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "ip error: " + e.Message);
            }
        }

        static void QwePing(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ping HOST [count]");
                return;
            }

            string host = args[0];
            string count = args.Length > 1 ? args[1] : "4";

            try
            {
                bool isWindows =
                    Environment.OSVersion.Platform == PlatformID.Win32NT ||
                    Environment.OSVersion.Platform == PlatformID.Win32S ||
                    Environment.OSVersion.Platform == PlatformID.Win32Windows;

                string pingArgs = isWindows
                    ? string.Format("-n {0} {1}", count, host)
                    : string.Format("-c {0} {1}", count, host);

                RunExternal("ping", pingArgs);
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "ping error: " + e.Message);
            }
        }

        static void QweDf(string[] args)
        {
            string path = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

            try
            {
                string root = Path.GetPathRoot(Path.GetFullPath(path));
                var drive = new DriveInfo(root);
                long total = drive.TotalSize;
                long free = drive.AvailableFreeSpace;
                long used = total - free;

                Func<long, string> h = n => FormatSize(n, true);
                Console.WriteLine("Total: {0} | Used: {1} | Free: {2}", h(total), h(used), h(free));
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "df error: " + e.Message);
            }
        }

        static void QweDu(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: du PATH");
                return;
            }

            string path = args[0];

            try
            {
                long size = DirSize(path);
                Console.WriteLine("{0}\t{1}", FormatSize(size, true), path);
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "du error: " + e.Message);
            }
        }

        static void QwePs(string[] args)
        {
            try
            {
                bool isWindows =
                    Environment.OSVersion.Platform == PlatformID.Win32NT ||
                    Environment.OSVersion.Platform == PlatformID.Win32S ||
                    Environment.OSVersion.Platform == PlatformID.Win32Windows;

                if (isWindows)
                {
                    RunExternal("tasklist", "");
                }
                else
                {
                    RunExternal("ps", "-ef");
                }
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "ps error: " + e.Message);
            }
        }

        static void QweKill(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: kill PID");
                return;
            }

            try
            {
                int pid = int.Parse(args[0]);
                bool isWindows =
                    Environment.OSVersion.Platform == PlatformID.Win32NT ||
                    Environment.OSVersion.Platform == PlatformID.Win32S ||
                    Environment.OSVersion.Platform == PlatformID.Win32Windows;

                if (isWindows)
                {
                    RunExternal("taskkill", string.Format("/PID {0} /F", pid));
                }
                else
                {
                    var proc = Process.GetProcessById(pid);
                    proc.Kill();
                }
                Console.WriteLine("Killed " + pid);
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "kill error: " + e.Message);
            }
        }

        static void QweSpecs()
        {
            Console.WriteLine("System Specs:");
            Console.WriteLine(" System: " + Environment.OSVersion.Platform);
            Console.WriteLine(" Node Name: " + Environment.MachineName);
            Console.WriteLine(" Release: " + Environment.OSVersion.Version);
            Console.WriteLine(" Version: " + Environment.OSVersion.VersionString);
            Console.WriteLine(" Machine: " + Environment.MachineName);
            Console.WriteLine(" Processor Count: " + Environment.ProcessorCount);
        }

        static void QweNyrver()
        {
            Console.WriteLine("Nyrion Version = " + NYRION_VERSION);
            Console.WriteLine("Build = " + BUILD);
            Console.WriteLine("Prefix = " + PREFIX);
        }

        static void QweBase()
        {
            Console.WriteLine("Underlying OS: " + Environment.OSVersion);
        }

        static void QweScript()
        {
            Console.WriteLine("Script editor (type 'run' to execute, 'exit' to quit)");
            List<string> lines = new List<string>();

            while (true)
            {
                Console.Write("script> ");
                string line = Console.ReadLine();
                if (line == null) break;

                string lower = line.Trim().ToLowerInvariant();
                if (lower == "exit")
                {
                    break;
                }
                else if (lower == "run")
                {
                    foreach (var cmd in lines)
                    {
                        ExecuteCommand("qwe " + cmd, false);
                    }
                    lines.Clear();
                }
                else
                {
                    lines.Add(line);
                }
            }
        }

        static void QweWmip()
        {
            try
            {
                string host = Dns.GetHostName();
                var addrs = Dns.GetHostAddresses(host)
                               .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                               .ToArray();
                if (addrs.Length > 0)
                {
                    Console.WriteLine("IP Address: " + addrs[0]);
                }
                else
                {
                    Console.WriteLine("No IPv4 address found.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot get IP address: " + e.Message);
            }
        }

        static void QweFind(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: find PATTERN [PATH]");
                return;
            }

            string pattern = args[0];
            string basePath = args.Length > 1 ? args[1] : ".";

            try
            {
                basePath = Path.GetFullPath(basePath);
                if (!Directory.Exists(basePath))
                {
                    Console.WriteLine("Base path does not exist.");
                    return;
                }

                var found = Directory.EnumerateFiles(basePath, pattern, SearchOption.AllDirectories).ToList();

                if (found.Count > 0)
                {
                    Console.WriteLine("Found {0} file(s):", found.Count);
                    foreach (var f in found)
                    {
                        Console.WriteLine(f);
                    }
                }
                else
                {
                    Console.WriteLine("File not found.");
                }
            }
            catch (Exception e)
            {
                WriteColor(ConsoleColor.Red, "find error: " + e.Message);
            }
        }

        static void QweSysinfo()
        {
            Console.WriteLine("System Info:");
            Console.WriteLine(" System: " + Environment.OSVersion);
            Console.WriteLine(" Machine: " + Environment.MachineName);
            Console.WriteLine(" Processor Count: " + Environment.ProcessorCount);
            Console.WriteLine(".NET Version: " + Environment.Version);
        }

        static void QweHistory()
        {
            Console.WriteLine("Last 5 Commands:");
            foreach (var cmd in CommandHistory.Skip(Math.Max(0, CommandHistory.Count - 5)))
            {
                Console.WriteLine(cmd);
            }
        }

        static void QweInfo()
        {
            string[] infos =
            {
                "Hey!, You found the secret message!. Let me tell something about myself then. Im an Dutch dev. Living my life coding Nyrion and going to school. Im Gay, Genderfluid and an secret Femboy. What about you?"
            };

            string info = infos[Rng.Next(infos.Length)];
            Console.WriteLine(info);
        }

        static void QweJoke()
        {
            string[] jokes =
            {
                "Why do programmers prefer dark mode? Because light attracts bugs"
            };

            string joke = jokes[Rng.Next(jokes.Length)];
            Console.WriteLine(joke);
        }

        static void QweReboot()
        {
            System.Threading.Thread.Sleep(1000);
            Console.Clear();
            Console.WriteLine(NYRION_VERSION);
            Console.WriteLine(BUILD);
            Console.WriteLine(PREFIX);
        }

        static void QweCrashtest()
        {
            Console.WriteLine("Nyrion Has Crashed!");
            Console.WriteLine("Luckily, This is only a test!");
            QweReboot();
        }

        static void QweTree(string[] args)
        {
            string path = args.Length > 0 ? args[0] : ".";
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Path not found.");
                return;
            }

            string basePath = Path.GetFullPath(path);
            int baseLen = basePath.Length;

            foreach (var dir in Directory.EnumerateDirectories(basePath, "*", SearchOption.AllDirectories)
                                         .OrderBy(d => d))
            {
                int level = dir.Substring(baseLen).Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Length;
                string indent = new string(' ', 4 * (level - 1));
                Console.WriteLine("{0}{1}/", indent, Path.GetFileName(dir));
                foreach (var f in Directory.GetFiles(dir))
                {
                    string subIndent = new string(' ', 4 * level);
                    Console.WriteLine("{0}{1}", subIndent, Path.GetFileName(f));
                }
            }

            // Root
            Console.WriteLine(Path.GetFileName(basePath) + "/");
            foreach (var f in Directory.GetFiles(basePath))
            {
                Console.WriteLine("    " + Path.GetFileName(f));
            }
        }

        static void QweTouch(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: touch <file>");
                return;
            }

            string fname = args[0];
            try
            {
                if (File.Exists(fname))
                {
                    File.SetLastWriteTime(fname, DateTime.Now);
                }
                else
                {
                    using (File.Create(fname)) { }
                }
                Console.WriteLine("Touched: " + fname);
            }
            catch (Exception e)
            {
                Console.WriteLine("touch error: " + e.Message);
            }
        }

        static void QweHead(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: head <file>");
                return;
            }

            string fname = args[0];
            try
            {
                string[] lines = File.ReadAllLines(fname);
                for (int i = 0; i < lines.Length && i < 10; i++)
                {
                    Console.WriteLine(lines[i]);
                }
            }
            catch
            {
                Console.WriteLine("File not found.");
            }
        }

        static void QweTail(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: tail <file>");
                return;
            }

            string fname = args[0];
            try
            {
                string[] lines = File.ReadAllLines(fname);
                foreach (var line in lines.Skip(Math.Max(0, lines.Length - 10)))
                {
                    Console.WriteLine(line);
                }
            }
            catch
            {
                Console.WriteLine("File not found.");
            }
        }

        static void QweUptime()
        {
            TimeSpan up = DateTime.Now - StartTime;
            Console.WriteLine("Uptime: {0:00}d {1:00}h {2:00}m {3:00}s",
                up.Days, up.Hours, up.Minutes, up.Seconds);
        }
    }
}