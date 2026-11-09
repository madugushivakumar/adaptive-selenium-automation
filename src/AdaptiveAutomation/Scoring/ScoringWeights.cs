namespace AdaptiveAutomation.Scoring;

public record ScoringWeights
{
    public double AssociatedLabelWeight { get; init; } = 40.0;
    public double AccessibleNameWeight { get; init; } = 35.0;
    public double ContextualTextWeight { get; init; } = 25.0;
    public double PlaceholderWeight { get; init; } = 20.0;
    public double TechnicalTypeWeight { get; init; } = 15.0;
    public double AttributeNameWeight { get; init; } = 15.0;
    public double ContainerRelevanceWeight { get; init; } = 15.0;
    public double ContradictoryPenalty { get; init; } = 50.0;
}

