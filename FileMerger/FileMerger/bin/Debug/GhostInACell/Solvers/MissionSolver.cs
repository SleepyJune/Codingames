using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    interface MissionSolver
    {
        void Solve();
        void Execute();
        void RateMission();
        int CalculateReward();
    }
}
