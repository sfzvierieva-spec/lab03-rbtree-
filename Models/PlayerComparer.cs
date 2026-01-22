namespace GameLeaderboard;

public class PlayerComparer : IComparer<Player>
{
    public int Compare(Player? x, Player? y)
    {
        // Edge cases: null handling
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        // Primary comparison: MMR descending (higher MMR = higher rank)
        // y.MMR compared to x.MMR to get descending order
        int mmrComparison = y.MMR.CompareTo(x.MMR);
        
        if (mmrComparison != 0)
            return mmrComparison;

        // Tiebreaker: Summoner name ascending (alphabetical)
        return string.Compare(x.SummonerName, y.SummonerName, StringComparison.Ordinal);
    }
}