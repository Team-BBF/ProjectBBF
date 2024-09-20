using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using ProjectBBF.Persistence;
using ProjectBBF.Singleton;
using UnityEngine;

[Serializable]
public enum GameTimeOfDay
{
    AM,
    PM,
    Night,
    EndOfDay
} 

[Serializable]
public struct GameTime
{
    public int Hour;
    public int Min;

    public GameTimeOfDay? TimeOfDay { get; set; }

    public GameTime(int hour, int min)
    {
        Hour = hour;
        Min = min;
        TimeOfDay = null;
    }
    
    public override string ToString()
    {
        return $"Game time((h){Hour} : (m){Min}, ({TimeOfDay}))";
    }

    public int TotalMinutes()
    {
        return Hour * 60 + Min;
    }

    public override bool Equals(object obj)
    {
        if (obj is GameTime)
        {
            return Equals((GameTime)obj);
        }
        return false;
    }

    public bool Equals(GameTime other)
    {
        return TotalMinutes() == other.TotalMinutes();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TotalMinutes());
    }

    public static bool operator ==(GameTime lhs, GameTime rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(GameTime lhs, GameTime rhs)
    {
        return !lhs.Equals(rhs);
    }

    public static bool operator <(GameTime lhs, GameTime rhs)
    {
        return lhs.TotalMinutes() < rhs.TotalMinutes();
    }

    public static bool operator <=(GameTime lhs, GameTime rhs)
    {
        return lhs.TotalMinutes() <= rhs.TotalMinutes();
    }

    public static bool operator >(GameTime lhs, GameTime rhs)
    {
        return lhs.TotalMinutes() > rhs.TotalMinutes();
    }


    public static bool operator >=(GameTime lhs, GameTime rhs)
    {
        return lhs.TotalMinutes() >= rhs.TotalMinutes();
    }
}

public struct RealTime
{
    public int Min { get; private set; }
    public int Sec{ get; private set; }

    public float RealTimeStamp{ get; private set; }

    public RealTime(int min, int sec, float realTimeStamp)
    {
        Min = min;
        Sec = sec;
        RealTimeStamp = realTimeStamp;
    }

    public RealTime(int min, int sec)
    {
        Min = min;
        Sec = sec;
        RealTimeStamp = min * 60 + sec;
    }

    public override string ToString()
    {
        return $"Real time((m){Min} : (s){Sec}, total: {RealTimeStamp})";
    }
}

[GameData]
[Serializable]
public class TimePersistenceObject
{
    private int _day = 1;

    public int Day
    {
        get => _day;
        set => _day = value;
    }
}

[Singleton(ESingletonType.Global)]
public class TimeManager : MonoBehaviourSingleton<TimeManager>
{
    private const string PATH = "Data/TimeData";
    private const string EVENT_PATH = "Event/Time";

    private TimeData _timeData;
    private float _realTimer;
    private GameTime _beforeTime;
    private List<ESOGameTimeEvent> _esoEvents = new List<ESOGameTimeEvent>(10);
    public TimePersistenceObject _saveData;

    public bool IsRunning { get; private set; }
    public bool IsBegin { get; private set; }

    public TimePersistenceObject SaveData
    {
        get
        {
            if (_saveData is null)
            {
                _saveData= PersistenceManager.Instance.LoadOrCreate<TimePersistenceObject>("TimeManager_GameTime");
            }

            return _saveData;
        }
    }

    public TimeData TimeData => _timeData;


    public override void PostInitialize()
    {
        _timeData = Resources.Load<TimeData>(PATH);
        var events = Resources.LoadAll<ESOGameTimeEvent>(EVENT_PATH);
        
        _esoEvents.AddRange(events);
    }

    public override void PostRelease()
    {
        _esoEvents.ForEach(x=>x.Release());
        _esoEvents.Clear();
        _esoEvents = null;
    }

    public void Begin()
    {
        if (IsBegin)
        {
            return;
        }
        
        Reset();
        
        IsBegin = true;
        IsRunning = true;
        SetTime(_timeData.MorningTime);
    }

