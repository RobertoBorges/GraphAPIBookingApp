using GraphAPIBookingApp.Models;
using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace GraphAPIBookingApp.Services
{
    public interface IBookingService
    {
        Task<List<StaffMember>> GetStaffMembersAsync();
        Task<List<AvailableSlot>> GetStaffAvailabilityAsync(string staffId, DateTime startDate, DateTime endDate);
        Task<bool> CreateAppointmentAsync(BookingRequest request);
        Task<List<Service>> GetServicesAsync();
    }

    public class BookingService : IBookingService
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly ILogger<BookingService> _logger;

        public BookingService(GraphServiceClient graphServiceClient, ILogger<BookingService> logger)
        {
            _graphServiceClient = graphServiceClient;
            _logger = logger;
        }

        public async Task<List<StaffMember>> GetStaffMembersAsync()
        {
            try
            {
                // For demo purposes, return some mock staff members
                // In a real implementation, you would fetch from Microsoft Graph
                var staffMembers = new List<StaffMember>
                {
                    new StaffMember { Id = "1", DisplayName = "Dr. Sarah Johnson", Email = "sarah.johnson@company.com", Role = "Doctor" },
                    new StaffMember { Id = "2", DisplayName = "Dr. Michael Chen", Email = "michael.chen@company.com", Role = "Doctor" },
                    new StaffMember { Id = "3", DisplayName = "Dr. Emily Rodriguez", Email = "emily.rodriguez@company.com", Role = "Doctor" }
                };

                // Simulate async operation
                await Task.Delay(1);
                return staffMembers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff members");
                throw;
            }
        }

        public async Task<List<AvailableSlot>> GetStaffAvailabilityAsync(string staffId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // For demo purposes, generate mock availability
                // In a real implementation, you would call the Microsoft Graph Bookings API
                var availableSlots = new List<AvailableSlot>();

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // Skip weekends for this demo
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    // Generate time slots from 9 AM to 5 PM
                    for (int hour = 9; hour < 17; hour++)
                    {
                        var startTime = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                        var endTime = startTime.AddHours(1);

                        availableSlots.Add(new AvailableSlot
                        {
                            Date = date,
                            StartTime = startTime.ToString("HH:mm"),
                            EndTime = endTime.ToString("HH:mm"),
                            IsAvailable = true,
                            StaffId = staffId
                        });
                    }
                }

                return availableSlots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff availability for staff {StaffId}", staffId);
                throw;
            }
        }

        public async Task<bool> CreateAppointmentAsync(BookingRequest request)
        {
            try
            {
                // For demo purposes, just log the appointment creation
                // In a real implementation, you would call the Microsoft Graph Bookings API
                _logger.LogInformation("Creating appointment for {FullName} on {Date} at {Time} with staff {StaffId}",
                    request.FullName, request.SelectedDate, request.SelectedTime, request.SelectedStaffId);

                // Simulate async operation
                await Task.Delay(100);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment for {FullName}", request.FullName);
                return false;
            }
        }

        public async Task<List<Service>> GetServicesAsync()
        {
            try
            {
                // For demo purposes, return some mock services
                var services = new List<Service>
                {
                    new Service { Id = "1", DisplayName = "General Consultation", Description = "General medical consultation", DurationInMinutes = 30 },
                    new Service { Id = "2", DisplayName = "Health Checkup", Description = "Complete health checkup", DurationInMinutes = 60 },
                    new Service { Id = "3", DisplayName = "Follow-up Visit", Description = "Follow-up consultation", DurationInMinutes = 15 }
                };

                // Simulate async operation
                await Task.Delay(1);
                return services;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services");
                throw;
            }
        }
    }
}