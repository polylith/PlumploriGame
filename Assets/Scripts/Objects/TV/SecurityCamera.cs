using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace Television
{
    /// <summary>
    /// This security camera can be rotated in user-definable
    /// Vector3 values. The duration of the rotation and the
    /// remaining time in a rotation position can be defined.
    /// </summary>
    public class SecurityCamera : AutoCameraRegisterer
    {
        public Transform rotationPoint;
        public Vector3[] rotations = new Vector3[]
        {
            new Vector3(0f, 0f, 100f),
            new Vector3(0f, 0f, 240f)
        };
        [Range(2f,float.PositiveInfinity)]
        public float rotateDuration = 5f;
        [Range(1f, float.PositiveInfinity)]
        public float restDuration = 15f;

        private int rotationIndex = 0;
        private IEnumerator ieRotate;
        private Sequence sequence;

        private void ClearSequence()
        {
            if (null != sequence)
            {
                sequence.Pause();
                sequence.Kill(false);
            }

            sequence = null;
        }

        protected override void OnEnabled()
        {
            if (null == ieRotate && rotations.Length > 0)
            {
                ieRotate = IERotate();
                StartCoroutine(ieRotate);
            }
        }

        protected override void OnDisabled()
        {
            if (null != ieRotate)
            {
                ClearSequence();
                StopCoroutine(ieRotate);
                ieRotate = null;
            }
        }

        private void Next()
        {
            rotationIndex++;
            rotationIndex %= rotations.Length;
        }

        private IEnumerator IERotate()
        {
            while (isEnabled)
            {
                yield return new WaitForSeconds(restDuration);

                DOTween.Sequence().
                    SetAutoKill(false).
                    Append(
                        rotationPoint.DOLocalRotate(
                            rotations[rotationIndex],
                            rotateDuration
                        ).SetEase(Ease.Linear)
                    ).
                    OnComplete(() => {
                        ClearSequence();
                    }).
                    Play();

                Next();

                yield return new WaitForSeconds(rotateDuration);
            }

            yield return null;

            ieRotate = null;
        }
    }
}
