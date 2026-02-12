using UnityEngine;

public enum TrainingCategory
{
    Concept,
    Calc,
    Idea,
    Design,
    Practice,
}

public static class TrainingSession
{
    public static TrainingCategory CurrentCategory = TrainingCategory.Concept;
}
