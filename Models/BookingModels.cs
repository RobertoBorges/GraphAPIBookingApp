using System.ComponentModel.DataAnnotations;

namespace GraphAPIBookingApp.Models
{
    public class BookingRequest
    {
        [Required]
        [Display(Name = "First and last name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Add any special requests")]
        public string SpecialRequests { get; set; } = string.Empty;

        [Required]
        public string SelectedStaffId { get; set; } = string.Empty;

        [Required]
        public DateTime SelectedDate { get; set; }

        [Required]
        public string SelectedTime { get; set; } = string.Empty;

        public string ServiceId { get; set; } = string.Empty;
    }

    public class StaffMember
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class AvailableSlot
    {
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string StaffId { get; set; } = string.Empty;
    }

    public class Service
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
    }

    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsToday { get; set; }
        public bool IsCurrentMonth { get; set; }
        public List<AvailableSlot> AvailableSlots { get; set; } = new List<AvailableSlot>();
    }

    public class BookingCalendar
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public List<CalendarDay> Days { get; set; } = new List<CalendarDay>();
        public List<StaffMember> Staff { get; set; } = new List<StaffMember>();
        public List<Service> Services { get; set; } = new List<Service>();
    }
}