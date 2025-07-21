# GraphAPIBookingApp

## Overview

GraphAPIBookingApp is a booking application that integrates with Microsoft Graph API to provide real-time staff availability and service management. The app allows users to:

- Browse staff members pulled directly from your organization's Entra ID (Azure Active Directory)
- Check real-time staff availability based on their Microsoft 365 calendars
- View services provided by your organization's Microsoft Bookings businesses
- Schedule appointments with available staff members

The application is built using ASP.NET Core Razor Pages and the Microsoft Graph API, enabling seamless integration with your Microsoft 365 environment.

## Features

- **Entra ID Integration**: Fetches staff information directly from your organization's directory
- **Calendar Integration**: Shows staff availability based on their actual Microsoft 365 calendars
- **Bookings Services Integration**: Displays service offerings from Microsoft Bookings businesses
- **Responsive Design**: Works on desktop and mobile devices
- **Fault Tolerance**: Falls back to mock data if Microsoft Graph API is unavailable

## Requirements

- .NET 8.0 SDK or later
- Microsoft 365 tenant with Entra ID
- App registration in Entra ID with appropriate permissions

## Configuration and Secrets Management

This application uses .NET User Secrets for storing sensitive configuration values during development.  
Production secrets should be stored in Azure Key Vault or another secure secrets management service.

### Setting up development secrets

1. Make sure you have the .NET SDK installed
2. Run the provided PowerShell script with your actual values:

```powershell
.\set-user-secrets.ps1 -TenantId "your-tenant-id" -ClientId "your-client-id" -ClientSecret "your-client-secret"
```

Alternatively, you can manually set the secrets with these commands:

```bash
dotnet user-secrets set "AzureAd:TenantId" "your-tenant-id"
dotnet user-secrets set "AzureAd:ClientId" "your-client-id" 
dotnet user-secrets set "AzureAd:ClientSecret" "your-client-secret"
```

### Required API Permissions

The application requires the following Microsoft Graph API permissions:

- **User.Read.All** - To read user profiles including job titles
- **User.ReadBasic.All** - For basic user profile information
- **Calendars.Read** - To check staff availability from calendars
- **Directory.Read.All** - To query directory data
- **BookingsApi.Read.All** - To access Microsoft Bookings services

These permissions must be granted to your app registration in the Entra ID admin center with admin consent.

### For Production Environments

In a production environment, consider using:

- Azure Key Vault
- Managed Identities
- Environment Variables with secure handling

Do not commit sensitive information to source control or embed it in Docker images.

## Getting Started

1. Clone the repository
2. Set up the secrets as described above
3. Run the application: `dotnet run`
4. Access the application at `https://localhost:7126` (or the port configured in your environment)

## Architecture

This application uses:

- **ASP.NET Core 8.0** with Razor Pages
- **Microsoft Graph SDK** for .NET
- **Microsoft Identity Web** for authentication
- **Bootstrap** for responsive UI

## License

See the [LICENSE](LICENSE) file for details.