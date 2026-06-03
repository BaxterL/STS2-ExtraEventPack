using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExtraEvents.ExtraEventsCode.Enchantments;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ExtraEvents.ExtraEventsCode.Events;

[RegisterSharedEvent]
public sealed class ShadowTadpole : ModEventTemplate
{
    public override bool IsShared => true;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new GoldVar(110),
        new CardsVar(1),
        new StringVar("Enchantment", "寄生")
    ];

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: Res.EventPortrait<ShadowTadpole>()
    );

    public override bool IsAllowed(IRunState runState)
    {
        return runState.CurrentActIndex == 0;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var pile = PileType.Deck.GetPile(Owner!);
        var embraceOption = pile.Cards.Any(c => CanEnchant(c))
            ? new EventOption(this, Embrace, InitialOptionKey("EMBRACE"), HoverTipFactory.FromEnchantment<Parasitism>().ToArray())
            : new EventOption(this, null, "EXTRA_EVENTS_EVENT_SHADOW_TADPOLE.pages.INITIAL.options.EMBRACE_LOCKED");
        var rejectOption = new EventOption(this, Reject, InitialOptionKey("REJECT"));

        return new[] { embraceOption, rejectOption };
    }

    private async Task Embrace()
    {
        var cards = (await CardSelectCmd.FromDeckForEnchantment(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, DynamicVars.Cards.IntValue),
            player: Owner!,
            enchantment: ModelDb.Enchantment<Parasitism>(),
            amount: 1
        )).ToList();

        foreach (var card in cards)
        {
            CardCmd.Enchant(ModelDb.Enchantment<Parasitism>().ToMutable(), card, 1m);
            var vfx = NCardEnchantVfx.Create(card);
            if (vfx != null)
            {
                NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(vfx);
            }
        }

        SetEventFinished(L10NLookup($"{Id.Entry}.pages.EMBRACE.description"));
    }

    private async Task Reject()
    {
        await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.REJECT.description"));
    }

    private static bool CanEnchant(CardModel card)
    {
        return ModelDb.Enchantment<Parasitism>().CanEnchant(card);
    }
}
