using System.Text;
using UnityEngine;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers.Entities;

internal sealed class AnimIdData : ITextAssetSerializable
{
    internal float ShadowSize { get; set; } = 1.0f;
    internal Vector3 StartScale { get; set; } = Vector3.one;
    internal float BleepPitch { get; set; } = 1.0f;
    internal int BleepId { get; set; }
    internal bool IsModelEntity { get; set; }
    internal Vector3 ModelScale { get; set; }
    internal Vector3 ModelOffset { get; set; }
    internal Vector3 FreezeSize { get; set; }
    internal Vector3 FreezeOffset { get; set; }
    internal Vector3 FreezeFlipOffset { get; set; }
    internal List<AnimIdResourcePreload> PreloadResources { get; } = new();
    internal bool ShakeOnDrop { get; set; }
    internal bool HasDigAnimation { get; set; }
    internal bool DoNotOverrideJump { get; set; }
    internal bool DontFreezeWhenFalling { get; set; }
    internal bool HasNoShadows { get; set; }
    internal EntityControl.WalkType WalkType { get; set; }
    private int UnusedBaseState { get; set; }
    private int UnusedBaseWalk { get; set; }
    internal float MinimumHeight { get; set; }
    private float UnusedStartingHeight { get; set; }
    internal float StartingBobSpeed { get; set; }
    internal float StartingBobRange { get; set; }
    internal bool HasIceAnimation { get; set; }
    internal bool HasNoFlyAnimation { get; set; }
    internal bool ForcesShadow { get; set; }
    internal bool Object { get; set; }

    string ITextAssetSerializable.GetTextAssetSerializedString()
    {
        StringBuilder sb = new();

        sb.Append(ShadowSize);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, StartScale);
        sb.Append(',');
        sb.Append(BleepPitch);
        sb.Append(',');
        sb.Append(BleepId);
        sb.Append(',');
        sb.Append(IsModelEntity);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, ModelScale);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, ModelOffset);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, FreezeSize);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, FreezeOffset);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, FreezeFlipOffset);
        sb.Append(',');

        IEnumerable<string> preloadResources = PreloadResources
            .Cast<ITextAssetSerializable>()
            .Select(p => p.GetTextAssetSerializedString());
        sb.Append(string.Join("?", preloadResources));

        sb.Append(',');
        sb.Append(ShakeOnDrop);
        sb.Append(',');
        sb.Append(HasDigAnimation);
        sb.Append(',');
        sb.Append(DoNotOverrideJump);
        sb.Append(',');
        sb.Append(DontFreezeWhenFalling);
        sb.Append(',');
        sb.Append(HasNoShadows);
        sb.Append(',');
        sb.Append((int)WalkType);
        sb.Append(',');
        sb.Append(UnusedBaseState);
        sb.Append(',');
        sb.Append(UnusedBaseWalk);
        sb.Append(',');
        sb.Append(MinimumHeight);
        sb.Append(',');
        sb.Append(UnusedStartingHeight);
        sb.Append(',');
        sb.Append(StartingBobSpeed);
        sb.Append(',');
        sb.Append(StartingBobRange);
        sb.Append(',');
        sb.Append(HasIceAnimation);
        sb.Append(',');
        sb.Append(HasNoFlyAnimation);
        sb.Append(',');
        sb.Append(ForcesShadow);
        sb.Append(',');
        sb.Append(Object);

        return sb.ToString();
    }

    private static void AppendVector3ToStringBuilder(StringBuilder sb, Vector3 vector)
    {
        sb.Append('(');
        sb.Append(vector.x);
        sb.Append(", ");
        sb.Append(vector.y);
        sb.Append(", ");
        sb.Append(vector.z);
        sb.Append(')');
    }

    void ITextAssetSerializable.FromTextAssetSerializedString(string text)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        ShadowSize = float.Parse(fields[0]);
        StartScale = ParseVector3(fields[1], fields[2], fields[3]);
        BleepPitch = int.Parse(fields[4]);
        BleepId = int.Parse(fields[5]);
        IsModelEntity = bool.Parse(fields[6]);
        ModelScale = ParseVector3(fields[7], fields[8], fields[9]);
        ModelOffset = ParseVector3(fields[10], fields[11], fields[12]);
        FreezeSize = ParseVector3(fields[13], fields[14], fields[15]);
        FreezeOffset = ParseVector3(fields[16], fields[17], fields[18]);
        FreezeFlipOffset = ParseVector3(fields[19], fields[20], fields[21]);

        PreloadResources.Clear();
        string[] preloadResources = fields[22].Split(StringUtils.QuestionMarkSplitDelimiter);
        foreach (string resource in preloadResources)
        {
            ITextAssetSerializable resourcePreload = new AnimIdResourcePreload();
            resourcePreload.FromTextAssetSerializedString(resource);
            PreloadResources.Add((AnimIdResourcePreload)resourcePreload);
        }

        ShakeOnDrop = bool.Parse(fields[23]);
        HasDigAnimation = bool.Parse(fields[24]);
        DoNotOverrideJump = bool.Parse(fields[25]);
        DontFreezeWhenFalling = bool.Parse(fields[26]);
        HasNoShadows = bool.Parse(fields[27]);
        WalkType = (EntityControl.WalkType)int.Parse(fields[28]);
        UnusedBaseState = int.Parse(fields[29]);
        UnusedBaseWalk = int.Parse(fields[30]);
        MinimumHeight = float.Parse(fields[31]);
        UnusedStartingHeight = float.Parse(fields[32]);
        StartingBobSpeed = float.Parse(fields[33]);
        StartingBobRange = float.Parse(fields[34]);
        HasIceAnimation = bool.Parse(fields[35]);
        HasNoFlyAnimation = bool.Parse(fields[36]);
        ForcesShadow = bool.Parse(fields[37]);
        Object = bool.Parse(fields[38]);
    }

    private Vector3 ParseVector3(string x, string y, string z)
    {
        string sanitizedX = x.Remove('(');
        string sanitizedY = y.Trim();
        string sanitizedZ = z.Trim().Remove(')');
        return new(float.Parse(sanitizedX), float.Parse(sanitizedY), float.Parse(sanitizedZ));
    }
}