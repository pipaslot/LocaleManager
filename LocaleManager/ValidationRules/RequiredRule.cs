using System.Globalization;
using System.Windows.Controls;

namespace LocaleManager.ValidationRules
{
    public class RequiredRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "Value is required");
            }
            return new ValidationResult(true, null);
        }
    }
}
