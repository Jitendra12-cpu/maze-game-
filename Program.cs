using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

enum GameState { Menu, Playing, Paused, Ended }

class Program
{
    static GameState State = GameState.Menu;
    const string ScoreFile = "scores.txt";
    static bool SoundOn = true;
    static string Difficulty = "Easy";

    static void Main()
    {
        // ===== FULL-SCREEN SETUP =====
        Console.CursorVisible = false; // Hide the blinking cursor
        Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
        Console.SetBufferSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
       

        while (true)
        {
            switch (State)
            {
                case GameState.Menu: ShowMainMenu(); break;
                case GameState.Playing: RunGameLoop(); break;
                case GameState.Paused: ShowPause(); break;
                case GameState.Ended: ShowEndMenu(); break;
            }
        }
    }

    static void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine("=== Escape the Maze ===");
        Console.WriteLine("1) Start Game");
        Console.WriteLine("2) View High Scores");
        Console.WriteLine("3) Difficulty (Current: " + Difficulty + ")");
        Console.WriteLine("4) Toggle Sound (Current: " + (SoundOn ? "On" : "Off") + ")");
        Console.WriteLine("5) Exit");
        Console.Write("Select: ");
        var key = Console.ReadKey(true).KeyChar;

        if (key == '1')
        {
            Maze.LoadMazes(Difficulty);
            Player.Reset();
            Countdown(3);
            State = GameState.Playing;
        }
        else if (key == '2') ShowHighScores();
        else if (key == '3') SelectDifficulty();
        else if (key == '4') SoundOn = !SoundOn;
        else if (key == '5') Environment.Exit(0);
    }

    static void SelectDifficulty()
    {
        Console.Clear();
        Console.WriteLine("Select Difficulty:");
        Console.WriteLine("1) Easy");
        Console.WriteLine("2) Medium");
        Console.WriteLine("3) Hard");
        var k = Console.ReadKey(true).KeyChar;
        if (k == '1') Difficulty = "Easy";
        else if (k == '2') Difficulty = "Medium";
        else if (k == '3') Difficulty = "Hard";
    }

    static void RunGameLoop()
    {
        Maze.Draw(Player.Steps, Player.Elapsed(), showSteps: true, showTime: true, SoundOn);

        var key = Console.ReadKey(true).Key;
        if (key == ConsoleKey.P) { State = GameState.Paused; return; }
        if (key == ConsoleKey.H) { ShowHint(); return; }
        if (key == ConsoleKey.F) { SoundOn = !SoundOn; return; }

        int dr = 0, dc = 0;
        if (key == ConsoleKey.W) dr = -1;
        else if (key == ConsoleKey.S) dr = +1;
        else if (key == ConsoleKey.A) dc = -1;
        else if (key == ConsoleKey.D) dc = +1;
        else return;

        var nr = Maze.PlayerPos.r + dr;
        var nc = Maze.PlayerPos.c + dc;

        if (!Maze.IsWalkable(nr, nc))
        {
            if (SoundOn) Console.Beep();
            return;
        }

        Maze.MovePlayer(dr, dc);
        Player.BumpStep();
        if (SoundOn) Console.Beep();

        if (Maze.AtExit())
        {
            if (SoundOn) Console.Beep();
            if (!Maze.NextMaze())
                State = GameState.Ended;
            else
            {
                Player.Reset();
                Countdown(3);
            }
        }
    }

    static void ShowPause()
    {
        Console.SetCursorPosition(0, Maze.Rows + 2);
        Console.WriteLine("Paused. Press P to resume.");
        while (true)
        {
            var k = Console.ReadKey(true).Key;
            if (k == ConsoleKey.P) { State = GameState.Playing; return; }
        }
    }

    static void ShowEndMenu()
    {
        Console.SetCursorPosition(0, Maze.Rows + 3);
        Console.WriteLine($"You finished all mazes! Steps={Player.Steps}, Time={Player.Elapsed():mm\\:ss}");
        SaveScore(Player.Steps, Player.Elapsed());
        Console.WriteLine("Press M for Menu or R to Replay.");

        while (true)
        {
            var k = Console.ReadKey(true).Key;
            if (k == ConsoleKey.M) { State = GameState.Menu; return; }
            if (k == ConsoleKey.R)
            {
                Maze.LoadMazes(Difficulty);
                Player.Reset();
                Countdown(3);
                State = GameState.Playing;
                return;
            }
        }
    }

    static void Countdown(int n)
    {
        for (int i = n; i >= 1; i--)
        {
            Console.Clear();
            Console.WriteLine($"Starting in {i}...");
            System.Threading.Thread.Sleep(600);
        }
    }

    static void SaveScore(int steps, TimeSpan time)
    {
        List<string> scores = new List<string>();
        if (File.Exists(ScoreFile))
            scores = File.ReadAllLines(ScoreFile).ToList();

        string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm} - Steps:{steps}, Time:{time:mm\\:ss}";
        scores.Add(entry);

        if (scores.Count > 10) scores = scores.Skip(scores.Count - 10).ToList();
        File.WriteAllLines(ScoreFile, scores);
    }

    static void ShowHighScores()
    {
        Console.Clear();
        Console.WriteLine("=== High Scores (Latest 10) ===");
        if (File.Exists(ScoreFile))
        {
            var lines = File.ReadAllLines(ScoreFile);
            foreach (var line in lines) Console.WriteLine(line);
        }
        else Console.WriteLine("No scores yet!");
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey(true);
        State = GameState.Menu;
    }

    static void ShowHint()
    {
        var hint = Maze.GetHint();
        if (hint == null)
        {
            Console.SetCursorPosition(0, Maze.Rows + 3);
            Console.WriteLine("No path available!");
            return;
        }

        var (dr, dc) = hint.Value;
        string dir = dr == -1 ? "Up" : dr == 1 ? "Down" : dc == -1 ? "Left" : "Right";
        Console.SetCursorPosition(0, Maze.Rows + 3);
        Console.WriteLine($"Hint: Move {dir} (Press any key to continue)...");
        Console.ReadKey(true);
    }
}
