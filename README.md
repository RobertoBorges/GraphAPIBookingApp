# GraphAPIBookingApp

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

### For Production Environments

In a production environment, consider using:

- Azure Key Vault
- Managed Identities
- Environment Variables with secure handling

Do not commit sensitive information to source control or embed it in Docker images.