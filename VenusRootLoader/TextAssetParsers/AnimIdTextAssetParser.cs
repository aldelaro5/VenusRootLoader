using System.Text;
using UnityEngine;
using VenusRootLoader.Api.Leaves;
using VenusRootLoader.Patching.Resources.TextAsset;
using VenusRootLoader.Utility;

namespace VenusRootLoader.TextAssetParsers;

internal sealed class AnimIdTextAssetParser : ITextAssetSerializable<AnimIdLeaf>
{
    public string GetTextAssetSerializedString(string subPath, AnimIdLeaf leaf)
    {
        StringBuilder sb = new();

        sb.Append(leaf.ShadowSize);
        sb.Append(',');
        AppendVector3ToStringBuilder(sb, leaf.StartScale);
        sb.Append(',');
        sb.Append(leaf.BleepPitch);
        sb.Append(',');
        sb.Append(leaf.BleepId);
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

        foreach (AnimIdLeaf.AnimIdResourcePreload animIdResourcePreload in leaf.PreloadResources)
        {
            if (animIdResourcePreload.PreloadOnlyDuringBattles)
                sb.Append('$');
            if (animIdResourcePreload.IsSprite)
                sb.Append('&');
            sb.Append(animIdResourcePreload.ResourcePath);
        }

        sb.Append(',');
        sb.Append(leaf.ShakeOnDrop);
        sb.Append(',');
        sb.Append(leaf.HasDigAnimation);
        sb.Append(',');
        sb.Append(leaf.DoNotOverrideJump);
        sb.Append(',');
        sb.Append(leaf.DontFreezeWhenFalling);
        sb.Append(',');
        sb.Append(leaf.HasNoShadows);
        sb.Append(',');
        sb.Append((int)leaf.WalkType);
        sb.Append(',');
        sb.Append(leaf.UnusedBaseState);
        sb.Append(',');
        sb.Append(leaf.UnusedBaseWalk);
        sb.Append(',');
        sb.Append(leaf.MinimumHeight);
        sb.Append(',');
        sb.Append(leaf.UnusedStartingHeight);
        sb.Append(',');
        sb.Append(leaf.StartingBobSpeed);
        sb.Append(',');
        sb.Append(leaf.StartingBobRange);
        sb.Append(',');
        sb.Append(leaf.HasIceAnimation);
        sb.Append(',');
        sb.Append(leaf.HasNoFlyAnimation);
        sb.Append(',');
        sb.Append(leaf.ForcesShadow);
        sb.Append(',');
        sb.Append(leaf.Object);

        return sb.ToString();
    }

    public void FromTextAssetSerializedString(string subPath, string text, AnimIdLeaf leaf)
    {
        string[] fields = text.Split(StringUtils.CommaSplitDelimiter);

        leaf.ShadowSize = float.Parse(fields[0]);
        leaf.StartScale = ParseVector3(fields[1], fields[2], fields[3]);
        leaf.BleepPitch = int.Parse(fields[4]);
        leaf.BleepId = int.Parse(fields[5]);
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
        leaf.DoNotOverrideJump = bool.Parse(fields[25]);
        leaf.DontFreezeWhenFalling = bool.Parse(fields[26]);
        leaf.HasNoShadows = bool.Parse(fields[27]);
        leaf.WalkType = (EntityControl.WalkType)int.Parse(fields[28]);
        leaf.UnusedBaseState = int.Parse(fields[29]);
        leaf.UnusedBaseWalk = int.Parse(fields[30]);
        leaf.MinimumHeight = float.Parse(fields[31]);
        leaf.UnusedStartingHeight = float.Parse(fields[32]);
        leaf.StartingBobSpeed = float.Parse(fields[33]);
        leaf.StartingBobRange = float.Parse(fields[34]);
        leaf.HasIceAnimation = bool.Parse(fields[35]);
        leaf.HasNoFlyAnimation = bool.Parse(fields[36]);
        leaf.ForcesShadow = bool.Parse(fields[37]);
        leaf.Object = bool.Parse(fields[38]);
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

    private Vector3 ParseVector3(string x, string y, string z)
    {
        string sanitizedX = x.Remove('(');
        string sanitizedY = y.Trim();
        string sanitizedZ = z.Trim().Remove(')');
        return new(float.Parse(sanitizedX), float.Parse(sanitizedY), float.Parse(sanitizedZ));
    }
}