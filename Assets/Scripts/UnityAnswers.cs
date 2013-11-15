using UnityEngine;
using System.Collections;

public class UnityAnswers : MonoBehaviour {

	public GameObject spiderPrefab;

	private int SpiderAmount = 0;

	// Use this for initialization
	void Start()
	{

	}
	
	// Update is called once per frame
	void Update()
	{

		if(Input.GetKeyDown(KeyCode.Space))
		{
			SpiderAmount = 2;
		}

		if(SpiderAmount > 0)
		{
			SpawnSpiders();
		}
	}

	void SpawnSpiders()
	{
		for (int i = 0; i < SpiderAmount; ++i)
		{
			Instantiate(spiderPrefab, new Vector3 (Random.Range(-2.0f,2.0f), Random.Range(-2.0f,2.0f), 0f), Quaternion.identity);
			print (i);
		}

		SpiderAmount = 0;
	}

}
