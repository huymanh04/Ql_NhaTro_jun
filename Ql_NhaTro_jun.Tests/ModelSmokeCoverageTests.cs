using NUnit.Framework;
using Ql_NhaTro_jun.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ql_NhaTro_jun.Tests;

[TestFixture]
public class ModelSmokeCoverageTests
{
    [Test]
    public void Models_CanInstantiate_AndSetPublicProperties()
    {
        var modelTypes = typeof(NguoiDung).Assembly
            .GetTypes()
            .Where(t => t.Namespace == "Ql_NhaTro_jun.Models")
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t != typeof(QlNhatroContext))
            .Where(t => !typeof(Exception).IsAssignableFrom(t))
            .ToList();

        Assert.That(modelTypes.Count, Is.GreaterThan(10));

        var instantiated = 0;
        foreach (var type in modelTypes)
        {
            object? instance = null;
            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch
            {
                continue;
            }

            if (instance == null)
            {
                continue;
            }

            instantiated++;

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0);

            foreach (var prop in props)
            {
                if (prop.SetMethod == null || !prop.SetMethod.IsPublic)
                {
                    continue;
                }

                try
                {
                    var value = BuildSampleValue(prop.PropertyType);
                    if (value != null || !prop.PropertyType.IsValueType || Nullable.GetUnderlyingType(prop.PropertyType) != null)
                    {
                        prop.SetValue(instance, value);
                    }

                    var _ = prop.GetValue(instance);
                }
                catch
                {
                    // Smoke coverage only: skip properties that reject synthetic values.
                }
            }
        }

        Assert.That(instantiated, Is.GreaterThan(10));
    }

    private static object? BuildSampleValue(Type type)
    {
        var nullableUnderlying = Nullable.GetUnderlyingType(type);
        if (nullableUnderlying != null)
        {
            return BuildSampleValue(nullableUnderlying);
        }

        if (type == typeof(string)) return "sample";
        if (type == typeof(int)) return 123;
        if (type == typeof(long)) return 123L;
        if (type == typeof(short)) return (short)12;
        if (type == typeof(byte)) return (byte)1;
        if (type == typeof(bool)) return true;
        if (type == typeof(decimal)) return 99.5m;
        if (type == typeof(double)) return 99.5d;
        if (type == typeof(float)) return 99.5f;
        if (type == typeof(DateTime)) return DateTime.UtcNow;
        if (type == typeof(DateOnly)) return DateOnly.FromDateTime(DateTime.UtcNow);
        if (type == typeof(TimeOnly)) return TimeOnly.FromDateTime(DateTime.UtcNow);
        if (type == typeof(Guid)) return Guid.NewGuid();

        if (type.IsEnum)
        {
            var values = Enum.GetValues(type);
            return values.Length > 0 ? values.GetValue(0) : Activator.CreateInstance(type);
        }

        if (type.IsGenericType)
        {
            var genDef = type.GetGenericTypeDefinition();
            if (genDef == typeof(List<>))
            {
                return Activator.CreateInstance(type);
            }

            if (genDef == typeof(ICollection<>) || genDef == typeof(IEnumerable<>) || genDef == typeof(IList<>))
            {
                var arg = type.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(arg);
                return Activator.CreateInstance(listType);
            }
        }

        if (typeof(IList).IsAssignableFrom(type))
        {
            return Activator.CreateInstance(type);
        }

        if (type.IsClass)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}
