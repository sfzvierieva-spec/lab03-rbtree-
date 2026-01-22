namespace GameLeaderboard;

/// <summary>
/// Reusable display methods for leaderboard visualization with enhanced terminal UI
/// </summary>
public static class LeaderboardDisplay
{
    private const string HorizontalLine = "─";
    private const string VerticalLine = "│";
    private const string TopLeft = "┌";
    private const string TopRight = "┐";
    private const string BottomLeft = "└";
    private const string BottomRight = "┘";
    private const string TLeft = "├";
    private const string TRight = "┤";

    /// <summary>
    /// Displays the main menu
    /// </summary>
    public static void ShowMainMenu()
    {
        Console.Clear();
        DrawBox("LEAGUE OF LEGENDS LEADERBOARD SYSTEM", 80, ConsoleColor.Magenta);
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  [1] Search by Player Name");
        Console.WriteLine("  [2] Browse by Rank");
        Console.WriteLine("  [3] View Top 10 Players");
        Console.WriteLine("  [4] View Tier Distribution");
        Console.WriteLine("  [5] Update Player MMR");
        Console.WriteLine("  [6] Exit");
        Console.ResetColor();
        Console.WriteLine();
        Console.Write("Enter your choice (1-6): ");
    }

    /// <summary>
    /// Draws a fancy box with a title
    /// </summary>
    public static void DrawBox(string title, int width = 80, ConsoleColor color = ConsoleColor.White)
    {
        Console.ForegroundColor = color;

        // Top border
        Console.WriteLine(TopLeft + new string(HorizontalLine[0], width - 2) + TopRight);

        // Title (centered)
        int padding = (width - 2 - title.Length) / 2;
        string centeredTitle = new string(' ', padding) + title + new string(' ', width - 2 - padding - title.Length);
        Console.WriteLine(VerticalLine + centeredTitle + VerticalLine);

        // Bottom border
        Console.WriteLine(BottomLeft + new string(HorizontalLine[0], width - 2) + BottomRight);

        Console.ResetColor();
    }

