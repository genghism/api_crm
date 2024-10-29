using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using api_crm.Services;
using api_crm.Attributes;

namespace api_crm.Validation
{
    public static class CustomValidationAttributes
    {
        public class NameValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value == null)
                {
                    return new ValidationResult("Name cannot be null");
                }

                string name = value.ToString()!;

                // Check for invalid characters
                if (Regex.IsMatch(name, @"['""\?\!\@\#\$\%\^\&\*\(\)\+\=\[\]\{\}\|\<\>\,\.\/\\\:;]"))
                {
                    return new ValidationResult("Name contains invalid characters");
                }

                // Check for required suffixes
                string[] requiredSuffixes = { "oğlu", "qızı", "oviç", "ovna", "оглы", "гызы", "ович", "овна" };
                if (!requiredSuffixes.Any(suffix => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
                {
                    return new ValidationResult("Name must end with one of the following : oğlu, qızı, oviç, ovna, оглы, гызы, ович, овна");
                }

                return ValidationResult.Success;
            }
        }

        public class ManagerExistsValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value == null)
                {
                    return new ValidationResult("Manager cannot be null");
                }

                try
                {
                    var services = ValidationContextProvider.GetServiceProvider(validationContext);

                    if (services == null)
                    {
                        return new ValidationResult("Service not available");
                    }

                    using var scope = services.CreateScope();
                    var dbHandler = scope.ServiceProvider.GetRequiredService<DbHandler>();

                    string managerCode = value.ToString()!;
                    bool managerExists = dbHandler.ManagerExistsAsync(managerCode).GetAwaiter().GetResult();

                    if (!managerExists)
                    {
                        return new ValidationResult(ErrorMessage ?? "Manager does not exist in the database");
                    }

                    return ValidationResult.Success;
                }
                catch (Exception ex)
                {
                    return new ValidationResult($"Validation failed: {ex.Message}");
                }
            }
        }

        public class SegmentExistsValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value == null)
                {
                    return new ValidationResult("Segment cannot be null");
                }

                try
                {
                    var services = ValidationContextProvider.GetServiceProvider(validationContext);

                    if (services == null)
                    {
                        return new ValidationResult("Service not available");
                    }

                    using var scope = services.CreateScope();
                    var dbHandler = scope.ServiceProvider.GetRequiredService<DbHandler>();

                    string segmentCode = value.ToString()!;
                    bool segmentExists = dbHandler.SegmentExistsAsync(segmentCode).GetAwaiter().GetResult();

                    if (!segmentExists)
                    {
                        return new ValidationResult(ErrorMessage ?? "Segment does not exist in the database");
                    }

                    return ValidationResult.Success;
                }
                catch (Exception ex)
                {
                    return new ValidationResult($"Validation failed: {ex.Message}");
                }
            }
        }

        public class CustomerExistsValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value == null)
                {
                    return new ValidationResult("Customer cannot be null");
                }

                try
                {
                    var services = ValidationContextProvider.GetServiceProvider(validationContext);

                    if (services == null)
                    {
                        return new ValidationResult("Service not available");
                    }

                    using var scope = services.CreateScope();
                    var dbHandler = scope.ServiceProvider.GetRequiredService<DbHandler>();

                    string customerCode = value.ToString()!;
                    bool customerExists = dbHandler.CustomerExistsAsync(customerCode).GetAwaiter().GetResult();

                    if (!customerExists)
                    {
                        return new ValidationResult(ErrorMessage ?? "Customer does not exist in the database");
                    }

                    return ValidationResult.Success;
                }
                catch (Exception ex)
                {
                    return new ValidationResult($"Validation failed: {ex.Message}");
                }
            }
        }
    }
}