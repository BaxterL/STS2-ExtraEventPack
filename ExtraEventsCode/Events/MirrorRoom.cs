using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ExtraEvents.ExtraEventsCode.Events;

[RegisterSharedEvent]
public sealed class MirrorRoom : ModEventTemplate
{
    public override bool IsShared => true;

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: Res.EventPortrait<MirrorRoom>()
    );

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.All(p => p.Creature.CurrentHp >= 21);
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new EventOption[]
        {
            new EventOption(this, RollDice, InitialOptionKey("ROLL"), HoverTipFactory.FromPotion(ModelDb.Potion<Duplicator>())),
            new EventOption(this, Leave, InitialOptionKey("LEAVE")),
        };
    }

    private async Task RollDice()
    {
        var roll = base.Rng.NextInt(1, 21);
        LocString desc;

        if (roll == 20)
        {
            var rewards = new List<Reward>();
            for (int i = 0; i < 20; i++)
                rewards.Add(new PotionReward(ModelDb.Potion<Duplicator>().ToMutable(), Owner!));
            await RewardsCmd.OfferCustom(Owner!, rewards);
            desc = L10NLookup($"{Id.Entry}.pages.CRIT.success");
        }
        else if (roll == 1)
        {
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, 20, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
            desc = L10NLookup($"{Id.Entry}.pages.FAIL.description");
        }
        else
        {
            await RewardsCmd.OfferCustom(Owner!, [
                new PotionReward(ModelDb.Potion<Duplicator>().ToMutable(), Owner!),
                new PotionReward(ModelDb.Potion<Duplicator>().ToMutable(), Owner!),
                new PotionReward(ModelDb.Potion<Duplicator>().ToMutable(), Owner!),
            ]);
            desc = L10NLookup($"{Id.Entry}.pages.NORMAL.description");
        }

        desc.Add("Roll", roll);
        SetEventFinished(desc);
    }

    private async Task Leave()
    {
        await PlayerCmd.GainGold(75, Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.LEAVE.description"));
    }
}
