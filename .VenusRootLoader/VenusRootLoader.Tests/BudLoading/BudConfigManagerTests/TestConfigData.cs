using Tomlet.Attributes;
using UnityEngine;

namespace VenusRootLoader.Tests.BudLoading.BudConfigManagerTests;

public sealed class TestConfigData
{
    public enum AnEnum
    {
        A,
        B,
        C,
        D,
        E
    }

    public sealed class NestedThing3
    {
        public int IntValue { get; set; }
    }

    public sealed class NestedThing2
    {
        public int IntValue { get; set; }
        public NestedThing3 NestedThing3 { get; set; } = new();
    }

    public sealed class NestedThing
    {
        public int IntValue { get; set; }
        public NestedThing2 NestedThing2 { get; set; } = new();
    }

    public sealed class TestConfigType
    {
        public int FieldValue;
        public NestedThing? NullableFieldValue;
        public bool BoolValue { get; set; }
        public string StringValue { get; set; } = "";
        public sbyte SByteValue { get; set; }
        public byte ByteValue { get; set; }

        [TomlPrecedingComment("Test some stuff")]
        public short ShortValue { get; set; }

        public ushort UShortValue { get; set; }
        public int IntValue { get; set; }
        public uint UIntValue { get; set; }
        public long LongValue { get; set; }
        public ulong ULongValue { get; set; }
        public float FloatValue { get; set; }
        public double DoubleValue { get; set; }
        public AnEnum AnEnumValue { get; set; }
        public DateTime DateTimeValue { get; set; }
        public TimeSpan TimeSpanValue { get; set; }
        public Vector2 Vector2Value { get; set; }
        public Vector2Int Vector2IntValue { get; set; }
        public Vector3 Vector3Value { get; set; }
        public Vector3Int Vector3IntValue { get; set; }
        public Vector4 Vector4Value { get; set; }

        [TomlProperty("some weird names with spaces!")]
        public Quaternion QuaternionValue { get; set; }

        public Rect RectValue { get; set; }
        public KeyCode KeyCodeValue { get; set; }
        public NestedThing NestedThing { get; set; } = new();
        public IEnumerable<int> IntArrayValue { get; set; } = [515, 21, 20];
        public IEnumerable<NestedThing2> NestedThing2ArrayValue { get; set; } = [new(), new()];

        public Dictionary<string, int> DictionaryValue { get; set; } = new()
        {
            ["A"] = 1,
            ["B"] = 2,
            ["C"] = 3
        };
    }
}