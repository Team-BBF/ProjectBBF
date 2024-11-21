using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Persistence;
using UnityEngine;

public class AutoSaveResetMainMenuUI : MonoBehaviour
{
    private void Start()
    {
        PersistenceManager.Instance.TryLoadOrCreateUserData("GameSetting", out GameSetting setting);

        if (setting.AutoSaveReset)
        {
            Metadata[] metadatas = PersistenceManager.GetAllSaveFileMetadata();

            foreach (Metadata meta in metadatas)
            {
                PersistenceManager.RemoveSaveFile(meta);
            }
        }
    }
}
