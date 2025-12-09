import os
import sys
import time
import platform
import socket
import shutil
import random
import subprocess
import base64
import glob
import tarfile
import gzip
import http.server
import urllib.request
import urllib.parse
import signal
import getpass
import stat as statmod
from pathlib import Path


# For colored output (Windows may need colorama installed)
try:
    from colorama import init, Fore, Style
    init()
except ImportError:
    class Fore:
        RED = ''
        GREEN = ''
        CYAN = ''
        YELLOW = ''
        RESET = ''
    class Style:
        BRIGHT = ''
        RESET_ALL = ''

NYRION_VERSION = "Nyrion LTA Atlas"
BUILD = "25S4.2"
PREFIX = "QopWarren Build 5125 - Prefix QWE"
command_history = []

#QopWarren version explanation
#5 is the the last digit of the year so 25 will be 5
#10 is the month so october is the month
#1 is the version number so this is the first version

#Love From Pingunix!

def clear_screen():
    qwe_reboot()

print(NYRION_VERSION)
print(BUILD)



def qwe_show_help():
    print(Fore.CYAN + "Available QopWarren commands:" + Fore.RESET)
    print(" qwe help          - Show this help list")
    print(" qwe time          - Show current time")
    print(" qwe date          - Show current date")
    print(" qwe cls           - Clear screen")
    print(" qwe dir           - List directory files")
    print(" qwe cd <dir>      - Change directory")
    print(" qwe type <file>   - Display file contents")
    print(" qwe copy <src> <dst> - Copy file")
    print(" qwe move <src> <dst> - Move file")
    print(" qwe del <file>    - Delete file")
    print(" qwe ren <old> <new>  - Rename file")
    print(" qwe mkdir <dir>   - Make directory")
    print(" qwe rmdir <dir>   - Remove directory")
    print(" qwe echo <text>   - Print text")
    print(" qwe pause         - Pause for keypress")
    print(" qwe calc          - Calculator")
    print(" qwe specs         - Show system specs")
    print(" qwe nyrver        - Show Nyrion version")
    print(" qwe base          - Show underlying OS")
    print(" qwe script        - Simple script editor")
    print(" qwe find <file>   - Find file")
    print(" qwe edit <file>   - Edit a text file")
    print(" qwe sysinfo       - Show system info")
    print(" qwe history       - Shows last 5 commands")
    print(" qwe joke          - Tell a joke")
    print(" qwe reboot        - Restart shell")
    print(" qwe exit          - Exit shell")
    print(" qwe echo          - Echo command")
    print(" qwe whoami        - Who are you?")
    print(" qwe hostname      - What's the hostname?")
    print(" qwe ls            - List directories")
    print(" qwe cp            - Copy file")
    print(" qwe mv            - Move file")
    print(" qwe rm            - Remove file")
    print(" qwe chmod         - Change file permissions")
    print(" qwe stat          - Check file info")
    print(" qwe find          - Find a file")
    print(" qwe open          - Open a file")
    print(" qwe edit          - Edit a file")
    print(" qwe base64        - Base64 encode/decode")
    print(" qwe hex           - Hex editor")
    print(" qwe tar           - Tar a file")
    print(" qwe untar         - Untar a file")
    print(" qwe gzip          -  Gzip a file")
    print(" qwe gunzip        - Gunzip a file")
    print(" qwe http          - Start a simple HTTP server")
    print(" qwe curl          - Curl a URL")
    print(" qwe ip            - Check your IP address")
    print(" qwe ping          - Ping a host")
    print(" qwe kill          - Kill a process")
    print(" qwe ps            - List processes")
    print(" qwe df            - Disk usage")
    print(" qwe du            - Directory size")
    
def qwe_show_time():
    print(time.strftime("%a %b %d %H:%M:%S %Y"))

def qwe_show_date():
    print(time.strftime("%Y-%m-%d"))

def qwe_list_dir():
    try:
        for f in os.listdir('.'):
            if os.path.isdir(f):
                print(Fore.CYAN + f + "\\" + Fore.RESET)
            else:
                print(f)
    except Exception as e:
        print(f"Error listing directory: {e}")

def qwe_change_dir(path):
    try:
        os.chdir(path)
    except Exception as e:
        print(f"Bad command or filename: {e}")

def qwe_type_filetype_file(filename):
    try:
        with open(filename, 'r') as f:
            print(f.read())
    except Exception as e:
        print(f"File not found: {e}")
        
