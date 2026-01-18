using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MyRealEstate.Application.Interfaces;

namespace MyRealEstate.Infrastructure.Services;

public class LocalFileStorage : IFileStorage
{
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string UploadDirectory = "uploads";
    
    public LocalFileStorage(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<FileUploadResult> SaveFileAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        var uploadsPath = Path.Combine(_environment.WebRootPath, UploadDirectory);
        Directory.CreateDirectory(uploadsPath);
        
        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(uploadsPath, uniqueFileName);
        
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream, cancellationToken);
        
        return new FileUploadResult
        {
            FilePath = Path.Combine(UploadDirectory, uniqueFileName).Replace("\\", "/"),
            FileName = fileName,
            FileSize = fileStream.Length,
            ContentType = GetContentType(fileName)
        };
    }
    
    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, filePath.Replace("/", "\\"));
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        
        return Task.CompletedTask;
    }
    
    public Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, filePath.Replace("/", "\\"));
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }
        
        return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }
    
    public string GetFileUrl(string filePath)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        
        if (request == null)
        {
            return $"/{filePath}";
        }
        
        return $"{request.Scheme}://{request.Host}/{filePath}";
    }
    
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
