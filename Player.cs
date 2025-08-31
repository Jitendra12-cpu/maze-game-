using System;

static class Player
{
    public static int Steps { get; private set; }       // tracks the number of steps the player has taken
    public static DateTime StartTime { get; private set; } // records the start time of the current game or maze

    // Reset player stats at the beginning of a maze or game
    public static void Reset()
    {
        Steps = 0;              // reset step count
        StartTime = DateTime.Now; // reset start time to current time
    }

    // Increment the step counter when the player moves
    public static void BumpStep() => Steps++;

    // Calculate the elapsed time since the start of the maze/game
    public static TimeSpan Elapsed() => DateTime.Now - StartTime;
}
