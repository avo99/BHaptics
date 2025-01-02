using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bhaptics.SDK2.Glove;
using Bhaptics.SDK2;
using UnityEditor.Experimental.GraphView;

public class HapticsManager : MonoBehaviour
{
    public bool rightHand = true; // Toggle this to switch between left and right gloves

    private GameObject R_Image_thumb;
    private GameObject R_Image_index;
    private GameObject R_Image_middle;
    private GameObject R_Image_ring;
    private GameObject R_Image_pinky;
    private GameObject R_Image_wrist;

    private GameObject L_Image_thumb;
    private GameObject L_Image_index;
    private GameObject L_Image_middle;
    private GameObject L_Image_ring;
    private GameObject L_Image_pinky;
    private GameObject L_Image_wrist;
    private GameObject[] R_Images = { null, null, null, null, null, null };
    private GameObject[] L_Images = { null, null, null, null, null, null };

    void Start()
    {
        R_Image_thumb = GameObject.Find("R_thumb");
        R_Image_index = GameObject.Find("R_index");
        R_Image_middle = GameObject.Find("R_middle");
        R_Image_ring = GameObject.Find("R_ring");
        R_Image_pinky = GameObject.Find("R_pinky");
        R_Image_wrist = GameObject.Find("R_wrist");

        L_Image_thumb = GameObject.Find("L_thumb");
        L_Image_index = GameObject.Find("L_index");
        L_Image_middle = GameObject.Find("L_middle");
        L_Image_ring = GameObject.Find("L_ring");
        L_Image_pinky = GameObject.Find("L_pinky");
        L_Image_wrist = GameObject.Find("L_wrist");

        R_Images[0] = R_Image_thumb;
        R_Images[1] = R_Image_index;
        R_Images[2] = R_Image_middle;
        R_Images[3] = R_Image_ring;
        R_Images[4] = R_Image_pinky;
        R_Images[5] = R_Image_wrist;

        L_Images[0] = L_Image_thumb;
        L_Images[1] = L_Image_index;
        L_Images[2] = L_Image_middle;
        L_Images[3] = L_Image_ring;
        L_Images[4] = L_Image_pinky;
        L_Images[5] = L_Image_wrist;

        for (int i = 0; i < 6; i++)
        {
            R_Images[i].SetActive(false);
            L_Images[i].SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Test back-and-forth vibration across fingers
            StartCoroutine(DirectionBackAndForth(3));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Test wrist actuator separately
            TestWristVibration();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // Test back-and-forth vibration across fingers & wrist
            StartCoroutine(DirectionBackAndForthWithWrist(3));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // Test back-and-forth vibration across fingers with constant wrist
            StartCoroutine(DirectionWithConstantWrist(3));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            // Test back-and-forth vibration across fingers with peripheral wrist
            StartCoroutine(DirectionWithPeripheralWrist(3));
        }
    }

