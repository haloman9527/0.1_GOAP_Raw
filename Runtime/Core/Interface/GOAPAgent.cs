using System.Collections.Generic;
using CZToolKit.Blackboard;

namespace CZToolKit.GOAP_Raw
{
    public class GOAPAgent : IGOAPAgent
    {
        public List<IGOAPAction> Actions { get; }
        
        public Dictionary<string, bool> States { get; }
        
        public List<GOAPGoal> Goals { get; }
        
        public List<IGOAPSensor> Sensors { get; }
        
        public Blackboard<string> Memory { get; }
    }
}