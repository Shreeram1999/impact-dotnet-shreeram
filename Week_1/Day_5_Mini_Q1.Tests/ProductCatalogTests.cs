public class ProductCatalogTests
{
    [Fact]
    public void GroupByCategory_MultipleCategories_GroupsEachCorrectly()
    {
        var products = new List<Product>
        {
            new("Laptop", 75000, ProductCategory.Electronics),
            new("Wireless Mouse", 799, ProductCategory.Electronics),
            new("Novel: Dune", 499, ProductCategory.Books),
            new("Cookbook", 899, ProductCategory.Books),
            new("Office Chair", 6500, ProductCategory.Furniture),
        };

        var groups = ProductCatalog.GroupByCategory(products).ToDictionary(g => g.Key, g => g.ToList());

        Assert.Equal(3, groups.Count);
        Assert.Equal(new[] { "Laptop", "Wireless Mouse" }, groups[ProductCategory.Electronics].Select(p => p.Name));
        Assert.Equal(new[] { "Novel: Dune", "Cookbook" }, groups[ProductCategory.Books].Select(p => p.Name));
        Assert.Equal(new[] { "Office Chair" }, groups[ProductCategory.Furniture].Select(p => p.Name));
    }

    [Fact]
    public void GroupByCategory_EmptyList_ReturnsNoGroups()
    {
        var groups = ProductCatalog.GroupByCategory(new List<Product>());

        Assert.Empty(groups);
    }

    [Fact]
    public void GroupByCategory_SingleProduct_ReturnsOneGroupWithOneItem()
    {
        var products = new List<Product> { new("Solo Item", 10, ProductCategory.Books) };

        var groups = ProductCatalog.GroupByCategory(products).ToList();

        Assert.Single(groups);
        Assert.Equal(ProductCategory.Books, groups[0].Key);
        Assert.Single(groups[0]);
    }

    [Fact]
    public void GroupByCategory_AllSameCategory_ReturnsSingleGroupWithAllItems()
    {
        var products = new List<Product>
        {
            new("Item A", 10, ProductCategory.Electronics),
            new("Item B", 20, ProductCategory.Electronics),
            new("Item C", 30, ProductCategory.Electronics),
        };

        var groups = ProductCatalog.GroupByCategory(products).ToList();

        Assert.Single(groups);
        Assert.Equal(3, groups[0].Count());
    }

    [Fact]
    public void GroupByCategory_NoCategoryIsFabricated_OnlyCategoriesPresentInInputAppear()
    {
        var products = new List<Product> { new("Only Furniture", 100, ProductCategory.Furniture) };

        var groups = ProductCatalog.GroupByCategory(products).Select(g => g.Key).ToList();

        Assert.DoesNotContain(ProductCategory.Electronics, groups);
        Assert.DoesNotContain(ProductCategory.Books, groups);
    }
}
