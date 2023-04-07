
using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays.Lcd;
using Meadow.Foundation.Relays;

class DebugManager
{
    private CharacterDisplay lcdDisplay;
    private PiezoPlayer player;
    private EffectsManager effects;
    private Relay relayLedRed;
    private Relay relayLedYellow;
    private Relay relayLedGreen;
    private Relay relayLedBlue;
    private Relay relayWinnerSound;

    private int selection = 0;

    //                                            |-----------------|
    private const string MX_Toggle_Red_Relay    = "MX:Toggle:Relay:R";
    private const string MX_Toggle_Yellow_Relay = "MX:Toggle:Relay:Y";
    private const string MX_Toggle_Green_Relay  = "MX:Toggle:Relay:G";
    private const string MX_Toggle_Blue_Relay   = "MX:Toggle:Relay:B";
    private const string MX_Toggle_Winner_Relay = "MX:Toggle:Relay:W";
    private const string FX_GameStart           = "FX:Game Start";
    private const string FX_FirstRound_R        = "FX:First Round:R";
    private const string FX_FirstRound_Y        = "FX:First Round:Y";
    private const string FX_FirstRound_G        = "FX:First Round:G";
    private const string FX_FirstRound_B        = "FX:First Round:B";
    private const string FX_NextRound_RYBG      = "FX:NextRound:RYBG";
    private const string FX_NextRound_BGRR      = "FX:NextRound:BGRR";
    private const string FX_InputCorrect_R      = "FX:InputCorrect:R";
    private const string FX_InputCorrect_Y      = "FX:InputCorrect:Y";
    private const string FX_InputCorrect_G      = "FX:InputCorrect:G";
    private const string FX_InputCorrect_B      = "FX:InputCorrect:B";
    private const string FX_RoundWin            = "FX:Round Win";
    private const string FX_RoundFail_R         = "FX:Round Fail: R";
    private const string FX_RoundFail_Y         = "FX:Round Fail: Y";
    private const string FX_RoundFail_G         = "FX:Round Fail: G";
    private const string FX_RoundFail_B         = "FX:Round Fail: B";
    private const string FX_HighScore           = "FX:High Score";
    private const string Song_GameStart         = "Song:Game Start";
    private const string Song_Tada              = "Song:Tada";
    private const string FN_SetLowScores        = "FN:Set Low Scores";
    private const string FN_EnterInitials       = "FN:Enter Initials";
    private const string FN_ExitDebug           = "FN:Exit Debug";

    private readonly List<string> Selections = new List<string>()
    {
        MX_Toggle_Red_Relay,
        MX_Toggle_Yellow_Relay,
        MX_Toggle_Green_Relay,
        MX_Toggle_Blue_Relay,
        MX_Toggle_Winner_Relay,
        FX_GameStart,
        FX_FirstRound_R,
        FX_FirstRound_Y,
        FX_FirstRound_G,
        FX_FirstRound_B,
        FX_NextRound_RYBG,
        FX_NextRound_BGRR,
        FX_InputCorrect_R,
        FX_InputCorrect_Y,
        FX_InputCorrect_G,
        FX_InputCorrect_B,
        FX_RoundWin,
        FX_RoundFail_R,
        FX_RoundFail_Y,
        FX_RoundFail_G,
        FX_RoundFail_B,
        FX_HighScore,
        Song_GameStart,
        Song_Tada,
        FN_SetLowScores,
        FN_EnterInitials,
        FN_ExitDebug,
    };

    public DebugManager(CharacterDisplay lcdDisplay, PiezoSpeaker speaker, EffectsManager effects, Relay relayLedRed, Relay relayLedYellow, Relay relayLedGreen, Relay relayLedBlue, Relay relayWinnerSound)
    {
        this.lcdDisplay = lcdDisplay;
        this.player = new PiezoPlayer(speaker);
        this.effects = effects;
        this.relayLedRed = relayLedRed;
        this.relayLedYellow = relayLedYellow;
        this.relayLedGreen = relayLedGreen;
        this.relayLedBlue = relayLedBlue;
        this.relayWinnerSound = relayWinnerSound;

        selection = Selections.FindIndex(0, Selections.Count, (m) => m == FN_ExitDebug);

        if (selection == -1)
            selection = 0;
    }

