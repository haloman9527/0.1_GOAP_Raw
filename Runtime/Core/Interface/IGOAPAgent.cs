using System.Collections.Generic;
using CZToolKit.Blackboard;

namespace CZToolKit.GOAP_Raw
{
    public interface IGOAPAgent
    { 
        Dictionary<string, bool> States { get; }

        Blackboard<string> Memory { get; }

        void SetState(string key, bool value);
    }
}