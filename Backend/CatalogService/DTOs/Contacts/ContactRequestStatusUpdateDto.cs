using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.Contacts;

public class ContactRequestStatusUpdateDto
{
    [Required]
    [MaxLength(20)]
    public string TrangThai { get; set; } = string.Empty;
}
