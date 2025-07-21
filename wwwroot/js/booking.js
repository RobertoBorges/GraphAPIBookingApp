// Booking functionality
let selectedDate = null;
let selectedTime = null;
let selectedStaffId = null;

document.addEventListener('DOMContentLoaded', function() {
    // Initialize event handlers
    const staffSelect = document.getElementById('staffSelect');
    const serviceSelect = document.getElementById('serviceSelect');
    const bookButton = document.getElementById('bookButton');

    if (staffSelect) {
        staffSelect.addEventListener('change', function() {
            selectedStaffId = this.value;
            updateTimeSelection();
            updateBookButton();
        });
    }

    if (serviceSelect) {
        serviceSelect.addEventListener('change', function() {
            updateTimeSelection();
            updateBookButton();
        });
    }
});

function selectDate(element) {
    // Remove previous selection
    document.querySelectorAll('.calendar-day.selected').forEach(day => {
        day.classList.remove('selected');
    });

    // Check if the day is available
    if (!element.classList.contains('available')) {
        return;
    }

    // Add selection to clicked day
    element.classList.add('selected');
    
    // Store selected date
    selectedDate = element.getAttribute('data-date');
    document.getElementById('selectedDate').value = selectedDate;
    
    // Update time selection
    updateTimeSelection();
    updateBookButton();
}

function selectTime(timeSlot) {
    // Remove previous selection
    document.querySelectorAll('.time-slot.selected').forEach(slot => {
        slot.classList.remove('selected');
    });

    // Check if the time slot is available
    if (timeSlot.classList.contains('unavailable')) {
        return;
    }

    // Add selection to clicked time slot
    timeSlot.classList.add('selected');
    
    // Store selected time
    selectedTime = timeSlot.getAttribute('data-time');
    document.getElementById('selectedTime').value = selectedTime;
    
    updateBookButton();
}

async function updateTimeSelection() {
    const timeSelectionDiv = document.getElementById('timeSelection');
    
    if (!selectedDate || !selectedStaffId) {
        timeSelectionDiv.innerHTML = '<div class="text-muted">Select a service and date to see available times.</div>';
        return;
    }

    try {
        // Show loading state
        timeSelectionDiv.innerHTML = '<div class="text-muted">Loading available times...</div>';

        // Fetch available time slots
        const response = await fetch(`/?handler=Availability&staffId=${selectedStaffId}&date=${selectedDate}`);
        const data = await response.json();

        if (data.success) {
            let timeSlotsHtml = '<div class="time-slots">';
            
            if (data.timeSlots && data.timeSlots.length > 0) {
                data.timeSlots.forEach(slot => {
                    const availableClass = slot.available ? '' : 'unavailable';
                    timeSlotsHtml += `
                        <div class="time-slot ${availableClass}" 
                             data-time="${slot.time}" 
                             onclick="selectTime(this)">
                            ${slot.time}
                        </div>
                    `;
                });
            } else {
                timeSlotsHtml += '<div class="text-muted">No available times for this date.</div>';
            }
            
            timeSlotsHtml += '</div>';
            timeSelectionDiv.innerHTML = timeSlotsHtml;
        } else {
            timeSelectionDiv.innerHTML = '<div class="text-danger">Error loading available times. Please try again.</div>';
        }
    } catch (error) {
        console.error('Error fetching time slots:', error);
        timeSelectionDiv.innerHTML = '<div class="text-danger">Error loading available times. Please try again.</div>';
    }
}

function updateBookButton() {
    const bookButton = document.getElementById('bookButton');
    const staffSelect = document.getElementById('staffSelect');
    const serviceSelect = document.getElementById('serviceSelect');
    
    // Enable book button only if all required fields are selected
    const isFormValid = selectedDate && 
                       selectedTime && 
                       staffSelect && staffSelect.value && 
                       serviceSelect && serviceSelect.value;
    
    if (bookButton) {
        bookButton.disabled = !isFormValid;
    }
}

function previousMonth() {
    // This would typically reload the page with the previous month
    // For now, we'll just log it - in a real implementation, you'd update the URL
    console.log('Previous month clicked');
}

function nextMonth() {
    // This would typically reload the page with the next month
    // For now, we'll just log it - in a real implementation, you'd update the URL
    console.log('Next month clicked');
}

// Form validation helpers
function validateForm() {
    const fullName = document.querySelector('input[name="BookingRequest.FullName"]').value;
    const email = document.querySelector('input[name="BookingRequest.Email"]').value;
    
    if (!fullName.trim()) {
        alert('Please enter your full name.');
        return false;
    }
    
    if (!email.trim()) {
        alert('Please enter your email address.');
        return false;
    }
    
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        alert('Please enter a valid email address.');
        return false;
    }
    
    if (!selectedDate) {
        alert('Please select a date for your appointment.');
        return false;
    }
    
    if (!selectedTime) {
        alert('Please select a time for your appointment.');
        return false;
    }
    
    if (!selectedStaffId) {
        alert('Please select a staff member.');
        return false;
    }
    
    return true;
}

// Add form validation on submit
document.addEventListener('DOMContentLoaded', function() {
    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function(e) {
            if (!validateForm()) {
                e.preventDefault();
                return false;
            }
        });
    }
});