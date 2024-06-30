using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace VanillaHotbarExtender {
    [Serializable]
    public struct HotbarSlotSave {
        public uint CommandId;
        public RaptureHotbarModule.HotbarSlotType CommandType;

        public HotbarSlotSave(uint commandId, RaptureHotbarModule.HotbarSlotType commandType) {
            this.CommandId = commandId;
            this.CommandType = commandType;
        }
    }

    [Serializable]
    public class Configuration : IPluginConfiguration {
        public int Version { get; set; } = 1;

        public Dictionary<string, HotbarSlotSave[]> Hotbars { get; set; } = new Dictionary<string, HotbarSlotSave[]>();

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private IDalamudPluginInterface? PluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface) {
            this.PluginInterface = pluginInterface;
        }

        public void Save() {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
