using System.Reflection;
using System.Runtime.InteropServices;

namespace BinaryConverter;

public static class ObjectExtensions
{
    private static readonly Dictionary<Type, bool> CachedTypes;
    
    static ObjectExtensions()
    {
        CachedTypes = new Dictionary<Type, bool>();
    }

    public static bool IsUnmanaged(this Type type)
    {
        if (CachedTypes.TryGetValue(type, out var isUnmanaged))
        {
            return isUnmanaged;
        }

        bool result;
        if (type.IsPrimitive || type.IsPointer || type.IsEnum)
        {
            result = true;
        }
        else if (type.IsGenericType || !type.IsValueType)
        {
            result = false;
        }
        else
        {
            result = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .All(f => IsUnmanaged(f.FieldType));
        }
        
        CachedTypes.Add(type, result);
        return result;
    }
    
    public static int SizeOfDefault(this Type type)
    {
        ;
        if (type.IsUnmanaged())
        {
            if (type.IsEnum)
            {
                return Enum.GetUnderlyingType(type).SizeOfDefault();
            }
            
            return Marshal.SizeOf(type);
        }
        
        if (type == typeof(string))
        {
            return sizeof(int);
        }
        
        if (type.IsArray)
        {
            return sizeof(int) + type.GetElementType()!.SizeOfDefault();
        }
        
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var size = 0;
        foreach (var field in fields)
        {
            size += field.FieldType.SizeOfDefault();
        }

        return size;
    }
}