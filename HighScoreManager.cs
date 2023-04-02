
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Meadow.Foundation.Displays.Lcd;

class HighScoreManager
{
    const int MAX_ENTRIES = 3;

    private List<KeyValuePair<string, int>> highScores = new List<KeyValuePair<string, int>>()
    {
        new KeyValuePair<string, int>("JGU", 15),
        new KeyValuePair<string, int>("EGG", 11),
        new KeyValuePair<string, int>("BOB", 9),
    };

    private int cursorLocation = 0;
    private char currentCharacter = 'A';
    private int currentScore = 0;
    private string currentInitials = String.Empty;

    private CharacterDisplay lcdDisplay;

    public HighScoreManager(CharacterDisplay lcdDisplay)
    {
        this.lcdDisplay = lcdDisplay;
        Reset();
    }

    public void SetScores(List<KeyValuePair<string, int>> scores)
    {
        highScores = scores;
    }

    public bool IsHighScore(int score)
    {
        foreach (var scoreEntry in highScores)
        {
            if (score > scoreEntry.Value)
            {
                return true;
            }
        }

        return false;
    }

    // --------------------
    // --- High Scores ---
    //    1st   AAA  454   
    //    2nd   AAA  454   
    //    3rd   AAA  454   
    // --------------------
    public void ShowHighScores()
    {
        lcdDisplay.ClearLines();

        var sortedList = highScores.OrderByDescending(e => e.Value).ToList();

        var initials1 = sortedList[0].Key;
        var score1 = sortedList[0].Value;

        var initials2 = sortedList[0].Key;
        var score2 = sortedList[0].Value;

        var initials3 = sortedList[0].Key;
        var score3 = sortedList[0].Value;

        lcdDisplay.WriteLine("--- High Scores ---", 0, false);
        lcdDisplay.WriteLine($"   1st   {initials1}  {score1}", 0, false);
        lcdDisplay.WriteLine($"   2nd   {initials2}  {score2}", 0, false);
        lcdDisplay.WriteLine($"   3rd   {initials3}  {score3}", 0, false);
    }

    public void StartEntry(int score)
    {
        Reset();
        currentScore = score;
        UpdateDisplay();
    }

    public void PreviousCharacter()
    {
        if (currentCharacter == 'A')
        {
            currentCharacter = 'Z';
        }
        else
        {
            currentCharacter--;
        }

        UpdateDisplay(partialUpdate: true);
    }

    public void NextCharacter()
    {
        if (currentCharacter == 'Z')
        {
            currentCharacter = 'A';
        }
        else
        {
            currentCharacter++;
        }

        UpdateDisplay(partialUpdate: true);
    }

    public bool ConfirmSelection()
    {
        if (cursorLocation == 3)
        {
            UpdateHighScoreList(currentInitials, currentScore);
            return true;
        }
        else
        {
            currentInitials.Remove(cursorLocation);
            currentInitials.Insert(currentCharacter, currentCharacter.ToString());
            cursorLocation++;
            UpdateDisplay(partialUpdate: true);
            return false;
        }
    }

    public void Back()
    {
        if (cursorLocation == 0)
            return;
        
        cursorLocation--;
        UpdateDisplay(partialUpdate: true);
    }

    private void Reset()
    {
        currentCharacter = 'A';
        cursorLocation = 0;
        currentScore = 0;
        currentInitials = String.Empty;
    }

    private void UpdateHighScoreList(string initials, int score)
    {
        var sortedList = highScores.OrderByDescending(e => e.Value).ToList();

        int insertAt = -1;

        for (var i = MAX_ENTRIES - 1; i >= 0; i--)
        {
            var entry = sortedList[i];

            if (score > entry.Value)
            {
                insertAt = i;
                break;
            }
        }

        if (insertAt != -1)
        {
            sortedList.Insert(insertAt, new KeyValuePair<string, int>(initials, score));
        }

        highScores = sortedList.GetRange(0, MAX_ENTRIES - 1);
    }

    // --------------------
    //  Please enter your
    //      initials:
    //         AAA [OK]    
    // OK=Green   Back=Blue
    // --------------------
    private void UpdateDisplay(bool partialUpdate = false)
    {
        if (!partialUpdate)
        {
            lcdDisplay.WriteLine(" Please enter your", 0);
            lcdDisplay.WriteLine("     initials:", 1);
            lcdDisplay.WriteLine("OK=Green   Back=Blue", 3);
        }

        var ok = cursorLocation == 3 ? "[OK]" : String.Empty;
        lcdDisplay.WriteLine($"         {currentInitials} {ok}", 2);

        if (cursorLocation == 3)
        {
            lcdDisplay.SetCursorPosition(14, 2);
        }
        else
        {
            lcdDisplay.SetCursorPosition((byte)(9 + cursorLocation), 2);
        }
    }
}