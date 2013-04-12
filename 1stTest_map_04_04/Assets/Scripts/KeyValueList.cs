using UnityEngine;
using System.Collections.Generic;

public class KeyValueList : List<KeyValuePair<string,string>>
{
	public void Add(string key, string value)
	{
		KeyValuePair<string,string> element = new KeyValuePair<string, string>(key, value);
		this.Add(element);
		
	}
}

