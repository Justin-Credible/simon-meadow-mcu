
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Relays;

class EffectsManager
{
    private PiezoPlayer player;
    private Relay relayWinnerSound;
    private Relay relayLedRed;
    private Relay relayLedYellow;
    private Relay relayLedGreen;
    private Relay relayLedBlue;

    public EffectsManager(PiezoSpeaker speaker, Relay relayWinnerSound, Relay relayLedRed, Relay relayLedYellow, Relay relayLedGreen, Relay relayLedBlue)
    {
        player = new PiezoPlayer(speaker);
        this.relayWinnerSound = relayWinnerSound;
        this.relayLedRed = relayLedRed;
        this.relayLedYellow = relayLedYellow;
        this.relayLedGreen = relayLedGreen;
        this.relayLedBlue = relayLedBlue;
    }

    public async Task PlayHardwareReady()
    {
        await player.PlaySong(Songs.Tada);
    }

    public async Task PlayGameStart()
    {
        var songTask = player.PlaySong(Songs.GameStart);

        relayLedRed.IsOn = true;
        relayLedYellow.IsOn = false;
        relayLedGreen.IsOn = true;
        relayLedBlue.IsOn = false;

        await Task.Delay(500);

        for (var i = 0; i < 4; i++)
        {
            relayLedRed.Toggle();
            relayLedYellow.Toggle();
            relayLedGreen.Toggle();
            relayLedBlue.Toggle();
            await Task.Delay(500);
        }

        await LedsChaseFoward();
        await LedsChaseBackward();

        relayLedRed.IsOn = true;
        relayLedYellow.IsOn = false;
        relayLedGreen.IsOn = true;
        relayLedBlue.IsOn = false;

        await Task.Delay(500);

        for (var i = 0; i < 4; i++)
        {
            relayLedRed.Toggle();
            relayLedYellow.Toggle();
            relayLedGreen.Toggle();
            relayLedBlue.Toggle();
            await Task.Delay(500);
        }

        relayLedRed.IsOn = false;
        relayLedYellow.IsOn = false;
        relayLedGreen.IsOn = false;
        relayLedBlue.IsOn = false;

        await songTask;
    }

    private async Task LedsChaseFoward()
    {
        relayLedRed.IsOn = false;
        relayLedYellow.IsOn = false;
        relayLedGreen.IsOn = false;
        relayLedBlue.IsOn = false;

        await Task.Delay(200);

        relayLedRed.IsOn = true;
        await Task.Delay(200);

        relayLedRed.IsOn = false;
        relayLedYellow.IsOn = true;
        await Task.Delay(200);

        relayLedYellow.IsOn = false;
        relayLedGreen.IsOn = true;
        await Task.Delay(200);

        relayLedGreen.IsOn = false;
        relayLedBlue.IsOn = true;
        await Task.Delay(200);

        relayLedRed.IsOn = false;
        relayLedYellow.IsOn = false;
        relayLedGreen.IsOn = false;
        relayLedBlue.IsOn = false;
    }

    private async Task LedsChaseBackward()
    {
        relayLedRed.IsOn = false;
        relayLedYellow.IsOn = false;
        relayLedGreen.IsOn = false;
        relayLedBlue.IsOn = false;

        await Task.Delay(200);

        relayLedBlue.IsOn = true;
        await Task.Delay(200);

        relayLedBlue.IsOn = false;
        relayLedGreen.IsOn = true;
        await Task.Delay(200);

        relayLedGreen.IsOn = false;
        relayLedYellow.IsOn = true;
        await Task.Delay(200);

        relayLedYellow.IsOn = false;
        relayLedRed.IsOn = true;
        await Task.Delay(200);

        relayLedRed.IsOn = false;
        relayLedYellow.IsOn = false;
        relayLedGreen.IsOn = false;
        relayLedBlue.IsOn = false;
    }

