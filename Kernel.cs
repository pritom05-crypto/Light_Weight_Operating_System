using System;
using System.Collections.Generic;
using Sys = Cosmos.System;

namespace Light_Weight_Operating_System
{
    public class Kernel : Sys.Kernel
    {
        // ===== In-Memory File System (flat, single directory) =====
        private readonly Dictionary<string, string> files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // ===== Simple Memory Manager (simulated) =====
        private class MemoryBlock
        {
            public int Id { get; set; }
            public int Size { get; set; }
            public bool Used { get; set; }
        }
        private readonly List<MemoryBlock> memoryBlocks = new List<MemoryBlock>();
        private int nextBlockId = 1;

        protected override void BeforeRun()
        {
            Console.Clear();
            Console.WriteLine("=======================================");
            Console.WriteLine("  Light Weight Operating System (LWOS) ");
            Console.WriteLine("  Booted successfully with Cosmos!     ");
            Console.WriteLine("  Type 'help' to see commands.         ");
            Console.WriteLine("=======================================");
        }

        protected override void Run()
        {
            Console.Write("LWOS> ");
            var input = Console.ReadLine() ?? string.Empty;
            HandleCommand(input);
        }

        private void HandleCommand(string input)
        {
            input = input.Trim();
            if (input.Length == 0) return;

            // split into: cmd + rest
            int sp = input.IndexOf(' ');
            string cmd = (sp < 0 ? input : input.Substring(0, sp)).ToLower();
            string arg = (sp < 0 ? "" : input.Substring(sp + 1)).Trim();

            switch (cmd)
            {
                case "help":
                    PrintHelp();
                    break;

                // ===== File system (in-memory) =====
                case "ls":
                    CmdLs();
                    break;

                case "dir":
                    CmdDir();
                    break;

                case "create":
                    CmdCreate(arg);
                    break;

                case "write":
                    CmdWrite(arg, append: false);
                    break;

                case "append":
                    CmdWrite(arg, append: true);
                    break;

                case "read":
                    CmdRead(arg);
                    break;

                case "delete":
                case "del":
                    CmdDelete(arg);
                    break;

                case "rename":
                    CmdRename(arg);
                    break;

                case "copy":
                    CmdCopy(arg);
                    break;

                // ===== Calculator =====
                case "calc":
                    Calculator();
                    break;

                // ===== System utilities =====
                case "date":
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    break;

                case "clear":
                    Console.Clear();
                    break;

                case "ver":
                    Console.WriteLine("Light_Weight_Operating_System v1.0");
                    break;

                case "sysinfo":
                    Console.WriteLine("OS: Light_Weight_Operating_System");
                    Console.WriteLine("CPU: Cosmos Virtual CPU");
                    Console.WriteLine("RAM: " + Cosmos.Core.CPU.GetAmountOfRAM() + " MB");
                    break;

                case "echo":
                    Console.WriteLine(arg);
                    break;

                case "rand":
                    {
                        var r = new Random();
                        Console.WriteLine("Random: " + r.Next(1, 100));
                    }
                    break;

                // ===== Memory management (simulated) =====
                case "memalloc":
                    if (int.TryParse(arg, out int size) && size > 0) AllocateMemory(size);
                    else Console.WriteLine("Usage: memalloc <size>");
                    break;

                case "memfree":
                    if (int.TryParse(arg, out int id) && id > 0) FreeMemory(id);
                    else Console.WriteLine("Usage: memfree <id>");
                    break;

                case "meminfo":
                    ShowMemoryInfo();
                    break;

                // ===== Power =====
                case "shutdown":
                    Console.WriteLine("System is shutting down...");
                    Sys.Power.Shutdown();
                    break;

                case "reboot":
                    Console.WriteLine("System is rebooting...");
                    Sys.Power.Reboot();
                    break;

                default:
                    Console.WriteLine("Unknown command. Type 'help' to see commands.");
                    break;
            }
        }

        private void PrintHelp()
        {
            Console.WriteLine("====== LWOS Command List ======");
            Console.WriteLine("  help                - Show this help menu");
            Console.WriteLine("  ls                  - List files");
            Console.WriteLine("  dir                 - Detailed file listing");
            Console.WriteLine("  create <file>       - Create new file");
            Console.WriteLine("  write <file>        - Overwrite file (prompts for text)");
            Console.WriteLine("  append <file>       - Append to file (prompts for text)");
            Console.WriteLine("  read <file>         - Read file");
            Console.WriteLine("  delete <file>       - Delete file");
            Console.WriteLine("  rename <old> <new>  - Rename file");
            Console.WriteLine("  copy <src> <dest>   - Copy file");
            Console.WriteLine("  calc                - Open calculator");
            Console.WriteLine("  date                - Show system date & time");
            Console.WriteLine("  clear               - Clear screen");
            Console.WriteLine("  ver                 - Show OS version");
            Console.WriteLine("  sysinfo             - Show system info");
            Console.WriteLine("  echo <text>         - Print text");
            Console.WriteLine("  rand                - Generate random number");
            Console.WriteLine("  memalloc <size>     - Allocate memory block");
            Console.WriteLine("  memfree <id>        - Free memory block by ID");
            Console.WriteLine("  meminfo             - Show memory usage");
            Console.WriteLine("  shutdown            - Power off");
            Console.WriteLine("  reboot              - Restart");
            Console.WriteLine("===============================");
        }

        // ===== In-memory file ops =====
        private void CmdLs()
        {
            if (files.Count == 0) { Console.WriteLine("(no files)"); return; }
            foreach (var name in files.Keys) Console.WriteLine(name);
        }

