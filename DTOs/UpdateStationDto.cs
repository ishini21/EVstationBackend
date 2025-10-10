using EVOwnerManagement.API.Models;
using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    public class UpdateStationDto
    {
        [Required]
        public string StationName { get; set; }

        [Required]
        public Location Location { get; set; }

        [Required]
        public string StationType { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public OperatingHours OperatingHours { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
