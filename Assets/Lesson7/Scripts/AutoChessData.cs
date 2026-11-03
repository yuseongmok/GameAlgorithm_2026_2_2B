using UnityEngine;

namespace AlgoLesson7
{
    public enum UnitRace
    {
        Human,
        Beast,
        Machine
    }

    public enum UnitJob
    {
        Warrior,
        Ranger,
        Mage
    }

    public readonly struct UnitSnapshot
    {
        public UnitSnapshot(
            int id,
            string unitName,
            UnitRace race,
            UnitJob job,
            Vector3 position,
            float currentHealth,
            float attackRange,
            bool isAlive)
        {
            Id = id;
            UnitName = unitName;
            Race = race;
            Job = job;
            Position = position;
            CurrentHealth = currentHealth;
            AttackRange = attackRange;
            IsAlive = isAlive;
        }

        public int Id { get; }
        public string UnitName { get; }
        public UnitRace Race { get; }
        public UnitJob Job { get; }
        public Vector3 Position { get; }
        public float CurrentHealth { get; }
        public float AttackRange { get; }
        public bool IsAlive { get; }
    }

    public readonly struct UnitDefinition
    {
        public UnitDefinition(
            string name,
            UnitRace race,
            UnitJob job,
            float health,
            float attack,
            float attackRange,
            Color color)
        {
            Name = name;
            Race = race;
            Job = job;
            Health = health;
            Attack = attack;
            AttackRange = attackRange;
            Color = color;
        }

        public string Name { get; }
        public UnitRace Race { get; }
        public UnitJob Job { get; }
        public float Health { get; }
        public float Attack { get; }
        public float AttackRange { get; }
        public Color Color { get; }
    }
}
