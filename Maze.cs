using System;
using System.Collections.Generic;

static class Maze
{
    public static char[,] Grid { get; private set; }
    public static int Rows { get; private set; }
    public static int Cols { get; private set; }
    public static (int r, int c) PlayerPos { get; private set; }
    public static (int r, int c) ExitPos { get; private set; }

    private static List<string[]> Mazes;
    private static int CurrentMazeIndex = 0;

    public static void LoadMazes(string difficulty)
    {
        Mazes = new List<string[]>();

        if (difficulty == "Easy")
        {
            Mazes.Add(new string[]
            {
                "# # # # # # # # # #",
                "#     E           #",
                "#   #   # #       #",
                "#   P       #     #",
                "# # # # # # # # # #"
            });
            Mazes.Add(new string[]
            {
                "# # # # # # # # # # #",
                "# P       #         #",
                "#   # #   #   #     #",
                "#       #     E     #",
                "# # # # # # # # # # #"
            });
        }
        else if (difficulty == "Medium")
        {
            Mazes.Add(new string[]
            {
                "# # # # # # # # # # # #",
                "# P     #   #         #",
                "#   # #   #   #   #   #",
                "#       #       #     #",
                "#   #       #       E #",
                "# # # # # # # # # # # #"
            });
            Mazes.Add(new string[]
            {
                "# # # # # # # # # # # # #",
                "# P     #     #   #      #",
                "#   #   # # #   #   #    #",
                "#       #     #       E  #",
                "# # # # # # # # # # # # #"
            });
        }
        else // Hard
        {
            Mazes.Add(new string[]
            {
                "# # # # # # # # # # # # # #",
                "# P   #     #   #     #    #",
                "#   #   # #   #   # #   #  #",
                "#       #     #         E  #",
                "# # # # # # # # # # # # # #"
            });
        }

        CurrentMazeIndex = 0;
        LoadMaze(CurrentMazeIndex);
    }

    public static void LoadMaze(int index)
    {
        string[] lines = Mazes[index];
        Rows = lines.Length;
        Cols = lines[0].Length;
        Grid = new char[Rows, Cols];

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                char ch = lines[r][c];
                Grid[r, c] = ch;
                if (ch == 'P') PlayerPos = (r, c);
                if (ch == 'E') ExitPos = (r, c);
            }
        }
    }

    public static void Draw(int steps, TimeSpan time, bool showSteps, bool showTime, bool soundOn)
    {
        Console.Clear();
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
                Console.Write(Grid[r, c]);
            Console.WriteLine();
        }

        Console.Write("HUD: ");
        if (showSteps) Console.Write($"Steps={steps}  ");
        if (showTime) Console.Write($"Time={time:mm\\:ss}  ");
        Console.WriteLine($"(W/A/S/D move, P pause, H hint, S sound {(soundOn ? "On" : "Off")})");
    }

    public static bool IsWalkable(int r, int c)
    {
        if (r < 0 || c < 0 || r >= Rows || c >= Cols) return false;
        return Grid[r, c] != '#';
    }

    public static void MovePlayer(int dr, int dc)
    {
        var (r, c) = PlayerPos;
        int nr = r + dr, nc = c + dc;

        if (!IsWalkable(nr, nc)) return;

        // Trail effect
        if (Grid[r, c] == 'P') Grid[r, c] = '.';

        Grid[nr, nc] = (Grid[nr, nc] == 'E') ? 'E' : 'P';
        PlayerPos = (nr, nc);
    }

    public static bool AtExit() => PlayerPos == ExitPos;

    public static bool NextMaze()
    {
        CurrentMazeIndex++;
        if (CurrentMazeIndex >= Mazes.Count) return false;
        LoadMaze(CurrentMazeIndex);
        return true;
    }

    // BFS Hint System
    public static (int dr, int dc)? GetHint()
    {
        var visited = new bool[Rows, Cols];
        var queue = new Queue<((int r, int c) pos, (int r, int c)? parent)>();
        queue.Enqueue((PlayerPos, null));
        visited[PlayerPos.r, PlayerPos.c] = true;

        var parentMap = new Dictionary<(int, int), (int, int)?>();
        int[] drs = { -1, 1, 0, 0 };
        int[] dcs = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            var (pos, parent) = queue.Dequeue();
            parentMap[pos] = parent;
            if (pos == ExitPos) break;

            for (int i = 0; i < 4; i++)
            {
                int nr = pos.r + drs[i];
                int nc = pos.c + dcs[i];
                if (IsWalkable(nr, nc) && !visited[nr, nc])
                {
                    visited[nr, nc] = true;
                    queue.Enqueue(((nr, nc), pos));
                }
            }
        }

        // Backtrack
        var step = ExitPos;
        while (parentMap.ContainsKey(step) && parentMap[step] != PlayerPos && parentMap[step] != null)
            step = parentMap[step].Value;

        if (!parentMap.ContainsKey(step) || step == ExitPos) return null;
        return (step.r - PlayerPos.r, step.c - PlayerPos.c);
    }
}
