namespace GameLeaderboard;

public class Player : IComparable<Player>
{
    public string SummonerName { get; set; }
    public int MMR { get; set; } // Match Making Rating (Elo-like score)
    public int Wins { get; set; }
    public int Losses { get; set; }
    public RankTier Tier { get; private set; }

    public Player(string summonerName, int mmr, int wins, int losses)
    {
        SummonerName = summonerName;
        MMR = mmr;
        Wins = wins;
        Losses = losses;
        Tier = CalculateTier(mmr);
    }

    public int TotalGames => Wins + Losses;
    public double WinRate => TotalGames > 0 ? (double)Wins / TotalGames * 100 : 0;

    private static RankTier CalculateTier(int mmr)
    {
        return mmr switch
        {
            >= 2400 => RankTier.Challenger,
            >= 2200 => RankTier.Grandmaster,
            >= 2000 => RankTier.Master,
            >= 1800 => RankTier.Diamond,
            >= 1600 => RankTier.Platinum,
            >= 1400 => RankTier.Gold,
            >= 1200 => RankTier.Silver,
            >= 1000 => RankTier.Bronze,
            _ => RankTier.Iron
        };
    }

    public void UpdateMMR(int newMMR)
    {
        MMR = newMMR;
        Tier = CalculateTier(newMMR);
    }

    public int CompareTo(Player? other)
    {
        if (other == null) return 1;
        
        return other.MMR.CompareTo(MMR);
    }

    public override string ToString()
    {
        return $"{SummonerName,-20} | {Tier,-12} | MMR: {MMR,4} | W/L: {Wins,3}/{Losses,3} ({WinRate:F1}%)";
    }
}
