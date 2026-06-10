using UnityEngine;

public class CinematicReplayInput : MonoBehaviour
{
    [SerializeField] private KeyCode replayKey = KeyCode.F9;

    private void Update()
    {
        if (Input.GetKeyDown(replayKey))
        {
            if (PauseMenuManager.IsPaused)
            {
                return;
            }

            if (CinematicPlayer.Instance != null && !CinematicPlayer.Instance.IsPlaying)
            {
                CinematicPlayer.Instance.PlayCinematicInCurrentScene();
            }
        }
    }
}