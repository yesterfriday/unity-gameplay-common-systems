namespace Yesterfriday.GameplayCommonSystems.Slingshot
{
    public enum SlingshotFailReason
    {
        None = 0,
        NullDefinition,
        NullPrefab,
        NullOrigin,
        NullCamera,
        CoolingDown,
        NoPlaneHit,
        NotPulling,
        PullTooSmall,
        InvalidParams,
        NoRigidbody2D,
        AlreadyPulling,
    }
}