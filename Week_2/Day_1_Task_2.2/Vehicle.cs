// Task 2.2 - Inheritance and overriding.
// Inheritance lets one class ("derived"/"child") reuse and extend another
// class ("base"/"parent"). Here Vehicle is the base class, and Car, Bike,
// and ElectricCar are all derived from it (or from each other). Every Car
// automatically has Make/Model/Year just by inheriting from Vehicle - we
// don't have to retype those properties.
public class Vehicle
{
    // `{ get; }` makes these "read-only properties": they can be set once
    // (in the constructor below) but never changed afterward from outside
    // the class. This is a lighter-weight form of encapsulation than
    // Task 2.1's private field + getter method.
    public string Make { get; }
    public string Model { get; }
    public int Year { get; }

    public Vehicle(string make, string model, int year)
    {
        Make = make;
        Model = model;
        Year = year;
    }

    // `virtual` is the keyword that says "derived classes are ALLOWED to
    // replace this method with their own version". Without `virtual` here,
    // Car/Bike/ElectricCar wouldn't be allowed to use `override` below.
    public virtual void DisplayInfo()
    {
        Console.WriteLine($"{Year} {Make} {Model}");
    }
}

public class Car : Vehicle
{
    // `: Vehicle` above means "Car inherits from Vehicle". Car gets
    // everything Vehicle has, plus this new property of its own.
    public int NumberOfDoors { get; }

    // `: base(make, model, year)` calls Vehicle's constructor first, using
    // these same three values, so Make/Model/Year get set up there before
    // we set NumberOfDoors here. Constructors always run base-class-first.
    public Car(string make, string model, int year, int numberOfDoors)
        : base(make, model, year)
    {
        NumberOfDoors = numberOfDoors;
    }

    // `override` means "replace Vehicle's DisplayInfo() with this version,
    // for any Car object". `base.DisplayInfo()` on the next line calls the
    // ORIGINAL Vehicle version first (so we still print Year/Make/Model),
    // and then we add an extra line about doors on top of it.
    public override void DisplayInfo()
    {
        base.DisplayInfo();
        Console.WriteLine($"  Car with {NumberOfDoors} doors");
    }
}

public class Bike : Vehicle
{
    public bool HasSidecar { get; }

    public Bike(string make, string model, int year, bool hasSidecar)
        : base(make, model, year)
    {
        HasSidecar = hasSidecar;
    }

    public override void DisplayInfo()
    {
        base.DisplayInfo();
        Console.WriteLine($"  Bike, sidecar: {(HasSidecar ? "yes" : "no")}");
    }
}

// ElectricCar inherits from Car, which itself inherits from Vehicle - so
// this is a 3-level chain: ElectricCar -> Car -> Vehicle. An ElectricCar
// object has Make/Model/Year (from Vehicle), NumberOfDoors (from Car), AND
// BatteryCapacityKwh (its own).
public class ElectricCar : Car
{
    public double BatteryCapacityKwh { get; }

    // This constructor calls Car's constructor (`base(...)`), which in
    // turn calls Vehicle's constructor. So by the time this constructor's
    // body runs, every property from all three classes is already set.
    public ElectricCar(string make, string model, int year, int numberOfDoors, double batteryCapacityKwh)
        : base(make, model, year, numberOfDoors)
    {
        BatteryCapacityKwh = batteryCapacityKwh;
    }

    // This overrides Car's ALREADY-overridden DisplayInfo() again.
    // Calling `base.DisplayInfo()` here runs Car's version, which itself
    // calls Vehicle's version - so one call to an ElectricCar's
    // DisplayInfo() ends up printing all three levels' output, in order.
    public override void DisplayInfo()
    {
        base.DisplayInfo();
        Console.WriteLine($"  Battery: {BatteryCapacityKwh} kWh");
    }
}