    public void Pause()
    {
        if (IsBegin is false) return;
        IsRunning = false;
    }

    public void Resume()
    {
        if (IsBegin is false) return;
        IsRunning = true;
    }

    public void End()
    {
        Reset();
        IsBegin = false;
        IsRunning = false;
    }

    public void Reset()
    {
        _timeData._dirty = false;
        _realTimer = 0f;
        _beforeTime = new GameTime(-1, -1);
        _esoEvents.ForEach(x=>x.Release());
    }
    
    private void Update()
    {
        if (IsRunning is false) return;
        
        _realTimer += Time.deltaTime;

        var newGameTime = GetGameTime();
        if (_beforeTime != newGameTime)
        {
            _beforeTime = newGameTime;

            foreach (ESOGameTimeEvent eso in _esoEvents)
            {
                if (eso.IsTriggered) continue;

                bool flag = false;

                //flag |= eso.OperationType == ESOGameTimeEvent.Operation.Equal               && newGameTime == eso.TargetGameTime;
                //flag |= eso.OperationType == ESOGameTimeEvent.Operation.NotEqual            && newGameTime != eso.TargetGameTime;
                flag |= eso.OperationType == ESOGameTimeEvent.Operation.GreaterThenEqual    && newGameTime >= eso.TargetGameTime;
                flag |= eso.OperationType == ESOGameTimeEvent.Operation.Greater             && newGameTime >  eso.TargetGameTime;
                flag |= eso.OperationType == ESOGameTimeEvent.Operation.Less                && newGameTime <  eso.TargetGameTime;
                flag |= eso.OperationType == ESOGameTimeEvent.Operation.LessThenEqual       && newGameTime <= eso.TargetGameTime;
                flag |= eso.OperationType == ESOGameTimeEvent.Operation.AllTicks;
                
                
                if (flag)
                {
                    eso.Signal(newGameTime);
                    
                    if (eso.OperationType == ESOGameTimeEvent.Operation.AllTicks)
                    {
                        eso.IsTriggered = false;
                    }
                }
            }
        }
    }

    public GameTimeOfDay GetTimeOfDay()
    {
        GameTime currentTime = GetGameTime();
        GameTimeOfDay value = GetTimeOfDay(currentTime);
        
        return value;
    }

    public GameTimeOfDay GetTimeOfDay(GameTime currentTime)
    {
        GameTimeOfDay value = GameTimeOfDay.AM;
        
        if (_timeData.DefinitionPm <= currentTime)
        {
            value = GameTimeOfDay.PM;
        }
        if (_timeData.DefinitionNight <= currentTime)
        {
            value = GameTimeOfDay.Night;
        }
        if (_timeData.DefinitionEndOfDay <= currentTime)
        {
            value = GameTimeOfDay.EndOfDay;
        }

        return value;
    }
    public GameTime GetGameTime()
    {
        if (_timeData._dirty)
        {
            _timeData._dirty = false;

            var curTime = _beforeTime;
            SetTime(curTime);
        }
        
        /*
         * r: realTime, x: totalTime of game time, a: totalTime of real time
         * 10 : r = x : a
         * rx = 10a
         * x = 10a / r
         * or
         * a = rx / 10
         */

        float x = (10f * _realTimer) / _timeData.Scale;

        int total = Mathf.FloorToInt(x);
        total = total - total % 10;
        
        int hour = total / 60;
        int min = total % 60;

        GameTime time = new GameTime(hour, min);
        time.TimeOfDay = GetTimeOfDay(time);

        return time;
    }

    public RealTime GetRealTime()
    {
        int hour = (int)_realTimer / 60;
        int min = (int)_realTimer % 60;
        return new RealTime(hour, min, _realTimer);
    }

    public void SetTime(GameTime time)
    {
        int gameTime = time.Hour * 60 + time.Min;

        float a = (_timeData.Scale * gameTime) / 10f;

        _realTimer = a;
    }
    
    public void SetTime(RealTime time)
    {
        _realTimer = time.RealTimeStamp;
    }
}