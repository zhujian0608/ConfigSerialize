using System;
[System.Serializable]
public class ConfigSerDataAudio : ConfigBase
{
    public ConfigSerDataAudio() : base() { }
    public int ID;
    public string Path;
    public float Volume;
    public string Remark;
    public ConfigSerDataAudio CopyData()
    {
        ConfigSerDataAudio clone = new ConfigSerDataAudio();
        clone.ID = ID;
        clone.Path = Path;
        clone.Volume = Volume;
        clone.Remark = Remark;
        return clone;
    }
}
