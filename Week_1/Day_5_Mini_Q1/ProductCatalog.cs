public static class ProductCatalog
{
    // Pulled out of Program.cs so it's a plain, testable method instead of
    // inline top-level-statement code. Groups products by category, only
    // emitting groups that actually have at least one product in them.
    public static IEnumerable<IGrouping<ProductCategory, Product>> GroupByCategory(IEnumerable<Product> products)
    {
        return products.GroupBy(p => p.Category);
    }
}