def qwe_echo(args):
    global echo_on
    if not args:
        print(Fore.RED + "The syntax of the command is incorrect." + Fore.RESET)
        return
    arg = args[0].lower()
    if arg == "off":
        echo_on = False
        settings["echo_on"] = False
    elif arg == "on":
        echo_on = True
        settings["echo_on"] = True
    else:
        print(' '.join(args))

def qwe_hostname(args):
    print(platform.node())

def qwe_ls(args):
    show_all = "-a" in args
    long = "-l" in args
    human = "-h" in args
    paths = [a for a in args if not a.startswith("-")] or ["."]
    def fmt_size(n):
        if not human:
            return str(n)
        for unit in ['B','K','M','G','T']:
            if n < 1024:
                return f"{n:.0f}{unit}"
            n /= 1024
        return f"{n:.0f}P"
    for p in paths:
        p = Path(p)
        try:
            if p.is_dir():
                entries = list(p.iterdir())
                if not show_all:
                    entries = [e for e in entries if e.name not in ('.','..') and not e.name.startswith('.')]
                if len(paths) > 1:
                    print(Fore.CYAN + f"{p}:" + Fore.RESET)
                for e in sorted(entries, key=lambda x: x.name.lower()):
                    name = e.name + (os.sep if e.is_dir() else "")
                    if long:
                        st = e.stat()
                        mtime = time.strftime("%Y-%m-%d %H:%M", time.localtime(st.st_mtime))
                        size = fmt_size(st.st_size)
                        perms = statmod.filemode(st.st_mode)
                        print(f"{perms} {size:>8} {mtime} {name}")
                    else:
                        print(name)
            else:
                print(p.name)
        except Exception as e:
            print(Fore.RED + f"ls error: {e}" + Fore.RESET)

def qwe_whoami(args):
    print(getpass.getuser())

def qwe_cp(args):
    if not args or len(args) < 2:
        print("Usage: cp [-r] SRC DST")
        return
    recursive = "-r" in args or "/s" in args
    src_dst = [a for a in args if not a.startswith("-")]
    if len(src_dst) != 2:
        print("Usage: cp [-r] SRC DST")
        return
    src, dst = src_dst
    try:
        if os.path.isdir(src):
            if not recursive:
                print(Fore.RED + "cp: -r required for directories" + Fore.RESET)
                return
            shutil.copytree(src, dst)
        else:
            shutil.copy2(src, dst)
        print("Copied.")
    except Exception as e:
        print(Fore.RED + f"cp error: {e}" + Fore.RESET)

def qwe_stat(args):
    if not args:
        print("Usage: stat FILE")
        return
    p = args[0]
    try:
        st = os.stat(p)
        print(f"Size: {st.st_size} bytes")
        print(f"Mode: {statmod.filemode(st.st_mode)}")
        print(f"Modified: {time.ctime(st.st_mtime)}")
        print(f"Created:  {time.ctime(st.st_ctime)}")
        print(f"Inode: {getattr(st, 'st_ino', 'n/a')}")
    except Exception as e:
        print(Fore.RED + f"stat error: {e}" + Fore.RESET)

def qwe_find(args):
    if not args:
        print("Usage: find PATTERN [PATH]")
        return
    pattern = args[0]
    base = args[1] if len(args) > 1 else "."
    pattern_path = os.path.join(base, pattern)
    for p in glob.iglob(pattern_path, recursive=True):
        print(p)

def qwe_rm(args):
    if not args:
        print("Usage: rm [-r] [-f] TARGET...")
        return
    recursive = "-r" in args or "/s" in args
    force = "-f" in args or "/f" in args
    targets = [a for a in args if not a.startswith("-")]
    for t in targets:
        try:
            if os.path.isdir(t) and not os.path.islink(t):
                if not recursive:
                    print(Fore.RED + f"rm: '{t}' is a directory (use -r)" + Fore.RESET)
                    continue
                if not force and not confirm(f"Recursively delete '{t}'? [y/N]: "):
                    print("Skipped.")
                    continue
                shutil.rmtree(t)
            else:
                if not force and not confirm(f"Delete '{t}'? [y/N]: "):
                    print("Skipped.")
                    continue
                os.remove(t)
            print(f"Deleted: {t}")
        except Exception as e:
            print(Fore.RED + f"rm error: {e}" + Fore.RESET)

def qwe_chmod(args):
    if len(args) != 2:
        print("Usage: chmod MODE FILE  (MODE numeric, e.g., 755)")
        return
    mode_str, path = args
    try:
        mode = int(mode_str, 8)
        os.chmod(path, mode)
        print("Mode set.")
    except Exception as e:
        print(Fore.RED + f"chmod error: {e}" + Fore.RESET)

