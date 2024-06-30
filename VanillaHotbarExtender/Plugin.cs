using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using Dalamud.Interface.Windowing;
using VanillaHotbarExtender.Windows;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Dalamud.Game.Command;

namespace VanillaHotbarExtender {
    public unsafe sealed class Plugin : IDalamudPlugin {
        public string Name => "Vanilla Hotbar Extender";
        private const string COMMAND = "/vhe";
        private const string SAVE_COMMAND = "save";
        private const string LOAD_COMMAND = "load";
        private const string CLEAR_COMMAND = "clear";
        private const int HOTBAR_SIZE = 12;
        private const int HOTBAR_COUNT = 18;
        private RaptureHotbarModule* raptureHotbarModule = Framework.Instance()->UIModule->GetRaptureHotbarModule();
        private ConfigWindow configWindow { get; init; }

        private IDalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public IChatGui ChatGui { get; init; }
        public IClientState ClientState { get; init; }
        public WindowSystem WindowSystem = new("VanillaHotbarExtender");

        public Plugin(
            IDalamudPluginInterface pluginInterface,
            ICommandManager commandManager,
            IChatGui chatGui,
            IClientState clientState    
        ) {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.ChatGui = chatGui;
            this.ClientState = clientState;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            configWindow = new ConfigWindow(this);
            WindowSystem.AddWindow(configWindow);

            this.CommandManager.AddHandler(COMMAND, new CommandInfo(OnCommand) {
                HelpMessage = "Open plugin window\n" +
                              "/vhe {save | load} {hotbar number} {save name} → Save or load a hotbar\n" +
                              "/vhe clear {hotbar number} → Clear a hotbar\n"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose() {
            this.WindowSystem.RemoveAllWindows();
            this.CommandManager.RemoveHandler(COMMAND);
        }

        private bool validateArguments(string[] args) {
            var usage = "Usage: /vhe clear {hotbar number}";
            var requiredArgs = 2;

            if(args[0] == SAVE_COMMAND || args[0] == LOAD_COMMAND) {
                usage = " Usage: /vhe {save | load} {hotbar number} {save name}";
                requiredArgs = 3;
            } else if(args[0] != CLEAR_COMMAND) {
                this.ChatGui.PrintError("Invalid command. Usage: /vhe [save | load | clear]");
                return false;
            }

            if (args.Length < requiredArgs) {
                this.ChatGui.PrintError("Invalid arguments." + usage);
                return false;
            }

            uint hotbarNumber;
            bool isNum = uint.TryParse(args[1], out hotbarNumber);
            if(!isNum) {
                this.ChatGui.PrintError("Invalid hotbar number." + usage);
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
                configWindow.IsOpen = true;
            } else {
                string[] argsList = args.Split(" ");
                if (!validateArguments(argsList)) {
                    return;
                }

                if(argsList[0] == SAVE_COMMAND) {
                    
                    uint hotbarNumber = uint.Parse(argsList[1]) - 1;
                    string saveName = String.Join(" ", argsList[2..]);

                    HotbarSlotSave[] saveHotbars = new HotbarSlotSave[HOTBAR_SIZE];
                    for(uint i = 0; i < HOTBAR_SIZE; i++) {
                        RaptureHotbarModule.HotbarSlot* slot = raptureHotbarModule->GetSlotById(hotbarNumber, i);
                        saveHotbars[i] = new HotbarSlotSave(slot->CommandId, slot->CommandType);
                    }

                    this.Configuration.Hotbars[saveName] = saveHotbars;
                    this.Configuration.Save();
                    this.ChatGui.Print("Hotbar \"" + (hotbarNumber + 1) + "\" saved as \"" + saveName + "\".");
                } else if(argsList[0] == LOAD_COMMAND) {
                    if(!validateArguments(argsList)) {
                        return;
                    }

                    uint hotbarNumber = uint.Parse(argsList[1]) - 1;
                    string saveName = String.Join(" ", argsList[2..]);

                    if(!this.Configuration.Hotbars.ContainsKey(saveName)) {
                        this.ChatGui.PrintError("No hotbar saved with the name \"" + saveName + "\".");
                        return;
                    }

                    HotbarSlotSave[] hotbar = this.Configuration.Hotbars[saveName];
                    for (uint i = 0; i < HOTBAR_SIZE; i++) {
                        RaptureHotbarModule.HotbarSlot* gameSlot = raptureHotbarModule->GetSlotById(hotbarNumber, i);
                        gameSlot->Set(hotbar[i].CommandType, hotbar[i].CommandId);
                        raptureHotbarModule->WriteSavedSlot(raptureHotbarModule->ActiveHotbarClassJobId, hotbarNumber, i, gameSlot, false, ClientState.IsPvP);
                    }
                } else if(argsList[0] == CLEAR_COMMAND) {
                    if(!validateArguments(argsList)) {
                        return;
                    }

                    uint hotbarNumber = uint.Parse(argsList[1]) - 1;

                    for (uint i = 0; i < HOTBAR_SIZE; i++) {
                        RaptureHotbarModule.HotbarSlot* gameSlot = raptureHotbarModule->GetSlotById(hotbarNumber, i);
                        gameSlot->Set(RaptureHotbarModule.HotbarSlotType.Empty, 0);
                        raptureHotbarModule->WriteSavedSlot(raptureHotbarModule->ActiveHotbarClassJobId, hotbarNumber, i, gameSlot, false, ClientState.IsPvP);
                    }
                }
            }
        }

        private void DrawUI() {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI() {
            configWindow.IsOpen = true;
        }
    }
}
