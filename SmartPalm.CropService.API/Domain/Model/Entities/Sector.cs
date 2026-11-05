namespace SmartPalm.CropService.Domain.Plantations;

public class Sector
{
    private Sector() { }

    internal Sector(long plantationId, string name, string? deviceMacAddress)
    {
        PlantationId = plantationId;
        Name = name.Trim();
        IotDeviceMacAddress = NormalizeMac(deviceMacAddress);
        Status = SectorStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public long Id { get; private set; }
    public long PlantationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? IotDeviceMacAddress { get; private set; }
    public SectorStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ActivatedAt { get; private set; }

    public void Activate()
    {
        if (Status != SectorStatus.Pending)
            throw new InvalidOperationException($"Sector is already in '{Status}' status.");
        Status = SectorStatus.Active;
        ActivatedAt = DateTime.UtcNow;
    }

    public void SetMaintenance()
    {
        if (Status != SectorStatus.Active)
            throw new InvalidOperationException("Only active sectors can enter maintenance.");
        Status = SectorStatus.Maintenance;
    }

    public static string? NormalizeMac(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}
