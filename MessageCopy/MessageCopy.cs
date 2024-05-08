using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using FrooxEngine;
using FrooxEngine.UIX;
using SkyFrost.Base;
using Elements.Core;

namespace MessageCopy;

public class MessageCopy : ResoniteMod
{
    public override string Name => "MessageCopy";
    public override string Author => "art0007i";
    public override string Version => "1.0.0";
    public override string Link => "https://github.com/art0007i/MessageCopy/";
    public override void OnEngineInit()
    {
        Harmony harmony = new Harmony("me.art0007i.MessageCopy");
        harmony.PatchAll();

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
