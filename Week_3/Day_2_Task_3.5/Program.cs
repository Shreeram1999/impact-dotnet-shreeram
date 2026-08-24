// Simple factory: we never write `new Car()` here - we just ask
// VehicleFactory for what we want by name, and get back an IVehicle.
Console.WriteLine("Simple factory (VehicleFactory.CreateVehicle):");
IVehicle car = VehicleFactory.CreateVehicle("car");
IVehicle bike = VehicleFactory.CreateVehicle("bike");
IVehicle truck = VehicleFactory.CreateVehicle("truck");
car.Drive();
bike.Drive();
truck.Drive();

try
{
    VehicleFactory.CreateVehicle("spaceship");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"  Unknown type rejected as expected: {ex.Message}");
}

// Factory Method: instead of passing a string, we pick a FACTORY OBJECT,
// and calling CreateVehicle() on it runs that specific factory's override.
// Still no `new Car()`/`new Bike()` here in Program.cs.
Console.WriteLine();
Console.WriteLine("Factory Method (CarFactory / BikeFactory):");
VehicleFactoryBase[] factories = [new CarFactory(), new BikeFactory()];
foreach (var factory in factories)
{
    var vehicle = factory.CreateVehicle();
    vehicle.Drive();
}
