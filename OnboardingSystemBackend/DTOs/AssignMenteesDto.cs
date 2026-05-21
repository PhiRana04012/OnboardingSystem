namespace OnboardingSystem.DTOs;

/// <summary>
/// DTO для массового назначения подопечных наставнику
/// </summary>
public class AssignMenteesDto
{
    /// <summary>
    /// ID наставника
    /// </summary>
    public int MentorId { get; set; }

    /// <summary>
    /// Список ID сотрудников, которые будут подопечными наставника
    /// </summary>
    public List<int> MenteeIds { get; set; } = new List<int>();
}
