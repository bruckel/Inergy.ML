using System;
using System.Collections.Generic;
using System.Text;

namespace Inergy.ML.Service
{
    public interface IApiService
    {
        IEnumerable<string> GetCups();
    }
}
