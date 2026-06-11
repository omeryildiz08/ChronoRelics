using UnityEngine.EventSystems;

public static class UIInputBlocker
{
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        for (int i = 0; i < UnityEngine.Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(UnityEngine.Input.GetTouch(i).fingerId))
            {
                return true;
            }
        }

        return false;
    }
}
