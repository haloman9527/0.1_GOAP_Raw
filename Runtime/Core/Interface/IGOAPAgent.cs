using System.Collections.Generic;
using Jiange.Blackboard;

namespace Jiange.GOAP_Raw
{
    public interface IGOAPAgent
    {
        AgentState State { get; }
        
        IGOAPAction CurrentAction { get; }
        
        List<IGOAPAction> Actions { get; }
        
        Dictionary<string, bool> States { get; }
        
        List<IGOAPGoal> Goals { get; }
        
        List<IGOAPSensor> Sensors { get; }

        Blackboard<string> Memory { get; }
        
        Events<string> Events { get; }

        void Update();
    }
}