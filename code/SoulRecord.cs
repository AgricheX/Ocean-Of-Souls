using System.Collections.Generic;

namespace OceanOfSouls
{
    internal sealed class SoulRecord
    {
        private const string PointsKey = "oos_points";
        private const string LevelKey = "oos_level";
        private const string PeakKey = "oos_peak";
        private const string LivesKey = "oos_lives";
        private const string YearsKey = "oos_years";
        private const string LastAgeKey = "oos_last_age";
        private const string DeedTotalKey = "oos_deed_total";
        private const string DeedBestKey = "oos_deed_best";
        private const string HistoryKey = "oos_history";
        private const string SoulIdKey = "oos_soul_id";
        private const string GrantedKey = "oos_granted";

        public bool Exists;
        public string SoulId;
        public string Granted;
        public int Points;
        public int Level;
        public int PeakLevel;
        public int Lives;
        public int SoulYears;
        public int LastLifeAge = -1;
        public int DeedTotal;
        public int DeedBest;
        public List<LifeEntry> History = new List<LifeEntry>();

        public static SoulRecord Load(ActorData data)
        {
            SoulRecord record = new SoulRecord();
            if (data == null) return record;

            data.get(LivesKey, out int lives, -1);
            if (lives < 0) return record;

            record.Exists = true;
            record.Lives = lives;
            data.get(SoulIdKey, out record.SoulId, null);
            data.get(PointsKey, out record.Points);
            data.get(LevelKey, out record.Level);
            data.get(PeakKey, out record.PeakLevel);
            data.get(YearsKey, out record.SoulYears);
            data.get(LastAgeKey, out record.LastLifeAge, -1);
            data.get(DeedTotalKey, out record.DeedTotal);
            data.get(DeedBestKey, out record.DeedBest);
            data.get(GrantedKey, out record.Granted, null);
            data.get(HistoryKey, out string history, string.Empty);
            record.History = SoulHistory.Parse(history);
            return record;
        }

        public void Store(ActorData data)
        {
            if (data == null) return;

            Exists = true;
            data.set(PointsKey, Points);
            data.set(LevelKey, Level);
            data.set(PeakKey, PeakLevel);
            data.set(LivesKey, Lives);
            data.set(YearsKey, SoulYears);
            data.set(LastAgeKey, LastLifeAge);
            data.set(DeedTotalKey, DeedTotal);
            data.set(DeedBestKey, DeedBest);
            data.set(HistoryKey, SoulHistory.Serialize(History));
            data.set(GrantedKey, Granted ?? string.Empty);
            if (!string.IsNullOrEmpty(SoulId))
            {
                data.set(SoulIdKey, SoulId);
            }
        }

        public static void Copy(ActorData from, ActorData to)
        {
            Load(from).Store(to);
        }
    }
}
