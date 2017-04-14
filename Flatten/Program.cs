using System;
using System.IO;
using System.Linq;

namespace AndroSan.Utilities
{
    class Program
    {
        public enum SizeUnit
        {
            Kilobyte, Megabyte, Gigabyte
        }

        public enum Mode
        {
            Move, Copy
        }

        static void Main(string[] args)
        {
            bool hasSetSrc = false;
            bool hasSetDest = false;

            bool isVerbose = false;
            var sourceFolder = ".";
            var destinationFolder = ".";
            var minSize = 0L;
            var mode = Mode.Copy;
            var argarry = string.Join("", args.ToList()).Replace("\"","").Split(new[] {"/"},StringSplitOptions.RemoveEmptyEntries);
            var sizeUnt = SizeUnit.Kilobyte;

            if (argarry.Any(x => x.StartsWith("-Help", StringComparison.CurrentCultureIgnoreCase)))
            {
                DisplayHelp();   
                Environment.Exit(0);
            }
            argarry.ToList().ForEach(x =>
            {
                x = x.Trim();
                if (x.Equals("Verbose", StringComparison.CurrentCultureIgnoreCase))
                {
                    isVerbose = true;
                }
                else if (x.StartsWith("Mode=", StringComparison.InvariantCultureIgnoreCase))
                {
                    var moderes = x.Split(new[] { "Mode=" },StringSplitOptions.RemoveEmptyEntries).Last();
                    if (moderes.Equals("Copy",StringComparison.InvariantCultureIgnoreCase))
                    {
                        mode = Mode.Copy;
                    }
                    else if (moderes.Equals("Move",StringComparison.InvariantCultureIgnoreCase))
                    {
                        mode = Mode.Move;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine("/Mode argument can have the following values: Copy | Move");
                        Console.ResetColor();
                        Environment.Exit(0);
                    }
                }
                else if (x.StartsWith("Source=", StringComparison.InvariantCultureIgnoreCase))
                {
                    sourceFolder = x.Split(new[] { "Source=" }, StringSplitOptions.RemoveEmptyEntries).Last();
                    if (!sourceFolder.EndsWith(@"\"))
                    {
                        sourceFolder += @"\";
                    }
                    hasSetSrc = true;
                }
                else if (x.StartsWith("Dest=", StringComparison.InvariantCultureIgnoreCase))
                {
                    destinationFolder = x.Split(new[] { "Dest=" }, StringSplitOptions.RemoveEmptyEntries).Last();
                    if (!destinationFolder.EndsWith(@"\"))
                    {
                        destinationFolder += @"\";
                    }
                    hasSetDest = true;
                }
                else if (x.StartsWith("MinSize=", StringComparison.InvariantCultureIgnoreCase))
                {
                    var minSizeStr = x.Split(new[] { "MinSize=" }, StringSplitOptions.RemoveEmptyEntries).Last();
                    if (!long.TryParse(minSizeStr, out minSize))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine("/MinSize argument requires a numeric value");
                        Console.ResetColor();
                        Environment.Exit(0);
                    }
                }
                else if (x.StartsWith("SizeUnit=", StringComparison.InvariantCultureIgnoreCase))
                {
                    var sizeUnit = x.Split(new[] { "SizeUnit=" }, StringSplitOptions.RemoveEmptyEntries).Last();
                    if (sizeUnit.Equals("KB",StringComparison.InvariantCultureIgnoreCase) 
                    || sizeUnit.Equals("MB",StringComparison.InvariantCultureIgnoreCase) 
                    || sizeUnit.Equals("GB", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (sizeUnit.Equals("KB", StringComparison.InvariantCultureIgnoreCase))
                        {
                            sizeUnt = SizeUnit.Kilobyte;
                        }
                        else if (sizeUnit.Equals("MB", StringComparison.InvariantCultureIgnoreCase))
                        {
                            sizeUnt = SizeUnit.Megabyte;
                        }
                        else if (sizeUnit.Equals("GB", StringComparison.InvariantCultureIgnoreCase))
                        {
                            sizeUnt = SizeUnit.Gigabyte;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine("/SizeUnit argument can have the following values: KB | MB | GB");
                        Console.ResetColor();
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"{x} - unknown argument");
                    Console.ResetColor();
                    Environment.Exit(0);
                }
            });

            if (!hasSetSrc || !hasSetDest)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"/Source and /Dest arguments must be set");
                Console.ResetColor();
                Environment.Exit(0);
            }

            var files = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories).ToList();
            files.ForEach(file =>
            {
                var info = new FileInfo(file);
                var size = 0;
                if (sizeUnt == SizeUnit.Kilobyte)
                {
                    size = 1024;
                }
                else if (sizeUnt == SizeUnit.Megabyte)
                {
                    size = 1024*1024;
                }
                else if (sizeUnt == SizeUnit.Gigabyte)
                {
                    size = 1024*1024*1024;
                }
                //

                if ((info.Length / size) >= minSize)
                {
                    try
                    {
                        if (!Directory.Exists(destinationFolder))
                        {
                            try
                            {
                              
                                Directory.CreateDirectory(destinationFolder);
                                if (isVerbose)
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Verbose: Created directory: " + destinationFolder);
                                    Console.WriteLine();
                                    Console.ResetColor();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Cannot create destination directory: " + destinationFolder);
                                Console.Error.WriteLine("Debug message: ");
                                Console.Error.WriteLine(ex.Message);
                                Console.ResetColor();
                                Console.WriteLine();
                                Environment.Exit(0);

                            }
                        }
                        if (mode == Mode.Move)
                        {
                            try
                            {
                                File.Move(file, Path.Combine(destinationFolder, Path.GetFileName(file)));
                                if (isVerbose)
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"Moved file {file} from {sourceFolder} to {destinationFolder}");
                                    Console.WriteLine();
                                    Console.ResetColor();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Unable to move file " + file + " -> " + Path.Combine(destinationFolder,Path.GetFileName(file)));
                                Console.Error.WriteLine("Debug message: ");
                                Console.Error.WriteLine(ex.Message);
                                Console.ResetColor();
                                Console.WriteLine();
                            }
                        }
                        else
                        {
                            try
                            {
                                File.Copy(file, Path.Combine(destinationFolder, Path.GetFileName(file)));
                                if (isVerbose)
                                {
                                    Console.WriteLine($"Copied file {file} from {sourceFolder} to {destinationFolder}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Unable to copy file " + file + " -> " + Path.Combine(destinationFolder, Path.GetFileName(file)));
                                Console.Error.WriteLine("Debug message: ");
                                Console.Error.WriteLine(ex.Message);
                                Console.ResetColor();
                                Console.WriteLine();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine(ex.Message);
                        Console.ResetColor();
                    }
                }
            });
        }

        static void DisplayHelp()
        {
            Console.WriteLine("");
            WriteTitleLine();
            Console.Write("*");
            WriteCenterSameLine("Created By: Andrej Bratož");
            WriteLeft("*");
            Console.Write("*");
            WriteCenterSameLine("This application will flatten a directory structure into the destination directory.");
            WriteLeft("*");
            Console.Write("*");
            WriteCenterSameLine("by copying or moving the files. It traverses the source folder and all of its subfolder");
            WriteLeft("*");
            Console.Write("*");
            WriteCenterSameLine("then moving or copying the files to the destination folder.");
            WriteLeft("*");
            WriteStarLine();
            Console.WriteLine("* Supported Arguments:");
            Console.WriteLine("* /Source=[path] - the source directory");
            Console.WriteLine("* /Dest=[path] - the desttination directory");
            Console.WriteLine("* /Mode=[Copy|Move] - determines if the files are to be copied or moved (Copy is default)");
            Console.WriteLine("* /SizeUnit=[KB|MB|GB] - determines how the -MinSize argument value is the be interpreted (KB is default)");
            Console.WriteLine("* /MinSize=[0-9]+ - item that are lover then the determined size will be ignored (0 is default)");
            Console.WriteLine("* /Help - displays the help message");
            Console.WriteLine("* /Verbose - verbose mode");
            Console.WriteLine();
        }

        static void WriteCenter(string s)
        {
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.WriteLine(s);
        }
        static void WriteCenterSameLine(string s)
        {
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.Write(s);
        }
        static void WriteLeft(string s)
        {
            Console.SetCursorPosition((Console.WindowWidth - s.Length), Console.CursorTop);
            Console.WriteLine(s);
        }

        public static void WriteStarLine()
        {
            int counter = 1;
            while(counter < Console.WindowWidth)
            {
                Console.Write("*");
                Console.SetCursorPosition((counter), Console.CursorTop);
                counter++;
            }
            Console.Write("*");
            Console.WriteLine();
        }

        public static void WriteTitleLine()
        {
            int counter = 1;
            while (counter < Console.WindowWidth)
            {
                Console.Write("*");
                if (counter == Console.WindowWidth/2 - 6)
                {
                    Console.Write(" FLATN.EXE ");
                    counter += 11;
                    continue;
                }
                Console.SetCursorPosition((counter), Console.CursorTop);
                counter++;
            }
            Console.Write("*");
            Console.WriteLine();
        }

    }
}
