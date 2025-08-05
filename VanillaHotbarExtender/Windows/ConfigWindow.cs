using System;
using System.Buffers.Text;
using System.IO;
using System.Numerics;
using System.Text.Unicode;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using Newtonsoft.Json;
using Dalamud.Interface.ImGuiNotification;

namespace VanillaHotbarExtender.Windows;

public class ConfigWindow : Window, IDisposable
{
    public const string NAME = "Vanilla Hotbar Extender";
    private Configuration Configuration;
    private Plugin plugin;
    private static JsonSerializer Json = JsonSerializer.CreateDefault();

    public ConfigWindow(Plugin plugin) : base(ConfigWindow.NAME) {
        this.Size = new Vector2(600, 800);
        this.SizeCondition = ImGuiCond.Appearing;

        this.plugin = plugin;
        this.Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw() {
        ImGui.Text("Hotbars");
        ImGui.Separator();

        if(ImGui.Button("Import")) {
            try {
                var inputCode = ImGui.GetClipboardText();
                var bytes = Convert.FromBase64String(inputCode);
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                var hotbar = Json.Deserialize<HotbarSlotSave[]>(new JsonTextReader(new StringReader(json)))!;
                this.Configuration.Hotbars.Add($"import-{DateTimeOffset.Now.ToUnixTimeMilliseconds()}", hotbar);
                Configuration.Save();
                var notification = new Notification() {
                    Content = "Hot bar imported from clipboard!",
                    Type = NotificationType.Success
                };
                plugin.NotificationManager.AddNotification(notification);
            } catch (Exception e) when (e is FormatException || e is JsonReaderException) {
                var notification = new Notification() {
                    Content = "Hot bar import failed: invalid code.",
                    Type = NotificationType.Error
                };
                plugin.NotificationManager.AddNotification(notification);
            }
        }

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

            ImGui.SameLine();
            if(ImGui.Button($"Export##{id}")) {
                var st = new StringWriter();
                Json.Serialize(st, hotbar.Value);
                var bytes = System.Text.Encoding.UTF8.GetBytes(st.ToString());
                ImGui.SetClipboardText(Convert.ToBase64String(bytes));

                var notification = new Notification() {
                    Content = "Hot bar exported to clipboard!",
                    Type = NotificationType.Success
                };
                plugin.NotificationManager.AddNotification(notification);
            }

            id++;
        }
    }
}
