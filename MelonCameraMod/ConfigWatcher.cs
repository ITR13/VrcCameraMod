﻿using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using MelonLoader.TinyJSON;
using UnityEngine;

namespace MelonCameraMod
{
    static class ConfigWatcher
    {
        private const string FileName = "CameraConfig.json";

        private static readonly string FileDirectory =
            Application.dataPath + "/../";

        private static readonly string FullPath = FileDirectory + FileName;

        public static List<CameraConfig> CameraConfigs =
            new List<CameraConfig>();

        private static readonly FileSystemWatcher FileSystemWatcher;
        private static bool _dirty = true;

        static ConfigWatcher()
        {
            FileSystemWatcher = new FileSystemWatcher(FileDirectory, FileName)
            {
                NotifyFilter = (NotifyFilters)((1 << 9) - 1),
                EnableRaisingEvents = true
            };
            FileSystemWatcher.Changed += (_, __) => _dirty = true;
        }

        public static void Unload()
        {
            FileSystemWatcher.EnableRaisingEvents = false;
            _dirty = false;
        }

        public static bool UpdateIfDirty()
        {
            if (!_dirty) return false;
            _dirty = false;

            if (!File.Exists(FullPath))
            {
                MelonModLogger.Log(
                    $"Creating default config file at \"{FullPath}\""
                );
                var sampleConfig = new List<CameraConfig>
                {
                    new CameraConfig(),
                    new CameraConfig
                    {
                        Aspect = 1,
                        LocalRotation = Quaternion.identity,
                        Rect = new Rect(0, 0, 0.25f, 0.25f),
                        UseAspect = true,
                        UseRotation = true,
                    }
                };

                var json = JSON.Dump(
                    sampleConfig,
                    EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints
                );
                File.WriteAllText(FullPath, json);
                return false;
            }

            MelonModLogger.Log("Updating camera configs");

            CameraConfigs.Clear();

            try
            {
                var json = File.ReadAllText($"./{FileName}");
                JSON.MakeInto(JSON.Load(json), out CameraConfigs);
            }
            catch (Exception e)
            {
                MelonModLogger.LogError(e.ToString());
            }

            return true;
        }
    }
}