using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ExtraEvents.ExtraEventsCode.Events;

[RegisterSharedEvent]
public sealed class OneWayDoor : ModEventTemplate
{
    private int _ramCount;
    private static readonly int[] RamCosts = [5, 6, 7];
    private static readonly int[] LeaveCosts = [30, 50, 88];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new GoldVar(88),
        new MaxHpVar(4)
    ];

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: Res.EventPortrait<OneWayDoor>()
    );

    public override bool IsAllowed(IRunState runState)
    {
        return runState.CurrentActIndex >= 1 && runState.Players.All(p => p.Gold >= 100);
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return new EventOption[]
        {
            new EventOption(this, Ram, InitialOptionKey("RAM")).ThatDecreasesMaxHp(RamCosts[0]),
            new EventOption(this, GoAround, InitialOptionKey("GO_AROUND")),
        };
    }

    private async Task GoAround()
    {
        await PlayerCmd.LoseGold(88, Owner!, GoldLossType.Spent);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.GO_AROUND.description"));
    }

    private async Task Ram()
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature, RamCosts[_ramCount], false);
        _ramCount++;

        if (_ramCount >= 3)
        {
            await RelicCmd.Obtain<JewelryBox>(Owner!);
            SetEventFinished(L10NLookup($"{Id.Entry}.pages.DOOR_OPEN.description"));
        }
        else
        {
            var ramOption = _ramCount == 2
                ? new EventOption(this, Ram, ModOptionKey($"RAM_{_ramCount}", "RAM"), HoverTipFactory.FromRelic<JewelryBox>()).ThatDecreasesMaxHp(RamCosts[_ramCount])
                : new EventOption(this, Ram, ModOptionKey($"RAM_{_ramCount}", "RAM")).ThatDecreasesMaxHp(RamCosts[_ramCount]);

            var options = new List<EventOption>
            {
                ramOption,
                new EventOption(this, LeaveMidway, ModOptionKey($"RAM_{_ramCount}", "LEAVE")),
            };
            SetEventState(L10NLookup($"{Id.Entry}.pages.RAM_{_ramCount}.description"), options);
        }
    }

    private async Task LeaveMidway()
    {
        await PlayerCmd.GainGold(LeaveCosts[_ramCount - 1], Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.LEAVE_MIDWAY.description"));
    }
}
