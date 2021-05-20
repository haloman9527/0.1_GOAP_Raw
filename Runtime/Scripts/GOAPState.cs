#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using System;

namespace CZToolKit.GOAP_Raw
{
    [Serializable]
    public class GOAPState
    {
        public string Key = "None";
        public bool Value;

        public GOAPState() { }

        public GOAPState(string _key, bool _value)
        {
            Key = _key;
            Value = _value;
        }
    }
}
