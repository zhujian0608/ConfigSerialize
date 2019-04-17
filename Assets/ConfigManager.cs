using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConfigManager
{
    public static bool isResourcesLoad = true;
    Dictionary<string, TextAsset> configTextDic = new Dictionary<string, TextAsset>();
    Dictionary<string, string> configTextEditor = new Dictionary<string, string>();

    private static ConfigManager instance;
    public static ConfigManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new ConfigManager();
            }
            return instance;
        }
    }


    //移动端统一用
    public void AddConfigText(string configName, TextAsset configText)
    {
        if (!configTextDic.ContainsKey(configName))
        {
            configTextDic.Add(configName, configText);
        }
    }

    //电脑端json用
    public void AddConfigText(string configName, string configText)
    {
        if (!configTextEditor.ContainsKey(configName))
        {
            configTextEditor.Add(configName, configText);
        }
    }

    public string GetConfigText(string configName, ref byte[] arg)
    {
        TextAsset text;
        if (configTextDic.TryGetValue(configName, out text))
        {
            arg = text.bytes;
            return text.text;
        }

        UnityEngine.Debug.LogError(string.Format("Can't find config: {0}", configName));
        return null;
    }

    //json用
    public string GetConfigText(string configName)
    {
        TextAsset text;
        if (configTextDic.TryGetValue(configName, out text))
        {
            return text.text;
        }

        string text2;
        if (configTextEditor.TryGetValue(configName, out text2))
            return text2;

        return "";
    }
}
