using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays.Lcd;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Relays;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Relays;
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

        private bool debugEnabled = true;
        private bool blockInput = true;
        private bool playNewGameEffect = true;

        private GameState gameState = GameState.Attract;
        private List<GameColor> pattern = new List<GameColor>();
        private List<GameColor> input = new List<GameColor>();
        private Random random = new Random((int)DateTime.Now.Ticks);
        private CancellationTokenSource attractModeCancelleationTokenSource;

        private HighScoreManager highScore;
        private DisplayManager display;
        private EffectsManager effects;

        private DebugManager debug;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            ledOnboard = new RgbPwmLed(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode
            );

            ledOnboard.SetColor(Color.Yellow);

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
            buttonYellow = new PushButton(Device.Pins.D01);
            buttonGreen = new PushButton(Device.Pins.D13);
            buttonBlue = new PushButton(Device.Pins.D03);

            buttonRed.Clicked += ButtonRed_Clicked;
            buttonYellow.Clicked += ButtonYellow_Clicked;
            buttonGreen.Clicked += ButtonGreen_Clicked;
            buttonBlue.Clicked += ButtonBlue_Clicked;

            relayLedRed = new Relay(Device.Pins.D11, RelayType.NormallyClosed) { IsOn = false };
            relayLedYellow = new Relay(Device.Pins.D12, RelayType.NormallyClosed) { IsOn = false };
            relayLedGreen = new Relay(Device.Pins.D15, RelayType.NormallyClosed) { IsOn = false };
            relayLedBlue = new Relay(Device.Pins.D02, RelayType.NormallyClosed) { IsOn = false };
            relayWinnerSound = new Relay(Device.Pins.D14, RelayType.NormallyClosed) { IsOn = false };

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

            debug = new DebugManager(lcdDisplay, speaker, effects, relayLedRed, relayLedYellow, relayLedGreen, relayLedBlue, relayWinnerSound);

            Console.WriteLine("Initialization complete!");

            return base.Initialize();
        }

        public override Task Run()
        {
            Thread.CurrentThread.Name = "Main";

            if (debugEnabled)
            {
                Console.WriteLine("Staring debug mode...");
                gameState = GameState.Debug;
                ledOnboard.SetColor(Color.White);
                debug.ShowMenu();
            }
            else
            {
                Console.WriteLine("Ready!");
                gameState = GameState.Attract;
                ledOnboard.SetColor(Color.Green);
                StartAttractMode();
            }

            Task.Run(() => effects.PlayHardwareReady());
            blockInput = false;

            return base.Run();
        }

        public override Task OnError(Exception e)
        {
            ledOnboard.SetColor(Color.Red);
            return base.OnError(e);
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

            blockInput = true;

            try
            {
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
            }
            finally
            {
                blockInput = false;
            }
        }

        private async Task HandleStartNewGame()
        {
            StopAttractMode();
            gameState = GameState.Playing;

            input.Clear();
            pattern.Clear();

            await Task.Delay(100);
            display.ShowGetReady();
            await Task.Delay(100);

            if (playNewGameEffect)
            {
                await effects.PlayGameStart();
                playNewGameEffect = false;
            }
            else
            {
                await Task.Delay(2000);
            }

            await StartNextRound();
        }

        private async Task StartNextRound()
        {
            input.Clear();
            pattern.Add(GetNextColor());

            // We only need to update the entire display the first round.
            // Between each subsequent round we only need to update a smaller portion.
            bool partialScreenUpdate = pattern.Count > 1;

            display.ShowWaitScreen(pattern.Count, partialScreenUpdate);

            if (pattern.Count == 1)
            {
                await effects.PlayFirstRound(pattern[0]);
            }
            else
            {
                await effects.PlayNextRound(pattern);
            }

            display.ShowYourTurnScreen(pattern.Count, partialScreenUpdate);
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
                    var roundWinEffect = effects.PlayRoundWin();
                    display.ShowRoundWin(pattern.Count, partialUpdate: true);
                    await roundWinEffect;
                    await StartNextRound();
                }
            }
            else
            {
                var roundFailEffect = effects.PlayRoundFail(expected);
                display.ShowGameOverScreen(pattern.Count);
                await roundFailEffect;

                var score = pattern.Count - 1;

                if (highScore.IsHighScore(score))
                {
                    var highScoreEffect = effects.PlayHighScore();
                    display.ShowCongratsScreen();
                    await highScoreEffect;
                    highScore.StartEntry(score);
                    gameState = GameState.NameEntry;
                }
                else
                {
                    highScore.ShowHighScores();
                    await Task.Delay(4000);
                    gameState = GameState.Attract;
                    StartAttractMode();
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
                StartAttractMode();
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
                ledOnboard.SetColor(Color.Green);
                gameState = GameState.Attract;
                StartAttractMode();
            }
        }

        private void StartAttractMode()
        {
            Console.WriteLine("Starting attract mode thread...");
            attractModeCancelleationTokenSource = new CancellationTokenSource();
            var cancellationToken = attractModeCancelleationTokenSource.Token;
            Task.Run(() => AttractMode(cancellationToken), cancellationToken);
        }

        private void StopAttractMode()
        {
            if (attractModeCancelleationTokenSource == null)
                return;

            Console.WriteLine("Ending attract mode thread...");
            attractModeCancelleationTokenSource.Cancel();
            attractModeCancelleationTokenSource.Dispose();
        }

        private async Task AttractMode(CancellationToken cancelleationToken)
        {
            Thread.CurrentThread.Name = "AttractMode";

            while (true)
            {
                display.ShowTitleScreen();

                for (var i = 0; i < 10; i++)
                {
                    await Task.Delay(500);
                    if (cancelleationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("Attract mode cancellation requested; ending attract mode.");
                        return;
                    }
                }

                display.ShowAttractScreen();

                for (var i = 0; i < 10; i++)
                {
                    await Task.Delay(500);
                    if (cancelleationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("Attract mode cancellation requested; ending attract mode.");
                        return;
                    }
                }

                highScore.ShowHighScores();

                for (var i = 0; i < 10; i++)
                {
                    await Task.Delay(500);
                    if (cancelleationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("Attract mode cancellation requested; ending attract mode.");
                        return;
                    }
                }

                playNewGameEffect = true;
            }
        }

        private GameColor GetNextColor()
        {
            return (GameColor)random.Next(0, 4);
        }
    }
}