using System;
using System.Collections.Generic;

namespace OnboardingSystem.Entities;

public partial class AnswerOption
{
    public int AnswerId { get; set; }

    public int QuestionId { get; set; }

    public string AnswerText { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public virtual Question Question { get; set; } = null!;
}
