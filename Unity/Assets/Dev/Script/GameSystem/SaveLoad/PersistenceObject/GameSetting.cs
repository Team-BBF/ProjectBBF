using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectBBF.Persistence
{
    [UserData, Serializable]
    public class GameSetting
    {
        public class VolumeAttribute : Attribute
        {
            public readonly string Key;

            public VolumeAttribute(string key)
            {
                Key = key;
            }
        }
        
        [SerializeField, Volume("Global")] private float _volumeMaster;
        [SerializeField, Volume("BGM")] private float _volumeBGM;
        [SerializeField, Volume("SFX")] private float _volumeSFX;
        [SerializeField, Volume("Player")] private float _volumePlayer;
        [SerializeField, Volume("Animal")] private float _volumeAnimal;
        [SerializeField, Volume("UI")] private float _volumeUI;

        [SerializeField] private int _vsyncCount;
        [SerializeField] private int _screenMode;
        [SerializeField] private Vector2Int _resolution;
        [SerializeField] private int _refreshRate;
        [SerializeField] private float _renderScale;

        [SerializeField] private bool _isCreatedScreen;
        [SerializeField] private bool _isCreatedAudio;

        public bool IsCreatedScreen
        {
            get => _isCreatedScreen;
            set => _isCreatedScreen = value;
        }

        public bool IsCreatedAudio
        {
            get => _isCreatedAudio;
            set => _isCreatedAudio = value;
        }

        public float VolumeBGM
        {
            get => _volumeBGM;
            set => _volumeBGM = value;
        }

        public float VolumeSFX
        {
            get => _volumeSFX;
            set => _volumeSFX = value;
        }

        public float VolumePlayer
        {
            get => _volumePlayer;
            set => _volumePlayer = value;
        }

        public float VolumeAnimal
        {
            get => _volumeAnimal;
            set => _volumeAnimal = value;
        }

        public float VolumeUI
        {
            get => _volumeUI;
            set => _volumeUI = value;
        }

        public int VsyncCount
        {
            get => _vsyncCount;
            set => _vsyncCount = value;
        }

        public int ScreenMode
        {
            get => _screenMode;
            set => _screenMode = value;
        }

        public Vector2Int Resolution
        {
            get => _resolution;
            set => _resolution = value;
        }
        public int RefreshRate
        {
            get => _refreshRate;
            set => _refreshRate = value;
        }

        public float RenderScale
        {
            get => _renderScale;
            set => _renderScale = value;
        }
    }
}