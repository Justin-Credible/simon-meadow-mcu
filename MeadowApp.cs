using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays.Lcd;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Relays;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private RgbPwmLed ledOnboard;
        private CharacterDisplay lcdDisplay;
        private PiezoSpeaker speaker;

        private PushButton buttonRed;
        private PushButton buttonYellow;
        private PushButton buttonGreen;
        private PushButton buttonBlue;

        private Relay relayLedRed;
        private Relay relayLedYellow;
        private Relay relayLedGreen;
        private Relay relayLedBlue;
        private Relay relayWinnerSound;

        private bool debugEnabled = true; // TODO: Set to false once done debugging.
        private bool blockInput = true;

        private GameState gameState = GameState.Attract;
        private List<GameColor> pattern = new List<GameColor>();
        private List<GameColor> input = new List<GameColor>();
        private Random random = new Random((int)DateTime.Now.Ticks);

        private HighScoreManager highScore;
        private DisplayManager display;
        private EffectsManager effects;

        private DebugManager debug;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            ledOnboard = new RgbPwmLed(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode
            );

            ledOnboard.SetColor(Color.Yellow);

            // TODO: Set pins based on hardware implementation!
            // Bailing out for now so I don't fry something.
            if (true)
            {
                Console.WriteLine("HALT: Need to set hardware pins.");
                ledOnboard.SetColor(Color.Red);
                return base.Initialize();
            }

            Console.WriteLine("Initializing hardware...");

            lcdDisplay = new CharacterDisplay(
                pinRS: MeadowApp.Device.Pins.D10,
                pinE: MeadowApp.Device.Pins.D09,
                pinD4: MeadowApp.Device.Pins.D08,
                pinD5: MeadowApp.Device.Pins.D07,
                pinD6: MeadowApp.Device.Pins.D06,
                pinD7: MeadowApp.Device.Pins.D05,
                rows: 4,
                columns: 20
            );

            speaker = new PiezoSpeaker(Device.Pins.D04);

            buttonRed = new PushButton(Device.Pins.D00);
            buttonYellow = new PushButton(Device.Pins.D00);
            buttonGreen = new PushButton(Device.Pins.D00);
            buttonBlue = new PushButton(Device.Pins.D00);

            buttonRed.Clicked += ButtonRed_Clicked;
            buttonYellow.Clicked += ButtonYellow_Clicked;
            buttonGreen.Clicked += ButtonGreen_Clicked;
            buttonBlue.Clicked += ButtonBlue_Clicked;

            relayLedRed = new Relay(Device.Pins.D00);
            relayLedYellow = new Relay(Device.Pins.D00);
            relayLedGreen = new Relay(Device.Pins.D00);
            relayLedBlue = new Relay(Device.Pins.D00);
            relayWinnerSound = new Relay(Device.Pins.D00);

            Console.WriteLine("Initializing managers...");

            display = new DisplayManager(lcdDisplay);
            highScore = new HighScoreManager(lcdDisplay);
            effects = new EffectsManager(
                speaker,
                relayWinnerSound,
                relayLedRed,
                relayLedYellow,
                relayLedGreen,
                relayLedBlue
            );

            debug = new DebugManager(lcdDisplay, speaker, effects);

            Console.WriteLine("Starting attract mode thread...");
            Task.Run(() => AttractMode());

            if (debugEnabled)
            {
                Console.WriteLine("Staring debug mode...");
                gameState = GameState.Debug;
                ledOnboard.SetColor(Color.Purple);
                debug.ShowMenu();
            }
            else
            {
                Console.WriteLine("Ready!");
                gameState = GameState.Attract;
                ledOnboard.SetColor(Color.Green);
            }

            return base.Initialize();
        }

        public override Task Run()
        {
            Console.WriteLine("Run...");

            return base.Run();
        }

        private void ButtonRed_Clicked(object sender, EventArgs e)
        {
            ButtonPressed(GameColor.Red);
        }

        private void ButtonYellow_Clicked(object sender, EventArgs e)
        {
            ButtonPressed(GameColor.Yellow);
        }

        private void ButtonGreen_Clicked(object sender, EventArgs e)
        {
            ButtonPressed(GameColor.Green);
        }

        private void ButtonBlue_Clicked(object sender, EventArgs e)
        {
            ButtonPressed(GameColor.Blue);
        }

        private async void ButtonPressed(GameColor color)
        {
            if (blockInput)
                return;

            switch (gameState)
            {
                case GameState.Attract:
                    await HandleStartNewGame();
                    break;
                case GameState.Playing:
                    await HandleNewInput(color);
                    break;
                case GameState.NameEntry:
                    await HandleNameEntry(color);
                    break;
                case GameState.Debug:
                    await HandleDebugMenu(color);
                    break;
            }

            blockInput = false;
        }

        private async Task HandleStartNewGame()
        {
            gameState = GameState.Playing;

            input.Clear();
            pattern.Clear();

            display.ShowGetReady();
            await effects.PlayGameStart();

            await StartNextRound();
        }

        private async Task StartNextRound()
        {
            pattern.Add(GetNextColor());

            display.ShowWaitScreen(pattern.Count);

            if (pattern.Count == 1)
            {
                await effects.PlayFirstRound(pattern[0]);
            }
            else
            {
                await effects.PlayNextRound(pattern);
            }

            display.ShowYourTurnScreen(pattern.Count);
        }

        private async Task HandleNewInput(GameColor actual)
        {
            input.Add(actual);

            var expected = pattern[input.Count - 1];

            if (expected == actual)
            {
                await effects.PlayInputCorrect(actual);

                if (input.Count == pattern.Count)
                {
                    display.ShowRoundWin(pattern.Count);
                    await effects.PlayRoundWin();
                    await StartNextRound();
                }
            }
            else
            {
                display.ShowGameOverScreen(pattern.Count);
                await effects.PlayRoundFail(expected);

                var score = pattern.Count - 1;

                if (highScore.IsHighScore(score))
                {
                    display.ShowCongratsScreen();
                    await effects.PlayHighScore();
                    highScore.StartEntry(score);
                    gameState = GameState.NameEntry;
                }
                else
                {
                    highScore.ShowHighScores();
                    await Task.Delay(4000);
                    gameState = GameState.Attract;
                }
            }
        }

        private async Task HandleNameEntry(GameColor color)
        {
            bool isDone = false;

            switch (color)
            {
                case GameColor.Red:
                    highScore.PreviousCharacter();
                    break;
                case GameColor.Yellow:
                    highScore.NextCharacter();
                    break;
                case GameColor.Green:
                    isDone = highScore.ConfirmSelection();
                    break;
                case GameColor.Blue:
                    highScore.Back();
                    break;
            }

            if (isDone)
            {
                await effects.PlayConfirm();
                highScore.ShowHighScores();
                await Task.Delay(4000);
                gameState = GameState.Attract;
            }
        }

        private async Task HandleDebugMenu(GameColor color)
        {
            var action = DebugAction.None;

            switch (color)
            {
                case GameColor.Red:
                    debug.PreviousOption();
                    break;
                case GameColor.Yellow:
                    debug.NextOption();
                    break;
                case GameColor.Green:
                    action = await debug.MakeSelection();
                    break;
                default:
                    break;
            }

            if (action == DebugAction.HighScoreEntry)
            {
                highScore.StartEntry(75);
                gameState = GameState.NameEntry;
            }
            else if (action == DebugAction.SetLowScores)
            {
                highScore.SetScores(new List<KeyValuePair<string, int>>()
                {
                    new KeyValuePair<string, int>("THR", 3),
                    new KeyValuePair<string, int>("TWO", 2),
                    new KeyValuePair<string, int>("ONE", 1),
                });
            }
            else if (action == DebugAction.Exit)
            {
                gameState = GameState.Attract;
            }
        }

        private async Task AttractMode()
        {
            while (true)
            {
                if (gameState != GameState.Attract)
                {
                    await Task.Delay(1000);
                    continue;
                }

                display.ShowTitleScreen();
                await Task.Delay(3000);

                if (gameState != GameState.Attract) { continue; }

                display.ShowAttractScreen();
                await Task.Delay(3000);

                if (gameState != GameState.Attract) { continue; }

                highScore.ShowHighScores();
                await Task.Delay(3000);
            }
        }

        private GameColor GetNextColor()
        {
            return (GameColor)random.Next(1, 4);
        }
    }
}