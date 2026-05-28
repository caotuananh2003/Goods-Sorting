using UnityEngine;

public static class MobileInput
{
    public static Vector3 PointerPosition()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }
#endif

        return Input.mousePosition;
    }

    public static bool PointerDown()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 0) return false;

        return Input.GetTouch(0).phase == TouchPhase.Began;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    public static bool PointerHold()
    {
#if UNITY_ANDROID || UNITY_IOS
        return Input.touchCount > 0;
#else
        return Input.GetMouseButton(0);
#endif
    }

    public static bool PointerUp()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 0) return false;

        TouchPhase phase = Input.GetTouch(0).phase;

        return phase == TouchPhase.Ended ||
               phase == TouchPhase.Canceled;
#else
        return Input.GetMouseButtonUp(0);
#endif
    }
}