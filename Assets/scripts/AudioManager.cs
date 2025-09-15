using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource audioSource;

    // Inspector'dan atayacağınız klipler
    public AudioClip levelSceneMusic;
    public AudioClip gameplaySceneMusic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        PlayForScene(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    public void PlayForScene(string sceneName)
    {
        // Sahne isimlerini Build Settings'teki ile eşleştirin.
        string n = sceneName.ToLowerInvariant();
        AudioClip toPlay = null;

        if (n.Contains("level")) toPlay = levelSceneMusic;
        else if (n.Contains("gameplay") || n.Contains("game")) toPlay = gameplaySceneMusic;

        if (toPlay == null)
        {
            audioSource.Stop();
            return;
        }

        if (audioSource.clip == toPlay && audioSource.isPlaying) return;

        audioSource.clip = toPlay;
        audioSource.Play();
    }

    // İhtiyaca göre dışarıdan geçici çalma fonksiyonu
    public void PlayClipOnce(AudioClip clip, float volume = 1f)
    {
        audioSource.PlayOneShot(clip, volume);
    }
}