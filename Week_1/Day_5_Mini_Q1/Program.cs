List<Product> products = new()
{
    new Product("Laptop", 75000, ProductCategory.Electronics),
    new Product("Wireless Mouse", 799, ProductCategory.Electronics),
    new Product("Novel: Dune", 499, ProductCategory.Books),
    new Product("Cookbook", 899, ProductCategory.Books),
    new Product("Office Chair", 6500, ProductCategory.Furniture),
};

// GroupBy(p => p.Category) buckets the products by their Category value,
// producing one group per distinct category — no manual dictionary needed.
var groups = ProductCatalog.GroupByCategory(products);

foreach (var group in groups)
{
    Console.WriteLine($"{group.Key}:");
    foreach (Product product in group)
    {
        Console.WriteLine($"  - {product.Name}: {product.Price:C}");
    }
}
