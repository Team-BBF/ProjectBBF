


using ProjectBBF.Event;

public interface IBOInteractiveSingle : IObjectBehaviour
{
    public void UpdateInteract(CollisionInteractionMono caller);
}
public interface IBOInteractiveMulti : IObjectBehaviour
{
    public void UpdateInteract(CollisionInteractionMono caller);
}
public interface IBOInteractiveTool : IObjectBehaviour
{
    public bool IsVaildTool(ToolRequireSet toolSet);
    public void UpdateInteract(CollisionInteractionMono caller);
}