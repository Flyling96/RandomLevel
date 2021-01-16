using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel.Scene
{
    [ExecuteInEditMode]
    public class LevelPanelDebugger : MonoBehaviour
    {
        [HideInInspector]
        public LevelGraph m_Owner;

        public LevelPanel m_Data;

        private void Update()
        {
            if(m_Data!= null)
            {
                m_Data.m_Position = transform.position;
            }
        }

        private void OnDestroy()
        {
            if(m_Owner != null)
            {
                m_Owner.RemovePanel(m_Data);
            }
        }
    }
}
