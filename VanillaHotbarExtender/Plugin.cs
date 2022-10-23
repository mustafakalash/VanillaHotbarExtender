using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Gui;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System;
using Dalamud.Interface.Windowing;
using VanillaHotbarExtender.Windows;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace VanillaHotbarExtender {
    public unsafe sealed class Plugin : IDalamudPlugin {
        public string Name => "Vanilla Hotbar Extender";
        private const string COMMAND = "/vhe";
        private const string SAVE_COMMAND = "save";
        private const string LOAD_COMMAND = "load";
        private const int HOTBAR_SIZE = 12;
        private const int HOTBAR_COUNT = 18;
        private HotBars* Hotbars = &Framework.Instance()->UIModule->GetRaptureHotbarModule()->HotBar;

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public ChatGui ChatGui { get; init; }
        public WindowSystem WindowSystem = new("VanillaHotbarExtender");

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] ChatGui chatGui) {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.ChatGui = chatGui;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            WindowSystem.AddWindow(new ConfigWindow(this));

            this.CommandManager.AddHandler(COMMAND, new CommandInfo(OnCommand) {
                HelpMessage = "Open plugin window\n" +
                              "/vhe {save | load} {hotbar number} {save name} → Save or load a hotbar"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose() {
            this.WindowSystem.RemoveAllWindows();
            this.CommandManager.RemoveHandler(COMMAND);
        }

        private HotBarSlots* GetHotbar(int hotbarNumber) {
            if (hotbarNumber < 1 || hotbarNumber > HOTBAR_COUNT) {
                throw new System.ArgumentOutOfRangeException(nameof(hotbarNumber), $"Hotbar number must be between 1 and {HOTBAR_COUNT}.");
            }

            return &(*this.Hotbars)[hotbarNumber - 1]->Slot;
        }

        private void SetHotbar(int hotbarNumber, HotbarSlotSave[] hotbar) {
            if (hotbarNumber < 1 || hotbarNumber > HOTBAR_COUNT) {
                throw new System.ArgumentOutOfRangeException(nameof(hotbarNumber), $"Hotbar number must be between 1 and {HOTBAR_COUNT}.");
            }

            HotBarSlots* gameHotbar = GetHotbar(hotbarNumber);
            for(int i = 0; i < HOTBAR_SIZE; i++) {
                HotBarSlot* gameSlot = (*gameHotbar)[i];
                gameSlot->Set(hotbar[i].CommandType, hotbar[i].CommandId);
            }
        }

        private bool validateArguments(string[] args) {
            if(args.Length < 3) {
                this.ChatGui.PrintError("Invalid arguments. Usage: /vhe {save | load} {hotbar number} {save name}");
                return false;
            }

            int hotbarNumber;
            bool isNum = int.TryParse(args[1], out hotbarNumber);
            if(!isNum) {
                this.ChatGui.PrintError("Invalid hotbar number. Usage: /vhe {save | load} {hotbar number} {save name}");
                return false;
            }
            if(hotbarNumber < 1 || hotbarNumber > HOTBAR_COUNT) {
                this.ChatGui.PrintError($"Invalid hotbar number. Must be between 1 and {HOTBAR_COUNT}.");
                return false;
            }

            return true;
        }

        private void OnCommand(string command, string args) {
            args = args.ToLower();
            if(String.IsNullOrEmpty(args)) {
                WindowSystem.GetWindow(ConfigWindow.NAME)!.IsOpen = true;
            } else {
                string[] argsList = args.Split(" ");
                if(argsList[0] == SAVE_COMMAND) {
                    if(!validateArguments(argsList)) {
                        return;
                    }

                    int hotbarNumber = int.Parse(argsList[1]);
                    string saveName = String.Join(" ", argsList[2..]);

                    HotBarSlots* gameHotbar = GetHotbar(hotbarNumber);
                    HotbarSlotSave[] saveHotbars = new HotbarSlotSave[HOTBAR_SIZE];
                    for(int i = 0; i < HOTBAR_SIZE; i++) {
                        HotBarSlot* slot = (*gameHotbar)[i];
                        saveHotbars[i] = new HotbarSlotSave(slot->CommandId, slot->CommandType);
                    }

                    this.Configuration.Hotbars[saveName] = saveHotbars;
                    this.Configuration.Save();
                    this.ChatGui.Print("Hotbar saved.");
                } else if(argsList[0] == LOAD_COMMAND) {
                    if(!validateArguments(argsList)) {
                        return;
                    }

                    int hotbarNumber = int.Parse(argsList[1]);
                    string saveName = String.Join(" ", argsList[2..]);

                    if(!this.Configuration.Hotbars.ContainsKey(saveName)) {
                        this.ChatGui.PrintError("No hotbar saved with that name.");
                        return;
                    }

                    this.SetHotbar(hotbarNumber, this.Configuration.Hotbars[saveName]);
                } else {
                    this.ChatGui.PrintError("Invalid command. Usage: /vhe [save | load]");
                }
            }
        }

        private void DrawUI() {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI() {
            WindowSystem.GetWindow(ConfigWindow.NAME)!.IsOpen = true;
        }
    }
}
