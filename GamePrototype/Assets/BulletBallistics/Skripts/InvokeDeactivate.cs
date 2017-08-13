using UnityEngine;
using System.Collections;

namespace Ballistics
{
    public class InvokeDeactivate : MonoBehaviour
    {
        public GameObject instanceOf;
        private PoolManager myPool;

        /// <summary>
        /// call this to send the object back to the object pool to reuse it later
        /// </summary>
        public void Deactivate()
        {
            GetComponent<ParticleSystem>().Stop();
            this.gameObject.SetActive(false);
            if (myPool == null)
            {
                myPool = PoolManager.instance;
            }
            myPool.AddObject(instanceOf, this.gameObject);
        }
    }
}