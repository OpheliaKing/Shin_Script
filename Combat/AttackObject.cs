using System;
using UnityEngine;

namespace OnePercent
{
    public class AttackObject : MonoBehaviour
    {
        /// <summary>
        /// 풀 반환 시 호출. 풀 매니저가 구독해 반환 처리.
        /// </summary>
        public Action<AttackObject> OnReturnToPool;

        /// <summary>
        /// 어느 풀에 속하는지 구분용. Get 시 설정되고, Return 시 사용.
        /// </summary>
        public string PoolKey { get; set; }

        /// <summary>
        /// 풀에 반환 요청. 파생 클래스에서 히트/타임아웃 시 호출.
        /// </summary>
        protected void ReturnToPool()
        {
            OnReturnToPool?.Invoke(this);
        }
    }
}
