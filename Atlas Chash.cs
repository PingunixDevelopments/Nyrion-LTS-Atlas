using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.IO.Compression;

namespace NyrionShell
{
    class Program
    {
        static List<string> commandHistory = new List<string>();
        static bool echoOn = true;

        static void Main()
        {
            Console.WriteLine("Atlas - Build 25S4.2");

            while (true)
            {
                Console.Write($"Nyrion:{Directory.GetCurrentDirectory()}> ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                ExecuteCommand(input);
            }
        }

        static void ExecuteCommand(string input)
        {
            if (echoOn) Console.WriteLine(input);
            commandHistory.Add(input);
            if (commandHistory.Count > 100) commandHistory.RemoveAt(0);

            var tokens = input.Split(' ').Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
            if (tokens.Length == 0) return;

            string cmd = tokens[0].ToLower();
            string[] args = tokens.Skip(1).ToArray();

            try
            {
                switch (cmd)
                {
                    case "help": CmdHelp(); break;
                    case "ls": CmdLs(args); break;
                    case "dir": CmdLs(args); break; // alias
                    case "cd": CmdCd(args); break;
                    case "cat": CmdCat(args); break;
                    case "type": CmdCat(args); break; // alias
                    case "mkdir": CmdMkdir(args); break;
                    case "rmdir": CmdRmdir(args); break;
                    case "echo": CmdEcho(args); break;
                    case "whoami": CmdWhoAmI(); break;
                    case "hostname": CmdHostname(); break;
                    case "history": CmdHistory(); break;
                    case "exit": Environment.Exit(0); break;
                    case "copy":
                    case "cp": CmdCopy(args); break;
                    case "move":
                    case "mv": CmdMove(args); break;
                    case "del":
                    case "rm": CmdDelete(args); break;
                    case "base64": CmdBase64(args); break;
                    case "gzip": CmdGzip(args); break;
                    case "gunzip": CmdGunzip(args); break;
                    case "ping": CmdPing(args); break;
                    case "curl": CmdCurl(args); break;
                    default:
                        Console.WriteLine($"'{cmd}' is not recognized as a Nyrion command.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command: {ex.Message}");
            }
        }

        static void CmdHelp()
        {
            Console.WriteLine(@"Available commands:
help, ls, dir, cd <dir>, cat <file>, type <file>, mkdir <dir>, rmdir <dir>, echo <text>, 
whoami, hostname, history, exit, copy/cp, move/mv, del/rm, base64, gzip, gunzip, ping, curl");
        }

        static void CmdLs(string[] args)
        {
            string path = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
            if (!Directory.Exists(path)) { Console.WriteLine("Directory not found."); return; }

            foreach (var entry in Directory.GetFileSystemEntries(path))
            {
                if (Directory.Exists(entry)) Console.WriteLine(Path.GetFileName(entry) + "\\");
                else Console.WriteLine(Path.GetFileName(entry));
            }
        }

        static void CmdCd(string[] args)
        {
            if (args.Length == 0) { Console.WriteLine("Missing directory."); return; }
            if (!Directory.Exists(args[0])) { Console.WriteLine("Directory not found."); return; }
            Directory.SetCurrentDirectory(args[0]);
        }

        static void CmdCat(string[] args)
        {
            if (args.Length == 0) { Console.WriteLine("Missing file."); return; }
            if (!File.Exists(args[0])) { Console.WriteLine("File not found."); return; }

            foreach (var line in File.ReadLines(args[0]))
                Console.WriteLine(line);
        }

        static void CmdMkdir(string[] args)
        {
            if (args.Length == 0) { Console.WriteLine("Missing directory name."); return; }
            Directory.CreateDirectory(args[0]);
            Console.WriteLine($"Directory created: {args[0]}");
        }

        static void CmdRmdir(string[] args)
        {
            if (args.Length == 0) { Console.WriteLine("Missing directory name."); return; }
            if (!Directory.Exists(args[0])) { Console.WriteLine("Directory does not exist."); return; }
            Directory.Delete(args[0], true);
            Console.WriteLine($"Directory removed: {args[0]}");
        }

        static void CmdEcho(string[] args)
        {
            Console.WriteLine(string.Join(" ", args));
        }

        static void CmdWhoAmI()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            Console.WriteLine(identity?.Name ?? "Unknown user");
        }

        static void CmdHostname()
        {
            Console.WriteLine(Environment.MachineName);
        }

        static void CmdHistory()
        {
            int start = Math.Max(0, commandHistory.Count - 5);
            for (int i = start; i < commandHistory.Count; i++)
                Console.WriteLine(commandHistory[i]);
        }

        static void CmdCopy(string[] args)
        {
            if (args.Length < 2) { Console.WriteLine("Usage: copy <src> <dst>"); return; }
            if (Directory.Exists(args[0]))
            {
                // Copy directory recursively
                CopyDirectory(args[0], args[1]);
            }
            else if (File.Exists(args[0]))
            {
                File.Copy(args[0], args[1], true);
            }
            else Console.WriteLine("Source not found.");
            Console.WriteLine("Copied.");
        }

        static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
            foreach (var dir in Directory.GetDirectories(sourceDir))
                CopyDirectory(dir, Path.Combine(destinationDir, Path.GetFileName(dir)));
        }

        static void CmdMove(string[] args)
        {
            if (args.Length < 2) { Console.WriteLine("Usage: move <src> <dst>"); return; }
            if (Directory.Exists(args[0]) || File.Exists(args[0]))
            {
                string dst = args[1];
                if (Directory.Exists(args[0])) Directory.Move(args[0], dst);
                else File.Move(args[0], dst);
                Console.WriteLine("Moved.");
            }
            else Console.WriteLine("Source not found.");
        }

        static void CmdDelete(string[] args)
        {
            if (args.Length == 0) { Console.WriteLine("Usage: del <target>"); return; }
            string target = args[0];
            if (Directory.Exists(target)) Directory.Delete(target, true);
            else if (File.Exists(target)) File.Delete(target);
            else Console.WriteLine("Target not found.");
            Console.WriteLine("Deleted.");
        }

        static void CmdBase64(string[] args)
        {
            if (args.Length < 2) { Console.WriteLine("Usage: base64 encode|decode <text>"); return; }
            string mode = args[0].ToLower();
            string text = string.Join(" ", args.Skip(1));
            if (mode == "encode") Console.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(text)));
            else if (mode == "decode") Console.WriteLine(Encoding.UTF8.GetString(Convert.FromBase64String(text)));
            else Console.WriteLine("Invalid mode. Use encode or decode.");
        }

