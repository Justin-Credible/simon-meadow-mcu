
using System.Collections.Generic;

class Songs
{
    /// <summary>
    /// Eine Kleine Nachtmusik (1st Movement)
    /// COMPOSED BY WOLFGANG AMADEUS MOZART
    /// https://www.youtube.com/watch?v=q5kddudO7o8
    /// https://www.musicnotes.com/sheetmusic/mtd.asp?ppn=MN0134684
    /// https://www.youtube.com/watch?v=CNRQ-DW7064
    /// </summary>
    public static readonly Song GameStart = new Song()
    {
        Tempo = 300,
        Notes = "fcfcfcfac bgbgbgegc ",
        Beats = new List<int>()
        {
            3, 1, 3, 1, 1, 1, 1, 1, 2, 2,
            3, 1, 3, 1, 1, 1, 1, 1, 2, 2,
        }
    };

    public static readonly Song Tada = new Song()
    {
        Tempo = 150,
        Notes = "ccg",
        Beats = new List<int>(){ 1, 1, 4 }
    };
}