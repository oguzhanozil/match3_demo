using UnityEngine;
using UnityEngine.SceneManagement;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource audioSource;

    // Inspector'dan atayacağınız klipler
    public AudioClip levelSceneMusic;
    public AudioClip gameplaySceneMusic;

    // Yeni: SFX klipleri
    public AudioClip swapClip;
    public AudioClip explodeClip;

    // Yeni: SFX için ayrı ses seviye çarpanı
    [Range(0f,1f)]
    public float sfxVolume = 1f;

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

            // Ensure 2D sound for SFX
            audioSource.spatialBlend = 0f;
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

    // Yeni: swap ve explode için yardımcı metodlar
    public void PlaySwap(float volume = 1f)
    {
        if (swapClip == null) return;
        audioSource.PlayOneShot(swapClip, Mathf.Clamp01(volume) * sfxVolume);
    }

    public void PlayExplode(float volume = 1f)
    {
        if (explodeClip == null) return;
        audioSource.PlayOneShot(explodeClip, Mathf.Clamp01(volume) * sfxVolume);
    }
}