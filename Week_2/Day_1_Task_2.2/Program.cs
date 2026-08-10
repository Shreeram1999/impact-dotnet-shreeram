// `Vehicle[]` is an array where every slot is DECLARED as type Vehicle -
// but we're actually putting a Car, a Bike, and an ElectricCar into it.
// That's allowed because Car/Bike/ElectricCar all ARE Vehicles (they
// inherit from it). The `[]` collection-literal syntax below is a modern
// C# shortcut for building an array without writing `new Vehicle[] { ... }`.
Vehicle[] vehicles =
[
    new Car("Toyota", "Corolla", 2022, 4),
    new Bike("Royal Enfield", "Classic 350", 2021, hasSidecar: false),
    new ElectricCar("Tesla", "Model 3", 2023, 4, 75.0),
];

// This is "runtime polymorphism": even though the loop variable `vehicle`
// is typed as `Vehicle`, calling vehicle.DisplayInfo() runs whichever
// override actually belongs to that object at runtime - Car's version for
// the Car, Bike's for the Bike, ElectricCar's for the ElectricCar. The
// loop code itself never needs an if/else to check "what type is this?".
foreach (var vehicle in vehicles)
{
    vehicle.DisplayInfo();
    Console.WriteLine();
}
