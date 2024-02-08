using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ShieldVSExtension.Common.Validators;

public class ProjectTokenValidationRule : ValidationRule
{
    public override ValidationResult Validate(object source, CultureInfo cultureInfo)
    {
        var token = (source ?? string.Empty).ToString();

        if (string.IsNullOrWhiteSpace(token))
        {
            return new ValidationResult(false, "Value cannot be empty");
        }

        const string tokenPattern =
            @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-4[0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$";

        return !Regex.IsMatch(token, tokenPattern)
            ? new ValidationResult(false, "Invalid token format")
            : ValidationResult.ValidResult;
    }
}