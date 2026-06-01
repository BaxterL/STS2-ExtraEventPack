using ExtraEvents.ExtraEventsCode.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ExtraEvents.ExtraEventsCode.Events;

[RegisterSharedEvent]
public sealed class VoidHand : ModEventTemplate
{
    public override bool IsShared => true;
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: Res.EventPortrait<VoidHand>()
    );

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new MaxHpVar(15),
        new StringVar("Relic", "干瘪之手")
    ];

    public override bool IsAllowed(IRunState runState) => runState.CurrentActIndex == 0;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, ReachOut, InitialOptionKey("REACH_OUT"), HoverTipFactory.FromCardWithCardHoverTips<MagicMissile>()),
        new EventOption(this, Ignore, InitialOptionKey("IGNORE")),
        new EventOption(this, Sever, InitialOptionKey("SEVER")),
    ];

    private async Task ReachOut()
    {
        var card = Owner!.RunState.CreateCard<MagicMissile>(Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 2f);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.OUTREACH.description"));
    }

    private async Task Ignore()
    {
        await CreatureCmd.GainMaxHp(Owner!.Creature, DynamicVars.MaxHp.BaseValue);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.IGNORE.description"));
    }

    private async Task Sever()
    {
        await RelicCmd.Obtain<MummifiedHand>(Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.SEVER.description"));
    }
}
