using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulateDropdown : MonoBehaviour
{

	private Dropdown dd;
	public string resourceFolder = "Bricks";

	void Awake()
	{
		dd = GetComponent<Dropdown>();
		dd.ClearOptions();

		List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

		TextAsset[] tas = Resources.LoadAll<TextAsset>(resourceFolder);
		foreach (TextAsset ta in tas)
			options.Add(new Dropdown.OptionData(ta.name));

		ScriptableObject[] sos = Resources.LoadAll<ScriptableObject>(resourceFolder);
		foreach (ScriptableObject so in sos)
			options.Add(new Dropdown.OptionData(so.name));

		GameObject[] prefabs = Resources.LoadAll<GameObject>(resourceFolder);
		foreach (GameObject go in prefabs)
			options.Add(new Dropdown.OptionData(go.name));

		AudioClip[] clips = Resources.LoadAll<AudioClip>(resourceFolder);
		foreach (AudioClip c in clips)
			options.Add(new Dropdown.OptionData(c.name));

		dd.AddOptions(options);
	}

}
