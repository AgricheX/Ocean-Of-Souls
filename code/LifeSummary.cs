namespace OceanOfSouls
{
    internal struct LifeSummary
    {
        public const string ReignsKey = "oos_h_reigns";
        public const string LeaderKey = "oos_h_leader";

        public int Age;
        public int Kills;
        public int Births;
        public int Renown;
        public int ExperienceLevel;
        public int Reigns;
        public int Leaderships;

        public static LifeSummary From(Actor actor)
        {
            ActorData data = actor.data;
            LifeSummary summary = new LifeSummary
            {
                Age = actor.getAge(),
                Kills = data.kills,
                Births = data.births,
                Renown = data.renown,
                ExperienceLevel = data.level
            };
            data.get(ReignsKey, out summary.Reigns);
            data.get(LeaderKey, out summary.Leaderships);
            return summary;
        }

        public static void Increment(Actor actor, string key)
        {
            ActorData data = actor?.data;
            if (data == null) return;
            data.get(key, out int current);
            data.set(key, current + 1);
        }
    }
}
