using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels;

public class CategoryFormVm : IValidatableObject
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }

    [Required(ErrorMessage = "Please enter the category name.")]
    [StringLength(100, ErrorMessage = "The category name can be at most 100 characters.")]
    [Display(Name = "Category name")]
    public string CategoryName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the distance.")]
    [Range(0.1, 1000, ErrorMessage = "The distance must be between 0.1 and 1000 km.")]
    [Display(Name = "Distance (km)")]
    public decimal? DistanceKm { get; set; }

    [Required(ErrorMessage = "Please enter the minimum age.")]
    [Range(0, 120, ErrorMessage = "The minimum age must be between 0 and 120.")]
    [Display(Name = "Minimum age")]
    public int? MinAge { get; set; }

    [Range(0, 120, ErrorMessage = "The maximum age must be between 0 and 120.")]
    [Display(Name = "Maximum age (optional)")]
    public int? MaxAge { get; set; }

    [Required(ErrorMessage = "Please enter the entry fee (use 0 for a free entry).")]
    [Range(0, 100000, ErrorMessage = "The entry fee must be between 0 and 100000.")]
    [Display(Name = "Entry fee (R)")]
    public decimal? EntryFee { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinAge.HasValue && MaxAge.HasValue && MaxAge.Value < MinAge.Value)
        {
            yield return new ValidationResult(
                "The maximum age cannot be less than the minimum age.",
                new[] { nameof(MaxAge) });
        }
    }
}