        private void CmdDir()
        {
            if (files.Count == 0) { Console.WriteLine("(no files)"); return; }
            foreach (var kv in files)
                Console.WriteLine($"{kv.Key,-24} {kv.Value.Length,6} bytes");
        }

        private void CmdCreate(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Usage: create <file>"); return; }
            if (files.ContainsKey(name)) { Console.WriteLine("File already exists."); return; }
            files[name] = string.Empty;
            Console.WriteLine($"File created: {name}");
        }

        private void CmdWrite(string name, bool append)
        {
            if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Usage: " + (append ? "append" : "write") + " <file>"); return; }
            if (!files.ContainsKey(name)) { Console.WriteLine("File not found."); return; }
            Console.Write("Enter text: ");
            string text = Console.ReadLine() ?? string.Empty;
            files[name] = append ? (files[name] + text) : text;
            Console.WriteLine((append ? "Appended to " : "Wrote ") + name);
        }

        private void CmdRead(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Usage: read <file>"); return; }
            if (!files.ContainsKey(name)) { Console.WriteLine("File not found."); return; }
            var content = files[name];
            if (content.Length == 0) Console.WriteLine("(empty file)");
            else Console.WriteLine(content);
        }

        private void CmdDelete(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Usage: delete <file>"); return; }
            if (!files.ContainsKey(name)) { Console.WriteLine("File not found."); return; }
            files.Remove(name);
            Console.WriteLine("Deleted: " + name);
        }

        private void CmdRename(string args)
        {
            // args: "<old> <new>"
            var parts = SplitTwo(args);
            string oldName = parts.Item1;
            string newName = parts.Item2;

            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
            {
                Console.WriteLine("Usage: rename <old> <new>");
                return;
            }
            if (!files.ContainsKey(oldName)) { Console.WriteLine("File not found: " + oldName); return; }
            if (files.ContainsKey(newName)) { Console.WriteLine("Target exists: " + newName); return; }

            var data = files[oldName];
            files.Remove(oldName);
            files[newName] = data;
            Console.WriteLine($"Renamed '{oldName}' -> '{newName}'");
        }

        private void CmdCopy(string args)
        {
            // args: "<src> <dest>"
            var parts = SplitTwo(args);
            string src = parts.Item1;
            string dest = parts.Item2;

            if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(dest))
            {
                Console.WriteLine("Usage: copy <src> <dest>");
                return;
            }
            if (!files.ContainsKey(src)) { Console.WriteLine("File not found: " + src); return; }
            if (files.ContainsKey(dest)) { Console.WriteLine("Target exists: " + dest); return; }

            files[dest] = files[src];
            Console.WriteLine($"Copied '{src}' -> '{dest}'");
        }

        private Tuple<string, string> SplitTwo(string args)
        {
            args = args ?? "";
            int sp = args.IndexOf(' ');
            if (sp < 0) return new Tuple<string, string>(args.Trim(), "");
            string a = args.Substring(0, sp).Trim();
            string b = args.Substring(sp + 1).Trim();
            return new Tuple<string, string>(a, b);
        }

        // ===== Calculator =====
        private void Calculator()
        {
            Console.WriteLine("=== Calculator ===");
            Console.Write("Enter first number: ");
            if (!double.TryParse(Console.ReadLine(), out double a)) { Console.WriteLine("Invalid number"); return; }
            Console.Write("Enter operator (+,-,*,/): ");
            string op = Console.ReadLine();
            Console.Write("Enter second number: ");
            if (!double.TryParse(Console.ReadLine(), out double b)) { Console.WriteLine("Invalid number"); return; }

            double result;
            switch (op)
            {
                case "+": result = a + b; break;
                case "-": result = a - b; break;
                case "*": result = a * b; break;
                case "/":
                    if (b == 0) { Console.WriteLine("Error: divide by zero"); return; }
                    result = a / b; break;
                default: Console.WriteLine("Invalid operator"); return;
            }
            Console.WriteLine("Result: " + result);
        }

        // ===== Memory Manager (simulated) =====
        private void AllocateMemory(int size)
        {
            var blk = new MemoryBlock { Id = nextBlockId++, Size = size, Used = true };
            memoryBlocks.Add(blk);
            Console.WriteLine($"Allocated block {blk.Id} ({blk.Size} bytes).");
        }

        private void FreeMemory(int id)
        {
            var memBlock = memoryBlocks.Find(b => b.Id == id && b.Used);
            if (memBlock == null) { Console.WriteLine("Invalid block ID!"); return; }
            memBlock.Used = false;
            Console.WriteLine($"Freed block {id} ({memBlock.Size} bytes).");
        }

        private void ShowMemoryInfo()
        {
            int used = 0, freeBlocks = 0, usedBlocks = 0;
            foreach (var blk in memoryBlocks)
            {
                if (blk.Used) { used += blk.Size; usedBlocks++; } else { freeBlocks++; }
            }
            Console.WriteLine("=== Memory Info ===");
            Console.WriteLine($"Blocks: {memoryBlocks.Count}  (Used: {usedBlocks}, Free: {freeBlocks})");
            Console.WriteLine($"Total used: {used} bytes");
            if (memoryBlocks.Count == 0) Console.WriteLine("No memory blocks yet.");
            foreach (var blk in memoryBlocks)
                Console.WriteLine($"  Block {blk.Id}: {blk.Size} bytes - {(blk.Used ? "Used" : "Free")}");
        }
    }
}