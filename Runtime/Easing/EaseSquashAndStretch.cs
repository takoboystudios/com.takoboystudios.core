using UnityEngine;

namespace TakoBoyStudios.Utilities
{
    public class EaseSquashAndStretch
    {
        public enum Pivot
        {
            Center,
            Bottom,
            Top,
            Left,
            Right
        }

        private float timeElapsed;
        private readonly float m_duration;
        private readonly Vector3 m_startScale;
        private readonly Vector3 m_endScale;
        private readonly EasingFunction.Function m_easingFunction;
        private readonly float m_squashFactor;
        private readonly float m_stretchFactor;
        private readonly Pivot m_pivot;

        public EaseSquashAndStretch(float duration, Vector3 endScale, EasingFunction.Function easingFunction, float squashFactor, float stretchFactor, Pivot pivot)
        {
            m_duration = duration;
            m_startScale = Vector3.one;
            m_endScale = endScale;
            m_easingFunction = easingFunction;
            m_squashFactor = squashFactor;
            m_stretchFactor = stretchFactor;
            m_pivot = pivot;
        }

        public Vector3 Update(float deltaTime, Transform transform)
        {
            timeElapsed += deltaTime;

            if (timeElapsed > m_duration)
            {
                return m_endScale;
            }

            float t = timeElapsed / m_duration;
            float easedT = m_easingFunction(0, 1, t);
            Vector3 previousScale = transform.localScale;
            Vector3 newScale = Vector3.LerpUnclamped(m_startScale, m_endScale, easedT);

            newScale.x *= (1.0f + m_squashFactor * (1.0f - easedT)); // Squash horizontally
            newScale.y *= (1.0f + m_stretchFactor * easedT); // Stretch vertically

            AdjustPositionBasedOnPivot(transform, previousScale, newScale, m_pivot);

            return newScale;
        }

        private void AdjustPositionBasedOnPivot(Transform transform, Vector3 oldScale, Vector3 newScale, Pivot pivot)
        {
            Vector3 scaleChange = newScale - oldScale;
            Vector3 positionAdjustment = Vector3.zero;

            switch (pivot)
            {
                case Pivot.Bottom:
                    positionAdjustment.y = scaleChange.y / 2;
                    break;
                case Pivot.Top:
                    positionAdjustment.y = -scaleChange.y / 2;
                    break;
                case Pivot.Left:
                    positionAdjustment.x = scaleChange.x / 2;
                    break;
                case Pivot.Right:
                    positionAdjustment.x = -scaleChange.x / 2;
                    break;
                // Center requires no position adjustment.
            }

            transform.localPosition += positionAdjustment;
        }

        public bool IsDone()
        {
            return timeElapsed > m_duration;
        }

        public float PercentComplete()
        {
            return timeElapsed / m_duration;
        }

        public Vector3 EndScale()
        {
            return m_endScale;
        }

        public void Complete()
        {
            timeElapsed = m_duration;
        }
    }
}