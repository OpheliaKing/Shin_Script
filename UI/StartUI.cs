using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnePercent
{
    public class StartUI : MonoBehaviour
    {
        public void OnClickGameStart()
        {
            GameManager.Instance.GameStart();
            gameObject.SetActive(false);
        }
    }

}
