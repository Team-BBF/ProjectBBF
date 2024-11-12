using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using ProjectBBF.Persistence;
using ProjectBBF.Singleton;
using UnityEngine;
using UnityEngine.Audio;

[Singleton(ESingletonType.Global, 3)]
public class AudioManager : MonoBehaviourSingleton<AudioManager>
{
    private const string PATH = "Audio/";

    private Dictionary<string, (AudioMixerGroup mixerGroup, AudioSource source, Dictionary<string, AudioClip> dict)> _audioTables;
    private AudioMixer _mixer;

    private Dictionary<string, float> _volumeTable = new();

    public event Action<string, float> OnChangedVolume;
    public override void PostInitialize()
    {
        _mixer = Resources.Load<AudioMixer>(PATH + "Master");
        
        var list = Resources.LoadAll<AudioTable>(PATH);

        _audioTables = new();

        foreach (var table in list)
        {
            Dictionary<string, AudioClip> audioClip = new();

            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = table.MixerGroup;
            _audioTables.Add(table.TableKey.Trim(), (table.MixerGroup, audioSource, audioClip));
            foreach (var set in table.List)
            {
                audioClip.Add(set.Key.Trim(), set.Audio);
            }
        }


        PersistenceManager.Instance.TryLoadOrCreateUserData("GameSetting", out GameSetting setting);

        var keyValuePairs = setting
            .GetType()
            .GetFields(BindingFlags.Public| BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.GetCustomAttribute<GameSetting.VolumeAttribute>() is not null)
            .Select(x =>
            {
                var att = x.GetCustomAttribute<GameSetting.VolumeAttribute>();
                return new KeyValuePair<string, float>(att.Key, (float)x.GetValue(setting));
            }).ToList();

        _volumeTable = new Dictionary<string, float>(keyValuePairs.ToList());
        
        foreach (KeyValuePair<string, float> pair in _volumeTable.ToList())
        {
            SetVolume(pair.Key, pair.Value);
        }

        StartCoroutine(CoVolumeUpdate(keyValuePairs, setting.IsCreatedAudio));

        if (setting.IsCreatedAudio is false)
        {
            setting.IsCreatedAudio = true;
        }
    }

    private IEnumerator CoVolumeUpdate(List<KeyValuePair<string, float>> list, bool isCreated)
    {
        yield return null;
        foreach (KeyValuePair<string, float> pair in list)
        {
            SetVolume(pair.Key, isCreated is false ? 1f : pair.Value);
        }
    }

    public void SaveSetting()
    {
        PersistenceManager.Instance.TryLoadOrCreateUserData("GameSetting", out GameSetting setting);
        
        var keyValuePairs = setting
            .GetType()
            .GetFields(BindingFlags.Public| BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.GetCustomAttribute<GameSetting.VolumeAttribute>() is not null)
            .Select(x =>
            {
                var att = x.GetCustomAttribute<GameSetting.VolumeAttribute>();
                return new KeyValuePair<FieldInfo, string>(x, att.Key);
            });

        foreach (KeyValuePair<FieldInfo,string> pair in keyValuePairs)
        {
            float volume = GetVolume(pair.Value);
            pair.Key.SetValue(setting, volume);
        }
        
        PersistenceManager.Instance.SaveUserData();
    }

    public override void PostRelease()
    {
        _audioTables = null;
    }

    public (AudioSource source, AudioClip clip) GetAudio(string tableKey, string audioKey, bool logError = true)
    {
        var table = _audioTables.GetValueOrDefault(tableKey);

        var clip = table.dict?.GetValueOrDefault(audioKey);

        if (logError && clip == false)
        {
            Debug.LogWarning($"table key({tableKey}), audioKey ({audioKey})");
        }
        
        return (table.source, clip);
    }

    public bool PlayScheduled(string tableKey, string audioKey, double time)
    {
        var tuple = GetAudio(tableKey, audioKey);
        if (tuple.clip == false) return false;
        
        tuple.source.clip = tuple.clip;
        tuple.source.PlayScheduled(time);
        return true;
    }

