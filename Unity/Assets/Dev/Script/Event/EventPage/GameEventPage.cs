using System.Linq;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using ProjectBBF.Persistence;

public class FlowNextDay : IEventCommand
{
}

[EventPage]
public class GameEventPage
{
    [EventHandler(typeof(FlowNextDay))]
    private static void OnFlowNextDay(IEventCommand iCmd)
    {
        var cObj = GameObjectStorage.Instance.StoredObjects.FirstOrDefault(x => x.gameObject.CompareTag("Player"));
        if (cObj is null) return;
        
        PlayerController controller = cObj.GetComponent<PlayerController>();

        if (controller == false) return;

        var loader = SceneLoader.Instance;
        controller.StateHandler.TranslateState("DoNothing");
        
        TimeManager.Instance.Pause();

        loader
            .WorkDirectorAsync(false)
            .ContinueWith(async _ => await loader.UnloadAllMapAsync())
            .ContinueWith(async _ => await loader.UnloadImmutableScenesAsync())
            .ContinueWith(_ =>
            {
                controller.StateHandler.TranslateState("EndOfDoNothing");
            })
            .ContinueWith(async ()=> await loader.LoadAllMapAsync())
            .ContinueWith(async _ => await loader.LoadImmutableScenesAsync())
            .ContinueWith(PlayerReset)
            .ContinueWith(async () => await loader.WorkDirectorAsync(true))
            .ContinueWith(_ => TimeManager.Instance.Resume())
            .Forget();
    }

    private static void PlayerReset(bool x)
    {
        var timeManager = TimeManager.Instance;
        timeManager.SaveData.Day += 1;
        timeManager.Reset();
        timeManager.SetTime(timeManager.TimeData.MorningTime);
        
        var blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");
        blackboard.Energy = blackboard.MaxEnergy;
        blackboard.Stemina = blackboard.MaxStemina;
    }
}