    // New function for back-and-forth directional vibration
    IEnumerator DirectionBackAndForth(int repetitions = 3)
    {
        int[] motorValues = { 0, 0, 0, 0, 0, 0 }; // Thumb to pinky, then wrist (last index)
        int duration = 300; // Duration in milliseconds
        int intensity = 100; // Motor intensity (can be adjusted for smoothness)

        for (int cycle = 0; cycle < repetitions; cycle++)
        {
            // Forward pass (thumb to pinky)
            for (int i = 0; i < 5; i++)
            {
                motorValues[i] = intensity; // Activate the current motor

                // Smooth overlap with previous and next motors (NEW)
                if (i > 0) motorValues[i - 1] = intensity / 2; // Previous motor at half intensity
                if (i < 4) motorValues[i + 1] = intensity / 4; // Next motor at quarter intensity

                UpdateImages(i, true);
                if (i > 0) UpdateImages(i - 1, true);
                if (i < 4) UpdateImages(i + 1, true);

                BhapticsLibrary.PlayMotors(
                    rightHand ? (int)PositionType.GloveR : (int)PositionType.GloveL,
                    motorValues,
                    duration
                );
                yield return new WaitForSeconds((duration - 100) / 1000f); // Slightly overlapping delay

                // Deactivate motors (original logic preserved)
                motorValues[i] = 0;
                if (i > 0) motorValues[i - 1] = 0;
                if (i < 4) motorValues[i + 1] = 0;

                UpdateImages(i, false);
                if (i > 0) UpdateImages(i - 1, false);
                if (i < 4) UpdateImages(i + 1, false);
            }

            // Backward pass (pinky to thumb)
            for (int i = 4; i >= 0; i--)
            {
                motorValues[i] = intensity; // Activate the current motor

                // Smooth overlap with next and previous motors (NEW)
                if (i < 4) motorValues[i + 1] = intensity / 2; // Next motor at half intensity
                if (i > 0) motorValues[i - 1] = intensity / 4; // Previous motor at quarter intensity

                UpdateImages(i, true);
                if (i < 4) UpdateImages(i + 1, true);
                if (i > 0) UpdateImages(i - 1, true);

                BhapticsLibrary.PlayMotors(
                    rightHand ? (int)PositionType.GloveR : (int)PositionType.GloveL,
                    motorValues,
                    duration
                );
                yield return new WaitForSeconds((duration - 100) / 1000f); // Slightly overlapping delay

                // Deactivate motors (original logic preserved)
                motorValues[i] = 0;
                if (i < 4) motorValues[i + 1] = 0;
                if (i > 0) motorValues[i - 1] = 0;

                UpdateImages(i, false);
                if (i < 4) UpdateImages(i + 1, false);
                if (i > 0) UpdateImages(i - 1, false);
            }
        }
    }


    // New function for back-and-forth directional vibration with wrist actuator
    IEnumerator DirectionBackAndForthWithWrist(int repetitions = 3)
    {
        int[] motorValues = { 0, 0, 0, 0, 0, 0 }; // Thumb to pinky, then wrist (last index)
        int duration = 300; // Duration in milliseconds
        int intensity = 100; // Motor intensity (can be adjusted for smoothness)

        for (int cycle = 0; cycle < repetitions; cycle++)
        {
            // Forward pass (thumb to pinky, then wrist)
            for (int i = 0; i <= 5; i++) // Include wrist (index 5)
            {
                motorValues[i] = intensity; // Activate the current motor

                // Smooth overlap with previous and next motors (NEW)
                if (i > 0) motorValues[i - 1] = intensity / 2; // Previous motor
                if (i < 5) motorValues[i + 1] = intensity / 4; // Next motor

                UpdateImages(i, true);
                if (i > 0) UpdateImages(i - 1, true);
                if (i < 5) UpdateImages(i + 1, true);

                BhapticsLibrary.PlayMotors(
                    rightHand ? (int)PositionType.GloveR : (int)PositionType.GloveL,
                    motorValues,
                    duration
                );

                yield return new WaitForSeconds((duration - 100) / 1000f); // Slightly overlapping delay

                // Deactivate motors (original logic preserved)
                motorValues[i] = 0;
                if (i > 0) motorValues[i - 1] = 0;
                if (i < 5) motorValues[i + 1] = 0;

                UpdateImages(i, false);
                if (i > 0) UpdateImages(i - 1, false);
                if (i < 5) UpdateImages(i + 1, false);
            }

            // Backward pass (wrist to thumb)
            for (int i = 5; i >= 0; i--) // Include wrist (index 5)
            {
                motorValues[i] = intensity; // Activate the current motor

                // Smooth overlap with next and previous motors (NEW)
                if (i < 5) motorValues[i + 1] = intensity / 2; // Next motor
                if (i > 0) motorValues[i - 1] = intensity / 4; // Previous motor

                UpdateImages(i, true);
                if (i < 5) UpdateImages(i + 1, true);
                if (i > 0) UpdateImages(i - 1, true);

                BhapticsLibrary.PlayMotors(
                    rightHand ? (int)PositionType.GloveR : (int)PositionType.GloveL,
                    motorValues,
                    duration
                );
                yield return new WaitForSeconds((duration - 100) / 1000f); // Slightly overlapping delay

                // Deactivate motors (original logic preserved)
                motorValues[i] = 0;
                if (i < 5) motorValues[i + 1] = 0;
                if (i > 0) motorValues[i - 1] = 0;

                UpdateImages(i, false);
                if (i < 5) UpdateImages(i + 1, false);
                if (i > 0) UpdateImages(i - 1, false);
            }
        }
    }


