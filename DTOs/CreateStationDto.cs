using EVOwnerManagement.API.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EVOwnerManagement.API.DTOs
{
    public class CreateStationDto
    {
        [Required]
        public string StationName { get; set; }

        [Required]
        public string StationCode { get; set; }

        [Required]
        public Location Location { get; set; }

        [Required]
        public string StationType { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of slots must be at least 1.")]
        public int NoOfSlots { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public OperatingHours OperatingHours { get; set; }

        [Required]
        public string Status { get; set; }

        //  Grouped slot input (used to auto-create slots)
        [Required]
        public List<SlotGroupDto> SlotGroups { get; set; }

        //  One or more operators must be added when creating a station
        [Required]
        [MinLength(1, ErrorMessage = "At least one operator must be created for this station.")]
        public List<OperatorDto> Operators { get; set; }
    }

    //  Used for grouped slot input
    public class SlotGroupDto
    {
        [Required]
        public ConnectorType ConnectorType { get; set; }

        [Required]
        public PowerRating PowerRating { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price per kWh must be positive.")]
        public double PricePerKWh { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Slot count must be at least 1.")]
        public int Count { get; set; }
    }

    //  Used to create station operators
    public class OperatorDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
