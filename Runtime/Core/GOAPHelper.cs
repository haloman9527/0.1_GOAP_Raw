using System.Collections.Generic;

namespace CZToolKit.GOAP_Raw
{
    public static class GOAPHelper
    {
        /// <summary> l是否达成r，r包含l </summary>
        public static bool IsAchieve(IReadOnlyDictionary<string, bool> l, IReadOnlyDictionary<string, bool> r)
        {
            if (r.Count == 0)
                return true;

            foreach (var pair in r)
            {
                if (!l.TryGetValue(pair.Key, out bool value) && pair.Value)
                    return false;
                if (pair.Value != value)
                    return false;
            }

            return true;
        }
    }

    public class GOAPGoalPriorityComparer : IComparer<IGOAPGoal>
    {
        /// <summary>
        /// 优先级高-低
        /// </summary>
        public static GOAPGoalPriorityComparer Default { get; } = new GOAPGoalPriorityComparer();
        
        public int Compare(IGOAPGoal x, IGOAPGoal y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return -x.Priority.CompareTo(y.Priority);
        }
    }

    public class GOAPActionComparer : IComparer<IGOAPAction>
    {
        public static GOAPActionComparer Default { get; } = new GOAPActionComparer();
        
        public int Compare(IGOAPAction x, IGOAPAction y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return x.Cost.CompareTo(y.Cost);
        }
    }
}