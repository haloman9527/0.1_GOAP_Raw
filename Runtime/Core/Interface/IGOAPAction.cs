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
 *  Blog: https://www.haloman.net/
 *
 */
#endregion

namespace CZToolKit.GOAP_Raw
{
    public interface IGOAPAction
    {
        string Name { get; }

        void DynamicallyEvaluateCost();

        void OnBeforePerform();

        GOAPActionStatus OnPerform();

        void OnAfterPerform(bool successed);
    }
}