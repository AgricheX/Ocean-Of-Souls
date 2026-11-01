namespace OceanOfSouls
{
    internal sealed class SoulEntry
    {
        public string Path;
        public string SoulId;
        public string WorldId;
        public string WorldName;
        public string Name;
        public int Level;
        public int PeakLevel;
        public int Lives;

        private SoulData data;
        private bool attempted;

        public SoulData Data
        {
            get
            {
                if (data == null && !attempted)
                {
                    attempted = true;
                    SoulStorage.TryLoad(Path, out data);
                }
                return data;
            }
        }

        public string DisplayName
        {
            get { return SoulNames.Decorate(Name, Level, Lives); }
        }

        public void Release()
        {
            data = null;
            attempted = false;
        }
    }
}
