using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Scheduling.ValueObjects
{
    public sealed class StreamCapacity
    {
        public int MaxParticipants { get; }

        public StreamCapacity(int maxParticipants)
        {
            if (maxParticipants <= 0)
                throw new ArgumentException("La capacidad máxima debe ser mayor que cero.");

            MaxParticipants = maxParticipants;
        }
    }
}
