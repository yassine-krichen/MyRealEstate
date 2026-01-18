namespace MyRealEstate.Application.Interfaces;

public interface IFileStorage
{
    Task<FileUploadResult> SaveFileAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    string GetFileUrl(string filePath);
}

public class FileUploadResult
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
}
