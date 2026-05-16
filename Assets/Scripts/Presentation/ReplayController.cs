using System.Collections.Generic;
using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Plays back a recorded action log at configurable speed.
    /// Attach to any active GameObject; call Activate(records) to begin.
    /// Calls OnTick each simulated step so watchers (ReplayUI) can update.
    /// </summary>
    public class ReplayController : MonoBehaviour
    {
        public static ReplayController Instance { get; private set; }

        private List<ActionRecord> _records;
        private int  _cursor;
        private float _elapsed;
        private bool  _playing;

        public float PlaybackSpeed  = 1f;   // 1 2 4 8
        public float TotalDuration  { get; private set; }
        public float CurrentTime    => _elapsed;
        public bool  IsPlaying      => _playing;
        public bool  IsLoaded       => _records != null && _records.Count > 0;

        /// <summary>Called each tick with current elapsed time for UI updates.</summary>
        public event System.Action<float> OnTick;

        /// <summary>Fired when playback reaches the end.</summary>
        public event System.Action OnComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>Load an action log and reset to the beginning.</summary>
        public void Activate(List<ActionRecord> records)
        {
            _records = records;
            _cursor  = 0;
            _elapsed = 0f;
            _playing = false;

            TotalDuration = 0f;
            if (records != null && records.Count > 0)
                TotalDuration = records[records.Count - 1].Timestamp;
        }

        /// <summary>Load the latest saved replay from disk.</summary>
        public void LoadLatestReplay()
        {
            Activate(ReplaySerializer.LoadLatest());
        }

        public void Play()  { _playing = true; }
        public void Pause() { _playing = false; }
        public void Toggle() { _playing = !_playing; }

        /// <summary>Jump to a specific time in the replay.</summary>
        public void SeekTo(float time)
        {
            _elapsed = Mathf.Clamp(time, 0f, TotalDuration);
            _cursor = 0;
            while (_cursor < _records.Count &&
                   _records[_cursor].Timestamp <= _elapsed)
                _cursor++;
            OnTick?.Invoke(_elapsed);
        }

        private void Update()
        {
            if (!_playing || _records == null) return;

            _elapsed += Time.deltaTime * PlaybackSpeed;

            while (_cursor < _records.Count &&
                   _records[_cursor].Timestamp <= _elapsed)
            {
                _cursor++;
            }

            OnTick?.Invoke(_elapsed);

            if (_elapsed >= TotalDuration)
            {
                _playing = false;
                OnComplete?.Invoke();
            }
        }
    }
}
