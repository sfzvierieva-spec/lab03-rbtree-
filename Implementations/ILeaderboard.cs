namespace GameLeaderboard;

public interface ILeaderboard
{
    int PlayerCount { get; }
    void AddOrUpdatePlayer(Player player);
    void BulkAddPlayers(IEnumerable<Player> players);
    int GetPlayerRank(string summonerName);
    Player? GetPlayerAtRank(int rank);
    List<Player> GetTopPlayers(int count);
    List<Player> GetPlayersAroundRank(int rank, int context);
    void UpdatePlayerMMR(string summonerName, int newMMR);
    Dictionary<RankTier, int> GetRankDistribution();
}
