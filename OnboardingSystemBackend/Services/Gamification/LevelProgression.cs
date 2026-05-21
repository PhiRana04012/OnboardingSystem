namespace OnboardingSystem.Services.Gamification;

public static class LevelProgression
{
    public static int ThresholdForLevel(int level) =>
        level <= 1 ? 0 : (int)(100 * Math.Pow(level - 1, 1.3));

    public static int GetLevelFromTotalXp(int totalXp)
    {
        var level = 1;
        while (totalXp >= ThresholdForLevel(level + 1))
            level++;
        return level;
    }

    public static int XpToNextLevel(int totalXp, int level) =>
        Math.Max(0, ThresholdForLevel(level + 1) - totalXp);

    public static double LevelProgressPercent(int totalXp, int level)
    {
        var current = ThresholdForLevel(level);
        var next = ThresholdForLevel(level + 1);
        if (next <= current) return 100;
        return Math.Min(100, Math.Max(0, (totalXp - current) / (double)(next - current) * 100));
    }
}