    IEnumerator DirectionWithConstantWrist(int repetitions = 3)
    {
        int[] motorValues = { 0, 0, 0, 0, 0, 100 }; // Thumb to pinky, with wrist always active
        int duration = 300; // Duration in milliseconds
        int intensity = 100; // Motor intensity for fingers

        for (int cycle = 0; cycle < repetitions; cycle++)
        {
            // Forward pass (thumb to pinky, wrist stays active)
            for (int i = 0; i < 5; i++)
            {
                motorValues[i] = intensity; // Activate the current motor

                // Smooth overlap with wrist and neighboring motors (NEW)
                if (i > 0) motorValues[i - 1] = intensity / 2; // Previous motor
                if (i < 4) motorValues[i + 1] = intensity / 4; // Next motor

                UpdateImages(i, true);
                if (i > 0) UpdateImages(i - 1, true);
                if (i < 4) UpdateImages(i + 1, true);

                BhapticsLibrary.PlayMotors(
                    rightHand ? (int)PositionType.GloveR : (int)PositionType.GloveL,
                    motorValues,
                    duration
                );
                yield return new WaitForSeconds((duration - 100) / 1000f); // Slightly overlapping delay

                motorValues[i] = 0; // Deactivate the current motor
                if (i > 0) motorValues[i - 1] = 0;
                if (i < 4) motorValues[i + 1] = 0;

                UpdateImages(i, false);
                if (i > 0) UpdateImages(i - 1, false);
                if (i < 4) UpdateImages(i + 1, false);
            }

            // Backward pass (pinky to thumb, wrist stays active)
            for (int i = 4; i >= 0; i--)
            {
                motorValues[i] = intensity; // Activate the current motor

                // Smooth overlap with wrist and neighboring motors (NEW)
                if (i < 4) motorValues[i + 1] = intensity / 2; // Next motor
                if (i > 0) motorValues[i - 1] = intensity / 4; // Previous motor

                UpdateImages(i, true);
                if (i < 4) UpdateImages(i + 1, true);
                if (i > 0) UpdateImages(i - 1, true);

                BhapticsLibrary.PlayMotors(
                    rightHand ? (int)PositionType.GloveR : (int)PositionType.GloveL,
                    motorValues,
                    duration
                );
                yield return new WaitForSeconds((duration - 100) / 1000f); // Slightly overlapping delay

                motorValues[i] = 0; // Deactivate the current motor
                if (i < 4) motorValues[i + 1] = 0;
                if (i > 0) motorValues[i - 1] = 0;

                UpdateImages(i, false);
                if (i < 4) UpdateImages(i + 1, false);
                if (i > 0) UpdateImages(i - 1, false);
            }
        }
    }

