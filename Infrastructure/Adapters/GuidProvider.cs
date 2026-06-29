using Application.Abstractions.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Adapters
{
    public class GuidProvider : IGuidProvider
    {
        public Guid NewGuid() => Guid.NewGuid();

    }
}
