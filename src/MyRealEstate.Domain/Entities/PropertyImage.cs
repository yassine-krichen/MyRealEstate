namespace MyRealEstate.Domain.Entities;

public class PropertyImage : BaseEntity
{
    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSize { get; set; }

    public void SetAsMain()
    {
        IsMain = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetMain()
    {
        IsMain = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
