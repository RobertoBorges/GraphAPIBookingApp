using GraphAPIBookingApp.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
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
                // Fetch users from Microsoft Graph API
                // Only fetch users that are enabled and have the "Doctor" job title
                var users = await _graphServiceClient.Users
                    .GetAsync(requestConfiguration =>
                    {
                        // requestConfiguration.QueryParameters.Filter = "accountEnabled eq true and jobTitle eq 'Doctor'";
                        // requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName", "mail", "jobTitle", "userPrincipalName" };
                        requestConfiguration.QueryParameters.Top = 50;
                    });

                var staffMembers = new List<StaffMember>();

                if (users?.Value != null)
                {
                    // Map Graph API users to our StaffMember model
                    foreach (var user in users.Value)
                    {
                        staffMembers.Add(new StaffMember
                        {
                            Id = user.Id ?? Guid.NewGuid().ToString(),
                            DisplayName = user.DisplayName ?? "Unknown Doctor",
                            Email = user.Mail ?? user.UserPrincipalName ?? $"{user.Id}@unknown.com",
                            Role = user.JobTitle ?? "Doctor"
                        });
                    }
                }

                _logger.LogInformation("Retrieved {Count} staff members from Entra ID", staffMembers.Count);
                return staffMembers;
            }
            catch (Microsoft.Graph.Models.ODataErrors.ODataError ex)
            {
                // Handle Microsoft Graph specific exceptions
                _logger.LogError(ex, "Microsoft Graph API error retrieving staff members: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff members from Entra ID");
                throw;
            }
        }

        public async Task<List<AvailableSlot>> GetStaffAvailabilityAsync(string staffId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var availableSlots = new List<AvailableSlot>();

                // Set business hours (9 AM to 5 PM)
                var businessStartHour = 9;
                var businessEndHour = 17;

                // First, generate all possible slots during business hours
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // Skip weekends
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    // Generate time slots during business hours
                    for (int hour = businessStartHour; hour < businessEndHour; hour++)
                    {
                        var slotStart = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                        var slotEnd = slotStart.AddHours(1);

                        availableSlots.Add(new AvailableSlot
                        {
                            Date = date,
                            StartTime = slotStart.ToString("HH:mm"),
                            EndTime = slotEnd.ToString("HH:mm"),
                            IsAvailable = true,
                            StaffId = staffId
                        });
                    }
                }

                // Try to get busy times from Microsoft Graph calendar if available
                try
                {
                    // Create a time window for the availability query
                    var viewStart = startDate.Date; // Start of the day
                    var viewEnd = endDate.Date.AddDays(1); // End of the last day

                    // Look up the user by ID to get their email/UPN
                    var user = await _graphServiceClient.Users[staffId]
                        .GetAsync(requestConfiguration =>
                        {
                            requestConfiguration.QueryParameters.Select = new string[] { "mail", "userPrincipalName" };
                        });

                    if (user != null)
                    {
                        string userEmail = user.Mail ?? user.UserPrincipalName ?? string.Empty;

                        if (!string.IsNullOrEmpty(userEmail))
                        {
                            // Get calendar events for this user in the date range
                            var events = await _graphServiceClient.Users[staffId].Calendar.Events
                                .GetAsync(requestConfiguration =>
                                {
                                    requestConfiguration.QueryParameters.Filter = $"start/dateTime ge '{viewStart:yyyy-MM-ddTHH:mm:ss}' and end/dateTime le '{viewEnd:yyyy-MM-ddTHH:mm:ss}'";
                                    requestConfiguration.QueryParameters.Select = new string[] { "subject", "start", "end", "showAs" };
                                });

                            // Mark slots as unavailable if they overlap with busy calendar events
                            if (events?.Value != null)
                            {
                                foreach (var calEvent in events.Value)
                                {
                                    if (calEvent.ShowAs == FreeBusyStatus.Busy ||
                                        calEvent.ShowAs == FreeBusyStatus.Oof ||
                                        calEvent.ShowAs == FreeBusyStatus.WorkingElsewhere)
                                    {
                                        // Parse event times (handling potential null values)
                                        if (DateTime.TryParse(calEvent.Start?.DateTime, out DateTime eventStart) &&
                                            DateTime.TryParse(calEvent.End?.DateTime, out DateTime eventEnd))
                                        {
                                            // Adjust for timezone if needed
                                            string startTimeZone = calEvent.Start?.TimeZone ?? "UTC";
                                            string endTimeZone = calEvent.End?.TimeZone ?? "UTC";

                                            // Mark any overlapping slots as unavailable
                                            foreach (var slot in availableSlots)
                                            {
                                                var slotStart = DateTime.Parse($"{slot.Date:yyyy-MM-dd} {slot.StartTime}");
                                                var slotEnd = DateTime.Parse($"{slot.Date:yyyy-MM-dd} {slot.EndTime}");

                                                // Check if this slot overlaps with the busy event
                                                if ((slotStart < eventEnd && slotEnd > eventStart))
                                                {
                                                    slot.IsAvailable = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail - fall back to default availability
                    _logger.LogWarning(ex, "Could not fetch calendar data for staff {StaffId}, using default availability", staffId);
                }

                // Only return slots that are available
                return availableSlots.Where(s => s.IsAvailable).ToList();
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
                // In a real app, you might want to allow filtering by specific business IDs
                // from configuration or user selection

                var services = new List<Service>();

                try
                {
                    // Try to fetch services from Microsoft Bookings
                    var bookingsBusinesses = await _graphServiceClient.Solutions.BookingBusinesses
                        .GetAsync();

                    if (bookingsBusinesses?.Value != null && bookingsBusinesses.Value.Count > 0)
                    {
                        _logger.LogInformation("Found {Count} booking businesses in your tenant", bookingsBusinesses.Value.Count);

                        // Loop through all businesses to collect their services
                        foreach (var business in bookingsBusinesses.Value)
                        {
                            if (business?.Id == null) continue;

                            _logger.LogInformation("Processing booking business: {BusinessName} ({BusinessId})",
                                business.DisplayName ?? "Unknown Business", business.Id);

                            try
                            {
                                // Get services for this business
                                var bookingServices = await _graphServiceClient.Solutions.BookingBusinesses[business.Id].Services
                                    .GetAsync();

                                if (bookingServices?.Value != null)
                                {
                                    foreach (var bookingService in bookingServices.Value)
                                    {
                                        if (bookingService != null)
                                        {
                                            var service = new Service
                                            {
                                                Id = bookingService.Id ?? Guid.NewGuid().ToString(),
                                                DisplayName = bookingService.DisplayName ?? "Unknown Service",
                                                Description = bookingService.Notes ?? bookingService.Description ?? "No description available",
                                                DurationInMinutes = (int)(bookingService.DefaultDuration?.TotalMinutes ?? 30)
                                            };

                                            // Add business name as prefix to display name to differentiate services from different businesses
                                            if (bookingsBusinesses.Value.Count > 1)
                                            {
                                                service.DisplayName = $"{business.DisplayName} - {service.DisplayName}";
                                            }

                                            services.Add(service);
                                        }
                                    }

                                    _logger.LogInformation("Retrieved {Count} services from business {BusinessName}",
                                        bookingServices.Value.Count, business.DisplayName ?? "Unknown Business");
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log but continue with other businesses
                                _logger.LogWarning(ex, "Error retrieving services for business {BusinessId}", business.Id);
                            }
                        }

                        _logger.LogInformation("Retrieved total of {Count} services from all businesses", services.Count);
                    }
                }
                catch (ODataError graphEx)
                {
                    // Log the specific Graph API error but continue to fallback
                    _logger.LogWarning(graphEx, "Could not fetch services from Microsoft Bookings: {Message}. Using fallback services.", graphEx.Message);

                    // If we couldn't get services from Bookings, use fallback mock data
                    if (services.Count == 0)
                    {
                        // Fallback to default services if we couldn't get any from the API
                        services = new List<Service>
                        {
                            new Service { Id = "1", DisplayName = "General Consultation", Description = "General medical consultation", DurationInMinutes = 30 },
                            new Service { Id = "2", DisplayName = "Health Checkup", Description = "Complete health checkup", DurationInMinutes = 60 },
                            new Service { Id = "3", DisplayName = "Follow-up Visit", Description = "Follow-up consultation", DurationInMinutes = 15 }
                        };

                        _logger.LogInformation("Using fallback service data with {Count} services", services.Count);
                    }
                }

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