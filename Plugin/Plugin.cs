using BepInEx;
using BepInEx.Logging;
using BepInEx.NET.Common;
using BepInExResoniteShim;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using SkyFrost.Base;

namespace MessageCopy;

[ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
[BepInDependency(BepInExResoniteShim.PluginMetadata.GUID, BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BasePlugin
{
#nullable disable
    internal static new ManualLogSource Log;
#nullable enable

    public override void Load()
    {
        Log = base.Log;
        HarmonyInstance.PatchAll();
    }
      [HarmonyPatch(typeof(ContactsDialog), "AddMessage")]
    class MessageCopyPatch
    {
        public static void Postfix(Image __result, UIBuilder ___messagesUi, Message message)
        {

            string copyText = "";

            switch (message.MessageType)
            {
                case SkyFrost.Base.MessageType.Text:
                    // handled in vanilla game
                    return;
                case SkyFrost.Base.MessageType.Sound:
                case SkyFrost.Base.MessageType.Object:
                    Record record = message.ExtractContent<Record>();
                    copyText = record.AssetURI;
                    break;
                case SkyFrost.Base.MessageType.SessionInvite:
                    SessionInfo sessionInfo = message.ExtractContent<SessionInfo>();
                    copyText = sessionInfo.SessionId + "\n" + string.Join("\n", sessionInfo.SessionURLs);
                    break;
                case SkyFrost.Base.MessageType.InviteRequest:
                    return; // Not sure what this should do
                default:
                    Log.LogWarning("Unknown message type: " + message.MessageType);
                    return;
            }

            Slot copyButton = __result.Slot.Children.Last().AddSlot("Copy");
            ___messagesUi.NestInto(copyButton);
            Button button = ___messagesUi.Button(OfficialAssets.Graphics.Icons.Inspector.Duplicate);
            ___messagesUi.NestOut();
            button.LocalPressed += (b,e) =>
            {
                Engine.Current.InputInterface.Clipboard.SetText(copyText);
            };

            var isInvite = message.MessageType == SkyFrost.Base.MessageType.SessionInvite;
            var isSound = message.MessageType == SkyFrost.Base.MessageType.Sound;

            if (message.IsSent && !isInvite && !isSound)
            {
                float2 position = new float2(2f);
                float2 v = float2.One;
                float2 size = v * 24;
                Rect rect = new Rect(in position, in size);
                float2 anchor = new float2(0f, 0f);
                button.RectTransform.SetFixedRect(rect, in anchor);
            }
            else
            {
                float2 position = new float2(-26f, isInvite ? -26f : 0);
                float2 v = float2.One;
                float2 size = v * 24;
                Rect rect2 = new Rect(in position, in size);
                float2 anchor = new float2(1f, isInvite ? 1 : 0);
                button.RectTransform.SetFixedRect(rect2, in anchor);
            }
            var hoverArea = __result.Slot.GetComponent<HoverArea>();
            copyButton.ActiveSelf_Field.DriveFrom(hoverArea.IsHovering);
        }
    }
}
