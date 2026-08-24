// Task 3.13 - static vs instance methods, part 2: OrderProcessor.
//
// Unlike MathHelper, OrderProcessor's methods DO need to remember state
// between calls - specifically, the running list of orders it has already
// processed. That's exactly the situation where INSTANCE methods (methods
// that belong to a particular OBJECT, accessed via `someProcessor.Method()`)
// are the right choice: each separate OrderProcessor object gets its OWN
// independent `processedOrderIds` list. If these were static methods
// instead, there would only be ONE shared list for the entire program,
// which would be wrong the moment you needed two independent processors
// (e.g. one per warehouse) that shouldn't see each other's orders.
public class OrderProcessor
{
    private readonly List<string> processedOrderIds = new();

    public string ProcessOrder(string orderId)
    {
        processedOrderIds.Add(orderId);
        return $"Order {orderId} processed. Total processed by this processor: {processedOrderIds.Count}.";
    }

    public IReadOnlyList<string> ProcessedOrderIds => processedOrderIds.AsReadOnly();
}
