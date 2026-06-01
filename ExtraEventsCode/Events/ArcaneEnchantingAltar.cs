using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ExtraEvents.ExtraEventsCode.Events;

[RegisterActEvent(typeof(Glory))]
public sealed class ArcaneEnchantingAltar : ModEventTemplate
{
    private EnchantmentModel? _selectedEnchantment;
    private int _readCount;
    private static readonly int[] ReadHpCosts = [9, 13, 27];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new MaxHpVar(10)
    ];

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: Res.EventPortrait<ArcaneEnchantingAltar>()
    );

    public override bool IsAllowed(IRunState runState)
    {
        var pile = PileType.Deck.GetPile(Owner!);
        return pile.Cards.Any();
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var pile = PileType.Deck.GetPile(Owner!);
        var allEnchantments = ModelDb.DebugEnchantments
            .Where(e => e.GetType() != typeof(MegaCrit.Sts2.Core.Models.Enchantments.DeprecatedEnchantment))
            .ToList();
        _selectedEnchantment = base.Rng.NextItem(allEnchantments);

        var canEnchant = _selectedEnchantment != null &&
            pile.Cards.Any(c => _selectedEnchantment.CanEnchant(c) && c.Enchantment == null);

        var options = new List<EventOption>
        {
            new EventOption(this, ReadMore, InitialOptionKey("READ_MORE")).ThatDecreasesMaxHp(ReadHpCosts[0])
        };

        if (canEnchant && _selectedEnchantment != null)
        {
            var hoverCopy = _selectedEnchantment.ToMutable();
            hoverCopy.Amount = 1;
            hoverCopy.RecalculateValues();
            options.Add(new EventOption(this, PerformEnchant, InitialOptionKey("ENCHANT"), hoverCopy.HoverTips.Cast<IHoverTip>().ToArray()));
        }

        options.Add(new EventOption(this, Leave, InitialOptionKey("LEAVE")));
        return options.ToArray();
    }

    private async Task ReadMore()
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature, ReadHpCosts[_readCount], false);
        _readCount++;

        if (_readCount >= 3)
            ShowOnlyEnchantPage();
        else
            ShowReadMorePage();
    }

    private void ShowReadMorePage()
    {
        var pile = PileType.Deck.GetPile(Owner!);
        var canEnchant = _selectedEnchantment != null &&
            pile.Cards.Any(c => _selectedEnchantment.CanEnchant(c) && c.Enchantment == null);

        var options = new List<EventOption>
        {
            new EventOption(this, ReadMore, ModOptionKey($"READ_{_readCount}", "READ_MORE")).ThatDecreasesMaxHp(ReadHpCosts[_readCount])
        };

        if (canEnchant && _selectedEnchantment != null)
        {
            var hoverCopy = _selectedEnchantment.ToMutable();
            hoverCopy.Amount = 1;
            hoverCopy.RecalculateValues();
            options.Add(new EventOption(this, PerformEnchant, ModOptionKey($"READ_{_readCount}", "ENCHANT"), hoverCopy.HoverTips.Cast<IHoverTip>().ToArray()));
        }

        SetEventState(L10NLookup($"{Id.Entry}.pages.READ_{_readCount}.description"), options);
    }

    private void ShowOnlyEnchantPage()
    {
        var pile = PileType.Deck.GetPile(Owner!);
        var canEnchant = _selectedEnchantment != null &&
            pile.Cards.Any(c => _selectedEnchantment.CanEnchant(c) && c.Enchantment == null);

        if (canEnchant && _selectedEnchantment != null)
        {
            var hoverCopy = _selectedEnchantment.ToMutable();
            hoverCopy.Amount = 1;
            hoverCopy.RecalculateValues();
            SetEventState(L10NLookup($"{Id.Entry}.pages.READ_3.description"), [
                new EventOption(this, PerformEnchant, ModOptionKey("READ_3", "ENCHANT"), hoverCopy.HoverTips.Cast<IHoverTip>().ToArray())
            ]);
        }
        else
        {
            SetEventFinished(L10NLookup($"{Id.Entry}.pages.NO_ENCHANT_TARGET.description"));
        }
    }

    private async Task PerformEnchant()
    {
        if (_selectedEnchantment == null)
        {
            SetEventFinished(L10NLookup($"{Id.Entry}.pages.LEAVE.description"));
            return;
        }

        var count = _readCount + 1;
        var pile = PileType.Deck.GetPile(Owner!);
        var mutable = _selectedEnchantment.ToMutable();
        var compatibleCards = pile.Cards
            .Where(c => mutable.CanEnchant(c))
            .Take(count)
            .ToList();

        foreach (var card in compatibleCards)
        {
            var copy = _selectedEnchantment.ToMutable();
            CardCmd.Enchant(copy, card, 1m);
            var vfx = NCardEnchantVfx.Create(card);
            if (vfx != null)
                NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(vfx);
        }

        var desc = L10NLookup($"{Id.Entry}.pages.ENCHANT.description");
        desc.Add("Enchantment", _selectedEnchantment.Title);
        desc.Add("Cards", compatibleCards.Count);
        SetEventFinished(desc);
    }

    private async Task Leave()
    {
        await CreatureCmd.GainMaxHp(Owner!.Creature, DynamicVars.MaxHp.BaseValue);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.LEAVE.description"));
    }
}
