using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinUpWorkerService.Jobs
{
    public interface IJob
    {
        Task ExecuteGetMarketAsync();
        Task ExecuteGetHistoryAsync();

    }
}
