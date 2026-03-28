using System.Globalization;
using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Registry;
using VenusRootLoader.Utility;

namespace VenusRootLoader.Patching.Resources.TextAssetPatchers.Parsers.GlobalData;

internal sealed class AnimIdTextAssetParser : ITextAssetParser<AnimIdLeaf>
{
    private readonly ILeavesRegistry<DialogueBleepLeaf> _bleepsRegistry;

    public AnimIdTextAssetParser(ILeavesRegistry<DialogueBleepLeaf> bleepsRegistry)
    {
        _bleepsRegistry = bleepsRegistry;
    }

    public string GetTextAssetSerializedString(string subPath, AnimIdLeaf leaf)
    {
        StringBuilder sb = new();

        sb.Append(leaf.ShadowSize);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, leaf.StartingScale);
        sb.Append(',');
        sb.Append(leaf.BleepPitch);
        sb.Append(',');
        sb.Append(leaf.BleepId.GameId);
        sb.Append(',');
        sb.Append(leaf.IsModelEntity);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, leaf.ModelScale);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, leaf.ModelOffset);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, leaf.FreezeSize);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, leaf.FreezeOffset);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, leaf.FreezeFlipOffset);
        sb.Append(',');

        IEnumerable<string> preloadResourcesStrings = leaf.PreloadResources
            .Select(FormatResourcePreload);
        sb.Append(string.Join("?", preloadResourcesStrings));

        sb.Append(',');
        sb.Append(leaf.ShakeOnDrop);
        sb.Append(',');
        sb.Append(leaf.HasDigAnimation);
        sb.Append(',');
        sb.Append(!leaf.HasJumpAnimationOverride);
        sb.Append(',');
        sb.Append(!leaf.FallsWhenFrozen);
        sb.Append(',');
        sb.Append(!leaf.HasShadow);
        sb.Append(',');
        sb.Append((int)leaf.WalkType);
        sb.Append(',');
        sb.Append(leaf.UnusedBaseIdleAnimState);
        sb.Append(',');
        sb.Append(leaf.UnusedBaseWalkAnimState);
        sb.Append(',');
        sb.Append(leaf.MinimumHeight);
        sb.Append(',');
        sb.Append(leaf.UnusedStartingHeight);
        sb.Append(',');
        sb.Append(leaf.StartingBobSpeed);
        sb.Append(',');
        sb.Append(leaf.StartingBobFrequency);
        sb.Append(',');
        sb.Append(leaf.HasIceAnimation);
        sb.Append(',');
        sb.Append(leaf.HasFlyingAnimationOverride);
        sb.Append(',');
        sb.Append(leaf.ForcesShadow);
        sb.Append(',');
        sb.Append(leaf.Object);

        return sb.ToString();
    }

    private static string FormatResourcePreload(AnimIdLeaf.AnimIdResourcePreload pr)
    {
        StringBuilder sbPreloadResource = new();
        if (pr.PreloadOnlyDuringBattles)
            sbPreloadResource.Append('$');
        if (pr.IsSprite)
            sbPreloadResource.Append('&');
        sbPreloadResource.Append(pr.ResourcePath);
        return sbPreloadResource.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, AnimIdLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        leaf.ShadowSize = float.Parse(fields[0], CultureInfo.InvariantCulture);
        leaf.StartingScale = ParseVector3(fields[1], fields[2], fields[3]);
        leaf.BleepPitch = float.Parse(fields[4], CultureInfo.InvariantCulture);
        leaf.BleepId = new(_bleepsRegistry.LeavesByGameIds[int.Parse(fields[5], CultureInfo.InvariantCulture)]);
        leaf.IsModelEntity = bool.Parse(fields[6]);
        leaf.ModelScale = ParseVector3(fields[7], fields[8], fields[9]);
        leaf.ModelOffset = ParseVector3(fields[10], fields[11], fields[12]);
        leaf.FreezeSize = ParseVector3(fields[13], fields[14], fields[15]);
        leaf.FreezeOffset = ParseVector3(fields[16], fields[17], fields[18]);
        leaf.FreezeFlipOffset = ParseVector3(fields[19], fields[20], fields[21]);

        leaf.PreloadResources.Clear();
        string[] preloadResources = fields[22].Split(StringUtils.QuestionMarkSplitDelimiter);
        foreach (string resource in preloadResources)
        {
            AnimIdLeaf.AnimIdResourcePreload resourcePreload = new();

            StringBuilder sb = new(resource);
            if (sb.Length > 0 && sb[0] == '$')
            {
                resourcePreload.PreloadOnlyDuringBattles = true;
                sb.Remove(0, 1);
            }

            if (sb.Length > 0 && sb[0] == '&')
            {
                resourcePreload.IsSprite = true;
                sb.Remove(0, 1);
            }

            resourcePreload.ResourcePath = sb.ToString();
            leaf.PreloadResources.Add(resourcePreload);
        }

        leaf.ShakeOnDrop = bool.Parse(fields[23]);
        leaf.HasDigAnimation = bool.Parse(fields[24]);
        leaf.HasJumpAnimationOverride = !bool.Parse(fields[25]);
        leaf.FallsWhenFrozen = !bool.Parse(fields[26]);
        leaf.HasShadow = !bool.Parse(fields[27]);
        leaf.WalkType = (EntityControl.WalkType)int.Parse(fields[28], CultureInfo.InvariantCulture);
        leaf.UnusedBaseIdleAnimState = int.Parse(fields[29], CultureInfo.InvariantCulture);
        leaf.UnusedBaseWalkAnimState = int.Parse(fields[30], CultureInfo.InvariantCulture);
        leaf.MinimumHeight = float.Parse(fields[31], CultureInfo.InvariantCulture);
        leaf.UnusedStartingHeight = float.Parse(fields[32], CultureInfo.InvariantCulture);
        leaf.StartingBobSpeed = float.Parse(fields[33], CultureInfo.InvariantCulture);
        leaf.StartingBobFrequency = float.Parse(fields[34], CultureInfo.InvariantCulture);
        leaf.HasIceAnimation = bool.Parse(fields[35]);
        leaf.HasFlyingAnimationOverride = bool.Parse(fields[36]);
        leaf.ForcesShadow = bool.Parse(fields[37]);
        leaf.Object = bool.Parse(fields[38]);
    }

    private static void AppendVector3ToStringBuilder(StringBuilder sb, Vector3 vector)
    {
        sb.Append('(');
        sb.Append(FormatVectorComponent(vector.x));
        sb.Append(", ");
        sb.Append(FormatVectorComponent(vector.y));
        sb.Append(", ");
        sb.Append(FormatVectorComponent(vector.z));
        sb.Append(')');
    }

    private static string FormatVectorComponent(float component) =>
        Math.Truncate(component).Equals(component)
            ? component.ToString("N1", CultureInfo.InvariantCulture)
            : component.ToString(CultureInfo.InvariantCulture);

    private Vector3 ParseVector3(string x, string y, string z)
    {
        string sanitizedX = x.Replace("(", "");
        string sanitizedY = y.Trim();
        string sanitizedZ = z.Trim().Replace(")", "");
        return new(
            float.Parse(sanitizedX, CultureInfo.InvariantCulture),
            float.Parse(sanitizedY, CultureInfo.InvariantCulture),
            float.Parse(sanitizedZ, CultureInfo.InvariantCulture));
    }
}