def lcr_stat(args):
    if not args:
        print("Usage: stat FILE")
        return
    p = args[0]
    try:
        st = os.stat(p)
        print(f"Size: {st.st_size} bytes")
        print(f"Mode: {statmod.filemode(st.st_mode)}")
        print(f"Modified: {time.ctime(st.st_mtime)}")
        print(f"Created:  {time.ctime(st.st_ctime)}")
        print(f"Inode: {getattr(st, 'st_ino', 'n/a')}")
    except Exception as e:
        print(Fore.RED + f"stat error: {e}" + Fore.RESET)

def qwe_mv(args):
    if len(args) != 2:
        print("Usage: mv SRC DST")
        return
    try:
        shutil.move(args[0], args[1])
        print("Moved.")
    except Exception as e:
        print(Fore.RED + f"mv error: {e}" + Fore.RESET)

def qwe_crashtest():
    print("Nyrion Has Crashed!")
    print("Luckily, This is only a test!")
    qwe_reboot()
    
def qwe_mkdir(dirname):
    try:
        os.mkdir(dirname)
        print(f"Directory created: {dirname}")
    except Exception as e:
        print(f"Cannot create directory: {e}")

def qwe_rmdir(dirname):
    try:
        os.rmdir(dirname)
        print(f"Directory removed: {dirname}")
    except Exception as e:
        print(f"Cannot remove directory: {e}")
        
def qwe_pause():
    input("Press Enter to continue...")

def qwe_calculator():
    print("Calculator started. Type 'exit' to quit.")
    while True:
        expr = input("calc> ")
        if expr.lower() in ('exit', 'quit'):
            break
        try:
            # Safe eval: allow digits and operators only
            allowed = "0123456789+-*/(). "
            if all(c in allowed for c in expr):
                print(eval(expr))
            else:
                print("Invalid characters in expression.")
        except Exception as e:
            print(f"Error: {e}")

def qwe_open(args):
    if not args:
        print("Usage: open FILE")
        return
    path = args[0]
    try:
        if os.name == 'nt':
            os.startfile(path)  # type: ignore
        elif sys.platform == "darwin":
            subprocess.run(["open", path])
        else:
            subprocess.run(["xdg-open", path])
    except Exception as e:
        print(Fore.RED + f"open error: {e}" + Fore.RESET)

def qwe_edit(args):
    if not args:
        print("Usage: edit FILE")
        return
    editor = os.environ.get("EDITOR")
    if not editor:
        editor = "notepad" if os.name == 'nt' else ("nano" if shutil.which("nano") else "vi")
    try:
        subprocess.run([editor, args[0]])
    except Exception as e:
        print(Fore.RED + f"edit error: {e}" + Fore.RESET)

def qwe_base64(args):
    if not args or args[0] not in ("encode", "decode"):
        print("Usage: base64 encode|decode <infile|-t TEXT> [outfile]")
        return
    mode = args[0]
    if len(args) >= 2 and args[1] == "-t":
        text = ' '.join(args[2:]) if len(args) > 2 else ""
        data = text.encode("utf-8")
    elif len(args) >= 2:
        infile = args[1]
        try:
            with open(infile, "rb") as f:
                data = f.read()
        except Exception as e:
            print(Fore.RED + f"base64 error: {e}" + Fore.RESET)
            return
    else:
        print("Usage: base64 encode|decode <infile|-t TEXT> [outfile]")
        return
    outfile = None
    if len(args) >= 3 and args[1] != "-t":
        outfile = args[2]
    try:
        if mode == "encode":
            out = base64.b64encode(data)
        else:
            out = base64.b64decode(data)
        if outfile:
            with open(outfile, "wb") as f:
                f.write(out)
            print(f"Wrote {outfile}")
        else:
            if mode == "encode":
                print(out.decode())
            else:
                try:
                    print(out.decode())
                except Exception:
                    print(out.hex())
    except Exception as e:
        print(Fore.RED + f"base64 error: {e}" + Fore.RESET)

def qwe_hex(args):
    if not args:
        print("Usage: hex FILE [bytes]")
        return
    fname = args[0]
    n = int(args[1]) if len(args) > 1 else 256
    try:
        with open(fname, "rb") as f:
            data = f.read(n)
        for i in range(0, len(data), 16):
            chunk = data[i:i+16]
            hexs = ' '.join(f"{b:02x}" for b in chunk)
            text = ''.join(chr(b) if 32 <= b < 127 else '.' for b in chunk)
            print(f"{i:08x}  {hexs:<47}  {text}")
    except Exception as e:
        print(Fore.RED + f"hex error: {e}" + Fore.RESET)

