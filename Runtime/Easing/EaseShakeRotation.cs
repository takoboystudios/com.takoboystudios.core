using UnityEngine;

namespace TakoBoyStudios.Utilities
{
    public class EaseShakeRotation
    {
        private readonly Vector3 m_startRotation;
        private readonly float m_strength;
        private readonly float m_decaySpeed;
        private float m_shakeLeft;

        public EaseShakeRotation(Vector3 startRotation, float strength, float duration)
        {
            m_startRotation = startRotation;
            m_shakeLeft = strength;
            m_strength = Mathf.Abs( strength );
            m_decaySpeed = m_strength / duration;
        }

        public Vector3 Update(float deltaTime)
        {
            float z = m_startRotation.z + m_shakeLeft;

            m_shakeLeft = Mathf.MoveTowards(m_shakeLeft, 0, m_decaySpeed * deltaTime);
            m_shakeLeft = -m_shakeLeft;

            return new Vector3(m_startRotation.x, m_startRotation.y, z);
        }

        public bool IsDone()
        {
            return Mathf.Approximately(m_shakeLeft, 0);
        }

        public float PercentageComplete()
        {
            return (m_strength - m_shakeLeft) / m_strength;
        }
    }
}