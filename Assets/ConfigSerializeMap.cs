using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ConfigSerializeMap
{
    public Dictionary<string, string> dic = new Dictionary<string, string>();
    public ConfigSerializeMap()
    {
        ConfigSerCsv<ConfigSerDataAudio> config_Audio = new ConfigSerCsv<ConfigSerDataAudio>("Audio");
        FlushToLocal("Audio", config_Audio.GetCopy());
    }


    public void FlushToLocal(string name, object obj)
    {
        string resConfigPath = Application.dataPath + "/ResourcesAb/config/";

        if (!Directory.Exists(resConfigPath))
            Directory.CreateDirectory(resConfigPath);
        resConfigPath = resConfigPath + name + ".txt";
        BinaryFormatter bf = new BinaryFormatter();

        FileStream fs = new FileStream(resConfigPath, FileMode.OpenOrCreate);
        bf.Serialize(fs, obj);
        fs.Flush();
        fs.Close();

        if (!dic.ContainsKey(name))
        {
            dic.Add(name, resConfigPath);
        }
    }

}
