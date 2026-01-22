namespace GameLeaderboard;

class Program
{
    private static ILeaderboard? _leaderboard;

    static void Main(string[] args)
    {
        InitializeLeaderboard();

        // Main menu loop
        bool running = true;
        while (running)
        {
            LeaderboardDisplay.ShowMainMenu();
            string? input = Console.ReadLine();

            switch (input?.Trim())
            {
                case "1":
                    SearchByPlayerName();
                    break;
                case "2":
                    BrowseByRank();
                    break;
                case "3":
                    ViewTop10();
                    break;
                case "4":
                    ViewTierDistribution();
                    break;
                case "5":
                    UpdatePlayerMMR();
                    break;
                case "6":
                    running = false;
                    ShowExitMessage();
                    break;
                default:
                    LeaderboardDisplay.ShowError("Invalid choice. Please enter a number from 1 to 6.");
                    LeaderboardDisplay.PromptContinue();
                    break;
            }
        }
    }

    static void InitializeLeaderboard()
    {
        Console.Clear();
        LeaderboardDisplay.DrawBox("INITIALIZING LEADERBOARD", 80, ConsoleColor.Cyan);
        Console.WriteLine();

        const int playerCount = 1_000_000;

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"  Generating {playerCount:N0} players...");
        Console.WriteLine();
        Console.ResetColor();

        _leaderboard = new RBTreeLeaderboard();

        // Generate players in batches for progress display
        const int batchSize = 50_000;
        int generatedCount = 0;

        while (generatedCount < playerCount)
        {
            int currentBatch = Math.Min(batchSize, playerCount - generatedCount);
            var players = PlayerGenerator.GeneratePlayers(currentBatch, seed: generatedCount);

            foreach (var player in players)
            {
                _leaderboard.AddOrUpdatePlayer(player);
            }

            generatedCount += currentBatch;
            LeaderboardDisplay.ShowProgress("  Loading players", generatedCount, playerCount);
        }

        LeaderboardDisplay.ShowSuccess($"Successfully loaded {playerCount:N0} players!");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  Using Red-Black Tree implementation for O(log n) performance.");
        Console.ResetColor();

        LeaderboardDisplay.PromptContinue();
    }

    static void SearchByPlayerName()
    {
        Console.Clear();
        LeaderboardDisplay.DrawBox("SEARCH BY PLAYER NAME", 80, ConsoleColor.Yellow);
        Console.WriteLine();

        Console.Write("Enter player name (or press Enter to cancel): ");
        string? playerName = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(playerName))
        {
            return;
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Searching...");
        Console.ResetColor();

        if (_leaderboard != null)
        {
            LeaderboardDisplay.ShowPlayerInfo(_leaderboard, playerName, context: 5);
        }

        LeaderboardDisplay.PromptContinue();
    }

    static void BrowseByRank()
    {
        Console.Clear();
        LeaderboardDisplay.DrawBox("BROWSE BY RANK", 80, ConsoleColor.Cyan);
        Console.WriteLine();

        Console.WriteLine($"Enter rank (1 - {_leaderboard?.PlayerCount:N0}): ");
        Console.Write("> ");
        string? input = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        if (!int.TryParse(input, out int rank) || rank < 1 || rank > (_leaderboard?.PlayerCount ?? 0))
        {
            LeaderboardDisplay.ShowError($"Invalid rank. Please enter a number between 1 and {_leaderboard?.PlayerCount:N0}.");
            LeaderboardDisplay.PromptContinue();
            return;
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Loading...");
        Console.ResetColor();

        if (_leaderboard != null)
        {
            LeaderboardDisplay.ShowPlayersAroundRank(_leaderboard, rank, context: 5);
        }

        LeaderboardDisplay.PromptContinue();
    }

    static void ViewTop10()
    {
        Console.Clear();
        LeaderboardDisplay.DrawBox("TOP 10 PLAYERS", 80, ConsoleColor.Magenta);

        if (_leaderboard != null)
        {
            LeaderboardDisplay.ShowTopPlayers(_leaderboard, 10);
        }

        LeaderboardDisplay.PromptContinue();
    }

    static void ViewTierDistribution()
    {
        Console.Clear();
        LeaderboardDisplay.DrawBox("TIER DISTRIBUTION", 80, ConsoleColor.Cyan);

        if (_leaderboard != null)
        {
            LeaderboardDisplay.ShowRankDistribution(_leaderboard);

            // Additional statistics
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($" Total Players: {_leaderboard.PlayerCount:N0}");
            Console.ResetColor();
        }

        LeaderboardDisplay.PromptContinue();
    }

    static void UpdatePlayerMMR()
    {
        Console.Clear();
        LeaderboardDisplay.DrawBox("UPDATE PLAYER MMR", 80, ConsoleColor.Yellow);
        Console.WriteLine();

        Console.Write("Enter player name (or press Enter to cancel): ");
        string? playerName = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(playerName))
        {
            return;
        }

        if (_leaderboard == null)
        {
            LeaderboardDisplay.ShowError("Leaderboard not initialized.");
            LeaderboardDisplay.PromptContinue();
            return;
        }

        // Check if player exists and show current info
        int currentRank = _leaderboard.GetPlayerRank(playerName);
        if (currentRank == -1)
        {
            LeaderboardDisplay.ShowError($"Player '{playerName}' not found in the leaderboard.");
            LeaderboardDisplay.PromptContinue();
            return;
        }

        // Display current player info
        LeaderboardDisplay.ShowPlayerInfo(_leaderboard, playerName, context: 2);

        // Ask for new MMR
        Console.WriteLine();
        Console.Write("Enter new MMR (or press Enter to cancel): ");
        string? mmrInput = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(mmrInput))
        {
            return;
        }

        if (!int.TryParse(mmrInput, out int newMMR) || newMMR < 0 || newMMR > 5000)
        {
            LeaderboardDisplay.ShowError("Invalid MMR. Please enter a number between 0 and 5000.");
            LeaderboardDisplay.PromptContinue();
            return;
        }

        // Update the player's MMR
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Updating MMR...");
        Console.ResetColor();

        _leaderboard.UpdatePlayerMMR(playerName, newMMR);

        LeaderboardDisplay.ShowSuccess($"Successfully updated {playerName}'s MMR to {newMMR}!");

        // Show updated player info
        LeaderboardDisplay.ShowPlayerInfo(_leaderboard, playerName, context: 5);

        LeaderboardDisplay.PromptContinue();
    }

    static void ShowExitMessage()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("  Goodbye!");
        Console.ResetColor();
        Console.WriteLine();
    }
}
