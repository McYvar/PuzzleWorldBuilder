public class GridObject : SceneObject
{
    protected override void OnEnable()
    {
        base.OnEnable();
        sceneObjects.Add(this);
    }
}