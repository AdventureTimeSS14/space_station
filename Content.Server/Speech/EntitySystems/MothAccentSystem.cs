﻿using System.Text.RegularExpressions;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed class MothAccentSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MothAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, MothAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // Changed By 1Stepka1 Moth_speech start
        
        message = Regex.Replace(message, "з{1,3}", "ззз");

        message = Regex.Replace(message, "с{1,3}", "зз");

        message = Regex.Replace(message, "ц{1,3}", "зз");

        message = Regex.Replace(message, "ж{1,3}", "жзж");

        message = Regex.Replace(message, "З{1,3}", "ЗЗЗ");

        message = Regex.Replace(message, "С{1,3}", "ЗЗ");

        message = Regex.Replace(message, "Ц{1,3}", "ЗЗ");

        message = Regex.Replace(message, "Ж{1,3}", "ЖЗЖ");

        // Changed By 1Stepka1 Moth_speech end
        
        args.Message = message;
    }
}
