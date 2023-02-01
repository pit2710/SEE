using UnityEngine;

namespace CVVTuber.UnityChan
{
    public class UnityChanFaceAnimationClipController : CVVTuberProcess
    {
        [Header("[Setting]")]

        public float delayWeight = 0.3f;

        public bool isKeepFace = false;

        [Header("[Target]")]

        public Animator target;

        public AnimationClip[] animations;

        protected float current = 0;


        #region CVVTuberProcess

        public override string GetDescription()
        {
            return "Update face AnimationClip of UnityChan by GUI Button.";
        }

        public override void Setup()
        {
            NullCheck(target, "target");
        }

        public override void LateUpdateValue()
        {
            if (Input.GetMouseButton(0))
            {
                current = 1;
            }
            else if (!isKeepFace)
            {
                current = Mathf.Lerp(current, 0, delayWeight);
            }
            target.SetLayerWeight(1, current);
        }

        #endregion


        protected virtual void OnGUI()
        {
            int topPos = 200;
            GUILayout.Space(topPos);

            GUILayout.Box("Face Update", GUILayout.Width(170), GUILayout.Height(25 * (animations.Length + 2)));
            Rect screenRect = new Rect(10, topPos + 25, 150, 25 * (animations.Length + 1));
            GUILayout.BeginArea(screenRect);
            foreach (var animation in animations)
            {
                if (GUILayout.RepeatButton(animation.name))
                {
                    target.CrossFade(animation.name, 0);
                }
            }
            isKeepFace = GUILayout.Toggle(isKeepFace, " Keep Face");
            GUILayout.EndArea();
        }

        public virtual void OnCallChangeFace(string str)
        {
            int ichecked = 0;
            foreach (var animation in animations)
            {
                if (str == animation.name)
                {
                    ChangeFace(str);
                    break;
                }
                else if (ichecked <= animations.Length)
                {
                    ichecked++;
                }
                else
                {
                    str = "default@unitychan";
                    ChangeFace(str);
                }
            }
        }

        protected virtual void ChangeFace(string str)
        {
            isKeepFace = true;
            current = 1;
            target.CrossFade(str, 0);
        }
    }
}