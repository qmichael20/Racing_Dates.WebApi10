using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Ports
{
    public interface IUnitOfWork
    {
        Task SaveAsync(CancellationToken? cancellationToken = null);
    }
}
