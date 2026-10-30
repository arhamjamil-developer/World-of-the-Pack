using UnityEngine;

namespace WorldOfThePack.Game
{
    public enum UnitFaction
    {
        Allies,
        Hunters
    }

    public interface IDamageable
    {
        UnitFaction Faction { get; }
        Vector3 Position { get; }
        bool IsAlive { get; }
        void TakeDamage(float amount, Combatant attacker = null);
        void Heal(float amount);
    }
}
