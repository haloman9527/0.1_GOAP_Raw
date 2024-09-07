using System.Collections.Generic;
using CZToolKit.Blackboard;

namespace CZToolKit.GOAP_Raw
{
    public interface IGOAPAgent
    {
        List<IGOAPAction> Actions { get; }
        
        Dictionary<string, bool> States { get; }
        
        List<IGOAPSensor> Sensors { get; }

        Blackboard<string> Memory { get; }
        
        List<GOAPGoal> Goals { get; }
    }
}