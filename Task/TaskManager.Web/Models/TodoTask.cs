using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Web.Models;

public class TodoTask
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [CustomValidation(typeof(TodoTask), nameof(ValidateDueDate))]
    public DateTime? DueDate { get; set; }

    [Range(1, 3, ErrorMessage = "Please select a priority level")]
    public int? Priority { get; set; } // 1: High, 2: Medium, 3: Low

    public static ValidationResult? ValidateDueDate(DateTime? dueDate, ValidationContext context)
    {
        if (dueDate.HasValue && dueDate.Value.Date < DateTime.Today)
        {
            return new ValidationResult("Due date cannot be in the past");
        }
        return ValidationResult.Success;
    }
} 