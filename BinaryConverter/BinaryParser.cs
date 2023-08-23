using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace BinaryConverter;

public sealed class BinaryParser
{
    private readonly MemoryStream _stream;
    
    public BinaryParser(byte[] bytes)
    {
        _stream = new MemoryStream(bytes);
    }

    /// <summary>
    /// Reads a byte array, and constructs an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"> The type of the object that is to be read from the byte array </typeparam>
    /// <returns> The constructed object </returns>
    public T Parse<T>()
    {
        return (T)ReadObject(typeof(T));
    }

    private unsafe object ReadObject(Type type)
    {
        if (_stream.Position >= _stream.Length)
        {
            throw new Exception("Reached the end of the stream.");
        }
        
        if (type.IsUnmanaged())
        {
            if (type.IsEnum)
            {
                var enumType = Enum.GetUnderlyingType(type);
                var value = ReadObject(enumType);
                return Enum.ToObject(type, value);
            }

            // Get the size of the primitive type
            var size = Marshal.SizeOf(type);
            var bytes = new byte[size];
            var readLength = _stream.Read(bytes, 0, size);
            if (readLength != size)
            {
                throw new Exception($"Failed to read {size} bytes from stream");
            }

            // Convert the bytes to the value type
            fixed (byte* ptr = bytes)
            {
                var valueType = Marshal.PtrToStructure(new IntPtr(ptr), type);
                if (valueType is null)
                {
                    throw new Exception($"Failed to convert bytes to {type.Name}");
                }
                
                return valueType;
            }
        }

        if (type == typeof(string))
        {
            var length = (int)ReadObject(typeof(int));
            var bytes = new byte[length];
            var readLength = _stream.Read(bytes, 0, length);
            if (readLength != length)
            {
                throw new Exception($"Failed to read {length} bytes from stream");
            }
            
            var str = Encoding.UTF8.GetString(bytes);
            return str;
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (elementType is null)
            {
                throw new Exception($"Failed to get element type of array type {type.Name}");
            }
            
            var length = (int)ReadObject(typeof(int));
            var array = Array.CreateInstance(elementType, length);
            for (var i = 0; i < length; i++)
            {
                array.SetValue(ReadObject(elementType), i);
            }
            
            return array;
        }

        // Create an instance of the type
        var instance = Activator.CreateInstance(type);
        if (instance is null)
        {
            throw new Exception($"Failed to create instance of type {type.Name}");
        }
        
        // Get all non-static fields
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            var fieldType = field.FieldType;
            var fieldValue = ReadObject(fieldType);
            field.SetValue(instance, fieldValue);
        }

        return instance;
    }
}