using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Debug = UnityEngine.Debug;

namespace ProjectBBF.Event
{
    [EventPage]
    public static partial class EventPage
    {
        [EventHandler(typeof(CollisionEventCommand))]
        public static void CollisionEventHandler(IEventCommand iCommand)
        {
            if (Guard<CollisionEventCommand>(iCommand, out var command)) return;

            command.groups.ForEach(x =>
            {
                //.ForEach(y => Debug.Log(y));
                //ebug.Log("--");
            });
        }



        public static bool Guard<T>(IEventCommand iCommand, out T command)
            where T : class, IEventCommand
        {
            if (iCommand is not T c)
            {
                command = null;
                
                StackTrace stackTrace = new StackTrace();
                var frames = stackTrace.GetFrames();
                
                Debug.Assert(frames is not null);
                if (frames.Length < 2) return true;
                
                MethodBase method = frames[^2].GetMethod();
                
                Debug.LogError($"EventPage handler({method.Name})와 호환되지 않는 Command 객체입니다. 현재 command({iCommand.GetType()}), 타겟 command({typeof(T)})");
                return true;
            }
            
            command = c;
            return false;
        }
    } 
}