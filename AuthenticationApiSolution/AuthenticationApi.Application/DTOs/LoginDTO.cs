using System.ComponentModel.DataAnnotations;

namespace AuthenticationApi.Application.DTOs
{
    public record LoginDTO
    (
        [Required, EmailAddress, DataType(DataType.EmailAddress)]
        string Email,

        [Required]
        string Password
    );
}
