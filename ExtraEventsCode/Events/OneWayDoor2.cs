using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExtraEvents.ExtraEventsCode.Cards;
using ExtraEvents.ExtraEventsCode.Relics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ExtraEvents.ExtraEventsCode.Events;

[RegisterSharedEvent]
public sealed class OneWayDoor2 : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: Res.EventPortrait<OneWayDoor>()
    );

    public override bool IsAllowed(IRunState runState)
    {
        return runState.CurrentActIndex >= 1;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new EventOption[]
        {
            new EventOption(this, BreakSeal, InitialOptionKey("BREAK_SEAL"),
                HoverTipFactory.FromRelic<CalamityRing>()
                    .Concat(HoverTipFactory.FromCardWithCardHoverTips<DispellingStone>())
                    .ToArray()),
            new EventOption(this, RemoveAndLeave, InitialOptionKey("REMOVE_AND_LEAVE")),
        };
    }

    private async Task BreakSeal()
    {
        await RelicCmd.Obtain<CalamityRing>(Owner!);
        var card = Owner!.RunState.CreateCard<DispellingStone>(Owner);
        await CardPileCmd.Add(card, PileType.Deck);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.BREAK_SEAL.description"));
    }

    private async Task RemoveAndLeave()
    {
        var cards = (await CardSelectCmd.FromDeckForRemoval(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1),
            player: Owner!
        )).ToList();

        if (cards.Count > 0)
            await CardPileCmd.RemoveFromDeck(cards);

        SetEventFinished(L10NLookup($"{Id.Entry}.pages.REMOVE_AND_LEAVE.description"));
    }
}
