using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;

namespace OnboardingSystem.Services.Gamification;

public class EngagementProfileBuilder
{
    private readonly AppDbContext _context;

    public EngagementProfileBuilder(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EngagementState> BuildAsync(int userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return new EngagementState { Cluster = EngagementCluster.Steady };

        var now = DateTime.UtcNow;
        var weekAgo = now.AddDays(-7);

        var departmentId = user.DepartmentId;
        var mandatoryModuleIds = await _context.Modules
            .Where(m => m.IsMandatory && (
                m.DepartmentId == departmentId ||
                m.ModuleDepartments.Any(md => md.DepartmentId == departmentId) ||
                (m.DepartmentId == null && !m.ModuleDepartments.Any())))
            .Select(m => m.ModuleId)
            .ToListAsync();

        var completedMandatory = mandatoryModuleIds.Count == 0
            ? 0
            : await _context.UserModuleProgresses.CountAsync(p =>
                p.UserId == userId &&
                mandatoryModuleIds.Contains(p.ModuleId) &&
                p.Status == "Завершён");

        var progressRatio = mandatoryModuleIds.Count == 0
            ? 1.0
            : (double)completedMandatory / mandatoryModuleIds.Count;

        var recentAttempts = await _context.TestAttempts
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.AttemptDate >= weekAgo)
            .OrderByDescending(t => t.AttemptDate)
            .Take(12)
            .ToListAsync();

        var failCount = recentAttempts.Count(t => !t.IsPassed);
        var frustration = recentAttempts.Count == 0
            ? 0.2
            : Math.Min(1.0, failCount / (double)Math.Max(3, recentAttempts.Count) + (progressRatio < 0.3 ? 0.2 : 0));

        var lastActivity = await _context.ActionLogs
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Select(a => (DateTime?)a.Timestamp)
            .FirstOrDefaultAsync();

        var daysInactive = lastActivity.HasValue
            ? Math.Max(0, (int)(now.Date - lastActivity.Value.Date).TotalDays)
            : 14;

        var modulesLast7 = await _context.UserModuleProgresses.CountAsync(p =>
            p.UserId == userId &&
            p.Status == "Завершён" &&
            p.CompletionDate >= weekAgo);

        var activityDays = await _context.ActionLogs
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Timestamp >= now.AddDays(-14))
            .Select(a => a.Timestamp.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync();

        var streak = 0;
        for (var i = 0; i < activityDays.Count; i++)
        {
            var expected = now.Date.AddDays(-i);
            if (activityDays.Contains(expected))
                streak++;
            else
                break;
        }

        var cluster = ResolveCluster(progressRatio, frustration, daysInactive, modulesLast7);

        return new EngagementState
        {
            ProgressRatio = progressRatio,
            FrustrationScore = frustration,
            DaysInactive = daysInactive,
            ModulesCompletedLast7Days = modulesLast7,
            ActivityStreakDays = streak,
            Cluster = cluster
        };
    }

    private static EngagementCluster ResolveCluster(double progress, double frustration, int daysInactive, int velocity)
    {
        if (daysInactive >= 5) return EngagementCluster.Coaster;
        if (frustration >= 0.55 && progress < 0.6) return EngagementCluster.Struggler;
        if (progress >= 0.85) return EngagementCluster.Finisher;
        if (velocity >= 3 && progress < 0.5) return EngagementCluster.Steady;
        return EngagementCluster.Steady;
    }
}