        static void CmdGzip(string[] args)
        {
            if (args.Length == 0) { Console.WriteLine("Usage: gzip <file>"); return; }
            using (FileStream originalFile = new FileStream(args[0], FileMode.Open))
            using (FileStream compressedFile = new FileStream(args[0] + ".gz", FileMode.Create))
            using (GZipStream compressionStream = new GZipStream(compressedFile, CompressionMode.Compress))
            {
                originalFile.CopyTo(compressionStream);
            }
            Console.WriteLine($"Created {args[0]}.gz");
        }

        static void CmdGunzip(string[] args)
        {
            if (args.Length == 0 || !args[0].EndsWith(".gz")) { Console.WriteLine("Usage: gunzip <file.gz>"); return; }
            string outFile = args[0].Substring(0, args[0].Length - 3);
            using (FileStream compressedFile = new FileStream(args[0], FileMode.Open))
            using (FileStream outputFile = new FileStream(outFile, FileMode.Create))
            using (GZipStream decompressionStream = new GZipStream(compressedFile, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(outputFile);
            }
            Console.WriteLine($"Created {outFile}");
        }

        static void CmdPing(string[] args)
        {
            if (args.Length == 0) { Console.WriteLine("Usage: ping <host>"); return; }
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(args[0]);
                Console.WriteLine($"Reply from {reply.Address}: Status={reply.Status} Time={reply.RoundtripTime}ms");
            }
            catch (Exception ex) { Console.WriteLine("Ping failed: " + ex.Message); }
        }

        static void CmdCurl(string[] args)
        {
            if (args.Length == 0) { Console.WriteLine("Usage: curl <url>"); return; }
            string url = args[0];
            try
            {
                WebClient wc = new WebClient();
                string filename = Path.GetFileName(new Uri(url).LocalPath);
                if (string.IsNullOrEmpty(filename)) filename = "downloaded.file";
                wc.DownloadFile(url, filename);
                Console.WriteLine($"Saved to {filename}");
            }
            catch (Exception ex) { Console.WriteLine("Curl failed: " + ex.Message); }
        }
    }
}
