namespace FixedCameraPrototype.Enemy
{
    // What an enemy does while it has no target. One per enemy; swapping it defines the enemy variant.
    public interface IEnemyIdleBehavior
    {
        void BeginIdle();
        void TickIdle(float deltaTime);
    }
}
