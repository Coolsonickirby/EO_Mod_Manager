using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Force.Crc32;
using System.Text.Json;

namespace EO_Mod_Manager
{
    public class Game
    {
        private static string BACKUP_FOLDER = "backups";
        private static string MODS_FOLDER = "mods";
        private static List<string> PATHS_TO_IGNORE = new List<string>() { ModConfig.CONFIG_FILE, Game.MODS_FOLDER, Game.BACKUP_FOLDER };
        public string key { get; set; }
        public string GameName { get; set; }
        public string GamePath { get; set; }
        public string GameExecutable { get; set; }
        public string GameFolderDataName { get; set; }
        public ObservableCollection<Mod> GameMods { get; set; }
        public IProgress<string> textProgress { get; set; }
        public static ObservableCollection<Mod> GetModsFromPath(string path)
        {
            ObservableCollection<Mod> mods = new ObservableCollection<Mod>();
            string mods_path = Path.Combine(path, MODS_FOLDER);
            foreach (var dir in Directory.EnumerateDirectories(mods_path))
            {
                ModConfig mod_config = ModConfig.LoadFromModPath(dir);
                if (mod_config == null)
                    continue;
                mods.Add(new Mod()
                {
                    Name = mod_config.name,
                    Author = mod_config.author,
                    Version = mod_config.version,
                    Description = mod_config.description,
                    mod_path = dir,
                    folder_name = Path.GetFileName(dir)
                });
            }
            return mods;
        }

        public void LoadDataFromKey()
        {
            List<string> mod_folders = new List<string>();
            List<bool> mod_statuses = new List<bool>();
            if (!File.Exists(this.key))
                return;
            string[] data = File.ReadAllText(this.key).Split('\n');
            foreach (string elem in data)
            {
                string[] split = elem.Split('|');
                string folder_name = String.Join('|', split.Take(split.Length - 1));
                string status = split[split.Length - 1];
                mod_folders.Add(folder_name);
                mod_statuses.Add((status == "True"));
            }
            this.GameMods = new ObservableCollection<Mod>(this.GameMods.OrderBy(e =>
            {
                int idx = mod_folders.IndexOf(e.folder_name);
                if (idx == -1)
                    return this.GameMods.Count;
                return idx;
            }));
            foreach (Mod mod in this.GameMods)
            {
                int idx = mod_folders.IndexOf(mod.folder_name);
                if (idx == -1)
                    continue;
                mod.enabled = mod_statuses[idx];
            }
        }

        public void Save()
        {
            this.textProgress.Report("Saving mods statuses...");
            string output = "";
            foreach (Mod mod in this.GameMods)
                output += $"{mod.folder_name}|{mod.enabled}\n";
            this.textProgress.Report("Writing mods statuses to key...");
            File.WriteAllText(this.key, output);
            this.textProgress.Report("Starting mod installation");
            Install();
        }

        private void PatchGame()
        {
            string target_folder = Path.Join(this.GamePath, this.GameFolderDataName, "StreamingAssets", "aa");
            string catalog_json = Path.Join(target_folder, "catalog.json");
            Patch.PatchCRC(catalog_json, catalog_json);
        }

        private void RestoreBackup()
        {
            string backup_dir = Path.Combine(this.GamePath, Game.BACKUP_FOLDER);
            if (!Directory.Exists(backup_dir))
                return;
            CloneDirectory(backup_dir, this.GamePath, this.textProgress);
        }

        private static void CloneDirectory(string root, string dest, IProgress<string> textProgress)
        {
            foreach (var directory in Directory.GetDirectories(root))
            {
                if (Game.PATHS_TO_IGNORE.Contains(Path.GetFileName(directory)))
                    continue;
                //Get the path of the new directory
                var newDirectory = Path.Combine(dest, Path.GetFileName(directory));
                //Create the directory if it doesn't already exist
                Directory.CreateDirectory(newDirectory);
                //Recursively clone the directory
                CloneDirectory(directory, newDirectory, textProgress);
            }

            foreach (var file in Directory.GetFiles(root))
            {
                if (Game.PATHS_TO_IGNORE.Contains(Path.GetFileName(file)))
                    continue;
                textProgress.Report($"Restoring {file}");
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);
            }
        }

