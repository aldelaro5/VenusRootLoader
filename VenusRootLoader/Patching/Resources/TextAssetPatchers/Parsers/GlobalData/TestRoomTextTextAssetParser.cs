using VenusRootLoader.Api.Leaves;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

/// <inheritdoc/>
internal sealed class TestRoomTextTextAssetParser : ITextAssetParser<TestRoomTextLeaf>
{
    public string GetTextAssetSerializedString(string subPath, TestRoomTextLeaf leaf) =>
        leaf.Text;

    public void FromTextAssetSerializedString(string subPath, string text, TestRoomTextLeaf leaf) =>
        leaf.Text = text;
}