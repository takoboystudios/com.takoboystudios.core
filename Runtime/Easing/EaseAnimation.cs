using System;
using TakoBoyStudios;
using UnityEngine;

namespace TakoBoyStudios.Utilities
{
    public class EaseAnimation
    {
        private float timeElapsed;
        private readonly float m_duration;
        private readonly Vector3 m_start;
        private readonly Vector3 m_end;
        private readonly EasingFunction.Function m_easingFunction;

        public EaseAnimation(float duration, Vector3 start, Vector3 end, EasingFunction.Function easingFunction)
        {
            m_duration = duration;
            m_start = start;
            m_end = end;
            m_easingFunction = easingFunction;
        }

        public Vector3 Update(float deltaTime)
        {
            timeElapsed += deltaTime;

            if (timeElapsed > m_duration)
            {
                return m_end;
            }

            var t = timeElapsed / m_duration;
            var easedT = m_easingFunction(0, 1, t);

            return Vector3.LerpUnclamped(m_start, m_end, easedT);
        }

        public bool IsDone()
        {
            return timeElapsed > m_duration;
        }

        public float PercentComplete()
        {
            return timeElapsed / m_duration;
        }

        public Vector3 EndPosition()
        {
            return m_end;
        }

        public void Complete()
        {
            timeElapsed = m_duration;
        }

        public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta, EasingFunction.Ease easeType)
        {
            Vector2 direction = (target - current).normalized;
            float distance = Vector2.Distance(current, target);
            EasingFunction.Function easingFunction=EasingFunction.GetEasingFunction(easeType);
            float movementAmount = easingFunction(0, distance, maxDistanceDelta);

            // Cap the movement to the target position
            if (movementAmount > distance)
                return target;

            return current + direction * movementAmount;
        }
    }
}