        private void CheckDirForMods(string inital_mod_path, string path, ref Dictionary<string, List<string>> keyValuePairs)
        {
            foreach (var directory in Directory.GetDirectories(path))
            {
                if (Game.PATHS_TO_IGNORE.Contains(Path.GetFileName(directory)))
                    continue;
                CheckDirForMods(inital_mod_path, directory, ref keyValuePairs);
            }

            foreach (var file in Directory.GetFiles(path))
            {
                if (Game.PATHS_TO_IGNORE.Contains(Path.GetFileName(file)))
                    continue;
                string key = file.Substring(inital_mod_path.Length + 1);
                if (!keyValuePairs.ContainsKey(key))
                    keyValuePairs.Add(key, new List<string>());
                keyValuePairs[key].Add(file);
            }
        }

        private void CopyOriginalToBackup(Dictionary<string, List<string>> keyValuePairs)
        {
            foreach (KeyValuePair<string, List<string>> entry in keyValuePairs)
            {
                string original_file = Path.Join(this.GamePath, entry.Key);
                string backup_file = Path.Join(this.GamePath, Game.BACKUP_FOLDER, entry.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(backup_file));
                try
                {
                    File.Copy(original_file, backup_file, true);
                }
                catch (Exception e)
                {
                    // Do nothing (in case people start adding their own bundle files and stuff)
                }
            }
        }

        public static void CreateNewCombinedBundle(string original, List<string> modded_paths, string output, IProgress<string> textProgress = null)
        {
            if (textProgress == null)
                textProgress = new Progress<string>();


            if(modded_paths.Count == 1)
            {
                textProgress.Report($"Only one modded bundle instance found! Overwriting...");
                File.Copy(modded_paths[0], output, true);
                return;
            }

            var am = new AssetsManager();


            // Original bundle file
            var og_bun = am.LoadBundleFile(original);

            // Original Assets
            var og_assetInst = am.LoadAssetsFileFromBundle(og_bun, 0, false);

            // Original Asset Index -> CRC32 Hash
            Dictionary<long, uint> og_asset_hash = new Dictionary<long, uint>();

            // Asset Index -> New Asset Replacer Object
            Dictionary<long, AssetsReplacer> idx_to_asset_replacement = new Dictionary<long, AssetsReplacer>();


            textProgress.Report("Adding original assets hashes to table...");
            foreach (var inf in og_assetInst.table.assetFileInfo)
            {
                // Ignore AssetBundle files
                if (inf.curFileType == (uint)AssetClassID.AssetBundle)
                    continue;

                // Get the Base Field of the Asset
                var baseField = am.GetTypeInstance(og_assetInst, inf).GetBaseField();

                // Add the original asset's CRC32 to the table (used to check if mods change said file)
                og_asset_hash.Add(inf.index, Crc32Algorithm.Compute(baseField.WriteToByteArray()));
            }

            foreach (string file in modded_paths)
            {
                textProgress.Report($"Loading modded bundle file... {file}");
                
                // Modded bundle file
                var mod_bundle = am.LoadBundleFile(file);

                // Modded Assets
                var mod_assetInst = am.LoadAssetsFileFromBundle(mod_bundle, 0, false);
                
                foreach (var inf in mod_assetInst.table.assetFileInfo)
                {
                    // Ignore AssetBundle files
                    if (inf.curFileType == (uint)AssetClassID.AssetBundle)
                        continue;

                    // Get the Base Field of the Asset
                    var baseField = am.GetTypeInstance(mod_assetInst, inf).GetBaseField();

                    // Check to see if the Asset is modded by comparing its hash to the original hash. If its the same (not modified), then skip.
                    if (og_asset_hash[inf.index] == Crc32Algorithm.Compute(baseField.WriteToByteArray()))
                        continue;

                    textProgress.Report($"Asset Index {inf.index} Data Hash is different! Overwriting asset...");

                    // Convert the modded asset to a byte array
                    var newBytes = baseField.WriteToByteArray();

                    // Check to see if the asset is already modded, and if so, then overwrite it. Else, just add it to the table
                    if (!idx_to_asset_replacement.ContainsKey(inf.index))
                        idx_to_asset_replacement.Add(inf.index, new AssetsReplacerFromMemory(0, inf.index, (int)inf.curFileType, 0xffff, newBytes));
                    else
                        idx_to_asset_replacement[inf.index] = new AssetsReplacerFromMemory(0, inf.index, (int)inf.curFileType, 0xffff, newBytes);
                }
            }

            // Convert all the asset replacements from the dictionary to a List
            List<AssetsReplacer> assetsReplacers = idx_to_asset_replacement.Select(e => e.Value).ToList();


            textProgress.Report("Writing new assets table...");
            // Create empty byte array for new asset data
            byte[] newAssetData;
            using (var stream = new MemoryStream())
            using (var writer = new AssetsFileWriter(stream))
            {
                // Write the new assets in and convert it to a byte array
                og_assetInst.file.Write(writer, 0, assetsReplacers, 0);
                newAssetData = stream.ToArray();
            }

            textProgress.Report("Writing new bundle file...");
            // Create a bundle replacer with the original asset name and the new asset data
            var bunRepl = new BundleReplacerFromMemory(og_assetInst.name, og_assetInst.name, true, newAssetData, -1);

            using (var stream = new MemoryStream())
            using (var writer = new AssetsFileWriter(stream))
            {
                // Write new bundle information to stream
                og_bun.file.Write(writer, new List<BundleReplacer>() { bunRepl });
                
                // Unload everything
                am.UnloadAll();

                // Write the new bundle to the output
                File.WriteAllBytes(output, stream.ToArray());
                textProgress.Report($"Bundle file {output} is done writing!");
            }
        }

