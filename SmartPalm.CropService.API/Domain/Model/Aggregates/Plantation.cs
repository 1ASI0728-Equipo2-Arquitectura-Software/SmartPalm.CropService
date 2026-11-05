namespace SmartPalm.CropService.Domain.Plantations;

public class Plantation
{
    private readonly List<Sector> _sectors = [];

    private Plantation() { }

    public Plantation(long palmGrowerId, string name, string address, string? coordinates, decimal hectares)
    {
        if (palmGrowerId <= 0) throw new ArgumentOutOfRangeException(nameof(palmGrowerId));
        ValidateDetails(name, address, hectares);

        PalmGrowerId = palmGrowerId;
        Name = name.Trim();
        Address = address.Trim();
        Coordinates = NormalizeOptional(coordinates);
        Hectares = hectares;
        Status = PlantationStatus.Installing;
        EstimatedSensors = Math.Max(1, (int)Math.Ceiling(hectares / 10));
        InstallationMessage = EstimatedSensors == 1
            ? "This plantation requires 1 sensor sector for monitoring."
            : $"This plantation requires {EstimatedSensors} sensor sectors ({hectares} ha / 10 ha per sensor).";
        CreatedAt = DateTime.UtcNow;
    }

    public long Id { get; private set; }
    public long PalmGrowerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string? Coordinates { get; private set; }
    public decimal Hectares { get; private set; }
    public PlantationStatus Status { get; private set; }
    public int EstimatedSensors { get; private set; }
    public string InstallationMessage { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyCollection<Sector> Sectors => _sectors;

    public void Update(string name, string address, string? coordinates, decimal hectares)
    {
        if (Status == PlantationStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled plantation.");

        ValidateDetails(name, address, hectares);
        Name = name.Trim();
        Address = address.Trim();
        Coordinates = NormalizeOptional(coordinates);
        Hectares = hectares;
        EstimatedSensors = Math.Max(1, (int)Math.Ceiling(hectares / 10));
        InstallationMessage = EstimatedSensors == 1
            ? "This plantation requires 1 sensor sector for monitoring."
            : $"This plantation requires {EstimatedSensors} sensor sectors ({hectares} ha / 10 ha per sensor).";
        UpdatedAt = DateTime.UtcNow;
    }

    public Sector AssignSector(string name, string? deviceMacAddress = null)
    {
        if (Status == PlantationStatus.Cancelled)
            throw new InvalidOperationException("Cannot assign sectors to a cancelled plantation.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Sector name is required.", nameof(name));
        var sector = new Sector(Id, name, deviceMacAddress);
        _sectors.Add(sector);
        return sector;
    }

    public void Activate()
    {
        if (Status != PlantationStatus.Installing)
            throw new InvalidOperationException($"Cannot activate plantation in '{Status}' status.");
        Status = PlantationStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == PlantationStatus.Cancelled)
            throw new InvalidOperationException("Plantation is already cancelled.");
        Status = PlantationStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateDetails(string name, string address, decimal hectares)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Plantation name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Address is required.", nameof(address));
        if (hectares <= 0) throw new ArgumentOutOfRangeException(nameof(hectares), "Hectares must be greater than zero.");
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
