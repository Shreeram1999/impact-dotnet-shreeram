public class TemperatureConverterTests
{
    // ---- Celsius -> Fahrenheit/Kelvin ----
    [Theory]
    [InlineData(0, 32, 273.15)]
    [InlineData(100, 212, 373.15)]
    [InlineData(-40, -40, 233.15)] // edge case: -40 is the point where C and F coincide
    public void Convert_FromCelsius_ProducesExpectedFahrenheitAndKelvin(double celsius, double expectedF, double expectedK)
    {
        var (f, k) = TemperatureConverter.Convert(new Celsius(celsius));

        Assert.Equal(expectedF, f.Value, precision: 6);
        Assert.Equal(expectedK, k.Value, precision: 6);
    }

    // ---- Fahrenheit -> Celsius/Kelvin ----
    [Theory]
    [InlineData(32, 0, 273.15)]
    [InlineData(212, 100, 373.15)]
    [InlineData(-40, -40, 233.15)]
    public void Convert_FromFahrenheit_ProducesExpectedCelsiusAndKelvin(double fahrenheit, double expectedC, double expectedK)
    {
        var (c, k) = TemperatureConverter.Convert(new Fahrenheit(fahrenheit));

        Assert.Equal(expectedC, c.Value, precision: 6);
        Assert.Equal(expectedK, k.Value, precision: 6);
    }

    // ---- Kelvin -> Celsius/Fahrenheit ----
    [Theory]
    [InlineData(273.15, 0, 32)]
    [InlineData(373.15, 100, 212)]
    [InlineData(0, -273.15, -459.67)] // edge case: absolute zero
    public void Convert_FromKelvin_ProducesExpectedCelsiusAndFahrenheit(double kelvin, double expectedC, double expectedF)
    {
        var (c, f) = TemperatureConverter.Convert(new Kelvin(kelvin));

        Assert.Equal(expectedC, c.Value, precision: 6);
        Assert.Equal(expectedF, f.Value, precision: 2);
    }

    // ---- Runtime string-unit entry point: happy paths ----
    [Theory]
    [InlineData("C", 212, 373.15)]
    [InlineData("F", 0, 273.15)]
    [InlineData("K", -273.15, -459.67)]
    public void ConvertByUnit_RecognizedUnit_DelegatesToMatchingOverload(string unit, double expectedFirst, double expectedSecond)
    {
        double value = unit switch { "C" => 100, "F" => 32, "K" => 0, _ => 0 };

        var (first, second) = TemperatureConverter.ConvertByUnit(value, unit);

        Assert.Equal(expectedFirst, first, precision: 2);
        Assert.Equal(expectedSecond, second, precision: 2);
    }

    [Theory]
    [InlineData("c")]
    [InlineData("f")]
    [InlineData("k")]
    [InlineData(" C ")]
    public void ConvertByUnit_IsCaseInsensitiveAndTrimsWhitespace(string unit)
    {
        // Should not throw for any of these — proves normalization happens
        // before the switch.
        var exception = Record.Exception(() => TemperatureConverter.ConvertByUnit(20, unit));

        Assert.Null(exception);
    }

    // ---- Edge case: invalid unit ----
    [Theory]
    [InlineData("X")]
    [InlineData("")]
    [InlineData("Celsius")]
    public void ConvertByUnit_InvalidUnit_ThrowsArgumentException(string unit)
    {
        var ex = Assert.Throws<ArgumentException>(() => TemperatureConverter.ConvertByUnit(20, unit));

        Assert.Contains(unit, ex.Message);
    }

    [Fact]
    public void ConvertByUnit_NullUnit_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => TemperatureConverter.ConvertByUnit(20, null!));
    }
}
