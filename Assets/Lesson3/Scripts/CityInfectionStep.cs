namespace AlgoCourse.Lesson3
{
    public readonly struct CityInfectionStep
    {
        public CityInfectionStep(int cityId, int infectionLevel, bool outbreak, bool changed)
        {
            CityId = cityId;
            InfectionLevel = infectionLevel;
            IsOutbreak = outbreak;
            Changed = changed;
        }

        public int CityId { get; }
        public int InfectionLevel { get; }
        public bool IsOutbreak { get; }
        public bool Changed { get; }
    }
}
