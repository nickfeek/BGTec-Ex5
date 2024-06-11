using System;
using System.ComponentModel.DataAnnotations;

namespace AnprFileService.Models
{
    public class FileRecord
    {
        public int Id { get; set; }

        [Required]
        public string CountryOfVehicle { get; set; } = "";

        [Required]
        public string RegNumber { get; set; } = "";

        [Required]
        public string ConfidenceLevel { get; set; } = "";

        [Required]
        public string CameraName { get; set; } = "";

        [Required]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Invalid Date format. Expected format: yyyyMMdd")]
        public int Date { get; set; }

        [Required]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Invalid Time format. Expected format: HHmm")]
        public int Time { get; set; }

        [Required]
        public string ImageFilename { get; set; } = "";

        [Required]
        public string Path { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
