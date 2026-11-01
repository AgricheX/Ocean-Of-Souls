using System.Collections.Generic;

namespace OceanOfSouls
{
    internal sealed class SoulData
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public string SoulId;
        public string Granted;
        public string Name;
        public string AssetId;
        public int Level;
        public int Points;
        public int PeakLevel;
        public int Lives;
        public int SoulYears;
        public int LastLifeAge = -1;
        public int DeedTotal;
        public int DeedBest;
        public List<LifeEntry> History = new List<LifeEntry>();
        public List<string> Traits = new List<string>();
        public string WorldId;
        public string WorldName;
        public int DeathYear;
        public SoulLook Look;
        public bool Favorite;

        public static SoulData Create(Actor actor, SoulRecord record)
        {
            SoulData data = new SoulData
            {
                SoulId = record.SoulId,
                Granted = record.Granted,
                Name = actor.getName(),
                AssetId = actor.asset != null ? actor.asset.id : null,
                Level = record.Level,
                Points = record.Points,
                PeakLevel = record.PeakLevel,
                Lives = record.Lives,
                SoulYears = record.SoulYears,
                LastLifeAge = record.LastLifeAge,
                DeedTotal = record.DeedTotal,
                DeedBest = record.DeedBest,
                History = new List<LifeEntry>(record.History),
                WorldId = SoulWorld.CurrentId(),
                WorldName = SoulWorld.CurrentName(),
                DeathYear = Date.getYear(World.world.getCurWorldTime()),
                Look = SoulLook.From(actor),
                Favorite = actor.isFavorite()
            };

            foreach (ActorTrait trait in actor.getTraits())
            {
                if (trait != null && !string.IsNullOrEmpty(trait.id))
                {
                    data.Traits.Add(trait.id);
                }
            }
            return data;
        }

        public SoulRecord ToRecord()
        {
            return new SoulRecord
            {
                Exists = true,
                SoulId = SoulId,
                Granted = Granted,
                Points = Points,
                Level = Level,
                PeakLevel = PeakLevel,
                Lives = Lives,
                SoulYears = SoulYears,
                LastLifeAge = LastLifeAge,
                DeedTotal = DeedTotal,
                DeedBest = DeedBest,
                History = new List<LifeEntry>(History)
            };
        }
    }
}
