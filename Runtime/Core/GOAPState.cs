#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.mindgear.net/
 *
 */
#endregion
using System;
using UnityEngine.Serialization;

namespace CZToolKit.GOAP_Raw
{
    [Serializable]
    public class GOAPState
    {
        public string key = "None";
        public bool value;

        public GOAPState() { }

        public GOAPState(string key, bool value)
        {
            this.key = key;
            this.value = value;
        }
    }
}