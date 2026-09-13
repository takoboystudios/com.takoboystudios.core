using UnityEngine;

namespace TakoBoyStudios.Utilities
{
    public class EaseShakePosition
    {
        private readonly Vector3 m_startPosition;
        private readonly float m_strength;
        private readonly float m_decaySpeed;
        private readonly bool m_shakeX;
        private readonly bool m_shakeY;
        private float m_shakeLeft;

        public EaseShakePosition(Vector3 startPosition, float strength, float duration, bool shakeX, bool shakeY)
        {
            m_startPosition = startPosition;
            m_shakeLeft = strength;
            m_strength = Mathf.Abs( strength );
            m_decaySpeed = m_strength / duration;
            m_shakeX = shakeX;
            m_shakeY = shakeY;
        }

        public Vector3 Update(float deltaTime)
        {
            float x = m_shakeX ? m_startPosition.x + m_shakeLeft : m_startPosition.x;
            float y = m_shakeY ? m_startPosition.y - m_shakeLeft : m_startPosition.y;

            m_shakeLeft = Mathf.MoveTowards(m_shakeLeft, 0, m_decaySpeed * deltaTime);
            m_shakeLeft = -m_shakeLeft;

            return new Vector3(x, y, m_startPosition.z);
        }

        public bool IsDone()
        {
            return Mathf.Approximately(m_shakeLeft, 0);
        }

        public float PercentageComplete()
        {
            return (m_strength - Mathf.Abs(m_shakeLeft)) / m_strength;
        }

        public Vector3 StartPosition()
        {
            return m_startPosition;
        }
    }
}