    // --------------------
    // Egg Game Debug Menu
    //   Perform Action:
    // 01:FX:Next Round: RYBG
    // R=Up Y=Down     G=OK
    // --------------------
    public void ShowMenu()
    {
        lcdDisplay.ClearLines();
        lcdDisplay.WriteLine("Egg Game Debug Menu", 0);
        lcdDisplay.WriteLine("  Perform Action:", 1);
        lcdDisplay.WriteLine("R=Up Y=Down     G=OK", 3);
        UpdateMenu();
    }

    public void PreviousOption()
    {
        if (selection == 0)
        {
            selection = Selections.Count - 1;
        }
        else
        {
            selection--;
        }

        UpdateMenu();
    }

    public void NextOption()
    {
        if (selection == Selections.Count - 1)
        {
            selection = 0;
        }
        else
        {
            selection++;
        }

        UpdateMenu();
    }

    public async Task<DebugAction> MakeSelection()
    {
        var name = Selections[selection];

        switch(name)
        {
            case MX_Toggle_Red_Relay:
                relayLedRed.Toggle();
                return DebugAction.None;
            case MX_Toggle_Yellow_Relay:
                relayLedYellow.Toggle();
                return DebugAction.None;
            case MX_Toggle_Green_Relay:
                relayLedGreen.Toggle();
                return DebugAction.None;
            case MX_Toggle_Blue_Relay:
                relayLedBlue.Toggle();
                return DebugAction.None;
            case MX_Toggle_Winner_Relay:
                relayWinnerSound.Toggle();
                return DebugAction.None;
            case FX_GameStart:
                await effects.PlayGameStart();
                return DebugAction.None;
            case FX_FirstRound_R:
                await effects.PlayFirstRound(GameColor.Red);
                return DebugAction.None;
            case FX_FirstRound_Y:
                await effects.PlayFirstRound(GameColor.Yellow);
                return DebugAction.None;
            case FX_FirstRound_G:
                await effects.PlayFirstRound(GameColor.Green);
                return DebugAction.None;
            case FX_FirstRound_B:
                await effects.PlayFirstRound(GameColor.Blue);
                return DebugAction.None;
            case FX_NextRound_RYBG:
                await effects.PlayNextRound(new List<GameColor>() { GameColor.Red, GameColor.Yellow, GameColor.Blue, GameColor.Green });
                return DebugAction.None;
            case FX_NextRound_BGRR:
                await effects.PlayNextRound(new List<GameColor>() { GameColor.Blue, GameColor.Green, GameColor.Red, GameColor.Red });
                return DebugAction.None;
            case FX_InputCorrect_R:
                await effects.PlayInputCorrect(GameColor.Red);
                return DebugAction.None;
            case FX_InputCorrect_Y:
                await effects.PlayInputCorrect(GameColor.Yellow);
                return DebugAction.None;
            case FX_InputCorrect_G:
                await effects.PlayInputCorrect(GameColor.Green);
                return DebugAction.None;
            case FX_InputCorrect_B:
                await effects.PlayInputCorrect(GameColor.Blue);
                return DebugAction.None;
            case FX_RoundWin:
                await effects.PlayRoundWin();
                return DebugAction.None;
            case FX_RoundFail_R:
                await effects.PlayRoundFail(GameColor.Red);
                return DebugAction.None;
            case FX_RoundFail_Y:
                await effects.PlayRoundFail(GameColor.Yellow);
                return DebugAction.None;
            case FX_RoundFail_G:
                await effects.PlayRoundFail(GameColor.Green);
                return DebugAction.None;
            case FX_RoundFail_B:
                await effects.PlayRoundFail(GameColor.Blue);
                return DebugAction.None;
            case FX_HighScore:
                await effects.PlayHighScore();
                return DebugAction.None;
            case Song_GameStart:
                await player.PlaySong(Songs.GameStart);
                return DebugAction.None;
            case Song_Tada:
                await player.PlaySong(Songs.Tada);
                return DebugAction.None;
            case FN_SetLowScores:
                return DebugAction.SetLowScores;
            case FN_EnterInitials:
                return DebugAction.HighScoreEntry;
            case FN_ExitDebug:
                return DebugAction.Exit;
            default:
                return DebugAction.None;
        }
    }

    private void UpdateMenu()
    {
        var number = selection.ToString("D2");
        var name = Selections[selection];
        lcdDisplay.WriteLine($"{number}:{name}", 2);
    }
}