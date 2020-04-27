using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;

namespace LDraw { 
    public class Step : MonoBehaviour
    {
        public const float ANIMATION_DISTANCE = 10f;
        public const float ANIMATION_DURATION = 1f;

        public string number = "";
        public Version numberVersion;
        public bool isSubStep = false;

        public Vector3 animationDirection = Vector3.down;

        public static List<Animation> runningAnimations = new List<Animation>();

        public void PlayAnimations()
        {
            // Walk back up the tree until you find another step or a subModel
            // and play the appropriate animation
            int currPartIdx = transform.GetSiblingIndex();
            while (currPartIdx > 0)
            {
                GameObject currPart = transform.parent.GetChild(--currPartIdx).gameObject;

                Step previousStep = currPart.GetComponent<Step>();
                if (previousStep != null) return;

                SubModel previousSubModel = currPart.GetComponent<SubModel>();
                if (previousSubModel != null)
                {
                    // Animate the entire sub-model
                    PlaySubModelAnimation(previousSubModel);
                    return;
                }

                // Animate each part
                PlayPartAnimation(currPart);
            }
        }

        private void PlaySubModelAnimation(SubModel subModel)
        {
            Animation animation = subModel.GetComponent<Animation>();
            if (animation == null)
            {
                animation = subModel.gameObject.AddComponent<Animation>();
                animation.AddClip(CreateAnimationClip(subModel.startPosition, subModel.finalPosition), "place");
            }

            animation.Play("place");

        }

        private void PlayPartAnimation(GameObject part)
        {
            Animation animation = part.GetComponent<Animation>();
            if (animation == null)
            {
                animation = part.AddComponent<Animation>();
                Vector3 finalPosition = part.transform.localPosition;
                Vector3 startPosition = finalPosition + (animationDirection * ANIMATION_DISTANCE);
                animation.AddClip(CreateAnimationClip(startPosition, finalPosition), "place");
            }

            animation.Play("place");
        }

        private AnimationClip CreateAnimationClip(Vector3 startPosition, Vector3 finalPosition)
        {
            AnimationCurve translateX = AnimationCurve.Linear(0f, startPosition.x, ANIMATION_DURATION, finalPosition.x);
            AnimationCurve translateY = AnimationCurve.Linear(0f, startPosition.y, ANIMATION_DURATION, finalPosition.y);
            AnimationCurve translateZ = AnimationCurve.Linear(0f, startPosition.z, ANIMATION_DURATION, finalPosition.z);

            AnimationClip animationClip = new AnimationClip();
            animationClip.legacy = true;
            animationClip.SetCurve("", typeof(Transform), "localPosition.x", translateX);
            animationClip.SetCurve("", typeof(Transform), "localPosition.y", translateY);
            animationClip.SetCurve("", typeof(Transform), "localPosition.z", translateZ);

            return animationClip;
        }
    }
}
