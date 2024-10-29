using System.ComponentModel.DataAnnotations;

namespace api_crm.Attributes
{
    public class ValidationContextProvider
    {
        private static IServiceProvider? _serviceProvider;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static IServiceProvider? GetServiceProvider(ValidationContext validationContext)
        {
            return _serviceProvider;
        }
    }
}