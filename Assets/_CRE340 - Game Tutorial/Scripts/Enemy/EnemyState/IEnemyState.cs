// Every enemy state has the same three moments in its life, plus a name for the label.
public interface IEnemyState
{
    string Name { get; }        // shown on the state label above the enemy

    void Enter(Enemy enemy);    // called once, when this state starts
    void Update(Enemy enemy);   // called every frame while this state is active
    void Exit(Enemy enemy);     // called once, when this state ends
}