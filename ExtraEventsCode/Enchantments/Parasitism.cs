using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ExtraEvents.ExtraEventsCode.Enchantments;

[RegisterEnchantment]
public sealed class Parasitism : ModEnchantmentTemplate
{
    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/enchantments/parasitism.png"
    );

    public override bool HasExtraCardText => true;

    public override bool CanEnchantCardType(CardType cardType)
    {
        return cardType == CardType.Attack;
    }

    public override decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
    {
        if (!props.IsPoweredAttack())
            return 1m;
        return 1.5m;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (cardPlay?.Target == null)
            return;

        var damage = base.Card.DynamicVars.Damage.BaseValue;
        var doomAmount = damage * 1.5m;

        IReadOnlyList<Creature> targets;
        if (base.Card.TargetType != TargetType.AllEnemies)
        {
            targets = new Creature[] { cardPlay.Target };
        }
        else
        {
            targets = base.Card.CombatState?.HittableEnemies ?? new Creature[] { cardPlay.Target };
        }

        await PowerCmd.Apply<DoomPower>(choiceContext, targets, doomAmount, base.Card.Owner.Creature, base.Card);
    }
}
