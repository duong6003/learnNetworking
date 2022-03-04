namespace test
{
    public enum ComplexType
    {
        None,
        IsEntity,
        IsCollection,
        IsFile
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class IsComplexAttribute : Attribute
    {
        public IsComplexAttribute(ComplexType type)
        {
            ComplexType = type;
        }
        public ComplexType ComplexType { get; set; }
    }
}