    public async Task PlayFirstRound(GameColor color)
    {
        SetLedState(color, true);

        var note = GetNoteForColor(color);

        for (var i = 0; i < 10; i++)
        {
            await player.PlayNote(note, new TimeSpan(0, 0, 0, 0, 50));
            await Task.Delay(50);
        }

        SetLedState(color, false);
    }

    public async Task PlayNextRound(List<GameColor> pattern)
    {
        for (var i = 0; i < pattern.Count; i++)
        {
            var color = pattern[i];
            var note = GetNoteForColor(color);

            SetLedState(color, true);
            await player.PlayNote(note, new TimeSpan(0, 0, 0, 0, 300));
            SetLedState(color, false);

            await Task.Delay(100);
        }
    }

    public async Task PlayInputCorrect(GameColor color)
    {
        var note = GetNoteForColor(color);

        var audio = player.PlayNote(note, new TimeSpan(0, 0, 0, 0, 300));
        SetLedState(color, true);
        await audio;
        await Task.Delay(100);
        SetLedState(color, false);
    }

    public async Task PlayRoundWin()
    {
        await player.PlaySong(Songs.Tada);
    }

    public async Task PlayRoundFail(GameColor expected)
    {
        var note = player.PlayNote('c', new TimeSpan(0, 0, 3));

        SetLedState(GameColor.Red, expected == GameColor.Red);
        SetLedState(GameColor.Yellow, expected == GameColor.Yellow);
        SetLedState(GameColor.Green, expected == GameColor.Green);
        SetLedState(GameColor.Blue, expected == GameColor.Blue);

        await note;

        SetLedState(GameColor.Red, false);
        SetLedState(GameColor.Yellow, false);
        SetLedState(GameColor.Green, false);
        SetLedState(GameColor.Blue, false);
    }

    public async Task PlayHighScore()
    {
        relayWinnerSound.IsOn = true;

        relayLedRed.IsOn = true;
        relayLedYellow.IsOn = false;
        relayLedGreen.IsOn = false;
        relayLedBlue.IsOn = true;

        for (var i = 0; i < 7; i++)
        {
            relayLedRed.Toggle();
            relayLedYellow.Toggle();
            relayLedGreen.Toggle();
            relayLedBlue.Toggle();
            await Task.Delay(200);
        }

        await LedsChaseFoward();
        await LedsChaseBackward();

        relayLedRed.IsOn = true;
        relayLedYellow.IsOn = false;
        relayLedGreen.IsOn = false;
        relayLedBlue.IsOn = true;

        for (var i = 0; i < 7; i++)
        {
            relayLedRed.Toggle();
            relayLedYellow.Toggle();
            relayLedGreen.Toggle();
            relayLedBlue.Toggle();
            await Task.Delay(200);
        }

        relayLedRed.IsOn = false;
        relayLedYellow.IsOn = false;
        relayLedGreen.IsOn = false;
        relayLedBlue.IsOn = false;

        await LedsChaseFoward();
        await LedsChaseBackward();

        relayLedRed.IsOn = false;
        relayLedYellow.IsOn = false;
        relayLedGreen.IsOn = false;
        relayLedBlue.IsOn = false;

        await Task.Delay(3000);
        relayWinnerSound.IsOn = false;
    }

    public async Task PlayConfirm()
    {
        await player.PlaySong(Songs.Tada);
    }

    private char GetNoteForColor(GameColor color)
    {
        switch (color)
        {
            case GameColor.Red:
                return 'b';
            case GameColor.Yellow:
                return 'g';
            case GameColor.Green:
                return 'e';
            case GameColor.Blue:
            default:
                return 'c';
        }
    }

    private void SetLedState(GameColor color, Boolean isOn)
    {
        switch (color)
        {
            case GameColor.Red:
                relayLedRed.IsOn = isOn;
                break;
            case GameColor.Yellow:
                relayLedYellow.IsOn = isOn;
                break;
            case GameColor.Green:
                relayLedGreen.IsOn = isOn;
                break;
            case GameColor.Blue:
                relayLedBlue.IsOn = isOn;
                break;
            default:
                break;
        }
    }
}
