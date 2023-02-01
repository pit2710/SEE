using System.Collections.Generic;
using UnityEngine;

namespace CVVTuber.UnityChan
{
    public class UnityChanFaceBlendShapeController : FaceAnimationController
    {
        [Header("[Target]")]

        public SkinnedMeshRenderer EYE_DEF;

        public SkinnedMeshRenderer EL_DEF;

        public SkinnedMeshRenderer BLW_DEF;

        public SkinnedMeshRenderer MTH_DEF;


        #region CVVTuberProcess

        public override string GetDescription()
        {
            return "Update face BlendShape of UnityChan using FaceLandmarkGetter.";
        }

        public override void LateUpdateValue()
        {
            if (enableEye && EYE_DEF != null && EL_DEF != null)
            {
                EYE_DEF.SetBlendShapeWeight(6, EyeParam * 100);
                EL_DEF.SetBlendShapeWeight(6, EyeParam * 100);
            }

            if (enableBrow && BLW_DEF != null)
            {
                BLW_DEF.SetBlendShapeWeight(0, BrowParam * 100);
            }

            if (enableMouth && MTH_DEF != null)
            {
                if (MouthOpenParam >= 0.7f)
                {
                    MTH_DEF.SetBlendShapeWeight(0, MouthOpenParam * 80);
                    MTH_DEF.SetBlendShapeWeight(10, MouthOpenParam * 60);
                }
                else if (MouthOpenParam >= 0.25f)
                {
                    MTH_DEF.SetBlendShapeWeight(0, MouthOpenParam * 100);
                }
                else
                {
                    MTH_DEF.SetBlendShapeWeight(0, 0);
                    MTH_DEF.SetBlendShapeWeight(10, 0);
                }
            }
        }

        #endregion


        #region FaceAnimationController

        public override void Setup()
        {
            base.Setup();

            NullCheck(EYE_DEF, "EYE_DEF");
            NullCheck(EL_DEF, "EL_DEF");
            NullCheck(BLW_DEF, "BLW_DEF");
            NullCheck(MTH_DEF, "MTH_DEF");
        }

        protected override void UpdateFaceAnimation(List<Vector2> points)
        {
            if (enableEye)
            {
                float eyeOpen = (GetLeftEyeOpenRatio(points) + GetRightEyeOpenRatio(points)) / 2.0f;
                //Debug.Log ("eyeOpen " + eyeOpen);

                if (eyeOpen >= 0.4f)
                {
                    eyeOpen = 1.0f;
                }
                else
                {
                    eyeOpen = 0.0f;
                }
                EyeParam = Mathf.Lerp(EyeParam, 1 - eyeOpen, eyeLeapT);
            }

            if (enableBrow)
            {
                float browOpen = (GetLeftEyebrowUPRatio(points) + GetRightEyebrowUPRatio(points)) / 2.0f;
                //Debug.Log("browOpen " + browOpen);

                if (browOpen >= 0.7f)
                {
                    browOpen = 1.0f;
                }
                else if (browOpen >= 0.3f)
                {
                    browOpen = 0.5f;
                }
                else
                {
                    browOpen = 0.0f;
                }
                BrowParam = Mathf.Lerp(BrowParam, browOpen, browLeapT);
            }

            if (enableMouth)
            {
                float mouthOpen = GetMouthOpenYRatio(points);
                //Debug.Log("mouthOpen " + mouthOpen);

                if (mouthOpen >= 0.7f)
                {
                    mouthOpen = 1.0f;
                }
                else if (mouthOpen >= 0.25f)
                {
                    mouthOpen = 0.5f;
                }
                else
                {
                    mouthOpen = 0.0f;
                }
                MouthOpenParam = Mathf.Lerp(MouthOpenParam, mouthOpen, mouthLeapT);
            }
        }

        #endregion
    }
}