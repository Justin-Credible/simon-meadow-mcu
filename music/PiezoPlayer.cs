
using System;
using System.Threading.Tasks;
using Meadow.Foundation.Audio;
using Meadow.Units;

class PiezoPlayer
{
    private PiezoSpeaker speaker;

    public PiezoPlayer(PiezoSpeaker speaker)
    {
        this.speaker = speaker;
    }

    public async Task PlaySong(Song song)
    {
        for (int i = 0; i < song.Notes.Length; i++)
        {
            if (song.Notes[i] == ' ')
            {
                await Task.Delay(song.Beats[i] * song.Tempo); // rest
            }
            else
            {
                var duration = new TimeSpan(song.Beats[i] * song.Tempo * 2000);
                await PlayNote(song.Notes[i], duration);
                // pause between notes
                await Task.Delay(song.Beats[i] * song.Tempo / 2);
            }
        }
    }

    public async Task PlayNote(char note, TimeSpan duration)
    {
        char[] names = { 'c', 'd', 'e', 'f', 'g', 'a', 'b', 'B'};
        int[] tones = { 523, 587, 659, 698, 783, 880, 987, 932 };

        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == note)
            {
                var tone = tones[i];
                await speaker.PlayTone(new Frequency(tone), duration);
            }
        }
    }
}