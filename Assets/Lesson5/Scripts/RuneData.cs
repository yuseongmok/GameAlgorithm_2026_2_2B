using System;
using UnityEngine;

namespace AlgoCourse.Lesson5
{
    public enum RuneElement { Fire, Water, Wind, Earth, Light, Dark }

    [Serializable]
    public sealed class RuneData
    {
        [SerializeField] private int id;
        [SerializeField] private string displayName;
        [SerializeField] private int value;
        [SerializeField] private RuneElement element;

        public int Id => id;
        public string DisplayName => displayName;
        public int Value => value;
        public RuneElement Element => element;

        public RuneData(int runeId, string name, int runeValue, RuneElement runeElement)
        {
            id = runeId;
            displayName = name;
            value = runeValue;
            element = runeElement;
        }
    }
}
