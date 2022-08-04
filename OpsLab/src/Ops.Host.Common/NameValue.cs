namespace Ops.Host.Common;

public class NameValue<TKey, TValue>
{
    public TKey Name { get; }

    public TValue Value { get; }

    public NameValue(TKey name, TValue value)
    {
        Name = name;
        Value = value;
    }
}

public class NameValue : NameValue<string, string>
{
    public NameValue(string name, string value) : base(name, value)
    {

    }
}
