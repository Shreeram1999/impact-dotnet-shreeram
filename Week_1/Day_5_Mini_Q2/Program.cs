// See TemperatureConverter.cs for the overloaded Convert(...) methods and
// why they had to move out of this file (local functions can't be
// overloaded — that was CS0128 on the first attempt).

var fromCelsius = TemperatureConverter.Convert(new Celsius(100));
Console.WriteLine($"100C -> {fromCelsius.F.Value}F, {fromCelsius.K.Value}K");

var fromFahrenheit = TemperatureConverter.Convert(new Fahrenheit(32));
Console.WriteLine($"32F -> {fromFahrenheit.C.Value}C, {fromFahrenheit.K.Value}K");

var fromKelvin = TemperatureConverter.Convert(new Kelvin(0));
Console.WriteLine($"0K -> {fromKelvin.C.Value}C, {fromKelvin.F.Value:F2}F");

// Runtime-unit entry point, e.g. as if the unit came from user input:
var (first, second) = TemperatureConverter.ConvertByUnit(100, "C");
Console.WriteLine($"ConvertByUnit(100, \"C\") -> {first}F, {second}K");

try
{
    TemperatureConverter.ConvertByUnit(100, "X");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid unit rejected: {ex.Message}");
}
