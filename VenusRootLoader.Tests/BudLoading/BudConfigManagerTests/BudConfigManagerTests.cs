using AwesomeAssertions;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using UnityEngine;
using VenusRootLoader.Api;
using VenusRootLoader.BudLoading;

namespace VenusRootLoader.Tests.BudLoading.BudConfigManagerTests;

public sealed class BudConfigManagerTests
{
    private static readonly string RootPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/" : "C:\\";
    private static readonly string BudsPath = Path.Combine(RootPath, "Buds");
    private static readonly string ConfigPath = Path.Combine(RootPath, "Config");
    private static readonly string LoaderPath = Path.Combine(RootPath, nameof(VenusRootLoader));

    private readonly MockFileSystem _fileSystem = new();

    private readonly BudLoaderContext _budLoaderContext = new()
    {
        BudsPath = BudsPath,
        ConfigPath = ConfigPath,
        LoaderPath = LoaderPath
    };

    private readonly IBudConfigManager _sut;

    public BudConfigManagerTests()
    {
        _fileSystem.Directory.CreateDirectory(BudsPath);
        _fileSystem.Directory.CreateDirectory(ConfigPath);
        _fileSystem.Directory.CreateDirectory(LoaderPath);

        _sut = new BudConfigManager(_fileSystem, _budLoaderContext);
    }

    [Fact]
    public void GetConfigPathForBud_ReturnsCorrectConfigPath_WhenReceivingBudId()
    {
        const string budId = "SomeBudId";

        string configPath = _sut.GetConfigPathForBud(budId);

        configPath.Should().Be(Path.Combine(ConfigPath, $"{budId}.toml"));
    }

    [Fact]
    public Task Save_SerializesConfigDataCorrectly_WhenBudConfigDataIsNotNull()
    {
        const string budId = "SomeBudId";

        TestConfigData.TestConfigType defaultTestConfigData = new()
        {
            FieldValue = 975,
            NullableFieldValue = null,
            BoolValue = true,
            StringValue = "stuff",
            SByteValue = -2,
            ByteValue = 7,
            ShortValue = -1459,
            UShortValue = 5154,
            IntValue = -1025545,
            UIntValue = 525454,
            LongValue = -9454525154,
            ULongValue = 415452124,
            FloatValue = 2.694f,
            DoubleValue = 845.474,
            AnEnumValue = TestConfigData.AnEnum.C,
            DateTimeValue = new DateTime(2026, 10, 1, 11, 50, 39, 584, DateTimeKind.Utc),
            TimeSpanValue = TimeSpan.FromHours(17),
            Vector2Value = new(25.14f, 48.26f),
            Vector2IntValue = new(8, 98),
            Vector3Value = new(58.948f, 487.215f, 9625.15f),
            Vector3IntValue = new(78, 874, 96),
            Vector4Value = new(84754.484f, 48457.9857f, 4852.184f, 8472.695f),
            QuaternionValue = new(844854.821f, 48557.4548f, 9615.48f, 8712.45f),
            RectValue = new(48845.151f, 4845.154f, 54548.154f, 4847.542f),
            KeyCodeValue = KeyCode.E,
            NestedThing = null!
        };

        TestConfigData.TestConfigType testConfigData = new()
        {
            FieldValue = 521,
            NullableFieldValue = new() { IntValue = 5 },
            BoolValue = false,
            StringValue = "more stuff",
            SByteValue = -5,
            ByteValue = 58,
            ShortValue = -2475,
            UShortValue = 4875,
            IntValue = -1475545,
            UIntValue = 527454,
            LongValue = -9454845154,
            ULongValue = 4157485124,
            FloatValue = 5.0f,
            DoubleValue = 800.0d,
            AnEnumValue = TestConfigData.AnEnum.D,
            DateTimeValue = new DateTime(2026, 10, 3, 11, 50, 39, 584, DateTimeKind.Utc),
            TimeSpanValue = TimeSpan.FromHours(21),
            Vector2Value = new(25.0f, 48.0f),
            Vector2IntValue = new(87, 47),
            Vector3Value = new(58.0f, 487.0f, 9625.0f),
            Vector3IntValue = new(47, 475, 961),
            Vector4Value = new(84754.0f, 48457.0f, 4852.0f, 8472.0f),
            QuaternionValue = new(844854.0f, 48557.0f, 9615.0f, 8712.0f),
            RectValue = new(48845.0f, 4845.0f, 54548.0f, 4847.0f),
            KeyCodeValue = KeyCode.X,
            NestedThing = new()
            {
                IntValue = -98,
                NestedThing2 = new()
                {
                    IntValue = 147,
                    NestedThing3 = new() { IntValue = 20 }
                }
            },
            DictionaryValue = new() { ["F"] = 84 },
            IntArrayValue = [974, 4852, 4852],
            NestedThing2ArrayValue = [new(), new() { IntValue = 85 }]
        };

        _sut.Save(budId, typeof(TestConfigData.TestConfigType), testConfigData, defaultTestConfigData);

        string expectedConfigFilePath = Path.Combine(ConfigPath, $"{budId}.toml");
        _fileSystem.File.Exists(expectedConfigFilePath).Should().BeTrue();
        string actualFile = _fileSystem.File.ReadAllText(expectedConfigFilePath);
        return Verify(actualFile);
    }

