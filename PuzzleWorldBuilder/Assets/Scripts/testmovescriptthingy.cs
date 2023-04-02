using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testmovescriptthingy : MonoBehaviour
{
    // Update is called once per frame
    [SerializeField] float directionStrenght;
    [SerializeField] float rotationStrength;
    void Update()
    {
        if (Input.GetKey(KeyCode.I))
        {
            transform.position += transform.up * directionStrenght * Time.deltaTime;
            transform.localEulerAngles += new Vector3(0, rotationStrength * Time.deltaTime, 0);
        }
    }
}
