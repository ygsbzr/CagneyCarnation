﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using ModCommon.Util;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CagneyCarnation
{
    public class CagneyCarnation : Mod, ITogglableMod
    {
        public static CagneyCarnation Instance;
        
        public static string ArenaAssetsPath;
        public static Dictionary<string, AssetBundle> Bundles = new Dictionary<string, AssetBundle>();
        public static Dictionary<string, GameObject> GameObjects = new Dictionary<string, GameObject>();
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        
        public static AudioClip Music;
        
        public override ModSettings SaveSettings
        {
            get => _settings;
            set => _settings = value as SaveSettings ?? _settings;
        }

        private SaveSettings _settings = new SaveSettings();
        
        public override string GetVersion()
        {
            return "1.0.0";
        }

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("GG_Hornet_2", "Boss Holder/Hornet Boss 2"),
                ("GG_Hornet_2", "Boss Scene Controller"),
                ("GG_Hornet_2", "_SceneManager"),
            };
        }
        
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            GameObjects.Add("Hornet", preloadedObjects["GG_Hornet_2"]["Boss Holder/Hornet Boss 2"]);
            GameObjects.Add("BSC", preloadedObjects["GG_Hornet_2"]["Boss Scene Controller"]);
            GameObjects.Add("SM", preloadedObjects["GG_Hornet_2"]["_SceneManager"]);

            Instance = this;

            GetResources();
            LoadAssets();

            Unload();
            
            ModHooks.Instance.AfterSavegameLoadHook += AfterSaveGameLoad;
            ModHooks.Instance.GetPlayerVariableHook += GetVariableHook;
            ModHooks.Instance.LanguageGetHook += LangGet;
            ModHooks.Instance.NewGameHook += AddComponent;
            ModHooks.Instance.SetPlayerVariableHook += SetVariableHook;
        }

        private void AfterSaveGameLoad(SaveGameData data) => AddComponent();

        private object GetVariableHook(Type t, string key, object orig)
        {
            if (key == "statueStateFlower")
            {
                return _settings.CompletionFlower;
            }
            
            return orig;
        }
        
        private string LangGet(string key, string sheettitle)
        {
            switch (key)
            {
                case "FLOWER_NAME": return "Cagney Carnation";
                case "FLOWER_DESC": return "Hostile god of the meadow.";
                default: return Language.Language.GetInternal(key, sheettitle);
            }
        }
        
        private void AddComponent()
        {
            GameManager.instance.gameObject.AddComponent<ArenaFinder>();
            GameManager.instance.gameObject.AddComponent<SceneLoader>();
        }
        
        private object SetVariableHook(Type t, string key, object obj)
        {
            if (key == "statueStateFlower")
            {
                _settings.CompletionFlower = (BossStatue.Completion) obj;
            }
            
            return obj;
        }
        
        private void GetResources()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (string res in asm.GetManifestResourceNames())
            {
                using (Stream s = asm.GetManifestResourceStream(res))
                {
                    string bundleName = Path.GetExtension(res).Substring(1);
                    Bundles[bundleName] = AssetBundle.LoadFromStream(s);
                }
            }
        }

        private void LoadAssets()
        {
            string flowerAssetsPath;
            switch (SystemInfo.operatingSystemFamily)
            {
                case OperatingSystemFamily.Windows:
                    flowerAssetsPath = "flowerwin";
                    break;
                case OperatingSystemFamily.Linux:
                    flowerAssetsPath = "flowerlin";
                    break;
                case OperatingSystemFamily.MacOSX:
                    flowerAssetsPath = "flowermac";
                    break;
                default:
                    Log("ERROR UNSUPPORTED SYSTEM: " + SystemInfo.operatingSystemFamily);
                    return;
            }
            
            switch (SystemInfo.operatingSystemFamily)
            {
                case OperatingSystemFamily.Windows:
                    ArenaAssetsPath = "arenawin";
                    break;
                case OperatingSystemFamily.Linux:
                    ArenaAssetsPath = "arenalin";
                    break;
                case OperatingSystemFamily.MacOSX:
                    ArenaAssetsPath = "arenamac";
                    break;
                default:
                    Log("ERROR UNSUPPORTED SYSTEM: " + SystemInfo.operatingSystemFamily);
                    return;
            }

            AssetBundle flowerBundle = Bundles[flowerAssetsPath];
            GameObjects["Flower"] = flowerBundle.LoadAsset<GameObject>("Cagney Carnation");
            Music = flowerBundle.LoadAsset<AudioClip>("MUS_Flower");
            Textures["Mugshot"] = flowerBundle.LoadAsset<Texture2D>("Flower Mugshot");
        }
        
        public void Unload()
        {
            ModHooks.Instance.AfterSavegameLoadHook -= AfterSaveGameLoad;
            ModHooks.Instance.GetPlayerVariableHook -= GetVariableHook;
            ModHooks.Instance.LanguageGetHook -= LangGet;
            ModHooks.Instance.SetPlayerVariableHook -= SetVariableHook;
            ModHooks.Instance.NewGameHook -= AddComponent;

            var finder = GameManager.instance?.gameObject.GetComponent<ArenaFinder>();
            if (finder == null)
            {
                return;
            }
            
            Object.Destroy(finder);
        }
    }
}