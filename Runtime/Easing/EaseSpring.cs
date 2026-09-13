using UnityEngine;

namespace TakoBoyStudios.Utilities
{
    /// <summary>
    /// Use this class to create a simple spring that will be modified by "hits".
    /// Create by having an instance variable in your class:
    /// private EaseSpring m_spring;
    /// then create it:
    /// m_spring = new EaseSpring(float duration, Vector3 start, Vector3 direction, float dampening, float tension);
    /// The spring will be setup but it's speed will be zero. You need to "hit" it to activate it.
    /// m_spring.Hit(new Vector3(power,power,power);
    /// You'll need to set the spring back to the object you want to spring in the Update every frame:
    /// transform.position = m_spring.Update(Time.deltaTime);
    /// </summary>
    public class EaseSpring
    {
        private readonly Vector3 m_direction;
        private readonly float m_dampening;
        private readonly float m_tension;
        private readonly Vector3 m_originalPosition;
        private Vector3 m_currentPosition;
        private Vector3 m_speed;

        public EaseSpring(Vector3 originalPosition, Vector3 direction, float dampening, float tension)
        {
            m_originalPosition = originalPosition;
            m_direction = direction.normalized;
            m_dampening = dampening;
            m_tension = tension;
            m_currentPosition = m_originalPosition;
            m_speed = Vector3.zero;
        }

        public Vector3 Update(float deltaTime)
        {
            Vector3 displacement = (m_direction + m_originalPosition) - m_currentPosition;
            m_speed += m_tension * displacement * deltaTime - m_speed * m_dampening;
            m_currentPosition += m_speed * deltaTime;

            return m_currentPosition;
        }

        public void Hit(Vector3 direction)
        {
            m_speed += direction;
        }

        public bool IsDone()
        {
            return Vector3.Distance(m_currentPosition, m_direction + m_originalPosition) <= 0.01f && m_speed.magnitude <= 0.01f;
        }
    }
}