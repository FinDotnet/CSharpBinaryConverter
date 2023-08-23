using System.Text;
using BinaryConverter;

namespace Tester;

public static class Program
{
    public static void Main()
    {
        var myClass = new MyClass
        {
            MyInt = 10,
            MyString = "Good",
            MyEnum = MyEnum.Value1 | MyEnum.Value2,
            MyStruct = new MyStruct
            {
                MyLong = 100,
                MyString = "Morning",
                MyDouble = 3.14,
                MyDecimal = 3.14m,
                MyInt128 = new Int128(ulong.MaxValue, ulong.MinValue)
            },
            MyClass2 = new MyClass2
            {
                MyInt = 20,
                MyString = "World!",
                MyEnum = MyEnum.Value3
            }
        };
        
        var emitter = new BinaryEmitter(myClass);
        var bytes = emitter.Emit(Encoding.UTF8);
        for (var i = 0; i < bytes.Length; i++)
        {
            // Write the bytes to the console in hexadecimal format, new line every 16 bytes
            var b = bytes[i];
            if (i % 16 == 0)
            {
                Console.WriteLine();
            }
            
            Console.Write($"{b:X2} ");
        }

        Console.WriteLine();
        
        var parser = new BinaryParser(bytes);
        var myReadClass = parser.Parse<MyClass>();
        Console.WriteLine(myReadClass);
    }
}

public sealed class MyClass
{
    public required int MyInt { get; init; }
    public required string MyString { get; init; }
    public required MyEnum MyEnum { get; init; }
    public required MyStruct MyStruct { get; init; }
    public required MyClass2 MyClass2 { get; init; }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"MyInt: {MyInt}");
        sb.AppendLine($"MyString: {MyString}");
        sb.AppendLine($"MyEnum: {MyEnum}");
        sb.AppendLine($"MyStruct: {MyStruct}");
        sb.AppendLine($"MyNullableClass: {MyClass2}");
        return sb.ToString();
    }
}

public sealed class MyClass2
{
    public required int MyInt { get; init; }
    public required string MyString { get; init; }
    public required MyEnum MyEnum { get; init; }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"MyInt: {MyInt}");
        sb.AppendLine($"MyString: {MyString}");
        sb.AppendLine($"MyEnum: {MyEnum}");
        return sb.ToString();
    }
}

[Flags]
public enum MyEnum
{
    Value1 = 0x01,
    Value2 = 0x02,
    Value3 = 0x04
}

public struct MyStruct
{
    public required long MyLong { get; init; }
    public required string MyString { get; init; }
    public required double MyDouble { get; init; }
    public required decimal MyDecimal { get; init; }
    public required Int128 MyInt128 { get; init; }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"MyLong: {MyLong}");
        sb.AppendLine($"MyString: {MyString}");
        sb.AppendLine($"MyDouble: {MyDouble}");
        sb.AppendLine($"MyDecimal: {MyDecimal}");
        sb.AppendLine($"MyInt128: {MyInt128}");
        return sb.ToString();
    }
}
