using HubStream.Shared.Kernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Domain.Scheduling.ValueObjects
{
    public record Rating(int Value)
    {
        public static Result<Rating> Create(int value)
        {
            if (value < 1 || value > 5)
            {
                return Result<Rating>.Failure(new Error("Rating.Create.InvalidRange", "Invalid rating range"));
            }
            return Result<Rating>.Success(new Rating(value));
        }
    }
}