        public void CreateNewMod(string folder_name, ModConfig config)
        {
            string target_dir = Path.Combine(this.GamePath, Game.MODS_FOLDER, folder_name);
            if (!Directory.Exists(target_dir))
                Directory.CreateDirectory(target_dir);
            File.WriteAllText(Path.Combine(target_dir, ModConfig.CONFIG_FILE), JsonSerializer.Serialize(config));
            this.Refresh();
        }

        public void Refresh()
        {
            ObservableCollection<Mod> new_mods = Game.GetModsFromPath(this.GamePath);
            List<string> old_mods_name = this.GameMods.Select(e => e.folder_name).ToList();
            List<string> new_mods_name = new_mods.Select(e => e.folder_name).ToList();
            List<string> new_mods_only = new_mods_name.Except(old_mods_name).ToList();

            Mod[] new_mods_to_append = new_mods.Where(e => new_mods_only.Contains(e.folder_name)).ToArray();
            foreach(Mod mod in new_mods_to_append)
                this.GameMods.Add(mod);
        }

        public void Install()
        {
            this.textProgress.Report("Patching catalog.json...");
            PatchGame();
            this.textProgress.Report("Restoring backups for original files...");
            RestoreBackup(); // Restore backup everytime mods get installed
            Dictionary<string, List<string>> keyValuePairs = new Dictionary<string, List<string>>();
            this.textProgress.Report("Scanning mods for files...");
            foreach (Mod mod in this.GameMods)
            {
                if (!mod.enabled)
                    continue;
                this.textProgress.Report($"Scanning {mod.Name}...");
                CheckDirForMods(mod.mod_path, mod.mod_path, ref keyValuePairs);
            }
            CopyOriginalToBackup(keyValuePairs);
            foreach (KeyValuePair<string, List<string>> entry in keyValuePairs)
            {
                var source_path = Path.Combine(this.GamePath, entry.Key);
                // If its not a bundle file, then we can't merge it. Meaning we just copy the highest priority mod
                if (!entry.Key.EndsWith(".bundle"))
                {
                    this.textProgress.Report("Not a .bundle file, so just copying the highest priority mod file over");
                    this.textProgress.Report($"Copying {entry.Value[0]} to {source_path}");
                    File.Copy(entry.Value[0], source_path, true);
                }
                else
                {
                    this.textProgress.Report("Found .bundle file! Merging all bundle files into one...");
                    // Reversing so that higher priority mods will be processed last (and thus overwriting previous mods)
                    entry.Value.Reverse();
                    CreateNewCombinedBundle(source_path, entry.Value, source_path, this.textProgress);
                }
            }
            this.textProgress.Report("Mods installation complete!");
        }
    }
}
