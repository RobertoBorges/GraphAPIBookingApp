using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GraphAPIBookingApp.Models;
using GraphAPIBookingApp.Services;
using System.Globalization;

namespace GraphAPIBookingApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IBookingService _bookingService;

    [BindProperty]
    public BookingRequest BookingRequest { get; set; } = new BookingRequest();

    public BookingCalendar Calendar { get; set; } = new BookingCalendar();
    public List<StaffMember> Staff { get; set; } = new List<StaffMember>();
    public List<Service> Services { get; set; } = new List<Service>();
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }

    public IndexModel(ILogger<IndexModel> logger, IBookingService bookingService)
    {
        _logger = logger;
        _bookingService = bookingService;
    }

    public async Task OnGetAsync(int? year = null, int? month = null)
    {
        try
        {
            // Load initial data
            Staff = await _bookingService.GetStaffMembersAsync();
            Services = await _bookingService.GetServicesAsync();

            // Set up calendar for current month or specified month
            var currentDate = DateTime.Now;
            var calendarYear = year ?? currentDate.Year;
            var calendarMonth = month ?? currentDate.Month;

            await LoadCalendarAsync(calendarYear, calendarMonth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading booking page");
            Message = "An error occurred while loading the booking page. Please try again.";
            IsSuccess = false;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Reload data for the view
            Staff = await _bookingService.GetStaffMembersAsync();
            Services = await _bookingService.GetServicesAsync();
            await LoadCalendarAsync(DateTime.Now.Year, DateTime.Now.Month);

            if (!ModelState.IsValid)
            {
                Message = "Please fill in all required fields correctly.";
                IsSuccess = false;
                return Page();
            }

            // Validate that all required selections are made
            if (string.IsNullOrEmpty(BookingRequest.SelectedStaffId) || 
                BookingRequest.SelectedDate == default || 
                string.IsNullOrEmpty(BookingRequest.SelectedTime))
            {
                Message = "Please select a staff member, date, and time for your appointment.";
                IsSuccess = false;
                return Page();
            }

            // Create the appointment
            var success = await _bookingService.CreateAppointmentAsync(BookingRequest);

            if (success)
            {
                Message = $"Your appointment has been successfully booked for {BookingRequest.SelectedDate:MMMM dd, yyyy} at {BookingRequest.SelectedTime}!";
                IsSuccess = true;
                
                // Clear the form
                BookingRequest = new BookingRequest();
            }
            else
            {
                Message = "There was an error booking your appointment. Please try again.";
                IsSuccess = false;
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing booking request");
            Message = "An error occurred while processing your booking. Please try again.";
            IsSuccess = false;
            return Page();
        }
    }

    private async Task LoadCalendarAsync(int year, int month)
    {
        Calendar = new BookingCalendar
        {
            Year = year,
            Month = month,
            MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
            Staff = Staff,
            Services = Services
        };

        // Get first day of the month and calculate calendar grid
        var firstDayOfMonth = new DateTime(year, month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

        // Calculate start date (may include days from previous month)
        var startDate = firstDayOfMonth.AddDays(-firstDayOfWeek);
        var endDate = startDate.AddDays(41); // 6 weeks * 7 days

        var days = new List<CalendarDay>();
        var today = DateTime.Today;

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var calendarDay = new CalendarDay
            {
                Date = date,
                IsCurrentMonth = date.Month == month,
                IsToday = date.Date == today,
                IsAvailable = date >= today && date.Month == month && 
                             date.DayOfWeek != DayOfWeek.Saturday && 
                             date.DayOfWeek != DayOfWeek.Sunday
            };

            days.Add(calendarDay);
        }

        Calendar.Days = days;
    }

    public async Task<IActionResult> OnGetAvailabilityAsync(string staffId, string date)
    {
        try
        {
            if (string.IsNullOrEmpty(staffId) || !DateTime.TryParse(date, out var selectedDate))
            {
                return new JsonResult(new { success = false, message = "Invalid parameters" });
            }

            var availability = await _bookingService.GetStaffAvailabilityAsync(staffId, selectedDate, selectedDate);
            var timeSlots = availability.Where(slot => slot.Date.Date == selectedDate.Date)
                                     .Select(slot => new { 
                                         time = slot.StartTime, 
                                         available = slot.IsAvailable 
                                     })
                                     .ToList();

            return new JsonResult(new { success = true, timeSlots });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting availability for staff {StaffId} on {Date}", staffId, date);
            return new JsonResult(new { success = false, message = "Error loading availability" });
        }
    }
}
