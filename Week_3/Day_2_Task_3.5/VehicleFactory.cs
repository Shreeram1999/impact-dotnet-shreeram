// Task 3.5 - the Factory pattern (specifically the "simple factory" style
// here - a single method that decides which concrete class to build).
//
// Without a factory, code that needs a vehicle would have to write
// `new Car()` or `new Bike()` directly, which means that code has to KNOW
// about every concrete vehicle class. A factory hides that decision behind
// one method: the caller just says WHAT KIND of vehicle it wants (as a
// string), and VehicleFactory decides HOW to actually build it. The caller
// only ever sees the IVehicle interface, never the concrete Car/Bike/Truck
// classes.
public static class VehicleFactory
{
    public static IVehicle CreateVehicle(string type)
    {
        // `switch` with a string, matched case-insensitively so "car",
        // "Car", and "CAR" all work the same way.
        return type.ToLowerInvariant() switch
        {
            "car" => new Car(),
            "bike" => new Bike(),
            "truck" => new Truck(),
            // Any type string we don't recognize is a programming mistake
            // somewhere, so we fail loudly with a clear message instead of
            // silently returning null.
            _ => throw new ArgumentException($"Unknown vehicle type: '{type}'.", nameof(type)),
        };
    }
}
