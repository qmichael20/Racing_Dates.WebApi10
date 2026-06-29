using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RepositoryAttribute : Attribute
    {
    }
}
