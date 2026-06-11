using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonClickSfxBinder : MonoBehaviour
{
    [Header("References")]
    public Transform bindingRoot;
    public AudioSource audioSource;
    public AudioClip clickClip;

    [Header("Binding")]
    public bool includeInactiveButtons = true;
    [Min(0f)] public float rescanInterval = 0.5f;

    [Header("Playback")]
    [Range(0f, 1f)] public float volume = 1f;
    public bool randomizePitch = false;
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private readonly HashSet<Button> boundButtons = new HashSet<Button>();
    private float nextRescanTime;
    private float defaultPitch = 1f;

    private void Awake()
    {
        if (bindingRoot == null)
        {
            bindingRoot = transform;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        defaultPitch = audioSource.pitch;
    }

    private void OnEnable()
    {
        BindButtons();
        nextRescanTime = Time.unscaledTime + rescanInterval;
    }

    private void Update()
    {
        if (rescanInterval <= 0f || Time.unscaledTime < nextRescanTime)
        {
            return;
        }

        BindButtons();
        nextRescanTime = Time.unscaledTime + rescanInterval;
    }

    public void BindButtons()
    {
        if (bindingRoot == null)
        {
            return;
        }

        Button[] buttons = bindingRoot.GetComponentsInChildren<Button>(includeInactiveButtons);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || boundButtons.Contains(button))
            {
                continue;
            }

            UIButtonClickSfxHandler handler = button.GetComponent<UIButtonClickSfxHandler>();
            if (handler == null)
            {
                handler = button.gameObject.AddComponent<UIButtonClickSfxHandler>();
            }

            handler.Initialize(this, button);
            boundButtons.Add(button);
        }
    }

    public void PlayClickSound()
    {
        if (audioSource == null || clickClip == null)
        {
            return;
        }

        if (randomizePitch)
        {
            audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        }
        else
        {
            audioSource.pitch = defaultPitch;
        }

        audioSource.PlayOneShot(clickClip, volume);
    }
}
