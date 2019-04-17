using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class ConfigSerCsv<T> : IEnumerable, IEnumerable<T> where T : ConfigBase, new()
{
    private Dictionary<string, T> configs;
    private List<T> configList;
    private string fileName;

    public ConfigSerCsv(string fileName, bool load = true) : base()
    {
        //这里开始获取数据
        configList = new List<T>();
        configs = new Dictionary<string, T>();
        fileName = fileName;

        if (ConfigManager.isResourcesLoad)
        {
            this.ParseConfig(fileName);
        }
        else
        {
            this.ParseConfigMobile(fileName);
        }

        foreach (KeyValuePair<string, T> pair in configs)
        {
            configList.Add(pair.Value);
        }
    }

    //电脑端解析 //编辑器模式，直接读取txt解析
    private void ParseConfig(string configName)
    {
        string path = Application.dataPath.Replace("Assets", "config/" + configName + ".txt");
        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
        string conts = sr.ReadToEnd();
        fs.Close();
        Regex regex = new Regex("\"[^\"]*\"");
        IEnumerable result = regex.Matches(conts);
        IEnumerator iter = result.GetEnumerator();
        while (iter.MoveNext())
        {
            string hit = iter.Current.ToString();
            if (hit.Contains("\n"))
            {
                conts = conts.Replace(hit, "---格式化替换---");
            }
        }

        string[] _lines = conts.Split('\n'); //分行
        string[] _names = _lines[0].Split('\t');
        string[] _rules = _lines[1].Split('\t'); //第二行拆分

        List<int> ikey = new List<int>();
        for (int n = 0; n < _rules.Length; n++)
        {
            string str = _rules[n];
            if (str.Contains("k"))
            {
                ikey.Add(n);
            }

            if (str.Contains("_"))
                _rules[n] = str.Split('_')[0];
        }

        bool isWarn = false;
        for (int n = 3; n < _lines.Length; n++)
        {
            T ttt = new T();
            string line = _lines[n];
            System.Type iType = ttt.GetType();
            FieldInfo[] fileds = iType.GetFields();

            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            string[] splitLine = line.Split('\t'); //拆分行

            if (ikey.Count > 0) //合并文本作为key
            {
                StringBuilder sb = new StringBuilder();
                foreach (int k in ikey)
                {
                    if (k >= splitLine.Length)
                    {
                        Debug.LogErrorFormat("获取key失败: {0} {1}行", configName, n);
                    }
                    sb.Append(splitLine[k]);
                    sb.Append(",");
                }
                ttt.key = sb.ToString().TrimEnd(',');
            }
            else
            {
                ttt.key = splitLine[0]; //第一位作为key
            }

            //遍历成员/配置字段
            for (int m = 0; m < fileds.Length; m++)
            {
                if (m >= splitLine.Length) { break; }
                string fName = fileds[m].Name;
                if (fName == "key")
                    continue;

                //根据名字找对应字段
                int valueIndex = -1;
                for (int index = 0; index < _names.Length; index++)
                {
                    string cfgFName = _names[index].Trim();
                    if (cfgFName == fName)
                    {

                        valueIndex = index;
                        break;
                    }
                }

                if (valueIndex == -1)
                {
                    UnityEngine.Debug.LogError("配置找不到字段: " + fileName + "." + fName);
                    continue;
                }
                //检查类型
                object convert = CheckType(_rules[valueIndex], splitLine[valueIndex], configName);
                //反射赋值给类
                try
                {
                    fileds[m].SetValue(ttt, convert);
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("配置对应代码类型错误,请重新生成代码: ConfigSerData{0}.{1}", configName, fileds[m].Name);
                    throw ex;
                }
            }

            if (!configs.ContainsKey(ttt.key))
            {
                configs.Add(ttt.key, ttt); //存入字典
            }
            else
            {
                if (!isWarn)
                {
                    Debug.LogError("配置表ID重复，请修改: " + configName + "." + ttt.key);
                    isWarn = true;
                }
            }
        }
    }


    private object CheckType(string rule, string content, string configName)
    {
        if (string.IsNullOrEmpty(content))
            return null;
        content = content.Trim();
        if (content.Contains("\""))
        {
            content = content.Replace("\"", "");
        }

        if (rule.Contains("[]"))
        {
            if (rule.Contains("int"))
            {
                if (string.IsNullOrEmpty(content))
                    return new int[] { };
                string[] args = content.Split(';');
                //return System.Array.ConvertAll<string, int>(args, s => int.Parse(s));
                List<int> list = new List<int>();
                for (int n = 0; n < args.Length; n++)
                {
                    int tmp = 0;
                    if (int.TryParse(args[n], out tmp))
                    {
                        list.Add(tmp);
                    }
                }
                return list.ToArray();
            }
            else if (rule.Contains("float"))
            {
                if (string.IsNullOrEmpty(content))
                    return new float[] { };
                string[] args = content.Split(';');
                //return System.Array.ConvertAll<string, float>(args, s => float.Parse(s));
                List<float> list = new List<float>();
                for (int n = 0; n < args.Length; n++)
                {
                    float tmp = 0;
                    if (float.TryParse(args[n], out tmp))
                    {
                        list.Add(tmp);
                    }
                }
                return list.ToArray();
            }
            else
            {
                if (string.IsNullOrEmpty(content))
                    return new string[] { };
                return content.Split(';');
            }
        }
        else
        {
            if (rule.Contains("int"))
            {
                if (string.IsNullOrEmpty(content))
                    return 0;
                int result = 0;
                if (int.TryParse(content, out result))
                    return result;
                else
                {
                    Debug.LogError("Parse int failed：" + configName + "." + content);
                    return 0;
                }
            }
            else if (rule.Contains("float"))
            {
                if (string.IsNullOrEmpty(content))
                    return 0;
                float result = 0;
                if (float.TryParse(content, out result))
                    return result;
                else
                {
                    Debug.LogError("Parse float failed：" + configName + "." + content);
                    return 0;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(content))
                    return "";
                return content;
            }
        }
    }

    //移动端解析
    private void ParseConfigMobile(string configName)
    {
        byte[] row = null;
        ConfigManager.Instance.GetConfigText(configName, ref row);

        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(row);
        List<T> list = (List<T>)bf.Deserialize(ms);
        configList = list;
        configs = new Dictionary<string, T>();
        for (int n = 0; n < list.Count; n++)
        {
            configs.Add(list[n].key, list[n]);
        }
    }

    public List<T> GetCopy()
    {
        List<T> clone = new List<T>();
        for (int n = 0; n < configList.Count; n++)
        {
            clone.Add(configList[n]);
        }
        return clone;
    }


    public IEnumerator<T> GetEnumerator()
    {
        return configs.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return configs.Values.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return configs.Values.GetEnumerator();
    }
}
