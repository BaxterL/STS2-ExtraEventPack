using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ExtraEvents.ExtraEventsCode.Events;

[RegisterSharedEvent]
public sealed class CursedTome : ModEventTemplate
{
    public override bool IsShared => true;

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: Res.EventPortrait<CursedTome>()
    );

    public override bool IsAllowed(IRunState runState)
    {
        return runState.CurrentActIndex >= 1;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new EventOption[]
        {
            new EventOption(this, ReadAll, InitialOptionKey("READ_ALL")),
            new EventOption(this, ReadOne, InitialOptionKey("READ_ONE")),
        };
    }

    private async Task ReadAll()
    {
        var rewards = new List<Reward>();
        for (int i = 0; i < 5; i++)
            rewards.Add(new RelicReward(RelicFactory.PullNextRelicFromFront(Owner!).ToMutable(), Owner!));

        await RewardsCmd.OfferCustom(Owner!, rewards);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.READ_ALL.description"));
    }

    private async Task ReadOne()
    {
        var hasArchaicTooth = Owner!.Relics.Any(r => r.Id == ModelDb.Relic<ArchaicTooth>().Id);
        var relic = hasArchaicTooth
            ? ModelDb.Relic<TouchOfOrobas>().ToMutable()
            : ModelDb.Relic<ArchaicTooth>().ToMutable();

        await RelicCmd.Obtain(relic, Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.READ_ONE.description"));
    }
}
