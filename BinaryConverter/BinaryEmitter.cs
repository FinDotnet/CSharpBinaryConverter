using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace BinaryConverter;

/// <summary>
/// This class is used to convert an object to it's flat binary representation.
/// </summary>
public sealed class BinaryEmitter
{
    private readonly object _object;
    private readonly MemoryStream _stream;
    
    public BinaryEmitter(object obj)
    {
        _object = obj;
        _stream = new MemoryStream();
    }

    /// <summary>
    /// /// Converts the object to it's flat binary representation.
    /// </summary>
    /// <param name="encoding"> The encoding used to serialize strings. </param>
    /// <returns> The flat binary representation of the object </returns>
    public byte[] Emit(Encoding encoding)
    {
        if (encoding == null)
        {
            throw new ArgumentNullException(nameof(encoding));
        }
        
        WriteObject(_object, encoding);
        return _stream.ToArray();
    }

    private unsafe void WriteObject(object value, Encoding encoding)
    {
        var type = value.GetType();
        if (type.IsUnmanaged())
        {
            if (type.IsEnum)
            {
                var enumValue = (Enum)value;
                var enumType = Enum.GetUnderlyingType(type);
                var convertedValue = Convert.ChangeType(enumValue, enumType);
                WriteObject(convertedValue, encoding);
                return;
            }
            
            var valueType = (ValueType)value;
            // Get the size of the primitive type
            var size = Marshal.SizeOf(type);
            var bytes = stackalloc byte[size];
            Marshal.StructureToPtr(valueType, (nint)bytes, true);

            // Write the bytes to the stream
            _stream.Write(new ReadOnlySpan<byte>(bytes, size));
            return;
        }
        
        if (type == typeof(string))
        {
            var str = (string)value;
            var bytes = encoding.GetBytes(str);
            var length = bytes.Length;
            WriteObject(length, encoding);
            _stream.Write(bytes, 0, length);
            return;
        }
        
        if (type.IsArray)
        {
            var array = (Array)value;
            var length = array.Length;
            WriteObject(length, encoding);
            foreach (var item in array)
            {
                WriteObject(item, encoding);
            }
            
            return;
        }
        
        // Get all non-static fields
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            var fieldValue = field.GetValue(value);
            if (fieldValue == null)
            {
                throw new Exception($"Failed to get value of field {field.Name}");
            }
            
            WriteObject(fieldValue, encoding);
        }
    }
}