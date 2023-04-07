
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
        new KeyValuePair<string, int>("JGU", 20),
        new KeyValuePair<string, int>("EGG", 15),
        new KeyValuePair<string, int>("BOB", 10),
    };

    private int cursorLocation = 0;
    private int currentScore = 0;
    private char[] currentInitials = new char[3] { 'A', ' ', ' ' };

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

        var initials2 = sortedList[1].Key;
        var score2 = sortedList[1].Value;

        var initials3 = sortedList[2].Key;
        var score3 = sortedList[2].Value;

        lcdDisplay.WriteLine("--- High Scores ---", 0, false);
        lcdDisplay.WriteLine($"   1st   {initials1}  {score1}", 1, false);
        lcdDisplay.WriteLine($"   2nd   {initials2}  {score2}", 2, false);
        lcdDisplay.WriteLine($"   3rd   {initials3}  {score3}", 3, false);
    }

    public void StartEntry(int score)
    {
        Reset();
        currentScore = score;
        UpdateDisplay();
    }

    public void PreviousCharacter()
    {
        var currentCharacter = currentInitials[cursorLocation];

        if (currentCharacter == 'A')
        {
            currentCharacter = ' ';
        }
        else if (currentCharacter == ' ')
        {
            currentCharacter = 'Z';
        }
        else
        {
            currentCharacter--;
        }

        currentInitials[cursorLocation] = currentCharacter;

        UpdateDisplay(partialUpdate: true);
    }

    public void NextCharacter()
    {
        var currentCharacter = currentInitials[cursorLocation];

        if (currentCharacter == 'Z')
        {
            currentCharacter = ' ';
        }
        else if (currentCharacter == ' ')
        {
            currentCharacter = 'A';
        }
        else
        {
            currentCharacter++;
        }

        currentInitials[cursorLocation] = currentCharacter;

        UpdateDisplay(partialUpdate: true);
    }

    public bool ConfirmSelection()
    {
        if (cursorLocation == 3)
        {
            var initials = new String(currentInitials);
            UpdateHighScoreList(initials, currentScore);
            return true;
        }
        else
        {
            if (cursorLocation != 2 && currentInitials[cursorLocation + 1] == ' ')
                currentInitials[cursorLocation + 1] = currentInitials[cursorLocation];

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
        currentInitials = new char[3] {'A', ' ', ' '};
        cursorLocation = 0;
        currentScore = 0;
    }

    private void UpdateHighScoreList(string initials, int score)
    {
        var sortedList = highScores.OrderByDescending(e => e.Value).ToList();

        int insertAt = -1;

        for (var i = 0; i < MAX_ENTRIES; i++)
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

        highScores = sortedList.GetRange(0, MAX_ENTRIES);
    }

    // --------------------
    // Enter your initials:
    //       |AAA| [OK]
    //        ^            
    // OK=Green   Back=Blue
    // --------------------
    private void UpdateDisplay(bool partialUpdate = false)
    {
        if (!partialUpdate)
        {
            lcdDisplay.ClearLines();
            lcdDisplay.WriteLine("Enter your initials:", 0);
            lcdDisplay.WriteLine("      |   |", 1);
            lcdDisplay.WriteLine("OK=Green   Back=Blue", 3);
        }

        var initials = new String(currentInitials);
        var ok = cursorLocation == 3 ? " [OK]" :"     ";

        lcdDisplay.SetCursorPosition(7, 1);
        lcdDisplay.Write($"{initials}|{ok}");

        lcdDisplay.SetCursorPosition(7, 2);

        if (cursorLocation == 0)
            lcdDisplay.Write("^      ");
        else if (cursorLocation == 1)
            lcdDisplay.Write(" ^     ");
        else if (cursorLocation == 2)
            lcdDisplay.Write("  ^    ");
        else if (cursorLocation == 3)
            lcdDisplay.Write("      ^");
    }
}