using System.Collections.Generic;

namespace AdaptiveAutomation.Models;

public record FieldDefinition
{
    public required string FieldName { get; init; }
    public required FieldType FieldType { get; init; }
    public required IReadOnlyList<string> Synonyms { get; init; }
    public IReadOnlyList<string> NegativeKeywords { get; init; } = new List<string>();
    public string ExpectedValue { get; init; } = string.Empty;

    public static FieldDefinition CreateFirstName(string expectedValue = "Shiva") => new()
    {
        FieldName = "First Name",
        FieldType = FieldType.TextInput,
        Synonyms = new[]
        {
            "First Name", "Firstname", "Given Name", "Your First Name", "Enter First Name",
            "Forename", "FName", "First_Name"
        },
        NegativeKeywords = new[]
        {
            "Last Name", "Lastname", "Surname", "Middle Name", "Middle", "User Name", "Username",
            "Company", "Organization", "Address", "Street", "City", "State", "Country", "Phone",
            "Mobile", "Email", "Password", "Confirm Password", "About", "Description"
        },
        ExpectedValue = expectedValue
    };

    public static FieldDefinition CreateGender(string expectedOption = "Male") => new()
    {
        FieldName = "Gender",
        FieldType = FieldType.RadioGroup,
        Synonyms = new[]
        {
            "Gender", "Select Gender", "Your Gender", "Choose Gender", "Sex", "Gender Identity"
        },
        NegativeKeywords = new[]
        {
            "Hobbies", "Hobby", "Marital", "Status", "Preference", "Agree", "Terms", "Dance", "Cricket"
        },
        ExpectedValue = expectedOption
    };

    public static FieldDefinition CreateState(string expectedOption = "India") => new()
    {
        FieldName = "State",
        FieldType = FieldType.Dropdown,
        Synonyms = new[]
        {
            "State", "Select State", "Choose State", "State/Province", "Province", "Region",
            "State or Province", "Territory"
        },
        NegativeKeywords = new[]
        {
            "Country", "City", "District", "Zip", "Postal", "County"
        },
        ExpectedValue = expectedOption
    };
}

