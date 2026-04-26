using MelonLoader;

namespace YunyunLocalePatcher
{
    public class PatchFile
    {
        public struct PatchKey
        {
            public string TableName;
            public string Key;

            public PatchKey(string tableName, string key)
            {
                TableName = tableName;
                Key = key;
            }
        }

        private readonly Dictionary<PatchKey, string> patches;

        public int Count {
            get { return patches.Count; }
        }

        public PatchFile() {
            this.patches = new Dictionary<PatchKey, string>();
        }

        private PatchFile(Dictionary<PatchKey, string> patches)
        {
            this.patches = patches;
        }

        public static PatchFile Load(string filename)
        {
            var patches = new Dictionary<PatchKey, string>();
            string csvText = File.ReadAllText(filename);
            int index = 0;

            while (true)
            {
                var row = Csv.ParseLine(csvText, ref index);
                if (row == null) break;

                if (row[1] == "TableName") // header
                {
                    continue;
                }
                else if (row.Count == 3)
                {
                    var key = new PatchKey(row[0].Trim(), row[1].Trim());
                    patches[key] = row[2].Trim();
                }
                else
                {
                    MelonLogger.Warning($"[{filename}]: Expected 3 comma separated values, got: `{row.Count}`");
                }
            }

            return new PatchFile(patches);
        }

        public string this[string tableName, string key]
        {
            get
            {
                var patchKey = new PatchKey(tableName, key);
                return this.patches.TryGetValue(patchKey, out string value) ? value : null;
            }
        }

        public void Append(PatchFile other)
        {
            if (other == null) return;
            foreach (var kvp in other.patches)
            {
                patches[kvp.Key] = kvp.Value;
            }
        }
    }
}