    public bool PlayDelayed(string tableKey, string audioKey, float delay)
    {
        var tuple = GetAudio(tableKey, audioKey);
        if (tuple.clip == false) return false;
        
        tuple.source.clip = tuple.clip;
        tuple.source.PlayDelayed(delay);
        return true;
    }

    public bool PlayOneShot(string tableKey, string audioKey, float? volumeScale = null)
    {
        var tuple = GetAudio(tableKey, audioKey);
        if (tuple.clip == false) return false;
        
        if (volumeScale.HasValue)
        {
            tuple.source.PlayOneShot(tuple.clip, volumeScale.Value);
        }
        else
        {
            tuple.source.PlayOneShot(tuple.clip);
        }
        return true;
    }
    public bool PlayOneShot(string tableKey, string audioKey, bool stopAndPlay, float? volumeScale = null)
    {
        var tuple = GetAudio(tableKey, audioKey);
        if (tuple.clip == false) return false;
        
        tuple.source.clip = tuple.clip;

        if (stopAndPlay)
        {
            tuple.source.Stop();
        }

        if (volumeScale.HasValue)
        {
            tuple.source.PlayOneShot(tuple.clip, volumeScale.Value);
        }
        else
        {
            tuple.source.PlayOneShot(tuple.clip);
        }
        return true;
    }
    
    public bool Play(string tableKey, string audioKey)
    {
        var tuple = GetAudio(tableKey, audioKey);
        if (tuple.clip == false) return false;
        
        tuple.source.clip = tuple.clip;
        tuple.source.Play();
        return true;
    }

    public void Stop(string tableKey)
    {
        var table = _audioTables.GetValueOrDefault(tableKey);

        if (table.source)
        {
            table.source.Stop();
        }
        
        
    }

    public void SetVolume(string groupKey, float value)
    {
        float originValue = value;
        value = Mathf.Max(0.001f, value);
        value = Mathf.Log10(value) * 20f;
        
        
        if (_volumeTable.ContainsKey(groupKey) is false)
        {
            Debug.LogWarning($"group key({groupKey})를 찾지 못함.");
            return;
        }

        _volumeTable[groupKey] = value;
        OnChangedVolume?.Invoke(groupKey, originValue);
        
        if (_mixer.SetFloat(groupKey,  value) is false)
        {
            Debug.LogWarning($"group key({groupKey})를 찾지 못함.");
        }
    }

    public float GetVolume(string groupKey)
    {
        if (_volumeTable.TryGetValue(groupKey, out float value) is false)
        {
            Debug.LogWarning($"group key({groupKey})를 찾지 못함.");
            return 0f;
        }

        value = Mathf.Pow(10f, value * 0.05f);
        return value;
    }

}

public static class AudioManagerExtensions
{
    public static bool Play(this AudioSource source, string tableKey, string audioKey)
    {
        var inst = AudioManager.Instance;
        if (inst == false) return false;
        
        var tuple = inst.GetAudio(tableKey, audioKey);

        if (source.clip != tuple.clip)
        {
            source.clip = tuple.clip;
        }
        source.Play();

        return true;
    }
    public static bool PlayOneShot(this AudioSource source, string tableKey, string audioKey, float? volumeScale = null)
    {
        var inst = AudioManager.Instance;
        if (inst == false) return false;
        
        var tuple = inst.GetAudio(tableKey, audioKey);
        if (volumeScale.HasValue)
        {
            source.PlayOneShot(tuple.clip, volumeScale.Value);
        }
        else
        {
            source.PlayOneShot(tuple.clip);
        }

        return true;
    }
    
    public static bool PlayDelayed(this AudioSource source, string tableKey, string audioKey, float delay)
    {
        var inst = AudioManager.Instance;
        if (inst == false) return false;
        
        var tuple = inst.GetAudio(tableKey, audioKey);
        
        source.clip = tuple.clip;
        source.PlayDelayed(delay);

        return true;
    }
    
    public static bool PlayScheduled(this AudioSource source, string tableKey, string audioKey, double time)
    {
        var inst = AudioManager.Instance;
        if (inst == false) return false;
        
        var tuple = inst.GetAudio(tableKey, audioKey);
        
        source.clip = tuple.clip;
        source.PlayScheduled(time);

        return true;
    }
    
}