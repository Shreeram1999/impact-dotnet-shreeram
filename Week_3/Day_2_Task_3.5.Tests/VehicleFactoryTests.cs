// Covers the simple factory's correct-type-per-input behavior (including
// that matching is case-insensitive), its "unknown type" failure path, and
// the separate Factory Method classes (CarFactory/BikeFactory).
public class VehicleFactoryTests
{
    [Theory]
    [InlineData("car", typeof(Car))]
    [InlineData("bike", typeof(Bike))]
    [InlineData("truck", typeof(Truck))]
    [InlineData("CAR", typeof(Car))]
    public void CreateVehicle_KnownType_ReturnsCorrectConcreteType(string type, Type expectedType)
    {
        var vehicle = VehicleFactory.CreateVehicle(type);

        Assert.IsType(expectedType, vehicle);
    }

    [Fact]
    public void CreateVehicle_UnknownType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => VehicleFactory.CreateVehicle("spaceship"));
    }

    [Fact]
    public void CarFactory_CreateVehicle_ReturnsCar()
    {
        VehicleFactoryBase factory = new CarFactory();

        Assert.IsType<Car>(factory.CreateVehicle());
    }

    [Fact]
    public void BikeFactory_CreateVehicle_ReturnsBike()
    {
        VehicleFactoryBase factory = new BikeFactory();

        Assert.IsType<Bike>(factory.CreateVehicle());
    }
}