    [Fact]
    public void Save_ThrowsFormatException_WhenBudConfigDataIsNull()
    {
        const string budId = "SomeBudId";

        TestConfigData.TestConfigType defaultTestConfigData = new()
        {
            FieldValue = 975,
            NullableFieldValue = null,
            BoolValue = true,
            StringValue = "stuff",
            SByteValue = -2,
            ByteValue = 7,
            ShortValue = -1459,
            UShortValue = 5154,
            IntValue = -1025545,
            UIntValue = 525454,
            LongValue = -9454525154,
            ULongValue = 415452124,
            FloatValue = 2.694f,
            DoubleValue = 845.474,
            AnEnumValue = TestConfigData.AnEnum.C,
            DateTimeValue = new DateTime(2026, 10, 1, 11, 50, 39, 584, DateTimeKind.Utc),
            TimeSpanValue = TimeSpan.FromHours(17),
            Vector2Value = new(25.14f, 48.26f),
            Vector2IntValue = new(8, 98),
            Vector3Value = new(58.948f, 487.215f, 9625.15f),
            Vector3IntValue = new(78, 874, 96),
            Vector4Value = new(84754.484f, 48457.9857f, 4852.184f, 8472.695f),
            QuaternionValue = new(844854.821f, 48557.4548f, 9615.48f, 8712.45f),
            RectValue = new(48845.151f, 4845.154f, 54548.154f, 4847.542f),
            KeyCodeValue = KeyCode.E,
            NestedThing = new()
        };

        TestConfigData.TestConfigType testConfigData = null!;

        _sut.Invoking(sut => sut.Save(
                budId,
                typeof(TestConfigData.TestConfigType),
                testConfigData,
                defaultTestConfigData))
            .Should().Throw<FormatException>();
    }

    [Fact]
    public void Load_DeserializesSameObject_WhenConfigFileExists()
    {
        const string budId = "SomeBudId";

        TestConfigData.TestConfigType defaultTestConfigData = new()
        {
            FieldValue = 975,
            NullableFieldValue = null,
            BoolValue = true,
            StringValue = "stuff",
            SByteValue = -2,
            ByteValue = 7,
            ShortValue = -1459,
            UShortValue = 5154,
            IntValue = -1025545,
            UIntValue = 525454,
            LongValue = -9454525154,
            ULongValue = 415452124,
            FloatValue = 2.694f,
            DoubleValue = 845.474,
            AnEnumValue = TestConfigData.AnEnum.C,
            DateTimeValue = new DateTime(2026, 10, 1, 11, 50, 39, 584, DateTimeKind.Utc),
            TimeSpanValue = TimeSpan.FromHours(17),
            Vector2Value = new(25.14f, 48.26f),
            Vector2IntValue = new(8, 98),
            Vector3Value = new(58.948f, 487.215f, 9625.15f),
            Vector3IntValue = new(78, 874, 96),
            Vector4Value = new(84754.484f, 48457.9857f, 4852.184f, 8472.695f),
            QuaternionValue = new(844854.821f, 48557.4548f, 9615.48f, 8712.45f),
            RectValue = new(48845.151f, 4845.154f, 54548.154f, 4847.542f),
            KeyCodeValue = KeyCode.E,
            NestedThing = new()
        };

        TestConfigData.TestConfigType testConfigData = new()
        {
            FieldValue = 521,
            NullableFieldValue = new() { IntValue = 5 },
            BoolValue = false,
            StringValue = "more stuff",
            SByteValue = -5,
            ByteValue = 58,
            ShortValue = -2475,
            UShortValue = 4875,
            IntValue = -1475545,
            UIntValue = 527454,
            LongValue = -9454845154,
            ULongValue = 4157485124,
            FloatValue = 5.0f,
            DoubleValue = 800.0d,
            AnEnumValue = TestConfigData.AnEnum.D,
            DateTimeValue = new DateTime(2026, 10, 3, 11, 50, 39, 584, DateTimeKind.Utc),
            TimeSpanValue = TimeSpan.FromHours(21),
            Vector2Value = new(25.0f, 48.0f),
            Vector2IntValue = new(87, 47),
            Vector3Value = new(58.0f, 487.0f, 9625.0f),
            Vector3IntValue = new(47, 475, 961),
            Vector4Value = new(84754.0f, 48457.0f, 4852.0f, 8472.0f),
            QuaternionValue = new(844854.0f, 48557.0f, 9615.0f, 8712.0f),
            RectValue = new(48845.0f, 4845.0f, 54548.0f, 4847.0f),
            KeyCodeValue = KeyCode.X,
            NestedThing = new()
            {
                IntValue = -98,
                NestedThing2 = new()
                {
                    IntValue = 147,
                    NestedThing3 = new() { IntValue = 20 }
                }
            },
            DictionaryValue = new() { ["F"] = 84 },
            IntArrayValue = [974, 4852, 4852],
            NestedThing2ArrayValue = [new(), new() { IntValue = 85 }]
        };

        _sut.Save(budId, typeof(TestConfigData.TestConfigType), testConfigData, defaultTestConfigData);
        object configObject = _sut.Load(budId, typeof(TestConfigData.TestConfigType));
        configObject.Should().BeOfType<TestConfigData.TestConfigType>();

        TestConfigData.TestConfigType config = (TestConfigData.TestConfigType)configObject;
        config.Should().BeEquivalentTo(testConfigData);
    }
}