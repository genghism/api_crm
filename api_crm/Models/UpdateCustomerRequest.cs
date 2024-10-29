using api_crm.Validation;
using System.ComponentModel.DataAnnotations;

namespace api_crm.Models
{
    public class UpdateCustomerRequest (
        string customerCode,
        string name,
        string changedBy,
        string manager,
        string segment,
        string mobileNumber)
    {
        [Required(ErrorMessage = "Customer code is required")]
        [StringLength(6, ErrorMessage = "Customer code cannot be longer than 6 characters")]
        [CustomValidationAttributes.CustomerExistsValidation(ErrorMessage = "Customer does not exist in the ERP database")]
        public string CustomerCode { get; set; } = customerCode;

        [Required(ErrorMessage = "Name is required")]
        [StringLength(130, ErrorMessage = "Name cannot be longer than 130 characters")]
        [CustomValidationAttributes.NameValidation(ErrorMessage = "Name contains invalid characters or doesn't include father's name")]
        public string Name { get; set; } = name;

        [Required(ErrorMessage = "ChangedBy is required")]
        [StringLength(30, ErrorMessage = "ChangedBy cannot be longer than 30 characters")]
        public string ChangedBy { get; set; } = changedBy;

        [StringLength(3, ErrorMessage = "Manager cannot be longer than 3 characters")]
        [CustomValidationAttributes.ManagerExistsValidation(ErrorMessage = "Manager does not exist in the ERP database or is not a customer manager")]
        public string Manager { get; set; } = manager;

        [Required(ErrorMessage = "Segment is required")]
        [StringLength(5, ErrorMessage = "Segment cannot be longer than 5 characters")]
        [CustomValidationAttributes.SegmentExistsValidation(ErrorMessage = "Segment does not exist in the ERP database")]
        public string Segment { get; set; } = segment;

        [StringLength(25, ErrorMessage = "Phone number cannot be longer than 25 characters")]
        public string MobileNumber { get; set; } = mobileNumber;
    }
}
