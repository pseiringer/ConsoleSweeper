using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSweeper
{
    public class Game
    {
        public bool IsInitialised { get; private set; } = false;

        public bool IsWon { get; private set; }
        public bool IsLost { get; private set; }


        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Mines { get; private set; }

        private int[,] field;
        private FieldTag[,] shadowOfWar;

        private Cursor cursor;

        public Stats Stats { get; private set; }
        private DateTime startTime;
        private DateTime endTime;

        public int MaxMove { get; private set; }

        public int ActiveFlags { get; private set; }
        public int MinesLeft() => Mines - ActiveFlags;
        public TimeSpan CurrentTime() => IsInitialised ? DateTime.Now.Subtract(startTime) : new TimeSpan();

        public Cheats Cheats { get; private set; }

        public void Init(int width, int height, int mines)
        {
            if (!IsInitValid(width, height, mines))
                return;

            //setup properties
            this.Width = width;
            this.Height = height;
            this.Mines = mines;

            field = new int[width, height];
            shadowOfWar = new FieldTag[width, height];

            IsWon = false;
            IsLost = false;

            MaxMove = Math.Max(width, height);

            ActiveFlags = 0;

            //setup mines
            var maxMineField = width * height;
            List<int> setMineFields = new List<int>();
            var rng = new Random();
            for (int i = 0; i < mines; i++)
            {
                var rdmField = rng.Next(0, maxMineField);

                foreach (var setField in setMineFields)
                {
                    if (rdmField >= setField)
                        rdmField++;
                }

                var x = rdmField / height;
                var y = rdmField % height;
                field[x, y] = -1;

                setMineFields.Add(rdmField);
                setMineFields.Sort();

                maxMineField--;
            }

            //setup numbers
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (field[x, y] == -1)
                        continue;

                    var nrOfMines = 0;
                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        for (int yOffset = -1; yOffset <= 1; yOffset++)
                        {
                            var searchX = x + xOffset;
                            var searchY = y + yOffset;
                            if (searchX >= 0 && searchX < width
                                && searchY >= 0 && searchY < height
                                && !(searchX == x && searchY == y))
                            {
                                if (field[searchX, searchY] == -1)
                                    nrOfMines++;
                            }
                        }
                    }

                    field[x, y] = nrOfMines;
                }
            }

            //setup fog of war
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    shadowOfWar[x, y] = FieldTag.Hidden;
                }
            }

            //setup cursor
            cursor = new Cursor(0, 0);

            //setup cheats
            Cheats = new Cheats();

            //setup stats
            Stats = new Stats();
            Stats.ThreeBV = CalculateThreeBV();
            startTime = DateTime.Now;

            IsInitialised = true;
        }

        public void InitFromSave(Save save)
        {
            field = save.Field;
            shadowOfWar = save.ShadowOfWar;

            Width = field.GetLength(0);
            Height = field.GetLength(1);

            Mines = 0;
            foreach (int value in field)
            {
                if (value < 0)
                    Mines++;
            }
            ActiveFlags = 0;
            foreach (var shadow in shadowOfWar)
            {
                if (shadow == FieldTag.Flagged)
                    ActiveFlags++;
            }

            IsWon = false;
            IsLost = false;

            MaxMove = Math.Max(Width, Height);

            cursor = save.Cursor;

            Stats = save.Stats;

            Cheats = save.Cheats;

            CorrectCursorPositions();

            startTime = DateTime.Now;
            IsInitialised = true;
        }

        private bool IsInitValid(int width, int height, int mines)
        {
            return (width > 0 && height > 0 && mines >= 0 && !((int.MaxValue / width) < height) && mines <= width * height);
        }

        public Save GetSave()
        {
            StopTime();

            return new Save()
            {
                Field = this.field,
                ShadowOfWar = this.shadowOfWar,
                Cursor = this.cursor,
                Stats = this.Stats,
                Cheats = this.Cheats
            };
        }


        public void Move(Direction dir, int amount = 1)
        {
            if (!IsInitialised) return;

            switch (dir)
            {
                case Direction.Up:
                    cursor.Y -= amount;
                    break;
                case Direction.Right:
                    cursor.X += amount;
                    break;
                case Direction.Down:
                    cursor.Y += amount;
                    break;
                case Direction.Left:
                    cursor.X -= amount;
                    break;
            }

            CorrectCursorPositions();
        }
        public void MoveWhileOpened(Direction dir)
        {
            if (!IsInitialised) return;

            bool isOpen = true;
            while (isOpen)
            {
                Move(dir);

                isOpen = shadowOfWar[cursor.X, cursor.Y] == FieldTag.Open
                    && field[cursor.X, cursor.Y] == 0
                    && ((dir == Direction.Up && cursor.Y > 0)
                        || (dir == Direction.Down && cursor.Y < Height - 1)
                        || (dir == Direction.Left && cursor.X > 0)
                        || (dir == Direction.Right && cursor.X < Width - 1));
            }
        }

        public void SetPosition(int xPosition, int yPosition)
        {
            if (!IsInitialised) return;

            cursor.X = xPosition;
            cursor.Y = yPosition;

            CorrectCursorPositions();
        }

        private void CorrectCursorPositions()
        {
            if (cursor.X < 0) cursor.X = 0;
            else if (cursor.X >= Width) cursor.X = Width - 1;

            if (cursor.Y < 0) cursor.Y = 0;
            else if (cursor.Y >= Height) cursor.Y = Height - 1;
        }

        public void Flag()
        {
            if (!IsInitialised) return;

            Stats.Flags += 1;
            Flag(cursor.X, cursor.Y);
        }
        private void Flag(int x, int y)
        {
            if (!IsInitialised) return;

            if (shadowOfWar[x, y] != FieldTag.Open)
            {
                if (shadowOfWar[x, y] == FieldTag.Flagged)
                {
                    shadowOfWar[x, y] = FieldTag.Hidden;
                    ActiveFlags--;
                }
                else
                {
                    shadowOfWar[x, y] = FieldTag.Flagged;
                    ActiveFlags++;
                }
            }
            else if (field[x, y] > 0)
            {
                //Chord Flag
                var neededFields = field[x, y];
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    for (int yOffset = -1; yOffset <= 1; yOffset++)
                    {
                        var searchX = x + xOffset;
                        var searchY = y + yOffset;
                        if (searchX >= 0 && searchX < Width
                            && searchY >= 0 && searchY < Height
                            && !(searchX == x && searchY == y)
                            && shadowOfWar[searchX, searchY] != FieldTag.Open)
                        {
                            neededFields -= 1;
                            if (neededFields < 0)
                                break;
                        }
                    }
                    if (neededFields < 0)
                        break;
                }

                if (neededFields == 0)
                {
                    Stats.Flags -= 1;
                    Stats.ChordFlags += 1;

                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        for (int yOffset = -1; yOffset <= 1; yOffset++)
                        {
                            var searchX = x + xOffset;
                            var searchY = y + yOffset;
                            if (searchX >= 0 && searchX < Width
                                && searchY >= 0 && searchY < Height
                                && !(searchX == x && searchY == y))
                            {
                                var shadow = shadowOfWar[searchX, searchY];
                                if (shadow != FieldTag.Flagged && shadow != FieldTag.Open)
                                    Flag(searchX, searchY);
                            }
                        }
                    }
                }
            }
            else
            {
                Stats.Flags -= 1;
            }
        }
        public void Question()
        {
            if (!IsInitialised) return;

            if (shadowOfWar[cursor.X, cursor.Y] != FieldTag.Open)
            {
                Stats.Questions += 1;
                if (shadowOfWar[cursor.X, cursor.Y] == FieldTag.Questioned)
                    shadowOfWar[cursor.X, cursor.Y] = FieldTag.Hidden;
                else
                {
                    if (shadowOfWar[cursor.X, cursor.Y] == FieldTag.Flagged)
                        ActiveFlags--;
                    shadowOfWar[cursor.X, cursor.Y] = FieldTag.Questioned;
                }
            }
        }
        public void Open()
        {
            if (!IsInitialised) return;

            if (shadowOfWar[cursor.X, cursor.Y] == FieldTag.Flagged)
                return;

            Stats.Opens += 1;
            Open(cursor.X, cursor.Y, true);
        }
        private void Open(int x, int y, bool canChord = false)
        {
            if (!IsInitialised) return;

            if (shadowOfWar[x, y] != FieldTag.Open)
            {
                shadowOfWar[x, y] = FieldTag.Open;
                if (field[x, y] == -1)
                {
                    FinishGame(false);
                }
                else if (field[x, y] == 0)
                {

                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        for (int yOffset = -1; yOffset <= 1; yOffset++)
                        {
                            var searchX = x + xOffset;
                            var searchY = y + yOffset;
                            if (searchX >= 0 && searchX < Width
                                && searchY >= 0 && searchY < Height
                                && !(searchX == x && searchY == y))
                            {
                                Open(searchX, searchY);
                            }
                        }
                    }
                }

                CheckWin();
            }
            else if (field[x, y] > 0 && canChord)
            {
                //Chord
                var neededFlags = field[x, y];
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    for (int yOffset = -1; yOffset <= 1; yOffset++)
                    {
                        var searchX = x + xOffset;
                        var searchY = y + yOffset;
                        if (searchX >= 0 && searchX < Width
                            && searchY >= 0 && searchY < Height
                            && !(searchX == x && searchY == y)
                            && shadowOfWar[searchX, searchY] == FieldTag.Flagged)
                        {
                            neededFlags -= 1;
                            if (neededFlags < 0)
                                break;
                        }
                    }
                    if (neededFlags < 0)
                        break;
                }

                if (neededFlags == 0)
                {
                    Stats.Opens -= 1;
                    Stats.Chords += 1;

                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        for (int yOffset = -1; yOffset <= 1; yOffset++)
                        {
                            var searchX = x + xOffset;
                            var searchY = y + yOffset;
                            if (searchX >= 0 && searchX < Width
                                && searchY >= 0 && searchY < Height
                                && !(searchX == x && searchY == y))
                            {
                                var shadow = shadowOfWar[searchX, searchY];
                                if (shadow != FieldTag.Flagged && shadow != FieldTag.Open)
                                    Open(searchX, searchY);
                            }
                        }
                    }
                }
            }
        }
        public void OpenIterative()
        {
            if (!IsInitialised) return;

            if (shadowOfWar[cursor.X, cursor.Y] == FieldTag.Flagged)
                return;

            Stats.Opens += 1;
            OpenIterative(cursor.X, cursor.Y, true);
        }
        private void OpenIterative(int x, int y, bool canChord = false)
        {
            if (!IsInitialised) return;

            var stack = new Stack<Cursor>();
            stack.Push(new Cursor(x, y));

            while (stack.Count > 0)
            {
                var cursor = stack.Pop();
                if (shadowOfWar[cursor.X, cursor.Y] != FieldTag.Open)
                {
                    shadowOfWar[cursor.X, cursor.Y] = FieldTag.Open;
                    if (field[cursor.X, cursor.Y] == -1)
                    {
                        FinishGame(false);
                    }
                    else if (field[cursor.X, cursor.Y] == 0)
                    {
                        for (int xOffset = -1; xOffset <= 1; xOffset++)
                        {
                            for (int yOffset = -1; yOffset <= 1; yOffset++)
                            {
                                var searchX = cursor.X + xOffset;
                                var searchY = cursor.Y + yOffset;
                                if (searchX >= 0 && searchX < Width
                                    && searchY >= 0 && searchY < Height
                                    && !(searchX == cursor.X && searchY == cursor.Y))
                                {
                                    stack.Push(new Cursor(searchX, searchY));
                                }
                            }
                        }
                    }
                    CheckWin();
                }
                else if (field[cursor.X, cursor.Y] > 0 && canChord)
                {
                    //Chord
                    var neededFlags = field[cursor.X, cursor.Y];
                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        for (int yOffset = -1; yOffset <= 1; yOffset++)
                        {
                            var searchX = x + xOffset;
                            var searchY = y + yOffset;
                            if (searchX >= 0 && searchX < Width
                                && searchY >= 0 && searchY < Height
                                && !(searchX == cursor.X && searchY == cursor.Y)
                                && shadowOfWar[searchX, searchY] == FieldTag.Flagged)
                            {
                                neededFlags -= 1;
                                if (neededFlags < 0)
                                    break;
                            }
                        }
                        if (neededFlags < 0)
                            break;
                    }

                    if (neededFlags == 0)
                    {
                        Stats.Opens -= 1;
                        Stats.Chords += 1;

                        for (int xOffset = -1; xOffset <= 1; xOffset++)
                        {
                            for (int yOffset = -1; yOffset <= 1; yOffset++)
                            {
                                var searchX = cursor.X + xOffset;
                                var searchY = cursor.Y + yOffset;
                                if (searchX >= 0 && searchX < Width
                                    && searchY >= 0 && searchY < Height
                                    && !(searchX == cursor.X && searchY == cursor.Y))
                                {
                                    var shadow = shadowOfWar[searchX, searchY];
                                    if (shadow != FieldTag.Flagged && shadow != FieldTag.Open)
                                        stack.Push(new Cursor(searchX, searchY));
                                }
                            }
                        }
                    }
                }

                canChord = false;
            }
        }

        private void CheckWin()
        {
            if (IsLost) return;

            int unopened = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (shadowOfWar[x, y] != FieldTag.Open)
                    {
                        unopened += 1;
                        if (unopened > Mines) return;
                    }
                }
            }
            FinishGame(true);
        }
        private void FinishGame(bool won)
        {
            IsWon = won;
            IsLost = !won;

            StopTime();
        }
        private void StopTime()
        {
            endTime = DateTime.Now;
            var timeDiff = endTime.Subtract(startTime);
            Stats.TimeSpent = Stats.TimeSpent.Add(timeDiff);
        }

        public void PrintField(int minOutputSize)
        {
            if (!IsInitialised) return;

            int fieldOutputSize = Width * 2 + 1;
            string lPad = "";
            if (fieldOutputSize < minOutputSize)
                lPad = Util.GetStringNTimes(" ", (minOutputSize - fieldOutputSize) / 2);


            if (Cheats.FullVisibility)
            {
                Console.WriteLine($"{lPad}{Util.CornerTL}{Util.GetStringNTimes(Util.LineH + Util.SplitHB, Width - 1)}{Util.LineH}{Util.CornerTR}");
                for (int y = 0; y < Height; y++)
                {
                    Console.Write(lPad);
                    for (int x = 0; x < Width; x++)
                    {
                        Console.Write(Util.LineV);

                        var value = field[x, y];
                        if (value < 0)
                            Console.Write(Util.SquareFilled);
                        else if (value == 0)
                            Console.Write(' ');
                        else
                            Console.Write(value);
                    }
                    Console.WriteLine(Util.LineV);


                    if (y < Height - 1)
                        Console.WriteLine($"{lPad}{Util.SplitVR}{Util.GetStringNTimes(Util.LineH + Util.Split4Way, Width - 1)}{Util.LineH}{Util.SplitVL}");
                }

                Console.WriteLine($"{lPad}{Util.CornerBL}{Util.GetStringNTimes(Util.LineH + Util.SplitHT, Width - 1)}{Util.LineH}{Util.CornerBR}");
                Console.WriteLine();
            }

            Console.WriteLine($"{lPad}{Util.CornerTL}{Util.GetStringNTimes(Util.LineH + Util.SplitHB, Width - 1)}{Util.LineH}{Util.CornerTR}");
            for (int y = 0; y < Height; y++)
            {
                Console.Write(lPad);
                for (int x = 0; x < Width; x++)
                {
                    Console.Write(Util.LineV);



                    //if (cursor.X == x && cursor.Y == y)
                    //{
                    //    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    //}

                    var shadow = shadowOfWar[x, y];
                    switch (shadow)
                    {
                        case FieldTag.Hidden:
                            if (cursor.X == x && cursor.Y == y)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkBlue;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            if (Cheats.ShowOpenings && field[x, y] == 0)
                            {
                                Console.Write(Util.SquareCheckered);
                            }
                            else if (Cheats.ShowMines && field[x, y] < 0)
                            {
                                Console.BackgroundColor = Console.ForegroundColor;
                                Console.ForegroundColor = ConsoleColor.Black;
                                Console.Write(Util.Mine);
                            }
                            else
                            {
                                Console.Write(Util.SquareFilled);
                            }
                            break;
                        case FieldTag.Questioned:
                            if (cursor.X == x && cursor.Y == y)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.BackgroundColor = ConsoleColor.DarkGray;
                            }
                            Console.Write(Util.QuestionMark);
                            break;
                        case FieldTag.Flagged:
                            if (cursor.X == x && cursor.Y == y)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.BackgroundColor = ConsoleColor.DarkGray;
                            }
                            Console.Write(Util.Flag);
                            break;
                        case FieldTag.Open:
                            if (cursor.X == x && cursor.Y == y)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.BackgroundColor = ConsoleColor.Black;
                            }
                            var value = field[x, y];
                            if (value < 0)
                                Console.Write(Util.Mine);
                            else if (value == 0)
                                Console.Write(' ');
                            else
                                Console.Write(value);
                            break;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;

                }
                Console.WriteLine(Util.LineV);


                if (y < Height - 1)
                    Console.WriteLine($"{lPad}{Util.SplitVR}{Util.GetStringNTimes(Util.LineH + Util.Split4Way, Width - 1)}{Util.LineH}{Util.SplitVL}");
            }

            Console.WriteLine($"{lPad}{Util.CornerBL}{Util.GetStringNTimes(Util.LineH + Util.SplitHT, Width - 1)}{Util.LineH}{Util.CornerBR}");
        }

        private int CalculateThreeBV()
        {
            int threeBV = 0;
            var clearedOpenings = new bool[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (field[x, y] == 0)
                    {
                        // add one for each unique opening
                        if (ClearOpeningToArrayIterative(x, y, clearedOpenings))
                            threeBV += 1;
                    }
                    else if (field[x, y] > 0)
                    {
                        // add one if no surrounding openings
                        var hasOpenings = false;
                        for (int xOffset = -1; xOffset <= 1; xOffset++)
                        {
                            if (hasOpenings)
                                break;

                            for (int yOffset = -1; yOffset <= 1; yOffset++)
                            {
                                var searchX = x + xOffset;
                                var searchY = y + yOffset;
                                if (searchX >= 0 && searchX < Width
                                    && searchY >= 0 && searchY < Height
                                    && !(searchX == x && searchY == y))
                                {
                                    if (field[searchX, searchY] == 0)
                                    {
                                        hasOpenings = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!hasOpenings)
                            threeBV += 1;
                    }
                }
            }
            return threeBV;
        }
        private bool ClearOpeningToArray(int x, int y, bool[,] clearedOpenings)
        {
            if (!clearedOpenings[x, y])
            {
                if (field[x, y] == 0)
                {
                    clearedOpenings[x, y] = true;
                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        for (int yOffset = -1; yOffset <= 1; yOffset++)
                        {
                            var searchX = x + xOffset;
                            var searchY = y + yOffset;
                            if (searchX >= 0 && searchX < Width
                                && searchY >= 0 && searchY < Height
                                && !(searchX == x && searchY == y))
                            {
                                ClearOpeningToArray(searchX, searchY, clearedOpenings);
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        private bool ClearOpeningToArrayIterative(int x, int y, bool[,] clearedOpenings)
        {
            var stack = new Stack<Cursor>();
            stack.Push(new Cursor(x, y));

            var result = false;
            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (!clearedOpenings[current.X, current.Y])
                {
                    if (field[current.X, current.Y] == 0)
                    {
                        clearedOpenings[current.X, current.Y] = true;
                        for (int xOffset = -1; xOffset <= 1; xOffset++)
                        {
                            for (int yOffset = -1; yOffset <= 1; yOffset++)
                            {
                                var searchX = current.X + xOffset;
                                var searchY = current.Y + yOffset;
                                if (searchX >= 0 && searchX < Width
                                    && searchY >= 0 && searchY < Height
                                    && !(searchX == current.X && searchY == current.Y))
                                {
                                    stack.Push(new Cursor(searchX, searchY));
                                }
                            }
                        }
                        result = true;
                    }
                }
            }
            return result;
        }
    }
}
