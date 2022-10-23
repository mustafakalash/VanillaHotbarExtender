using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace VanillaHotbarExtender.Windows;

public class ConfigWindow : Window, IDisposable {
    public const string NAME = "Vanilla Hotbar Extender";
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base(ConfigWindow.NAME) {
        this.Size = new Vector2(600, 800);
        this.SizeCondition = ImGuiCond.Appearing;

        this.Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw() {
        ImGui.Text("Hotbars");
        ImGui.Separator();
        ImGui.Spacing();
        int id = 0;
        foreach(var hotbar in this.Configuration.Hotbars) {
            string inputText = hotbar.Key;
            ImGui.Text("Name");
            ImGui.SameLine();
            if(ImGui.InputText($"##{id}", ref inputText, 32) && !String.IsNullOrWhiteSpace(inputText)) {
                this.Configuration.Hotbars.Remove(hotbar.Key);
                this.Configuration.Hotbars.Add(inputText, hotbar.Value);
                this.Configuration.Save();
            }
            ImGui.SameLine();
            if(ImGui.Button($"Delete##{id}")) {
                this.Configuration.Hotbars.Remove(hotbar.Key);
                this.Configuration.Save();
            }
            id++;
        }
    }
}
