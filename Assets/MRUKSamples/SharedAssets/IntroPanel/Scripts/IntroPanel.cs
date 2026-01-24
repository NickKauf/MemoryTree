// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.Events;

public class IntroPanel : MonoBehaviour
{
    public UnityEvent ButtonPressed;

    private void Update()
    {
        if (OVRInput.Get(OVRInput.RawButton.X) || OVRInput.Get(OVRInput.RawButton.A) || Input.GetKeyDown(KeyCode.Space))
        {
            ButtonPressed.Invoke();
        }
    }

    public void EnableObject(GameObject goToEnable)
    {
        goToEnable.SetActive(true);
    }

    public void DisableObject(GameObject goToDisable)
    {
        goToDisable.SetActive(false);
    }

}
