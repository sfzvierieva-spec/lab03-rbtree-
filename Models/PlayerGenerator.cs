namespace GameLeaderboard;

public static class PlayerGenerator
{
    private static readonly string[] FirstNames =
    {
        "Shadow", "Toxic", "Silent", "Dark", "Epic", "Mystic", "Fatal", "Swift", "Iron", "Frost",
        "Blade", "Storm", "Night", "Dragon", "Phoenix", "Ghost", "Demon", "Angel", "Cyber", "Nova",
        "Inferno", "Thunder", "Lightning", "Venom", "Chaos", "Nebula", "Quantum", "Phantom", "Blaze", "Void"
    };

    private static readonly string[] LastNames =
    {
        "Slayer", "Hunter", "Killer", "Master", "Lord", "King", "Warrior", "Ninja", "Samurai", "Knight",
        "Reaper", "Striker", "Sniper", "Assassin", "Legend", "Beast", "Titan", "Champion", "Destroyer", "Executioner",
        "Predator", "Viper", "Raven", "Wolf", "Tiger", "Hawk", "Eagle", "Falcon", "Panther", "Lion"
    };

    private static readonly string[] Suffixes =
    {
        "TTV", "YT", "xX", "Xx", "69", "420", "Pro", "God", "OTP", "Main",
        "Smurf", "Alt", "Best", "Top", "GG", "OP", "Clutch", "Carry", "Ace", "MVP"
    };

    public static List<Player> GeneratePlayers(int count, int seed = 42)
    {
        var random = new Random(seed);
        var players = new List<Player>(count);
        var usedNames = new HashSet<string>();

        for (int i = 0; i < count; i++)
        {
            string name;
            do
            {
                name = GenerateSummonerName(random);
            } while (usedNames.Contains(name));

            usedNames.Add(name);

            // Generate MMR with normal distribution (mean = 1400, stddev = 300)
            int mmr = GenerateNormalMMR(random, 1400, 300);
            mmr = Math.Clamp(mmr, 500, 3000);

            // Generate wins/losses based on MMR (better players have more games and better winrate)
            int totalGames = random.Next(50, 500);
            double baseWinRate = 0.35 + (mmr - 500) / 2500.0 * 0.30; // 35% to 65% based on MMR
            double winRate = Math.Clamp(baseWinRate + (random.NextDouble() - 0.5) * 0.1, 0.3, 0.7);
            int wins = (int)(totalGames * winRate);
            int losses = totalGames - wins;

            players.Add(new Player(name, mmr, wins, losses));
        }

        return players;
    }

    public static List<Player> GeneratePlayersBatch(int count, int batchId, int seed = 42)
    {
        var random = new Random(seed + batchId);
        var players = new List<Player>(count);

        for (int i = 0; i < count; i++)
        {
            string name = GenerateSummonerName(random);

            int mmr = GenerateNormalMMR(random, 1400, 300);
            mmr = Math.Clamp(mmr, 500, 3000);

            int totalGames = random.Next(50, 500);
            double baseWinRate = 0.35 + (mmr - 500) / 2500.0 * 0.30;
            double winRate = Math.Clamp(baseWinRate + (random.NextDouble() - 0.5) * 0.1, 0.3, 0.7);
            int wins = (int)(totalGames * winRate);
            int losses = totalGames - wins;

            players.Add(new Player(name, mmr, wins, losses));
        }

        return players;
    }

    private static string GenerateSummonerName(Random random)
    {
        int style = random.Next(4);

        return style switch
        {
            0 => $"{FirstNames[random.Next(FirstNames.Length)]}{LastNames[random.Next(LastNames.Length)]}",
            1 => $"{FirstNames[random.Next(FirstNames.Length)]}{LastNames[random.Next(LastNames.Length)]}{random.Next(1, 1000)}",
            2 => $"{Suffixes[random.Next(Suffixes.Length)]}{FirstNames[random.Next(FirstNames.Length)]}{Suffixes[random.Next(Suffixes.Length)]}",
            _ => $"xX{FirstNames[random.Next(FirstNames.Length)]}{LastNames[random.Next(LastNames.Length)]}Xx"
        };
    }

    private static int GenerateNormalMMR(Random random, double mean, double stdDev)
    {
        // Box-Muller transform for normal distribution
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return (int)(mean + stdDev * randStdNormal);
    }

    public static Player GetRandomPlayer(List<Player> players, Random random)
    {
        return players[random.Next(players.Count)];
    }

    public static List<(string name, int newMMR)> GenerateMMRUpdates(List<Player> players, int count, int seed = 42)
    {
        var random = new Random(seed);
        var updates = new List<(string, int)>(count);

        for (int i = 0; i < count; i++)
        {
            var player = players[random.Next(players.Count)];
            int mmrChange = random.Next(-50, 51); // +/- 50 MMR
            int newMMR = Math.Clamp(player.MMR + mmrChange, 500, 3000);
            updates.Add((player.SummonerName, newMMR));
        }

        return updates;
    }
}
