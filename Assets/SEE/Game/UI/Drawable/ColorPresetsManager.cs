﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// File from the hsvpicker package 
/// https://github.com/judah4/HSV-Color-Picker-Unity/blob/master/Packages/com.judahperez.hsvcolorpicker/UI/ColorPresetManager.cs
/// added remove color and change color
/// </summary>
public static class ColorPresetManager
{
    private static Dictionary<string, ColorPresetList> _presets = new Dictionary<string, ColorPresetList>();

    public static ColorPresetList Get(string listId = "default")
    {
        ColorPresetList preset;
        if (!_presets.TryGetValue(listId, out preset))
        {
            preset = new ColorPresetList(listId);
            _presets.Add(listId, preset);
        }

        return preset;
    }


}

public class ColorPresetList
{
    public string ListId { get; private set; }
    public List<Color> Colors { get; private set; }

    public event UnityAction<List<Color>> OnColorsUpdated;

    public ColorPresetList(string listId, List<Color> colors = null)
    {
        if (colors == null)
        {
            colors = new List<Color>();
        }

        Colors = colors;
        ListId = listId;
    }

    public void AddColor(Color color)
    {
        Colors.Add(color);
        if (OnColorsUpdated != null)
        {
            OnColorsUpdated.Invoke(Colors);
        }
    }

    public void RemoveColor(Color color)
    {
        Colors.Remove(color);
        if (OnColorsUpdated != null)
        {
            OnColorsUpdated.Invoke(Colors);
        }
    }

    public void ChangeColor(int index, Color newColor)
    {
        Colors[index] = newColor;
        if (OnColorsUpdated != null)
        {
            OnColorsUpdated.Invoke(Colors);
        }
    }

    public void UpdateList(IEnumerable<Color> colors)
    {
        Colors.Clear();
        Colors.AddRange(colors);

        if (OnColorsUpdated != null)
        {
            OnColorsUpdated.Invoke(Colors);
        }
    }


}

