// Bonus coverage alongside MathHelper, since OrderProcessor lives in the
// same project: proves each OrderProcessor instance tracks its own orders
// independently, which is the whole reason it uses instance methods.
public class OrderProcessorTests
{
    [Fact]
    public void ProcessOrder_AddsToThisInstancesList()
    {
        var processor = new OrderProcessor();

        processor.ProcessOrder("A-1001");

        Assert.Single(processor.ProcessedOrderIds);
        Assert.Equal("A-1001", processor.ProcessedOrderIds[0]);
    }

    [Fact]
    public void ProcessOrder_TwoSeparateInstances_DoNotShareState()
    {
        var processorA = new OrderProcessor();
        var processorB = new OrderProcessor();

        processorA.ProcessOrder("A-1001");

        Assert.Single(processorA.ProcessedOrderIds);
        Assert.Empty(processorB.ProcessedOrderIds);
    }
}
