using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.ViewModels;

public class DateInFutureAttribute : ValidationAttribute
{
    public DateInFutureAttribute()
    {
        ErrorMessage = "Date must be in the future.";
    }

    public override bool IsValid(object? value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.Date >= DateTime.Today;
        }
        return true;
    }
}

public class DateGreaterThanAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateGreaterThanAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
        ErrorMessage = "Date must be greater than " + comparisonProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var currentValue = (DateTime?)value;
        var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

        if (property == null)
        {
            return new ValidationResult($"Unknown property: {_comparisonProperty}");
        }

        var comparisonValue = (DateTime?)property.GetValue(validationContext.ObjectInstance);

        if (currentValue.HasValue && comparisonValue.HasValue && currentValue <= comparisonValue)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}
