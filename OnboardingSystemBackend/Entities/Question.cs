using System;
using System.Collections.Generic;

namespace OnboardingSystem.Entities;

public partial class Question
{
    public int QuestionId { get; set; }

    public int ModuleId { get; set; }

    public string QuestionText { get; set; } = null!;

    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();

    public virtual Module Module { get; set; } = null!;
}