    IEnumerator DirectionWithPeripheralWrist(int repetitions = 3)
    {
        int[] motorValues = { 0, 0, 0, 0, 0, 0 }; // Thumb to pinky, with wrist (last index)
        int duration = 300; // Duration in milliseconds
        int intensity = 100; // Motor intensity for fingers
        int peripheralWristIntensity = 50; // Peripheral intensity for the wrist


        for (int cycle = 0; cycle < repetitions; cycle++)
        {
            // Forward pass (thumb to pinky)
            for (int i = 0; i < 5; i++) // Thumb (index 0) to Pinky (index 4)
            {
                motorValues[i] = intensity; // Activate the current motor
                motorValues[5] = peripheralWristIntensity; // Peripheral wrist vibration

                // Smooth overlap with previous and next motors
                if (i > 0) motorValues[i - 1] = intensity / 2; // Previous motor
                if (i < 4) motorValues[i + 1] = intensity / 4; // Next motor

                UpdateImages(i, true);
                if (i > 0) UpdateImages(i - 1, true);
                if (i < 4) UpdateImages(i + 1, true);

                BhapticsLibrary.PlayMotors(
                    rightHand ? (int)PositionType.GloveR : (int)PositionType.GloveL,
                    motorValues,
                    duration
                );
                yield return new WaitForSeconds((duration - 100) / 1000f); // Slightly overlapping delay

                // Deactivate motors (except wrist)
                motorValues[i] = 0;
                if (i > 0) motorValues[i - 1] = 0;
                if (i < 4) motorValues[i + 1] = 0;

                UpdateImages(i, false);
                if (i > 0) UpdateImages(i - 1, false);
                if (i < 4) UpdateImages(i + 1, false);
            }

            // Backward pass (pinky to thumb)
            for (int i = 4; i >= 0; i--) // Pinky (index 4) to Thumb (index 0)
            {
                motorValues[i] = intensity; // Activate the current motor
                motorValues[5] = peripheralWristIntensity; // Peripheral wrist vibration

                // Smooth overlap with next and previous motors
                if (i < 4) motorValues[i + 1] = intensity / 2; // Next motor
                if (i > 0) motorValues[i - 1] = intensity / 4; // Previous motor

                UpdateImages(i, true);
                if (i < 4) UpdateImages(i + 1, true);
                if (i > 0) UpdateImages(i - 1, true);

                BhapticsLibrary.PlayMotors(
                    rightHand ? (int)PositionType.GloveR : (int)PositionType.GloveL,
                    motorValues,
                    duration
                );
                yield return new WaitForSeconds((duration - 100) / 1000f); // Slightly overlapping delay

                // Deactivate motors (except wrist)
                motorValues[i] = 0;
                if (i < 4) motorValues[i + 1] = 0;
                if (i > 0) motorValues[i - 1] = 0;

                UpdateImages(i, false);
                if (i < 4) UpdateImages(i + 1, false);
                if (i > 0) UpdateImages(i - 1, false);
            }
        }

        // Deactivate the wrist after the flow ends
        motorValues[5] = 0;
        BhapticsLibrary.PlayMotors(
            rightHand ? (int)PositionType.GloveR : (int)PositionType.GloveL,
            motorValues,
            duration
        );
    }


    // Function to test wrist actuator
    void TestWristVibration()
    {
        int[] motorValues = { 0, 0, 0, 0, 0, 100 }; // Only the wrist actuator
        int duration = 1000; // Duration in milliseconds

        UpdateImages(5, true); // Activate the wrist image

        BhapticsLibrary.PlayMotors(
            rightHand ? (int)PositionType.GloveR : (int)PositionType.GloveL,
            motorValues,
            duration
        );
    }

    public IEnumerator SendDiscreteHapticWithDelay(int index, int intensity)
    {
        // Commented out logic for vibration testing focus
        /*
        bool temp = false;
        while (true)
        {
            if (index == -1)
            {
                index++;
                temp = true;
                yield return new WaitForSeconds(1.01f);
            }
            Collision col = new Collision();12345
            if (!temp) InactiveImages();
            GameObject.Find("[bHapticsGlove]").GetComponent<BhapticsPhysicsGlove>().HapticVolume = intensity;
            BhapticsPhysicsGlove.Instance.SendEnterHaptic(rightHand ? PositionType.GloveR : PositionType.GloveL, index, col);
            if (rightHand) R_Images[index].SetActive(true);
            else L_Images[index].SetActive(true);
            index++;

            if (index > 4) yield break;
            yield return new WaitForSeconds(1f);
        }
        */
        yield break; // Placeholder to satisfy IEnumerator
    }

    void UpdateImages(int index, bool isActive)
    {
        if (rightHand)
        {
            R_Images[index]?.SetActive(isActive);
        }
        else
        {
            L_Images[index]?.SetActive(isActive);
        }
    }


void InactiveImages()
    {
        for (int i = 0; i < 6; i++)
        {
            R_Images[i].SetActive(false);
            L_Images[i].SetActive(false);
        }
    }
}