def qwe_tar(args):
    if len(args) != 2:
        print("Usage: tar SRC DEST.tar|.tar.gz")
        return
    src, dest = args
    try:
        mode = "w:gz" if dest.endswith(".tar.gz") or dest.endswith(".tgz") else "w"
        with tarfile.open(dest, mode) as tf:
            arcname = os.path.basename(src.rstrip(os.sep))
            tf.add(src, arcname=arcname)
        print(f"Created {dest}")
    except Exception as e:
        print(Fore.RED + f"tar error: {e}" + Fore.RESET)

def qwe_untar(args):
    if len(args) < 1:
        print("Usage: untar FILE.tar[.gz] [DEST]")
        return
    src = args[0]
    dest = args[1] if len(args) > 1 else "."
    try:
        with tarfile.open(src, "r:*") as tf:
            tf.extractall(path=dest)
        print(f"Extracted to {dest}")
    except Exception as e:
        print(Fore.RED + f"untar error: {e}" + Fore.RESET)

def qwe_gzip(args):
    if len(args) != 1:
        print("Usage: gzip FILE")
        return
    src = args[0]
    try:
        dst = src + ".gz"
        with open(src, "rb") as f_in, gzip.open(dst, "wb") as f_out:
            shutil.copyfileobj(f_in, f_out)
        print(f"Created {dst}")
    except Exception as e:
        print(Fore.RED + f"gzip error: {e}" + Fore.RESET)

def qwe_gunzip(args):
    if len(args) != 1:
        print("Usage: gunzip FILE.gz")
        return
    src = args[0]
    try:
        if not src.endswith(".gz"):
            print("gunzip expects a .gz file")
            return
        dst = src[:-3]
        with gzip.open(src, "rb") as f_in, open(dst, "wb") as f_out:
            shutil.copyfileobj(f_in, f_out)
        print(f"Created {dst}")
    except Exception as e:
        print(Fore.RED + f"gunzip error: {e}" + Fore.RESET)

def qwe_http(args):
    port = int(args[0]) if args else 8000
    Handler = http.server.SimpleHTTPRequestHandler
    Server = getattr(http.server, "ThreadingHTTPServer", http.server.HTTPServer)
    server = Server(("", port), Handler)
    print(Fore.GREEN + f"Serving HTTP on 0.0.0.0:{port} (Ctrl+C to stop)" + Fore.RESET)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        server.server_close()
        print("Server stopped.")

def qwe_curl(args):
    if not args:
        print("Usage: curl URL [outfile]")
        return
    url = args[0]
    out = args[1] if len(args) > 1 else os.path.basename(urllib.parse.urlparse(url).path) or "downloaded.file"
    try:
        with urllib.request.urlopen(url) as r, open(out, "wb") as f:
            shutil.copyfileobj(r, f)
        print(f"Saved to {out}")
    except Exception as e:
        print(Fore.RED + f"curl error: {e}" + Fore.RESET)

def qwe_ip(args):
    try:
        host = socket.gethostname()
        print(f"Host: {host}")
        addrs = set()
        for family in (socket.AF_INET, socket.AF_INET6):
            try:
                infos = socket.getaddrinfo(host, None, family, socket.SOCK_STREAM)
                for info in infos:
                    addrs.add(info[4][0])
            except Exception:
                pass
        for a in sorted(addrs):
            print(a)
    except Exception as e:
        print(Fore.RED + f"ip error: {e}" + Fore.RESET)

def qwe_ping(args):
    if not args:
        print("Usage: ping HOST [count]")
        return
    host = args[0]
    count = args[1] if len(args) > 1 else "4"
    try:
        if os.name == 'nt':
            subprocess.run(["ping", "-n", str(count), host])
        else:
            subprocess.run(["ping", "-c", str(count), host])
    except Exception as e:
        print(Fore.RED + f"ping error: {e}" + Fore.RESET)

def qwe_df(args):
    path = args[0] if args else os.getcwd()
    try:
        total, used, free = shutil.disk_usage(path)
        def h(n):
            for u in "BKMGT":
                if n < 1024: return f"{n:.0f}{u}"
                n/=1024
            return f"{n:.0f}P"
        print(f"Total: {h(total)} | Used: {h(used)} | Free: {h(free)}")
    except Exception as e:
        print(Fore.RED + f"df error: {e}" + Fore.RESET)

