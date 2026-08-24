// The "Factory Method" pattern is a step up from the simple factory above.
// Instead of one static method with a big switch statement, each KIND of
// factory is its own class, and the decision of "which concrete class to
// build" is made by overriding an abstract method - i.e. it uses
// inheritance/polymorphism (Week 2!) instead of an if/switch.
//
// Why bother? With VehicleFactory.CreateVehicle(string), adding a new
// vehicle type means editing the switch statement inside an existing
// class. With the Factory Method version, adding a new vehicle type just
// means adding a NEW factory class (e.g. TruckFactory) - nothing that
// already exists has to change. That "add new code instead of editing old
// code" property is called the Open/Closed Principle, one of the SOLID
// principles used later this week.
public abstract class VehicleFactoryBase
{
    public abstract IVehicle CreateVehicle();
}

public class CarFactory : VehicleFactoryBase
{
    public override IVehicle CreateVehicle() => new Car();
}

public class BikeFactory : VehicleFactoryBase
{
    public override IVehicle CreateVehicle() => new Bike();
}
