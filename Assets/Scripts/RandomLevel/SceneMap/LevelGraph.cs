using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSlay.RandomLevel
{
    public class LevelGraph
    {
        [SerializeField, Header("随机点水平分布")]
        private int m_Width = 400;

        [SerializeField, Header("随机点垂直分布")]
        private int m_Height = 400;

        [SerializeField, Header("初始点数量")]
        private int m_InitCount = 30;

        [SerializeField, Header("房间数量")]
        private int m_NeedMainRoom = 5;

        [SerializeField, Header("房间筛选，以宽高之和判定")]
        private int m_RoomFilter = 200;

        [SerializeField, Range(50, 200), Header("随机宽高范围")]
        private int m_PointRandomRange = 120;

        [SerializeField, Range(0, 1f), Header("融合三角剖分百分比")]
        private float m_MixPersents = 0.15f;

        void GenerateShape()
        {

        }

    }
}
