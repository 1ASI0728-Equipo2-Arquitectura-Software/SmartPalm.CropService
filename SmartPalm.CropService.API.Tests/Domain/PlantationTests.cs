using SmartPalm.CropService.Domain.Plantations;

namespace SmartPalm.CropService.UnitTests;

public class PlantationTests
{
    [Fact]
    public void Creation_calculates_one_sensor_per_ten_hectares()
    {
        var plantation = new Plantation(12, "Lote Norte", "Ucayali", null, 21);

        Assert.Equal(3, plantation.EstimatedSensors);
        Assert.Equal(PlantationStatus.Installing, plantation.Status);
    }

    [Fact]
    public void Assign_sector_normalizes_device_mac()
    {
        var plantation = new Plantation(12, "Lote Norte", "Ucayali", null, 10);

        var sector = plantation.AssignSector("Sector A", " aa:bb:cc:dd:ee:ff ");

        Assert.Equal("AA:BB:CC:DD:EE:FF", sector.IotDeviceMacAddress);
        Assert.Equal(SectorStatus.Pending, sector.Status);
    }

    [Fact]
    public void Cancelled_plantation_rejects_changes()
    {
        var plantation = new Plantation(12, "Lote Norte", "Ucayali", null, 10);
        plantation.Cancel();

        Assert.Throws<InvalidOperationException>(() =>
            plantation.Update("Otro", "Ucayali", null, 20));
    }

    [Fact]
    public void Hectares_must_be_positive() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Plantation(12, "Lote Norte", "Ucayali", null, 0));
}