    /// <summary>
    /// Shows a loading progress bar
    /// </summary>
    public static void ShowProgress(string message, int current, int total)
    {
        int barWidth = 50;
        double percentage = (double)current / total;
        int filledWidth = (int)(barWidth * percentage);

        Console.SetCursorPosition(0, Console.CursorTop);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"{message} [");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(new string('█', filledWidth));
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(new string('─', barWidth - filledWidth));
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"] {percentage:P0}");
        Console.ResetColor();

        if (current == total)
            Console.WriteLine();
    }

    /// <summary>
    /// Displays a section header
    /// </summary>
    public static void ShowHeader(string title, ConsoleColor color = ConsoleColor.Cyan)
    {
        Console.WriteLine();
        Console.ForegroundColor = color;
        Console.WriteLine(new string('═', 85));
        Console.WriteLine($" {title}");
        Console.WriteLine(new string('═', 85));
        Console.ResetColor();
    }

    /// <summary>
    /// Displays top N players with formatted table
    /// </summary>
    public static void ShowTopPlayers(ILeaderboard leaderboard, int count = 10)
    {
        ShowHeader($"TOP {count} PLAYERS", ConsoleColor.Magenta);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"{"Rank",-6} {"Summoner Name",-20} {"Tier",-12} {"MMR",-6} {"Wins",-6} {"Losses",-6} {"Win Rate",-8}");
        Console.WriteLine(new string('─', 85));
        Console.ResetColor();

        var topPlayers = leaderboard.GetTopPlayers(count);
        for (int i = 0; i < topPlayers.Count; i++)
        {
            var player = topPlayers[i];
            Console.ForegroundColor = GetTierColor(player.Tier);
            Console.WriteLine($"#{i + 1,-5} {player.SummonerName,-20} {player.Tier,-12} {player.MMR,-6} {player.Wins,-6} {player.Losses,-6} {player.WinRate,-7:F1}%");
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Shows player information with surrounding players
    /// </summary>
    public static void ShowPlayerInfo(ILeaderboard leaderboard, string playerName, int context = 5)
    {
        int rank = leaderboard.GetPlayerRank(playerName);

        if (rank == -1)
        {
            ShowError($"Player '{playerName}' not found in the leaderboard.");
            return;
        }

        var players = leaderboard.GetPlayersAroundRank(rank, context);
        var targetPlayer = players.FirstOrDefault(p => p.SummonerName.Equals(playerName, StringComparison.OrdinalIgnoreCase));

        if (targetPlayer != null)
        {
            ShowHeader($"PLAYER: {targetPlayer.SummonerName}", ConsoleColor.Yellow);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  Rank:     #{rank:N0} / {leaderboard.PlayerCount:N0}");
            Console.WriteLine($"  Tier:     {targetPlayer.Tier}");
            Console.WriteLine($"  MMR:      {targetPlayer.MMR}");
            Console.WriteLine($"  Wins:     {targetPlayer.Wins}");
            Console.WriteLine($"  Losses:   {targetPlayer.Losses}");
            Console.WriteLine($"  Win Rate: {targetPlayer.WinRate:F1}%");
            Console.ResetColor();
        }

        ShowPlayersAroundRank(leaderboard, rank, playerName, context);
    }

    /// <summary>
    /// Shows players around a specific rank
    /// </summary>
    public static void ShowPlayersAroundRank(ILeaderboard leaderboard, int rank, string? highlightPlayerName = null, int context = 5)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($" Surrounding Players (±{context} ranks):");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(new string('─', 85));
        Console.ResetColor();

        var nearbyPlayers = leaderboard.GetPlayersAroundRank(rank, context);
        foreach (var player in nearbyPlayers)
        {
            int playerRank = leaderboard.GetPlayerRank(player.SummonerName);
            bool isHighlighted = player.SummonerName.Equals(highlightPlayerName, StringComparison.OrdinalIgnoreCase);

            if (isHighlighted)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.Write($"#{playerRank,-5} {player.SummonerName,-20} {player.Tier,-12} {player.MMR,-6} {player.Wins,-6} {player.Losses,-6} {player.WinRate,-7:F1}%");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" ◄ YOU");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = GetTierColor(player.Tier);
                Console.WriteLine($"#{playerRank,-5} {player.SummonerName,-20} {player.Tier,-12} {player.MMR,-6} {player.Wins,-6} {player.Losses,-6} {player.WinRate,-7:F1}%");
                Console.ResetColor();
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Shows rank distribution across all tiers with visual bar chart
    /// </summary>
    public static void ShowRankDistribution(ILeaderboard leaderboard)
    {
        ShowHeader("TIER DISTRIBUTION", ConsoleColor.Cyan);

        var distribution = leaderboard.GetRankDistribution();
        int maxCount = distribution.Values.Max();

        foreach (var (tier, count) in distribution.OrderByDescending(static kv => kv.Key))
        {
            if (count > 0)
            {
                double percentage = (double)count / leaderboard.PlayerCount * 100;
                int barLength = (int)((double)count / maxCount * 40);
                string bar = new string('█', barLength);

                Console.ForegroundColor = GetTierColor(tier);
                Console.Write($"{tier,-12} │ ");
                Console.Write(bar);
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($" {count,8:N0} ({percentage,5:F2}%)");
                Console.ResetColor();
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Shows an error message
    /// </summary>
    public static void ShowError(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ ERROR: {message}");
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Shows a success message
    /// </summary>
    public static void ShowSuccess(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {message}");
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Prompts user to continue
    /// </summary>
    public static void PromptContinue()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Press any key to continue...");
        Console.ResetColor();
        Console.ReadKey(true);
    }

    private static ConsoleColor GetTierColor(RankTier tier)
    {
        return tier switch
        {
            RankTier.Challenger => ConsoleColor.Magenta,
            RankTier.Grandmaster => ConsoleColor.Red,
            RankTier.Master => ConsoleColor.DarkRed,
            RankTier.Diamond => ConsoleColor.Cyan,
            RankTier.Platinum => ConsoleColor.Green,
            RankTier.Gold => ConsoleColor.Yellow,
            RankTier.Silver => ConsoleColor.Gray,
            RankTier.Bronze => ConsoleColor.DarkYellow,
            RankTier.Iron => ConsoleColor.DarkGray,
            _ => ConsoleColor.White
        };
    }
}
