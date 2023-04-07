
using Meadow.Foundation.Displays.Lcd;

class DisplayManager
{
    private CharacterDisplay lcdDisplay;

    public DisplayManager(CharacterDisplay lcdDisplay)
    {
        this.lcdDisplay = lcdDisplay;
    }

    // --------------------
    //  Egg Matching Game
    //
    //     Easter 2023
    //  by Justin-Credible
    // --------------------
    public void ShowTitleScreen()
    {
        lcdDisplay.ClearLines();
        lcdDisplay.WriteLine(" Egg Matching Game", 0);
        lcdDisplay.WriteLine("    Easter 2023", 2);
        lcdDisplay.WriteLine(" by Justin-Credible", 3);
    }

    // --------------------
    //  Egg Matching Game
    //
    //   Press any button
    //    to start game...
    // --------------------
    public void ShowAttractScreen()
    {
        lcdDisplay.ClearLines();
        lcdDisplay.WriteLine(" Egg Matching Game", 0);
        lcdDisplay.WriteLine("  Press any button", 2);
        lcdDisplay.WriteLine("   to start game...", 3);
    }

    // --------------------
    //  Egg Matching Game
    //
    //   Match the pattern!
    //      Get ready!
    // --------------------
    public void ShowGetReady()
    {
        lcdDisplay.ClearLines();
        lcdDisplay.WriteLine(" Egg Matching Game", 0);
        lcdDisplay.WriteLine("  Match the pattern!", 2);
        lcdDisplay.WriteLine("     Get ready!", 3);
    }

    // --------------------
    //   CONGRATULATIONS!  
    //                     
    //     You got a         
    //    high score!!!     
    // --------------------
    public void ShowCongratsScreen()
    {
        lcdDisplay.ClearLines();
        lcdDisplay.WriteLine("  CONGRATULATIONS!", 0);
        lcdDisplay.WriteLine("    You got a", 2);
        lcdDisplay.WriteLine("   high score!!!", 3);
    }

    // --------------------
    //       Round 24      
    //
    //   Watch closely...
    //
    // --------------------
    public void ShowWaitScreen(int round, bool partialUpdate = false)
    {
        if (!partialUpdate)
        {
            lcdDisplay.ClearLines();
            lcdDisplay.WriteLine($"      Round {round}", 0);
        }
        else
        {
            lcdDisplay.SetCursorPosition(12, 0);
            lcdDisplay.Write($"{round}");
        }

        lcdDisplay.WriteLine("  Watch closely...", 2);
    }

    // --------------------
    //       Round 24      
    //
    //     Now you try...
    //
    // --------------------
    public void ShowYourTurnScreen(int round, bool partialUpdate = false)
    {
        if (!partialUpdate)
        {
            lcdDisplay.ClearLines();
            lcdDisplay.WriteLine($"      Round {round}", 0);
        }
        else
        {
            lcdDisplay.SetCursorPosition(12, 0);
            lcdDisplay.Write($"{round}");
        }

        lcdDisplay.WriteLine("    Now you try...", 2);
    }

    // --------------------
    //       Round 24      
    //
    //        Great!!
    //
    // --------------------
    public void ShowRoundWin(int round, bool partialUpdate = false)
    {
        if (!partialUpdate)
        {
            lcdDisplay.ClearLines();
            lcdDisplay.WriteLine($"      Round {round}", 0);
        }
        else
        {
            lcdDisplay.SetCursorPosition(12, 0);
            lcdDisplay.Write($"{round}");
        }

        lcdDisplay.WriteLine("       Great!!", 2);
    }

    // --------------------
    //  Round 24 - FAILED
    //
    //       OH NO!
    //     GAME OVER!
    // --------------------
    public void ShowGameOverScreen(int round)
    {
        lcdDisplay.ClearLines();
        lcdDisplay.WriteLine($" Round {round} - FAILED", 0);
        lcdDisplay.WriteLine("      OH NO!", 2);
        lcdDisplay.WriteLine("    GAME OVER!", 3);
    }
}
