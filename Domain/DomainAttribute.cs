using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DomainServiceAttribute : Attribute
    {
    }
}
