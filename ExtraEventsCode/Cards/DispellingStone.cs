using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ExtraEvents.ExtraEventsCode.Cards;

[RegisterCard(typeof(EventCardPool))]
public sealed class DispellingStone : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.None;
    private const bool ShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            yield return CardKeyword.Exhaust;
        }
    }

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: Res.CardPortrait<DispellingStone>()
    );

    public DispellingStone()
        : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var curses = new List<CardModel>();
        foreach (var pt in new[] { PileType.Draw, PileType.Hand, PileType.Discard })
        {
            var pile = pt.GetPile(Owner);
            if (pile != null)
                curses.AddRange(pile.Cards.Where(c => c.Type == CardType.Curse));
        }

        foreach (var curse in curses)
            await CardPileCmd.Add(curse, PileType.Exhaust, CardPilePosition.Bottom, null);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
