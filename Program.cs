using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Represents the current state of the game
enum GameState { Menu, Playing, Paused, Ended }

class Program
{
    static GameState State = GameState.Menu; // current game state
    const string ScoreFile = "scores.txt";  // file to store high scores
    static bool SoundOn = true;             // sound toggle
    static string Difficulty = "Easy";      // current difficulty setting

    static void Main()
    {
        // ===== FULL-SCREEN SETUP =====
        Console.CursorVisible = false; // hide blinking cursor
        Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight); // maximize window
        Console.SetBufferSize(Console.LargestWindowWidth, Console.LargestWindowHeight); // set buffer size

        // Main loop to handle game states
        while (true)
        {
            switch (State)
            {
                case GameState.Menu: ShowMainMenu(); break; // show menu
                case GameState.Playing: RunGameLoop(); break; // run gameplay
                case GameState.Paused: ShowPause(); break; // show pause screen
                case GameState.Ended: ShowEndMenu(); break; // show end screen
            }
        }
    }

    // Show main menu and handle user input
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

        if (key == '1') // start game
        {
            Maze.LoadMazes(Difficulty); // load maze for selected difficulty
            Player.Reset();             // reset steps and timer
            Countdown(3);               // countdown before game starts
            State = GameState.Playing;  // switch to playing
        }
        else if (key == '2') ShowHighScores();  // view scores
        else if (key == '3') SelectDifficulty(); // select difficulty
        else if (key == '4') SoundOn = !SoundOn; // toggle sound
        else if (key == '5') Environment.Exit(0); // exit game
    }

    // Handle difficulty selection
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

    // Game loop handling player movement and maze updates
    static void RunGameLoop()
    {
        Maze.Draw(Player.Steps, Player.Elapsed(), showSteps: true, showTime: true, SoundOn); // draw maze and HUD

        var key = Console.ReadKey(true).Key;

        // Pause, hint, or sound toggle
        if (key == ConsoleKey.P) { State = GameState.Paused; return; }
        if (key == ConsoleKey.H) { ShowHint(); return; }
        if (key == ConsoleKey.F) { SoundOn = !SoundOn; return; }

        // Determine movement direction
        int dr = 0, dc = 0;
        if (key == ConsoleKey.W) dr = -1; // move up
        else if (key == ConsoleKey.S) dr = +1; // move down
        else if (key == ConsoleKey.A) dc = -1; // move left
        else if (key == ConsoleKey.D) dc = +1; // move right
        else return; // invalid key

        var nr = Maze.PlayerPos.r + dr;
        var nc = Maze.PlayerPos.c + dc;

        // Check if next cell is walkable
        if (!Maze.IsWalkable(nr, nc))
        {
            if (SoundOn) Console.Beep(); // beep if hitting wall
            return;
        }

        Maze.MovePlayer(dr, dc); // move player
        Player.BumpStep();       // increment step counter
        if (SoundOn) Console.Beep(); // beep for valid move

        // Check if player reached exit
        if (Maze.AtExit())
        {
            if (SoundOn) Console.Beep();
            if (!Maze.NextMaze()) // no more mazes
                State = GameState.Ended; // switch to end screen
            else
            {
                Player.Reset(); // reset steps and timer for next maze
                Countdown(3);   // countdown before next maze
            }
        }
    }

    // Pause screen logic
    static void ShowPause()
    {
        Console.SetCursorPosition(0, Maze.Rows + 2);
        Console.WriteLine("Paused. Press P to resume.");
        while (true)
        {
            var k = Console.ReadKey(true).Key;
            if (k == ConsoleKey.P) { State = GameState.Playing; return; } // resume game
        }
    }

    // Show end menu and allow replay or return to menu
    static void ShowEndMenu()
    {
        Console.SetCursorPosition(0, Maze.Rows + 3);
        Console.WriteLine($"You finished all mazes! Steps={Player.Steps}, Time={Player.Elapsed():mm\\:ss}");
        SaveScore(Player.Steps, Player.Elapsed()); // save score to file
        Console.WriteLine("Press M for Menu or R to Replay.");

        while (true)
        {
            var k = Console.ReadKey(true).Key;
            if (k == ConsoleKey.M) { State = GameState.Menu; return; } // go back to menu
            if (k == ConsoleKey.R) // replay game
            {
                Maze.LoadMazes(Difficulty);
                Player.Reset();
                Countdown(3);
                State = GameState.Playing;
                return;
            }
        }
    }

    // Countdown display before starting game
    static void Countdown(int n)
    {
        for (int i = n; i >= 1; i--)
        {
            Console.Clear();
            Console.WriteLine($"Starting in {i}...");
            System.Threading.Thread.Sleep(600); // wait 0.6 sec
        }
    }

    // Save player score to file
    static void SaveScore(int steps, TimeSpan time)
    {
        List<string> scores = new List<string>();
        if (File.Exists(ScoreFile))
            scores = File.ReadAllLines(ScoreFile).ToList(); // load existing scores

        string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm} - Steps:{steps}, Time:{time:mm\\:ss}";
        scores.Add(entry); // add new score

        if (scores.Count > 10) scores = scores.Skip(scores.Count - 10).ToList(); // keep last 10 scores
        File.WriteAllLines(ScoreFile, scores); // write scores to file
    }

    // Show top 10 high scores
    static void ShowHighScores()
    {
        Console.Clear();
        Console.WriteLine("=== High Scores (Latest 10) ===");
        if (File.Exists(ScoreFile))
        {
            var lines = File.ReadAllLines(ScoreFile);
            foreach (var line in lines) Console.WriteLine(line); // display scores
        }
        else Console.WriteLine("No scores yet!"); // no scores available
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey(true);
        State = GameState.Menu; // return to menu
    }

    // Show hint for next move
    static void ShowHint()
    {
        var hint = Maze.GetHint(); // get suggested move
        if (hint == null)
        {
            Console.SetCursorPosition(0, Maze.Rows + 3);
            Console.WriteLine("No path available!"); // no solution found
            return;
        }

        var (dr, dc) = hint.Value;
        string dir = dr == -1 ? "Up" : dr == 1 ? "Down" : dc == -1 ? "Left" : "Right"; // convert move to text
        Console.SetCursorPosition(0, Maze.Rows + 3);
        Console.WriteLine($"Hint: Move {dir} (Press any key to continue)...");
        Console.ReadKey(true); // wait for input
    }
}
