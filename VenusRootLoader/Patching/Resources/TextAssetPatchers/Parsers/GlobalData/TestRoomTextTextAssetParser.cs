using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class TestRoomTextTextAssetParser : ITextAssetParser<TestRoomTextLeaf>
{
    public string GetTextAssetSerializedString(string subPath, TestRoomTextLeaf value) =>
        value.Text;

    public void FromTextAssetSerializedString(string subPath, string text, TestRoomTextLeaf value) =>
        value.Text = text;
}