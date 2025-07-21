param(
    [Parameter(Mandatory=$true)]
    [string]$TenantId,
    
    [Parameter(Mandatory=$true)]
    [string]$ClientId,
    
    [Parameter(Mandatory=$true)]
    [string]$ClientSecret
)

# Setting user secrets for AzureAd section
dotnet user-secrets set "AzureAd:TenantId" $TenantId
dotnet user-secrets set "AzureAd:ClientId" $ClientId
dotnet user-secrets set "AzureAd:ClientSecret" $ClientSecret

Write-Host "User secrets have been set successfully!" -ForegroundColor Green
