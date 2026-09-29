using System;
using System.Reflection;
using MelonLoader;
using ModSettings;
using UnityEngine;

[assembly: MelonInfo(typeof(BuffNotificationMover.Main), "Buff Notification Mover", "1.0.0", "KryPot")]

namespace BuffNotificationMover;

internal class BuffNotificationSettings : JsonModSettings
{
    [Section("Buff notification position")]
    [Name("X position")]
    [Description("Horizontal position of the buff/status notification.")]
    [Slider(-525, 525, 1051)]
    public int positionX = 0;

    [Name("Y position")]
    [Description("Vertical position of the buff/status notification.")]
    [Slider(-350, 235, 586)]
    public int positionY = 0;
}

internal static class Settings
{
    public static BuffNotificationSettings options;

    public static void OnLoad()
    {
        options = new BuffNotificationSettings();
        options.AddToModSettings("Buff Notification Mover");
    }
}

public class Main : MelonMod
{
    private object _panelHud;
    private GameObject _buffParent;
    private float _scanTimer;

    public override void OnApplicationStart()
    {
        Settings.OnLoad();
    }

    public override void OnUpdate()
    {
        if (_buffParent == null)
        {
            _scanTimer += Time.unscaledDeltaTime;

            if (_scanTimer < 2f)
                return;

            _scanTimer = 0f;
            FindPanelHUD();

            return;
        }

        ApplyPosition();
    }

    private void FindPanelHUD()
    {
        try
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go == null || !go.scene.IsValid())
                    continue;

                foreach (Component component in go.GetComponents<Component>())
                {
                    if (component == null || component.GetType().Name != "Panel_HUD")
                        continue;

                    _panelHud = component;
                    _buffParent = GetMember(_panelHud, "m_BuffNotificationParent") as GameObject;

                    if (_buffParent == null)
                        return;

                    ApplyPosition();

                    MelonLogger.Msg(
                        $"Buff notification mover loaded. Position: {_buffParent.transform.localPosition}"
                    );

                    return;
                }
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to find Panel_HUD: {ex}");
        }
    }

    private void ApplyPosition()
    {
        if (_buffParent == null || Settings.options == null)
            return;

        Vector3 targetPosition = new(
            Settings.options.positionX,
            Settings.options.positionY,
            0f
        );

        if (_buffParent.transform.localPosition != targetPosition)
            _buffParent.transform.localPosition = targetPosition;
    }

    private static object GetMember(object obj, string name)
    {
        if (obj == null)
            return null;

        Type type = obj.GetType();

        PropertyInfo property = type.GetProperty(
            name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        if (property != null)
            return property.GetValue(obj);

        FieldInfo field = type.GetField(
            name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        return field?.GetValue(obj);
    }
}
