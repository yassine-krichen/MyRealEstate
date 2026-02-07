# TinyMCE API Key Configuration

This document explains how to securely configure your TinyMCE API key for the EstateFlow CMS.

## Security Approach

The TinyMCE API key is stored securely using:

- **Development**: ASP.NET Core User Secrets (not committed to source control)
- **Production**: Environment Variables

## Setup Instructions

### For Development (Your Local Machine)

1. **Get your TinyMCE API Key** from https://www.tiny.cloud/

2. **Store it in User Secrets**:

    ```bash
    cd src/MyRealEstate.Web
    dotnet user-secrets set "TinyMCE:ApiKey" "YOUR-ACTUAL-API-KEY-HERE"
    ```

3. **Verify it's stored**:
    ```bash
    dotnet user-secrets list
    ```

### For Production Deployment

#### Option 1: Azure App Service

1. Go to your App Service → **Configuration** → **Application settings**
2. Add a new setting:
    - Name: `TinyMCE__ApiKey`
    - Value: `YOUR-ACTUAL-API-KEY-HERE`
3. Save and restart the app

#### Option 2: IIS / Windows Server

1. Set environment variable:
    ```powershell
    [System.Environment]::SetEnvironmentVariable("TinyMCE__ApiKey", "YOUR-ACTUAL-API-KEY-HERE", "Machine")
    ```
2. Restart IIS

#### Option 3: Docker

Add to your docker-compose.yml or docker run command:

```yaml
environment:
    - TinyMCE__ApiKey=YOUR-ACTUAL-API-KEY-HERE
```

#### Option 4: appsettings.Production.json (Less Secure)

**⚠️ Not recommended** - Only if you secure this file separately

```json
{
    "TinyMCE": {
        "ApiKey": "YOUR-ACTUAL-API-KEY-HERE"
    }
}
```

## How It Works

1. **Configuration Injection**: The `ContentController` receives `IConfiguration` via dependency injection
2. **ViewBag Transfer**: API key is passed to views via `ViewBag.TinyMceApiKey`
3. **CDN URL**: TinyMCE script URL is dynamically constructed in the view
4. **Fallback**: If no key is found, defaults to `"no-api-key"` (limited features)

## Files Modified

- `ContentController.cs` - Injects IConfiguration and passes API key to views
- `Create.cshtml` - Uses `@ViewBag.TinyMceApiKey` in script URL
- `Edit.cshtml` - Uses `@ViewBag.TinyMceApiKey` in script URL

## Security Best Practices ✅

✅ **Never commit** API keys to source control  
✅ **Use User Secrets** for local development  
✅ **Use Environment Variables** for production  
✅ **Add to .gitignore**: `appsettings.Production.json` (if used)  
✅ **Rotate keys** if accidentally exposed

## Troubleshooting

**Problem**: TinyMCE shows "This domain is not registered with Tiny Cloud"

- **Solution**: Verify your API key is correct and domain is registered at https://www.tiny.cloud/my-account/domains/

**Problem**: `ViewBag.TinyMceApiKey` is null

- **Solution**: Run `dotnet user-secrets list` to verify the key is stored. If not, run the set command again.

**Problem**: API key works locally but not in production

- **Solution**: Check environment variables are set correctly. In Azure, verify Application Settings. Note: Use double underscore `__` in environment variables instead of colon `:`.

## Where to Get API Key

1. Visit https://www.tiny.cloud/auth/signup/
2. Sign up for a free account
3. Navigate to **API Key Manager**
4. Copy your API key
5. Add approved domains (e.g., `localhost`, `your-production-domain.com`)

## Free vs Paid Plans

- **Free Tier**: 1,000 editor loads per month, core features
- **Paid Plans**: Higher limits, premium plugins, priority support

For EstateFlow's content management needs, the free tier should be sufficient for most use cases.
