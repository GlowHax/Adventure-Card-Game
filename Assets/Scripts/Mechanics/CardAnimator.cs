using System.Collections;
using UnityEngine;

namespace AdventureCardGame.Mechanics
{
    public class CardAnimator : MonoBehaviour
    {
        private static CardAnimator _instance;
        public static CardAnimator Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("CardAnimator");
                    _instance = go.AddComponent<CardAnimator>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Animates a card to a specific position and rotation.
        /// </summary>
        public IEnumerator AnimateCard(Transform card, Vector3 targetPosition, Quaternion targetRotation, float duration)
        {
            if (card == null) yield break;

            Vector3 startPos = card.position;
            Quaternion startRot = card.rotation;

            // Optional: Lift the card up slightly during movement for a nice arc
            Vector3 midPoint = Vector3.Lerp(startPos, targetPosition, 0.5f);
            midPoint.y += 0.5f;

            float time = 0;
            while (time < duration)
            {
                if (card == null) yield break; // In case the card was destroyed during animation

                time += Time.deltaTime;
                float t = time / duration;

                // SmoothStep for easing in and out
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                // Bezier curve interpolation for position (arc)
                Vector3 currentPos = Vector3.Lerp(Vector3.Lerp(startPos, midPoint, smoothT), Vector3.Lerp(midPoint, targetPosition, smoothT), smoothT);
                card.position = currentPos;

                // Slerp for rotation
                card.rotation = Quaternion.Slerp(startRot, targetRotation, smoothT);

                yield return null;
            }

            if (card != null)
            {
                card.position = targetPosition;
                card.rotation = targetRotation;
            }
        }

        /// <summary>
        /// Animates a card with a clean, wobble-free flip by separating base rotation and flip angle.
        /// </summary>
        public IEnumerator AnimateCardWithFlip(Transform card, Vector3 targetPosition, Quaternion startBaseRot, Quaternion targetBaseRot, float startFlipAngle, float endFlipAngle, float duration)
        {
            if (card == null) yield break;

            Vector3 startPos = card.position;

            Vector3 midPoint = Vector3.Lerp(startPos, targetPosition, 0.5f);
            midPoint.y += 0.5f;

            float time = 0;
            while (time < duration)
            {
                if (card == null) yield break;

                time += Time.deltaTime;
                float t = time / duration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                Vector3 currentPos = Vector3.Lerp(Vector3.Lerp(startPos, midPoint, smoothT), Vector3.Lerp(midPoint, targetPosition, smoothT), smoothT);
                card.position = currentPos;

                Quaternion baseRot = Quaternion.Slerp(startBaseRot, targetBaseRot, smoothT);
                card.rotation = baseRot * Quaternion.Euler(0, Mathf.Lerp(startFlipAngle, endFlipAngle, smoothT), 0);

                yield return null;
            }

            if (card != null)
            {
                card.position = targetPosition;
                card.rotation = targetBaseRot * Quaternion.Euler(0, endFlipAngle, 0);
            }
        }
    }
}
