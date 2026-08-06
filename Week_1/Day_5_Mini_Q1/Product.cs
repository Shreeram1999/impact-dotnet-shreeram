public enum ProductCategory
{
    Electronics,
    Books,
    Furniture
}

public record Product(string Name, double Price, ProductCategory Category);
