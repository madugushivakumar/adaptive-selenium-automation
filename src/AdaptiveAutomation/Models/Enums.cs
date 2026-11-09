namespace AdaptiveAutomation.Models;

public enum FieldType
{
    TextInput,
    RadioGroup,
    Dropdown
}

public enum ConfidenceLevel
{
    Rejected = 0,
    Low = 10,
    Medium = 50,
    High = 80
}

public enum TestStatus
{
    Passed,
    Failed,
    Skipped,
    Ambiguous
}

