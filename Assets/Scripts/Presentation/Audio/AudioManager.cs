using System.Collections.Generic;
using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Central audio manager. Subscribes to simulation events and plays
    /// appropriate sound effects. Also manages background music.
    /// Audio clips are assigned in Inspector or loaded from Resources.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music")]
        [SerializeField] private AudioClip _backgroundMusic;
        [SerializeField] [Range(0f, 1f)] private float _musicVolume = 0.3f;

        [Header("SFX")]
        [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 0.6f;

        [Header("Clips (assign in Inspector or leave null)")]
        [SerializeField] private AudioClip _buildingPlaced;
        [SerializeField] private AudioClip _buildingComplete;
        [SerializeField] private AudioClip _productionComplete;
        [SerializeField] private AudioClip _sectorConquered;
        [SerializeField] private AudioClip _techResearched;
        [SerializeField] private AudioClip _vpGained;
        [SerializeField] private AudioClip _combatStart;
        [SerializeField] private AudioClip _uiClick;

        private AudioSource _musicSource;
        private AudioSource _sfxSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = _musicVolume;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = _sfxVolume;
        }

        private void Start()
        {
            SubscribeToEvents();

            if (_backgroundMusic != null)
            {
                _musicSource.clip = _backgroundMusic;
                _musicSource.Play();
            }
        }

        private void SubscribeToEvents()
        {
            var gc = GameController.Instance;
            if (gc?.Events == null) return;

            gc.Events.Subscribe<BuildingCompletedEvent>(e => PlaySFX(_buildingComplete));
            gc.Events.Subscribe<ProductionCompleteEvent>(e => PlaySFX(_productionComplete));
            gc.Events.Subscribe<SectorConqueredEvent>(e => PlaySFX(_sectorConquered));
            gc.Events.Subscribe<TechResearchedEvent>(e => PlaySFX(_techResearched));
            gc.Events.Subscribe<VPChangedEvent>(e => { if (e.Gained) PlaySFX(_vpGained); });
            gc.Events.Subscribe<CombatResolvedEvent>(e => PlaySFX(_combatStart));
        }

        /// <summary>Play a sound effect.</summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }

        /// <summary>Play the UI click sound.</summary>
        public void PlayUIClick()
        {
            PlaySFX(_uiClick);
        }

        /// <summary>Play building placed sound.</summary>
        public void PlayBuildingPlaced()
        {
            PlaySFX(_buildingPlaced);
        }

        /// <summary>Set music volume (0-1).</summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
                _musicSource.volume = _musicVolume;
        }

        /// <summary>Set SFX volume (0-1).</summary>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        /// <summary>Toggle music on/off.</summary>
        public void ToggleMusic()
        {
            if (_musicSource == null) return;
            if (_musicSource.isPlaying) _musicSource.Pause();
            else _musicSource.UnPause();
        }

        /// <summary>Mute or unmute all audio sources.</summary>
        public void SetMasterMute(bool muted)
        {
            if (_musicSource != null) _musicSource.mute = muted;
            if (_sfxSource != null)   _sfxSource.mute   = muted;
        }

        /// <summary>Current music volume (0-1).</summary>
        public float MusicVolume => _musicVolume;

        /// <summary>Current SFX volume (0-1).</summary>
        public float SfxVolume => _sfxVolume;
    }
}
