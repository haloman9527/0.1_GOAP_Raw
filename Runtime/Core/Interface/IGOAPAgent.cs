﻿using System.Collections.Generic;
using Atom;

namespace Atom.GOAP_Raw
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
        
        EventStation<string> Events { get; }

        void Update();
    }
}