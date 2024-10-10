using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBBF.Persistence
{
    [Serializable]
    public class Metadata
    {
        [SerializeField] private string _saveFileName;
        [SerializeField] private string _playerName;
        [SerializeField] private int _day = 1;

        public string SaveFileName
        {
            get => _saveFileName;
            set => _saveFileName = value;
        }

        public string PlayerName
        {
            get => _playerName;
            set => _playerName = value;
        }

        public int Day
        {
            get => _day;
            set => _day = value;
        }
    }
}