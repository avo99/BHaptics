using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bhaptics.SDK2.Glove;
using Bhaptics.SDK2;

public class HapticsManager : MonoBehaviour
{
    public bool rightHand = true;

    private GameObject R_Image_thumb;
    private GameObject R_Image_index;
    private GameObject R_Image_middle;
    private GameObject R_Image_ring;
    private GameObject R_Image_pinky;

    private GameObject L_Image_thumb;
    private GameObject L_Image_index;
    private GameObject L_Image_middle;
    private GameObject L_Image_ring;
    private GameObject L_Image_pinky;
    private GameObject[] R_Images = { null, null, null, null, null };
    private GameObject[] L_Images = { null, null, null, null, null };

    void Start()
    {
        R_Image_thumb = GameObject.Find("R_thumb");
        R_Image_index = GameObject.Find("R_index");
        R_Image_middle = GameObject.Find("R_middle");
        R_Image_ring = GameObject.Find("R_ring");
        R_Image_pinky = GameObject.Find("R_pinky");

        L_Image_thumb = GameObject.Find("L_thumb");
        L_Image_index = GameObject.Find("L_index");
        L_Image_middle = GameObject.Find("L_middle");
        L_Image_ring = GameObject.Find("L_ring");
        L_Image_pinky = GameObject.Find("L_pinky");

        R_Images[0] = R_Image_thumb;
        R_Images[1] = R_Image_index;
        R_Images[2] = R_Image_middle;
        R_Images[3] = R_Image_ring;
        R_Images[4] = R_Image_pinky;

        L_Images[0] = L_Image_thumb;
        L_Images[1] = L_Image_index;
        L_Images[2] = L_Image_middle;
        L_Images[3] = L_Image_ring;
        L_Images[4] = L_Image_pinky;

        for (int i = 0; i < 5; i++)
        {
            R_Images[i].SetActive(false);
            L_Images[i].SetActive(false);
        }

    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(SendDiscreteHapticWithDelay(0,5));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(SendDiscreteHapticWithDelay(0,5));
            StartCoroutine(SendDiscreteHapticWithDelay(-1,1));

        }

    }

    public IEnumerator SendDiscreteHapticWithDelay(int index, int intensity)
    {
        bool temp = false;
        while (true)
        {

            if (index == -1)
            {
                index++;
                temp = true;
                yield return new WaitForSeconds(1.01f);
            }
            Collision col = new Collision();
            if(!temp) InactiveImages();
            GameObject.Find("[bHapticsGlove]").GetComponent<BhapticsPhysicsGlove>().HapticVolume = intensity;
            BhapticsPhysicsGlove.Instance.SendEnterHaptic(rightHand ? PositionType.GloveR : PositionType.GloveL, index, col);
            if (rightHand) R_Images[index].SetActive(true);
            else L_Images[index].SetActive(true);
            index++;
            
            if (index > 4) yield break;
            yield return new WaitForSeconds(1f);
        }
    }

   

    void InactiveImages()
    {
        for (int i = 0; i < 5; i++)
        {
            R_Images[i].SetActive(false);
            L_Images[i].SetActive(false);
        }
    }

}
