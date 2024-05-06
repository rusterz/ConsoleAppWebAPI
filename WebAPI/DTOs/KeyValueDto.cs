using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs;

public class KeyValueDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Key { get; set; }
    
    [Range(1, 1000)]
    public double Value { get; set; }
}
