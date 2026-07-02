namespace NewFeature.Models
{
    public enum ProjectStatus
    {
        Planning,
        Active,
        Completed,
        OnHold
    }

    public enum VehicleStatus
    {
        Available,
        Active,
        InMaintenance,
        OutOfService
    }

    public enum TripStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled
    }

    public enum TaskStatus
    {
        ToDo,
        InProgress,
        InReview,
        Done
    }

    public enum DependencyType
    {
        FinishToStart,
        StartToStart,
        FinishToFinish,
        StartToFinish
    }
}
