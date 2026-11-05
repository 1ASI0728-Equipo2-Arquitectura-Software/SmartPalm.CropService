namespace SmartPalm.CropService.Domain.Plantations;

public class AgronomistAffiliation
{
    private AgronomistAffiliation() { }

    public AgronomistAffiliation(long agronomistId, long plantationId)
    {
        if (agronomistId <= 0) throw new ArgumentOutOfRangeException(nameof(agronomistId));
        if (plantationId <= 0) throw new ArgumentOutOfRangeException(nameof(plantationId));
        AgronomistId = agronomistId;
        PlantationId = plantationId;
        AffiliatedAt = DateTime.UtcNow;
    }

    public long Id { get; private set; }
    public long AgronomistId { get; private set; }
    public long PlantationId { get; private set; }
    public DateTime AffiliatedAt { get; private set; }
    public DateTime? DetachedAt { get; private set; }
    public bool IsActive => DetachedAt is null;

    public void Detach()
    {
        if (DetachedAt.HasValue) throw new InvalidOperationException("Affiliation is already detached.");
        DetachedAt = DateTime.UtcNow;
    }
}
