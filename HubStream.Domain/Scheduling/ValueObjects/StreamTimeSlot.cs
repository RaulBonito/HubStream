using System;

namespace HubStream.Domain.Scheduling.ValueObjects
{
    public sealed class StreamTimeSlot
    {
        public DateTime Start { get; }
        public TimeSpan Duration { get; }
        public DateTime End => Start + Duration;

        public StreamTimeSlot(DateTime start, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("La duración debe ser mayor que cero.");

            Start = start;
            Duration = duration;
        }
    }
}
