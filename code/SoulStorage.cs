using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace OceanOfSouls
{
    internal static class SoulStorage
    {
        private sealed class WriteJob
        {
            public string Path;
            public string Json;
            public string[] Remove;
            public string MetaPath;
            public string MetaJson;
        }

        private sealed class WorldMeta
        {
            public string Id;
            public string Name;
        }

        private static readonly BlockingCollection<WriteJob> queue = new BlockingCollection<WriteJob>(new ConcurrentQueue<WriteJob>());
        private static readonly Dictionary<string, string> knownWorldNames = new Dictionary<string, string>();
        private static readonly Random random = new Random();
        private static readonly object workerLock = new object();
        private static Thread worker;
        private static readonly Dictionary<string, int> writesSinceCheck = new Dictionary<string, int>();
        private const int CapCheckInterval = 8;
        private static int pending;
        private static readonly object indexLock = new object();
        private static readonly Dictionary<string, List<string>> index = new Dictionary<string, List<string>>();

        public static void Init()
        {
            UnityEngine.Application.quitting += Flush;

            string root = SoulPaths.Root;
            Thread migration = new Thread(MigrateLegacy)
            {
                IsBackground = true,
                Name = "OceanOfSouls.Migration",
                Priority = ThreadPriority.BelowNormal
            };
            migration.Start(root);
        }

        private static void MigrateLegacy(object state)
        {
            try
            {
                string root = (string)state;
                if (!Directory.Exists(root)) return;

                foreach (string worldFolder in SafeDirectories(root))
                {
                    string world = Path.GetFileName(worldFolder);
                    for (int level = SoulLevels.MinLevel; level <= SoulLevels.MaxLevel; level++)
                    {
                        string[] files;
                        lock (indexLock)
                        {
                            files = Index(world, level).ToArray();
                        }

                        foreach (string file in files)
                        {
                            if (SoulPaths.TryParseFile(file, out _, out _, out _, out _)) continue;
                            MigrateFile(file);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private static void MigrateFile(string file)
        {
            try
            {
                if (!TryLoad(file, out SoulData loaded)) return;

                string target = Path.Combine(Path.GetDirectoryName(file), SoulPaths.FileName(loaded.Name, loaded.PeakLevel, loaded.Lives, loaded.SoulId));
                if (File.Exists(target))
                {
                    File.Delete(file);
                }
                else
                {
                    File.Move(file, target);
                }
                Forget(file);
                Learn(target);
            }
            catch
            {
            }
        }

        public static void Save(SoulData data)
        {
            if (data == null || string.IsNullOrEmpty(data.SoulId) || string.IsNullOrEmpty(data.WorldId)) return;

            List<string> remove = new List<string>();
            for (int level = SoulLevels.MinLevel; level <= SoulLevels.MaxLevel; level++)
            {
                remove.Add(SoulPaths.LegacyFile(data.WorldId, level, data.SoulId));
            }

            WriteJob job = new WriteJob
            {
                Path = SoulPaths.SoulFile(data.WorldId, data.Level, data.Name, data.PeakLevel, data.Lives, data.SoulId),
                Json = JsonConvert.SerializeObject(data, Formatting.None),
                Remove = remove.ToArray()
            };

            string worldName = data.WorldName ?? string.Empty;
            if (!knownWorldNames.TryGetValue(data.WorldId, out string known) || known != worldName)
            {
                knownWorldNames[data.WorldId] = worldName;
                job.MetaPath = SoulPaths.WorldMetaPath(data.WorldId);
                job.MetaJson = JsonConvert.SerializeObject(new WorldMeta { Id = data.WorldId, Name = worldName }, Formatting.None);
            }

            Enqueue(job);
        }

        public static bool TryLoad(string path, out SoulData data)
        {
            data = null;
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;

                SoulData loaded = JsonConvert.DeserializeObject<SoulData>(File.ReadAllText(path, Encoding.UTF8));
                if (loaded == null || string.IsNullOrEmpty(loaded.SoulId)) return false;
                if (loaded.Level < SoulLevels.MinLevel || loaded.Level > SoulLevels.MaxLevel) return false;

                data = loaded;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Delete(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;
                File.Delete(path);
                Forget(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryPickRandom(out string path, out SoulData data)
        {
            return TryPickRandom(SoulWorld.CurrentId(), false, out path, out data);
        }

        public static bool TryPickRandom(string currentWorld, bool prioritize, out string path, out SoulData data)
        {
            path = null;
            data = null;

            List<string> worlds = CollectWorlds(currentWorld);
            if (worlds.Count == 0) return false;

            int split = SoulConfig.PriorityMinLevel;
            int high = 0;
            int low = 0;
            lock (indexLock)
            {
                foreach (string world in worlds)
                {
                    for (int level = SoulLevels.MinLevel; level <= SoulLevels.MaxLevel; level++)
                    {
                        int count = Index(world, level).Count;
                        if (level >= split) high += count; else low += count;
                    }
                }
            }

            int total = high + low;
            if (total == 0) return false;

            int minLevel = SoulLevels.MinLevel;
            int maxLevel = SoulLevels.MaxLevel;
            int pool = total;
            if (prioritize && split > SoulLevels.MinLevel && split <= SoulLevels.MaxLevel)
            {
                bool useHigh;
                if (high == 0) useHigh = false;
                else if (low == 0) useHigh = true;
                else useHigh = random.Next(100) < SoulConfig.PriorityChance;

                if (useHigh)
                {
                    minLevel = split;
                    pool = high;
                }
                else
                {
                    maxLevel = split - 1;
                    pool = low;
                }
            }
            if (pool == 0) return false;

            for (int attempt = 0; attempt < 8; attempt++)
            {
                string file = PickFromPool(worlds, minLevel, maxLevel, pool);
                if (file == null) return false;

                if (TryLoad(file, out SoulData loaded))
                {
                    path = file;
                    data = loaded;
                    return true;
                }
                Forget(file);
                pool--;
                if (pool <= 0) return false;
            }
            return false;
        }

        private static string PickFromPool(List<string> worlds, int minLevel, int maxLevel, int pool)
        {
            lock (indexLock)
            {
                int target = random.Next(pool);
                foreach (string world in worlds)
                {
                    for (int level = minLevel; level <= maxLevel; level++)
                    {
                        List<string> list = Index(world, level);
                        if (target < list.Count) return list[target];
                        target -= list.Count;
                    }
                }
            }
            return null;
        }

        private static List<string> Index(string world, int level)
        {
            string folder = SoulPaths.LevelFolder(world, level);
            if (!index.TryGetValue(folder, out List<string> list))
            {
                list = new List<string>(SafeFiles(folder));
                index[folder] = list;
            }
            return list;
        }

        private static void Learn(string path)
        {
            lock (indexLock)
            {
                string folder = Path.GetDirectoryName(path);
                if (!index.TryGetValue(folder, out List<string> list)) return;
                if (!list.Contains(path)) list.Add(path);
            }
        }

        private static void Forget(string path)
        {
            lock (indexLock)
            {
                string folder = Path.GetDirectoryName(path);
                if (index.TryGetValue(folder, out List<string> list))
                {
                    list.Remove(path);
                }
            }
        }

        public static List<SoulEntry> ListEntries(string currentWorld, bool everyWorld = false)
        {
            List<SoulEntry> entries = new List<SoulEntry>();
            foreach (string world in CollectWorlds(currentWorld, everyWorld))
            {
                string worldName = ReadWorldName(world);
                for (int level = SoulLevels.MinLevel; level <= SoulLevels.MaxLevel; level++)
                {
                    string[] files;
                    lock (indexLock)
                    {
                        files = Index(world, level).ToArray();
                    }

                    foreach (string file in files)
                    {
                        if (!SoulPaths.TryParseFile(file, out string name, out int peak, out int lives, out string soulId)) continue;
                        entries.Add(new SoulEntry
                        {
                            Path = file,
                            SoulId = soulId,
                            WorldId = world,
                            WorldName = worldName,
                            Name = name,
                            Level = level,
                            PeakLevel = peak,
                            Lives = lives
                        });
                    }
                }
            }
            return entries;
        }

        private static string ReadWorldName(string world)
        {
            try
            {
                string path = SoulPaths.WorldMetaPath(world);
                if (!File.Exists(path)) return null;
                WorldMeta meta = JsonConvert.DeserializeObject<WorldMeta>(File.ReadAllText(path, Encoding.UTF8));
                return meta != null ? meta.Name : null;
            }
            catch
            {
                return null;
            }
        }

        private static List<string> CollectWorlds(string currentWorld, bool everyWorld = false)
        {
            List<string> worlds = new List<string>();
            if (Directory.Exists(SoulPaths.WorldFolder(currentWorld)))
            {
                worlds.Add(currentWorld);
            }

            if ((everyWorld || SoulConfig.AllWorlds) && Directory.Exists(SoulPaths.Root))
            {
                foreach (string directory in SafeDirectories(SoulPaths.Root))
                {
                    string worldId = Path.GetFileName(directory);
                    if (worldId != currentWorld)
                    {
                        worlds.Add(worldId);
                    }
                }
            }
            return worlds;
        }

        public static void Flush()
        {
            int waited = 0;
            while (Volatile.Read(ref pending) > 0 && waited < 5000)
            {
                Thread.Sleep(20);
                waited += 20;
            }
        }

        private static string[] SafeFiles(string directory)
        {
            try
            {
                if (!Directory.Exists(directory)) return new string[0];
                return Directory.GetFiles(directory, "*" + SoulPaths.FileExtension);
            }
            catch
            {
                return new string[0];
            }
        }

        private static string[] SafeDirectories(string directory)
        {
            try
            {
                return Directory.GetDirectories(directory);
            }
            catch
            {
                return new string[0];
            }
        }

        private static void Enqueue(WriteJob job)
        {
            EnsureWorker();
            Interlocked.Increment(ref pending);
            queue.Add(job);
        }

        private static void EnsureWorker()
        {
            if (worker != null) return;
            lock (workerLock)
            {
                if (worker != null) return;
                worker = new Thread(Run)
                {
                    IsBackground = true,
                    Name = "OceanOfSouls.Storage"
                };
                worker.Start();
            }
        }

        private static void Run()
        {
            foreach (WriteJob job in queue.GetConsumingEnumerable())
            {
                try
                {
                    Execute(job);
                }
                catch
                {
                }
                finally
                {
                    Interlocked.Decrement(ref pending);
                }
            }
        }

        private static void Execute(WriteJob job)
        {
            if (job.MetaPath != null)
            {
                WriteAtomic(job.MetaPath, job.MetaJson);
            }

            WriteAtomic(job.Path, job.Json);
            Learn(job.Path);

            foreach (string stale in job.Remove)
            {
                try
                {
                    File.Delete(stale);
                    Forget(stale);
                }
                catch
                {
                }
            }

            EnforceCap(job.Path);
        }

        private static void EnforceCap(string path)
        {
            int cap = SoulConfig.MaxSoulsPerLevel;
            if (cap <= 0) return;

            string folder = Path.GetDirectoryName(path);
            writesSinceCheck.TryGetValue(folder, out int writes);
            writes++;
            if (writes < CapCheckInterval)
            {
                writesSinceCheck[folder] = writes;
                return;
            }
            writesSinceCheck[folder] = 0;

            string[] files = Directory.GetFiles(folder, "*" + SoulPaths.FileExtension);
            int excess = files.Length - cap;
            if (excess <= 0) return;

            Array.Sort(files, (a, b) => File.GetLastWriteTimeUtc(a).CompareTo(File.GetLastWriteTimeUtc(b)));
            foreach (string file in files)
            {
                if (excess <= 0) break;
                if (file == path) continue;
                try
                {
                    File.Delete(file);
                    Forget(file);
                    excess--;
                }
                catch
                {
                }
            }
        }

        private static void WriteAtomic(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp = path + ".tmp";
            File.WriteAllText(temp, content, Encoding.UTF8);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(temp, path);
        }
    }
}
