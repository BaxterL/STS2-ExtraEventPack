using ExtraEvents.ExtraEventsCode.Cards;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ExtraEvents.ExtraEventsCode.Events;

[RegisterSharedEvent]
public sealed class InfiniteGold : ModEventTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new GoldVar(999),
        new GoldVar("PartialGold", 333),
    ];

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: Res.EventPortrait<InfiniteGold>()
    );

    public override bool IsAllowed(IRunState runState) => true;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, TakeAll, InitialOptionKey("TAKE_ALL"), HoverTipFactory.FromCardWithCardHoverTips<Supernormality>()),
        new EventOption(this, TakeSome, InitialOptionKey("TAKE_SOME"), HoverTipFactory.FromCardWithCardHoverTips<Greed>()),
    ];

    private async Task TakeAll()
    {
        await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner!);
        var curse = Owner!.RunState.CreateCard<Supernormality>(Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(curse, PileType.Deck), 2f);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.TAKE_ALL.description"));
    }

    private async Task TakeSome()
    {
        await PlayerCmd.GainGold(DynamicVars["PartialGold"].BaseValue, Owner!);
        var curse = Owner!.RunState.CreateCard<Greed>(Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(curse, PileType.Deck), 2f);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.TAKE_SOME.description"));
    }
}
