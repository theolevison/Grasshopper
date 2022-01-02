using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceGlow : MonoBehaviour
{
    [SerializeField] GameObject[] sideFaces;
    DiceStat diceStat;
    // Start is called before the first frame update
    void Start()
    {
        diceStat = gameObject.GetComponent<DiceStat>();
    }

    // Update is called once per frame
    void Update()
    {
        HighlightSides();
    }

    void HighlightSides(){
        for (int i = 0; i < sideFaces.Length; i++)
        {
            sideFaces[i].SetActive(false);
        }
        sideFaces[diceStat.side-1].SetActive(true);
    }
}
