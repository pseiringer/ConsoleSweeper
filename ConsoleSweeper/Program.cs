using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ConsoleSweeper
{
    class Program
    {
        private static Game game;
        private static readonly int minOutputSize = 65;
        private static readonly string saveFile = @"Saves\Save.json";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (!GetSetup(args))
                return;

            bool gameRunning = true;
            bool interactiveMode = true;
            string outputMessage = string.Empty;
            bool showHelp = false;

            while (gameRunning)
            {
                RedrawField(interactiveMode, gameRunning, outputMessage, showHelp);
                outputMessage = string.Empty;
                showHelp = false;
                if (interactiveMode)
                {
                    var key = Console.ReadKey(true);

                    //navigate with wasd or arrow keys, open with space or enter, flag with f, question with q
                    //modifiers: shift -> move to next unempty field, ctrl -> move to end, openrecursive
                    switch (key.Key)
                    {
                        case ConsoleKey.W:
                        case ConsoleKey.UpArrow:
                            HandleMove(Direction.Up, key.Modifiers);
                            break;
                        case ConsoleKey.A:
                        case ConsoleKey.LeftArrow:
                            HandleMove(Direction.Left, key.Modifiers);
                            break;
                        case ConsoleKey.S:
                        case ConsoleKey.DownArrow:
                            HandleMove(Direction.Down, key.Modifiers);
                            break;
                        case ConsoleKey.D:
                        case ConsoleKey.RightArrow:
                            HandleMove(Direction.Right, key.Modifiers);
                            break;
                        case ConsoleKey.Spacebar:
                        case ConsoleKey.Enter:
                            HandleOpen(key.Modifiers);
                            break;
                        case ConsoleKey.F:
                            game.Flag();
                            break;
                        case ConsoleKey.Q:
                            game.Question();
                            break;
                        case ConsoleKey.Escape:
                            interactiveMode = false;
                            break;
                    }
                }
                else
                {
                    var line = Console.ReadLine().ToLower().Trim();

                    //save game with :w / :write, quit with :q / :quit, save and quit with :wq / :writequit, back to interactive with :i / :interactive, help with :h / :help
                    //play the game with :up [amount] / :u, :position [xPos] [yPos] / :p / :pos, :m / :mark / :question, :flag / :f, :open / :o,
                    switch (line)
                    {
                        case ":w":
                        case ":write":
                            SaveGame();
                            break;
                        case ":q":
                        case ":quit":
                            gameRunning = false;
                            break;
                        case ":wq":
                        case ":writequit":
                            SaveGame();
                            gameRunning = false;
                            break;
                        case ":i":
                        case ":interactive":
                            interactiveMode = true;
                            break;
                        case ":h":
                        case ":help":
                            showHelp = true;
                            break;
                        case ":m":
                        case ":mark":
                        case ":question":
                            game.Question();
                            break;
                        case ":f":
                        case ":flag":
                            game.Flag();
                            break;
                        case ":o":
                        case ":open":
                        case ":oit":
                        case ":openiterative":
                            game.OpenIterative();
                            break;
                        case ":orec":
                        case ":openrecursive":
                            game.Open();
                            break;
                        default:
                            bool inputInvalid = false;
                            try
                            {
                                if (line.StartsWith(':'))
                                {
                                    var split = line.Substring(1).Split(' ');

                                    if (split[0] == "u" || split[0] == "up")
                                    {
                                        if (split.Length == 1)
                                        {
                                            game.Move(Direction.Up);
                                        }
                                        else
                                        {
                                            var value = int.Parse(split[1]);
                                            game.Move(Direction.Up, value);
                                        }
                                    }
                                    else if (split[0] == "r" || split[0] == "right")
                                    {
                                        if (split.Length == 1)
                                        {
                                            game.Move(Direction.Right);
                                        }
                                        else
                                        {
                                            var value = int.Parse(split[1]);
                                            game.Move(Direction.Right, value);
                                        }
                                    }

                                    else if (split[0] == "d" || split[0] == "down")
                                    {
                                        if (split.Length == 1)
                                        {
                                            game.Move(Direction.Down);
                                        }
                                        else
                                        {
                                            var value = int.Parse(split[1]);
                                            game.Move(Direction.Down, value);
                                        }
                                    }

                                    else if (split[0] == "l" || split[0] == "left")
                                    {
                                        if (split.Length == 1)
                                        {
                                            game.Move(Direction.Left);
                                        }
                                        else
                                        {
                                            var value = int.Parse(split[1]);
                                            game.Move(Direction.Left, value);
                                        }
                                    }

                                    else if (split[0] == "p" || split[0] == "pos" || split[0] == "position")
                                    {
                                        var xpos = int.Parse(split[1]);
                                        var ypos = int.Parse(split[2]);
                                        game.SetPosition(xpos, ypos);
                                    }

                                    else if (split[0] == "c" || split[0] == "cheats")
                                    {
                                        var code = split[1];
                                        if (code == "xrayvision")
                                        {
                                            game.Cheats.FullVisibility = !game.Cheats.FullVisibility;
                                            outputMessage = $"full visibility: {(game.Cheats.FullVisibility ? "on" : "off")}";
                                        }
                                        else if (code == "theworld")
                                        {
                                            game.Cheats.ShowOpenings = !game.Cheats.ShowOpenings;
                                            outputMessage = $"show openings: {(game.Cheats.ShowOpenings ? "on" : "off")}";
                                        }
                                        else if (code == "pyromaniac")
                                        {
                                            game.Cheats.ShowMines = !game.Cheats.ShowMines;
                                            outputMessage = $"show mines: {(game.Cheats.ShowMines ? "on" : "off")}";
                                        }
                                        //else if (code == "stopwatch")
                                        //{
                                        //    game.Cheats.FreezeTime = !game.Cheats.FreezeTime;
                                        //    outputMessage = $"freeze time: {(game.Cheats.FreezeTime ? "on" : "off")}";
                                        //}
                                        else
                                        {
                                            outputMessage = "cheat code invalid!";
                                        }
                                    }
                                    else
                                    {
                                        inputInvalid = true;
                                    }
                                }
                                else
                                {
                                    inputInvalid = true;
                                }
                            }
                            catch (Exception) { inputInvalid = true; }

                            if (inputInvalid)
                            {
                                outputMessage = "Invalid Input";
                            }
                            break;
                    }
                }


                if (game.IsWon || game.IsLost)
                {
                    gameRunning = false;
                    outputMessage = "";
                }
            }
            RedrawField(interactiveMode, gameRunning, outputMessage, false);

            PrintLinesInBox(new string[] {
                game.IsWon ? "You Won!" : game.IsLost? "You Lost!" : "Game Quit!",
                string.Empty,
                "Statistics",
                " ",
                $"Time Spent: {game.Stats.TimeSpent}",
                $"Clicks: {game.Stats.Clicks()}",
                $"Clicks per Second: {game.Stats.ClicksPerSec()}",
                $"3BV: {game.Stats.ThreeBV}",
                $"3BV per Second: {game.Stats.ThreeBVPerSec()}",
                $"Efficiency: {game.Stats.Efficiency()}%"
            });

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        private static bool GetSetup(string[] args)
        {
            game = new Game();
            string gamemode = string.Empty;

            if (args.Length > 0)
                gamemode = args[0];


            if (string.IsNullOrEmpty(gamemode))
            {
                Console.WriteLine("Please enter a gamemode (beginner, intermediate, expert, insane, custom) or read save file with (read).");
                Console.Write("gamemode = ");
                gamemode = Console.ReadLine();
            }

            switch (gamemode.ToLower())
            {
                case "b":
                case "beginner":
                    game.Init(9, 9, 10);
                    break;
                case "i":
                case "int":
                case "intermediate":
                    game.Init(16, 16, 40);
                    break;
                case "e":
                case "expert":
                    game.Init(30, 16, 99);
                    break;
                case "ins":
                case "insane":
                    game.Init(40, 16, 150);
                    break;
                case "c":
                case "custom":
                    string widthStr = string.Empty;
                    string heightStr = string.Empty;
                    string minesStr = string.Empty;

                    if (args.Length > 1)
                        widthStr = args[1];
                    if (args.Length > 2)
                        heightStr = args[2];
                    if (args.Length > 3)
                        minesStr = args[3];


                    if (string.IsNullOrEmpty(widthStr))
                    {
                        Console.WriteLine("Please enter a field width.");
                        Console.Write("width = ");
                        widthStr = Console.ReadLine();
                    }
                    if (string.IsNullOrEmpty(heightStr))
                    {
                        Console.WriteLine("Please enter a field height.");
                        Console.Write("height = ");
                        heightStr = Console.ReadLine();
                    }
                    if (string.IsNullOrEmpty(minesStr))
                    {
                        Console.WriteLine("Please enter a number of mines on field.");
                        Console.Write("mines = ");
                        minesStr = Console.ReadLine();
                    }

                    try
                    {
                        game.Init(int.Parse(widthStr), int.Parse(heightStr), int.Parse(minesStr));
                    }
                    catch (Exception) { }

                    break;
                case "r":
                case "read":
                    try
                    {
                        game.InitFromSave(JsonConvert.DeserializeObject<Save>(File.ReadAllText(saveFile)));
                    }
                    catch (Exception) { }

                    break;
            }

            if (!game.IsInitialised)
            {
                Console.WriteLine("------ ERROR ------");
                Console.WriteLine("-  Invalid Input  -");
                Console.WriteLine("-------------------");
                Console.ReadKey();
                return false;
            }
            return true;
        }

        private static void RedrawField(bool isInteractiveMode, bool isGameRunning, string outputMessage, bool showHelp)
        {
            Console.Clear();

            PrintLinesInBox(new string[] {
                "ConsoleSweeper",
                string.Empty,
                $"{game.Width} x {game.Height} / {game.Mines}",
                " ",
                $"Time: {(int)Math.Floor(game.CurrentTime().TotalSeconds)}",
                $"Mines: {game.MinesLeft()}"});

            game.PrintField(minOutputSize);

            var outputBox = new List<string>() { "Controls", " " };
            if (isInteractiveMode)
            {
                //navigate with wasd or arrow keys, open with space or enter, flag with f, question with q
                //modifiers: shift -> move to next unempty field, ctrl -> move to end, openrecursive

                outputBox.Add($"Navigation: {Util.ArrowUp},{Util.ArrowLeft},{Util.ArrowDown},{Util.ArrowRight} or W,A,S,D");
                outputBox.Add($"Open: Space or Enter");
                outputBox.Add($"Flag: F");
                outputBox.Add($"Question: Q");
                outputBox.Add($"Exit Interactive Mode: Escape");
            }
            else
            {
                //save game with :w / :write, quit with :q / :quit, save and quit with :wq / :writequit, back to interactive with :i / :interactive, help with :h / :help
                //play the game with :up [amount] / :u, :position [xPos] [yPos] / :p / :pos, :m / :mark / :question, :flag / :f, :open / :o,

                outputBox.Add($"Enter Commands with ':[command]'.");
                outputBox.Add($"Enter ':help' for more information.");
            }


            if (outputMessage != string.Empty)
            {
                outputBox.Add(string.Empty);
                outputBox.Add("Output:");
                outputBox.Add(" ");
                outputBox.Add(outputMessage);
            }

            if (showHelp)
            {
                outputBox.Add(string.Empty);
                outputBox.Add("Help:");
                outputBox.Add(" ");
                outputBox.Add(":w, :write => save game");
                outputBox.Add(":q, :quit => quit game");
                outputBox.Add(":wq, :writequit => save and quit game");
                outputBox.Add(":i, :interactive => back to interactive mode");
                outputBox.Add(":h, :help => show help");
                outputBox.Add(":u [amount], :up => move up by amount (default 1)");
                outputBox.Add(":l [amount], :left => move left by amount (default 1)");
                outputBox.Add(":d [amount], :down => move down by amount (default 1)");
                outputBox.Add(":r [amount], :right => move right by amount (default 1)");
                outputBox.Add(":p [x] [y], :position => move to position (x/y)");
                outputBox.Add(":m, :mark, :question => question current field");
                outputBox.Add(":f, :flag => flag current field");
                outputBox.Add(":o, :open => open current field");
                outputBox.Add(":orec, :openrecursive => open current field recursively");
                outputBox.Add(":c [code], :cheats => toggle cheatcode");
            }

            PrintLinesInBox(outputBox.ToArray());
        }
        private static void PrintLinesInBox(params string[] lines)
        {
            int boxSize = game.Width * 2 + 1;
            if (boxSize < minOutputSize)
                boxSize = minOutputSize;
            int boxContentSize = boxSize - 2;
            int maxLineSize = boxContentSize - 2;
            Console.WriteLine($"{Util.CornerTL}{Util.GetStringNTimes(Util.LineH, boxContentSize)}{Util.CornerTR}");
            foreach (var line in lines)
            {
                var sizedLineStack = new Stack<string>();
                sizedLineStack.Push(line);
                while (sizedLineStack.Count > 0)
                {
                    var sizedLine = sizedLineStack.Pop();
                    if (sizedLine.Length > maxLineSize)
                    {
                        sizedLineStack.Push(sizedLine.Substring(maxLineSize));
                        sizedLineStack.Push(sizedLine.Substring(0, maxLineSize));
                    }
                    else
                    {
                        if (sizedLine == string.Empty)
                        {
                            Console.WriteLine($"{Util.SplitVRSingle}{Util.GetStringNTimes(Util.LineHSingle, boxContentSize)}{Util.SplitVLSingle}");
                        }
                        else
                        {
                            int spaceToFill = boxContentSize - sizedLine.Length;
                            Console.WriteLine($"{Util.LineV}{Util.GetStringNTimes(" ", (int)Math.Floor(spaceToFill / 2d))}{sizedLine}{Util.GetStringNTimes(" ", (int)Math.Floor(spaceToFill / 2d) + (spaceToFill % 2))}{Util.LineV}");
                        }
                    }
                }
            }
            Console.WriteLine($"{Util.CornerBL}{Util.GetStringNTimes(Util.LineH, boxContentSize)}{Util.CornerBR}");
        }

        private static void HandleMove(Direction dir, ConsoleModifiers modifiers)
        {
            if (modifiers.HasFlag(ConsoleModifiers.Shift))
            {
                game.MoveWhileOpened(dir);
            }
            else if (modifiers.HasFlag(ConsoleModifiers.Control))
            {
                game.Move(dir, game.MaxMove);
            }
            else
            {
                game.Move(dir);
            }
        }

        private static void HandleOpen(ConsoleModifiers modifiers)
        {
            if (modifiers.HasFlag(ConsoleModifiers.Control))
            {
                game.Open();
            }
            else
            {
                game.OpenIterative();
            }
        }

        private static void SaveGame()
        {
            File.WriteAllText(saveFile, JsonConvert.SerializeObject(game.GetSave(), Formatting.Indented));
        }
    }
}
