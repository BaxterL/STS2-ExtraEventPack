using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;


namespace ExtraEvents.ExtraEventsCode.Cards;

public sealed class Supernormality : ModCardTemplate
{
    private const int NumOfCardsPerTurn = 3;
    private const string CalculatedCardsKey = "CalculatedCards";

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            yield return CardKeyword.Eternal;
            yield return CardKeyword.Unplayable;
        }
    }

    public override int MaxUpgradeLevel => 0;

    protected override bool ShouldGlowRedInternal => ShouldPreventCardPlay;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: Res.CardPortrait<Supernormality>()
    );

    public Supernormality()
        : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new CalculationBaseVar(3m);
            yield return new CalculationExtraVar(-1m);
            yield return new CalculatedVar(CalculatedCardsKey).WithMultiplier(
                (CardModel card, Creature? _) => (decimal)Math.Min(NumOfCardsPerTurn, ((Supernormality)card).CardsPlayedThisTurn)
            );
        }
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType _)
    {
        if (card.Owner != Owner)
            return true;

        return Pile is not { Type: PileType.Hand } || !ShouldPreventCardPlay;
    }

    private bool ShouldPreventCardPlay => CardsPlayedThisTurn >= NumOfCardsPerTurn;

    private int CardsPlayedThisTurn
    {
        get
        {
            return CombatManager.Instance.History.CardPlaysStarted.Count(
                e => e.HappenedThisTurn(CombatState) && e.CardPlay.Card.Owner == Owner
            );
        }
    }
}
