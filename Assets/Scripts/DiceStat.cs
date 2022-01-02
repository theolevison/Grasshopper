using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceStat : MonoBehaviour
{
    [SerializeField] Transform[] diceSides;
    public int side = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckDiceSide();
    }

    void CheckDiceSide(){
        //find face value that's the highest
        for (int i = 0; i < diceSides.Length; i++ ){
            if (diceSides[i].position.y > diceSides[side - 1].position.y){
                side = i + 1;
            }
        }
    }
}
