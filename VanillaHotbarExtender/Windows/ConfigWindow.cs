using System;
using System.Buffers.Text;
using System.IO;
using System.Numerics;
using System.Text.Unicode;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Newtonsoft.Json;

namespace VanillaHotbarExtender.Windows;

public class ConfigWindow : Window, IDisposable
{
    public const string NAME = "Vanilla Hotbar Extender";
    private Configuration Configuration;
    private string outputCode = "";
    private string inputCode = "";
    private static JsonSerializer Json = JsonSerializer.CreateDefault();

    public ConfigWindow(Plugin plugin) : base(ConfigWindow.NAME)
    {
        this.Size = new Vector2(600, 800);
        this.SizeCondition = ImGuiCond.Appearing;

        this.Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Hotbars");
        ImGui.Separator();
        if (ImGui.Button("Import"))
        {
            ImGui.OpenPopup("vhe_import_popup");
        }

        if (ImGui.BeginPopup("vhe_import_popup"))
        {
            ImGui.InputText("Hotbar code", ref inputCode, 9999999);
            if (ImGui.Button("Import"))
            {
                var bytes = Convert.FromBase64String(inputCode);
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                var hotbar = Json.Deserialize<HotbarSlotSave[]>(new JsonTextReader(new StringReader(json)))!;
                this.Configuration.Hotbars.Add($"import-{DateTimeOffset.Now.ToUnixTimeMilliseconds()}", hotbar);
                Configuration.Save();
            }

            ImGui.EndPopup();
        }

        ImGui.Separator();
        ImGui.Spacing();
        int id = 0;
        foreach (var hotbar in this.Configuration.Hotbars)
        {
            string inputText = hotbar.Key;
            ImGui.Text("Name");
            ImGui.SameLine();
            if (ImGui.InputText($"##{id}", ref inputText, 32) && !String.IsNullOrWhiteSpace(inputText))
            {
                this.Configuration.Hotbars.Remove(hotbar.Key);
                this.Configuration.Hotbars.Add(inputText, hotbar.Value);
                this.Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGui.Button($"Delete##{id}"))
            {
                this.Configuration.Hotbars.Remove(hotbar.Key);
                this.Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGui.Button($"Export##{id}"))
            {
                var st = new StringWriter();
                Json.Serialize(st, hotbar.Value);
                var bytes = System.Text.Encoding.UTF8.GetBytes(st.ToString());
                outputCode = Convert.ToBase64String(bytes);

                ImGui.OpenPopup($"vhe_export_{id}");
            }

            if (ImGui.BeginPopup($"vhe_export_{id}"))
            {
                ImGui.InputText("", ref outputCode, 9999999);
                ImGui.EndPopup();
            }

            id++;
        }
    }
}