def qwe_du(args):
    if not args:
        print("Usage: du PATH")
        return
    path = args[0]
    try:
        size = dir_size(path)
        def h(n):
            for u in "BKMGT":
                if n < 1024: return f"{n:.0f}{u}"
                n/=1024
            return f"{n:.0f}P"
        print(f"{h(size)}\t{path}")
    except Exception as e:
        print(Fore.RED + f"du error: {e}" + Fore.RESET)

def qwe_ps(args):
    try:
        if os.name == 'nt':
            subprocess.run(["tasklist"])
        else:
            subprocess.run(["ps", "-ef"])
    except Exception as e:
        print(Fore.RED + f"ps error: {e}" + Fore.RESET)

def qwe_kill(args):
    if not args:
        print("Usage: kill PID")
        return
    try:
        pid = int(args[0])
        if os.name == 'nt':
            subprocess.run(["taskkill", "/PID", str(pid), "/F"])
        else:
            os.kill(pid, signal.SIGTERM)
        print(f"Killed {pid}")
    except Exception as e:
        print(Fore.RED + f"kill error: {e}" + Fore.RESET)


def qwe_specs():
    print("System Specs:")
    print(f" System: {platform.system()}")
    print(f" Node Name: {platform.node()}")
    print(f" Release: {platform.release()}")
    print(f" Version: {platform.version()}")
    print(f" Machine: {platform.machine()}")
    print(f" Processor: {platform.processor()}")

def qwe_nyrver():
    print(f"Nyrion Version = {NYRION_VERSION}")
    print("Build = {BUILD}")
    print("Prefix = {PREFIX}")

def qwe_base():
    print(f"Underlying OS: {platform.system()} {platform.release()}")

def qwe_script():
    print("Script editor (type 'run' to execute, 'exit' to quit)")
    script_lines = []
    while True:
        line = input("script> ")
        if line.lower() == 'exit':
            break
        elif line.lower() == 'run':
            for cmd in script_lines:
                execute_command("lion " + cmd, echo=False)
            script_lines.clear()
        else:
            script_lines.append(line)

def qwe_wmip():
    try:
        hostname = socket.gethostname()
        ip = socket.gethostbyname(hostname)
        print(f"IP Address: {ip}")
    except Exception as e:
        print(f"Cannot get IP address: {e}")

def qwe_find(filename):
    found = []
    for root, dirs, files in os.walk('.'):
        if filename in files:
            found.append(os.path.join(root, filename))
    if found:
        print(f"Found {len(found)} file(s):")
        for f in found:
            print(f)
    else:
        print("File not found.")

def qwe_edit(filename):
    print(f"Editing {filename}. Type 'SAVE' alone on a line to save and exit.")
    lines = []
    if os.path.exists(filename):
        with open(filename) as f:
            lines = f.read().splitlines()
        print("\n".join(lines))
    else:
        print("New file.")

    new_lines = []
    while True:
        line = input()
        if line.strip().upper() == "SAVE":
            break
        new_lines.append(line)
    try:
        with open(filename, 'w') as f:
            f.write('\n'.join(new_lines))
        print(f"{filename} saved.")
    except Exception as e:
        print(f"Failed to save file: {e}")

def qwe_sysinfo():
    print("System Info:")
    print(f" System: {platform.system()} {platform.release()} ({platform.version()})")
    print(f" Machine: {platform.machine()}")
    print(f" Processor: {platform.processor()}")
    print(f" Python Version: {platform.python_version()}")

def qwe_history():
    print("Last 5 Commands:")
    for cmd in command_history[-5:]:
        print(cmd)

def qwe_info():
    infos = [
        "Hey!, You found the secret message!. Let me tell something about myself then. Im an Dutch dev. Living my life coding Nyrion and going to school. Im Gay, Genderfluid and an secret Femboy. What about you?",
    ]
    print(random.choice(infos))

def qwe_joke():
    jokes = [
        "Why do programmers prefer dark mode? Because light attracts bugs"

    ]
    print(random.choice(jokes))

def qwe_reboot():
    time.sleep(1)
    main()

