namespace LogoDesignPortal.Domain.Enums;

/// <summary>
/// Specific design type within a category.
/// Embroidery: LeftChest, JacketBack
/// Vector: SimpleVector, ComplexVector
/// </summary>
public enum DesignType
{
    LeftChest = 1,      // Embroidery - default PKR 350
    JacketBack = 2,     // Embroidery - default PKR 700-800
    SimpleVector = 3,   // Vector - default PKR 350
    ComplexVector = 4   // Vector - custom price required
}
