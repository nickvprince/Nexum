
using System.ComponentModel.DataAnnotations;

namespace SharedComponents.Entities.WebEntities.Requests.TenantRequests
{
    public class TenantCreateRequest
    {
        [Required(ErrorMessage = "Name is Required")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Contact Name is Required")]
        public string? ContactName { get; set; }
        [Required(ErrorMessage = "Contact Email Address is Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? ContactEmail { get; set; }
        [Required(ErrorMessage = "Contact Phone Number is Required")]
        [Phone(ErrorMessage = "Phone number is invalid")]
        public string? ContactPhone { get; set; }
        [Required(ErrorMessage = "Address is Required")]
        public string? Address { get; set; }
        [Required(ErrorMessage = "City is Required")]
        public string? City { get; set; }
        [Required(ErrorMessage = "State is Required")]
        public string? State { get; set; }
        [Required(ErrorMessage = "ZIP/Postal Code is Required")]
        [RegularExpression(@"(^\d{5}(-\d{4})?$)|(^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$)", ErrorMessage = "Invalid ZIP/Postal Code")]
        public string? Zip { get; set; }
        [Required(ErrorMessage = "Country is Required")]
        public string? Country { get; set; }
    }
}