def execute_command(cmd, echo=True):
    global command_history
    if echo:
        print(cmd)

    command_history.append(cmd)
    if len(command_history) > 100:
        command_history.pop(0)

    parts = cmd.strip().split()
    if len(parts) < 2:
        print("Command must start with 'qwe' and have a subcommand.")
        return
    if parts[0].lower() != 'qwe':
        print("Commands must start with 'qwe'")
        return

    command = parts[1].lower()
    args = parts[2:]

    try:
        if command == 'help':
            qwe_show_help()
        elif command == 'time':
            qwe_show_time()
        elif command == 'date':
            qwe_show_date()
        elif command == 'cls':
            clear_screen()
        elif command == 'dir':
            qwe_list_dir()
        elif command == 'cd':
            if args:
                qwe_change_dir(args[0])
            else:
                print("Missing directory.")
        elif command == 'type':
            if args:
                qwe_type_file(args[0])
            else:
                print("Missing filename.")
        elif command == 'copy':
            if len(args) == 2:
                qwe_copy_file(args[0], args[1])
            else:
                print("Syntax: lion copy <src> <dst>")
        elif command == 'move':
            if len(args) == 2:
                qwe_move_file(args[0], args[1])
            else:
                print("Syntax: lion move <src> <dst>")
        elif command == 'del':
            if args:
                qwe_delete_file(args[0])
            else:
                print("Missing filename.")
        elif command == 'ren':
            if len(args) == 2:
                qwe_rename_file(args[0], args[1])
            else:
                print("Syntax: lion ren <old> <new>")
        elif command == 'mkdir':
            if args:
                qwe_mkdir(args[0])
            else:
                print("Missing directory name.")
        elif command == 'rmdir':
            if args:
                qwe_rmdir(args[0])
            else:
                print("Missing directory name.")
        elif command == 'echo':
            echo_command(args)
        elif command == 'pause':
            pause()
        elif command == 'echo':
            qwe_echo(args)
        elif command == 'whoami':
            qwe_whoami(args)
        elif command == 'hostname':
            qwe_hostname(args)
        elif command == 'ls':
            qwe_ls(args)
        elif command == 'cp':
            qwe_cp(args)
        elif command == 'mv':
            qwe_mv(args)
        elif command == 'rm':
            qwe_rm(args)
        elif command == 'chmod':
            qwe_chmod(args)
        elif command == 'stat':
            qwe_stat(args)
        elif command == 'find':
            qwe_find(args)
        elif command == 'open':
            qwe_open(args)
        elif command == 'edit':
            qwe_edit(args)
        elif command == 'base64':
            qwe_base64(args)
        elif command == 'hex':
            qwe_hex(args)
        elif command == 'tar':
            qwe_tar(args)
        elif command == 'untar':
            qwe_untar(args)
        elif command == 'gzip':
            qwe_gzip(args)
        elif command == 'gunzip':
            qwe_gunzip(args)
        elif command == 'http':
            qwe_http(args)
        elif command == 'curl':
            qwe_curl(args)
        elif command == 'ip':
            qwe_ip(args)
        elif command == 'ping':
            qwe_ping(args)
        elif command == 'kill':
            qwe_kill(args)
        elif command == 'ps':
            qwe_ps(args)
        elif command == 'df':
            qwe_df(args)
        elif command == 'du':
            qwe_du(args)  
        elif command == 'calc':
            calculator()
        elif command == 'specs':
            _specs()
        elif command == 'nyrver':
            qwe_nyrver()
        elif command == 'base':
            qwe_base()
        elif command == 'script':
            qwe_script()
        elif command == 'wmip':
            qwe_wmip()
        elif command == 'info':
            qwe_info()
        elif command == 'find':
            if args:
                qwe_find(args[0])
            else:
                print("Missing filename.")
        elif command == 'edit':
            if args:
                qwe_edit(args[0])
            else:
                print("Missing filename.")
        elif command == 'sysinfo':
            qwe_sysinfo()
        elif command == 'history':
            qwe_history()
        elif command == 'joke':
            qwe_joke()
        elif command == 'crashtest':
            qwe_crashtest()
        elif command == 'reboot':
            qwe_reboot()
        elif command == 'exit':
            print("Exiting Nyrion shell...")
            sys.exit(0)
        else:
            print(f"'{command}' is not recognized as a QopWarren command.")
    except Exception as e:
        print(f"Error executing command: {e}")

def main():
    while True:
        try:
            cwd = os.getcwd()
            inp = input(f"{Fore.GREEN}Nyrion:{cwd}> {Fore.RESET}").strip()
            if not inp:
                continue
            execute_command(inp)
        except KeyboardInterrupt:
            print("\nUse 'qwe exit' to quit.")
        except Exception as e:
            print(f"Unexpected error: {e}")

if __name__ == "__main__":
    main()
