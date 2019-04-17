using UnityEngine;
using System.Collections;

public class GameApp : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        ConfigSerCsv<ConfigSerDataAudio> msConfSerAudio = new ConfigSerCsv<ConfigSerDataAudio>("Audio");

        foreach(var item in msConfSerAudio)
        {
            Debug.Log(item.ID);
            Debug.Log(item.key);
        }

        ConfigSerializeMap map = new ConfigSerializeMap();
    }
	

}
