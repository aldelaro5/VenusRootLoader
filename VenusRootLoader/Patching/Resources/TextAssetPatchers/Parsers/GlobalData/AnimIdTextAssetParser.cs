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

    public string GetTextAssetSerializedString(string subPath, AnimIdLeaf value)
    {
        StringBuilder sb = new();

        sb.Append(value.ShadowSize);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, value.StartingScale);
        sb.Append(',');
        sb.Append(value.BleepPitch);
        sb.Append(',');
        sb.Append(value.BleepId.GameId);
        sb.Append(',');
        sb.Append(value.IsModelEntity);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, value.ModelScale);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, value.ModelOffset);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, value.FreezeSize);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, value.FreezeOffset);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, value.FreezeFlipOffset);
        sb.Append(',');

        IEnumerable<string> preloadResourcesStrings = value.PreloadResources
            .Select(FormatResourcePreload);
        sb.Append(string.Join("?", preloadResourcesStrings));

        sb.Append(',');
        sb.Append(value.ShakeOnDrop);
        sb.Append(',');
        sb.Append(value.HasDigAnimation);
        sb.Append(',');
        sb.Append(!value.HasJumpAnimationOverride);
        sb.Append(',');
        sb.Append(!value.FallsWhenFrozen);
        sb.Append(',');
        sb.Append(!value.HasShadow);
        sb.Append(',');
        sb.Append((int)value.WalkType);
        sb.Append(',');
        sb.Append(value.UnusedBaseIdleAnimState);
        sb.Append(',');
        sb.Append(value.UnusedBaseWalkAnimState);
        sb.Append(',');
        sb.Append(value.MinimumHeight);
        sb.Append(',');
        sb.Append(value.UnusedStartingHeight);
        sb.Append(',');
        sb.Append(value.StartingBobSpeed);
        sb.Append(',');
        sb.Append(value.StartingBobFrequency);
        sb.Append(',');
        sb.Append(value.HasIceAnimation);
        sb.Append(',');
        sb.Append(value.HasFlyingAnimationOverride);
        sb.Append(',');
        sb.Append(value.ForcesShadow);
        sb.Append(',');
        sb.Append(value.Object);

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

    public void FromTextAssetSerializedString(string subPath, string text, AnimIdLeaf value)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        value.ShadowSize = float.Parse(fields[0], CultureInfo.InvariantCulture);
        value.StartingScale = ParseVector3(fields[1], fields[2], fields[3]);
        value.BleepPitch = float.Parse(fields[4], CultureInfo.InvariantCulture);
        value.BleepId = new(_bleepsRegistry.LeavesByGameIds[int.Parse(fields[5], CultureInfo.InvariantCulture)]);
        value.IsModelEntity = bool.Parse(fields[6]);
        value.ModelScale = ParseVector3(fields[7], fields[8], fields[9]);
        value.ModelOffset = ParseVector3(fields[10], fields[11], fields[12]);
        value.FreezeSize = ParseVector3(fields[13], fields[14], fields[15]);
        value.FreezeOffset = ParseVector3(fields[16], fields[17], fields[18]);
        value.FreezeFlipOffset = ParseVector3(fields[19], fields[20], fields[21]);

        value.PreloadResources.Clear();
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
            value.PreloadResources.Add(resourcePreload);
        }

        value.ShakeOnDrop = bool.Parse(fields[23]);
        value.HasDigAnimation = bool.Parse(fields[24]);
        value.HasJumpAnimationOverride = !bool.Parse(fields[25]);
        value.FallsWhenFrozen = !bool.Parse(fields[26]);
        value.HasShadow = !bool.Parse(fields[27]);
        value.WalkType = (EntityControl.WalkType)int.Parse(fields[28], CultureInfo.InvariantCulture);
        value.UnusedBaseIdleAnimState = int.Parse(fields[29], CultureInfo.InvariantCulture);
        value.UnusedBaseWalkAnimState = int.Parse(fields[30], CultureInfo.InvariantCulture);
        value.MinimumHeight = float.Parse(fields[31], CultureInfo.InvariantCulture);
        value.UnusedStartingHeight = float.Parse(fields[32], CultureInfo.InvariantCulture);
        value.StartingBobSpeed = float.Parse(fields[33], CultureInfo.InvariantCulture);
        value.StartingBobFrequency = float.Parse(fields[34], CultureInfo.InvariantCulture);
        value.HasIceAnimation = bool.Parse(fields[35]);
        value.HasFlyingAnimationOverride = bool.Parse(fields[36]);
        value.ForcesShadow = bool.Parse(fields[37]);
        value.Object = bool.Parse(fields[38]);
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