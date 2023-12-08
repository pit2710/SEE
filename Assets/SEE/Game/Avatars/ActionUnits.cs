using System;
using System.Collections;
using System.Collections.Generic;
using Dissonance;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SEE.Game.Avatars
{
    public class ActionUnits : MonoBehaviour
    {
        private SkinnedMeshRenderer SkinnedMeshRenderer;

        private const int NoValue = 0;

        public enum ActionUnit
        {
            ActionUnit01,
            ActionUnit02,
            ActionUnit03
        }

        [SerializeField]
        public ActionUnit ActionUnitList = new ActionUnit();

        public bool ToggleAU01,
                    ToggleAU02,
                    ToggleAU03NotFacs,
                    ToggleAU04,
                    ToggleAU05,
                    ToggleAU06,
                    ToggleAU07,
                    ToggleAU08,
                    ToggleAU09,
                    ToggleAU10,
                    ToggleAU11,
                    ToggleAU12,
                    ToggleAU13,
                    ToggleAU14,
                    ToggleAU15,
                    ToggleAU16,
                    ToggleAU17,
                    ToggleAU18,
                    ToggleAU19,
                    ToggleAU20,
                    ToggleAU21,
                    ToggleAU22,
                    ToggleAU23,
                    ToggleAU24,
                    ToggleAU25,
                    ToggleAU26,
                    ToggleAU27,
                    ToggleAU28,
                    ToggleAU29,
                    ToggleAU30,
                    ToggleAU31,
                    ToggleAU32,
                    ToggleAU33,
                    ToggleAU34,
                    ToggleAU35,
                    ToggleAU36,
                    ToggleAU37,
                    ToggleAU38,
                    ToggleAU39,
                    ToggleAU43,
                    ToggleAU45,
                    ToggleAU46;


        [ShowIf("@this.ToggleAU01")] [Range(0.0f, 100.0f)]
        public float AU_01;

        [ShowIf("@this.ToggleAU02")] [Range(0.0f, 100.0f)]
        public float AU_02;

        [ShowIf("@this.ToggleAU03NotFacs")] [Range(0.0f, 100.0f)]
        public float AU_03;

        [ShowIf("@this.ToggleAU04")] [Range(0.0f, 100.0f)]
        public float AU_04;

        [ShowIf("@this.ToggleAU05")] [Range(0.0f, 100.0f)]
        public float AU_05;

        [ShowIf("@this.ToggleAU06")] [Range(0.0f, 100.0f)]
        public float AU_06;

        [ShowIf("@this.ToggleAU07")] [Range(0.0f, 100.0f)]
        public float AU_07;

        [ShowIf("@this.ToggleAU08")] [Range(0.0f, 100.0f)]
        public float AU_08;

        [ShowIf("@this.ToggleAU09")] [Range(0.0f, 100.0f)]
        public float AU_09;

        [ShowIf("@this.ToggleAU10")] [Range(0.0f, 100.0f)]
        public float AU_10;

        [ShowIf("@this.ToggleAU11")] [Range(0.0f, 100.0f)]
        public float AU_11;

        [ShowIf("@this.ToggleAU12")] [Range(0.0f, 100.0f)]
        public float AU_12;

        [ShowIf("@this.ToggleAU13")] [Range(0.0f, 100.0f)]
        public float AU_13;

        [ShowIf("@this.ToggleAU14")] [Range(0.0f, 100.0f)]
        public float AU_14;

        [ShowIf("@this.ToggleAU15")] [Range(0.0f, 100.0f)]
        public float AU_15;

        [ShowIf("@this.ToggleAU16")] [Range(0.0f, 100.0f)]
        public float AU_16;

        [ShowIf("@this.ToggleAU17")] [Range(0.0f, 100.0f)]
        public float AU_17;

        [ShowIf("@this.ToggleAU18")] [Range(0.0f, 100.0f)]
        public float AU_18;

        [ShowIf("@this.ToggleAU19")] [Range(0.0f, 100.0f)]
        public float AU_19;

        [ShowIf("@this.ToggleAU20")] [Range(0.0f, 100.0f)]
        public float AU_20;

        [ShowIf("@this.ToggleAU21")] [Range(0.0f, 100.0f)]
        public float AU_21;

        [ShowIf("@this.ToggleAU22")] [Range(0.0f, 100.0f)]
        public float AU_22;

        [ShowIf("@this.ToggleAU23")] [Range(0.0f, 100.0f)]
        public float AU_23;

        [ShowIf("@this.ToggleAU24")] [Range(0.0f, 100.0f)]
        public float AU_24;

        [ShowIf("@this.ToggleAU25")] [Range(0.0f, 100.0f)]
        public float AU_25;

        [ShowIf("@this.ToggleAU26")] [Range(0.0f, 100.0f)]
        public float AU_26;

        [ShowIf("@this.ToggleAU27")] [Range(0.0f, 100.0f)]
        public float AU_27;

        [ShowIf("@this.ToggleAU28")] [Range(0.0f, 100.0f)]
        public float AU_28;

        [ShowIf("@this.ToggleAU29")] [Range(0.0f, 100.0f)]
        public float AU_29;

        [ShowIf("@this.ToggleAU30")] [Range(0.0f, 100.0f)]
        public float AU_30;

        [ShowIf("@this.ToggleAU31")] [Range(0.0f, 100.0f)]
        public float AU_31;

        [ShowIf("@this.ToggleAU32")] [Range(0.0f, 100.0f)]
        public float AU_32;

        [ShowIf("@this.ToggleAU33")] [Range(0.0f, 100.0f)]
        public float AU_33;

        [ShowIf("@this.ToggleAU34")] [Range(0.0f, 100.0f)]
        public float AU_34;

        [ShowIf("@this.ToggleAU35")] [Range(0.0f, 100.0f)]
        public float AU_35;

        [ShowIf("@this.ToggleAU36")] [Range(0.0f, 100.0f)]
        public float AU_36;

        [ShowIf("@this.ToggleAU37")] [Range(0.0f, 100.0f)]
        public float AU_37;

        [ShowIf("@this.ToggleAU38")] [Range(0.0f, 100.0f)]
        public float AU_38;

        [ShowIf("@this.ToggleAU39")] [Range(0.0f, 100.0f)]
        public float AU_39;

        [ShowIf("@this.ToggleAU43")] [Range(0.0f, 100.0f)]
        public float AU_43;

        [ShowIf("@this.ToggleAU45")] [Range(0.0f, 100.0f)]
        public float AU_45;

        [ShowIf("@this.ToggleAU46")] [Range(0.0f, 100.0f)]
        public float AU_46;


        [ReadOnly] [ShowIf("@this.ToggleAU01 || ToggleAU03NotFacs")]
        public float Brow_Raise_Inner_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU01 || ToggleAU03NotFacs")]
        public float Brow_Raise_Inner_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU02 || ToggleAU03NotFacs || ToggleAU06 || ToggleAU43")]
        public float Brow_Raise_Outer_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU02 || ToggleAU03NotFacs || ToggleAU06 || ToggleAU43")]
        public float Brow_Raise_Outer_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU04")]
        public float Brow_Drop_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU04")]
        public float Brow_Drop_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU04")]
        public float Brow_Compress_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU04")]
        public float Brow_Compress_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU06 || ToggleAU46")]
        public float Cheek_Raise_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU06")]
        public float Cheek_Raise_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU06 || ToggleAU12 || ToggleAU43")]
        public float Mouth_Smile_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU06 || ToggleAU12 || ToggleAU43")]
        public float Mouth_Smile_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU09")]
        public float Nose_Sneer_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU09")]
        public float Nose_Sneer_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU17")]
        public float Mouth_Chin_Up_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU20")]
        public float Mouth_Stretch_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU20")]
        public float Mouth_Stretch_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU24")]
        public float Mouth_Press_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU24")]
        public float Mouth_Press_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU05 || ToggleAU43")]
        public float Eye_Wide_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU05 || ToggleAU43")]
        public float Eye_Wide_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU07 || ToggleAU43 || ToggleAU46")]
        public float Eye_Squint_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU07 || ToggleAU43")]
        public float Eye_Squint_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU08")]
        public float Mouth_Close_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU08")]
        public float Mouth_Contract_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU08 || ToggleAU19 || ToggleAU27 || ToggleAU32 || ToggleAU37")]
        public float Jaw_Open_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU10")]
        public float Mouth_Up_Upper_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU10 || ToggleAU37")]
        public float Mouth_Up_Upper_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU11")]
        public float Nose_Crease_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU11")]
        public float Nose_Crease_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU13")]
        public float Mouth_Smile_Sharp_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU13")]
        public float Mouth_Smile_Sharp_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU14")]
        public float Mouth_Dimple_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU14")]
        public float Mouth_Dimple_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU15")]
        public float Mouth_Frown_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU15")]
        public float Mouth_Frown_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU16 || ToggleAU19")]
        public float Mouth_Down_Lower_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU16 || ToggleAU19")]
        public float Mouth_Down_Lower_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU18")]
        public float Mouth_Pucker_Up_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU18")]
        public float Mouth_Pucker_Up_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU18")]
        public float Mouth_Pucker_Down_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU18")]
        public float Mouth_Pucker_Down_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU19 || ToggleAU37")]
        public float Tongue_Out_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU19 || ToggleAU37")]
        public float Tongue_Up_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU19")]
        public float Tongue_Tip_Down_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU19")]
        public float Tongue_Narrow_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU21")]
        public float Neck_Tighten_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU21")]
        public float Neck_Tighten_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU22")]
        public float Mouth_Funnel_Up_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU22")]
        public float Mouth_Funnel_Up_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU22")]
        public float Mouth_Funnel_Down_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU22")]
        public float Mouth_Funnel_Down_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU23")]
        public float Mouth_Tighten_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU23")]
        public float Mouth_Tighten_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU25 || ToggleAU32")]
        public float Mouth_Shrug_Upper_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU25 || ToggleAU32")]
        public float Mouth_Shrug_Lower_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU26")]
        public float Jaw_Down_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU28")]
        public float Mouth_Roll_In_Upper_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU28")]
        public float Mouth_Roll_In_Upper_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU28 || ToggleAU32")]
        public float Mouth_Roll_In_Lower_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU28 || ToggleAU32")]
        public float Mouth_Roll_In_Lower_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU29")]
        public float Jaw_Forward_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU30")]
        public float Jaw_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU31")]
        public float Jaw_Up_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU33")]
        public float Mouth_Blow_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU33")]
        public float Mouth_Blow_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU34")]
        public float Cheek_Puff_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU34")]
        public float Cheek_Puff_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU35")]
        public float Cheek_Suck_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU35")]
        public float Cheek_Suck_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU36")]
        public float Tongue_Bulge_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU37")]
        public float Mouth_Up_Lower_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU37")]
        public float Tongue_Wide_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU37")]
        public float Tongue_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU38")]
        public float Nose_Nostril_Dilate_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU38")]
        public float Nose_Nostril_Dilate_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU39")]
        public float Nose_Nostril_In_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU39")]
        public float Nose_Nostril_In_R_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU43 || ToggleAU45 || ToggleAU46")]
        public float Eye_Blink_L_Value;

        [ReadOnly] [ShowIf("@this.ToggleAU43 || ToggleAU45")]
        public float Eye_Blink_R_Value;




        // Start is called before the first frame update
        void Start()
        {
            SkinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
        }


        // Update is called once per frame
        void Update()
        {
            if (ToggleAU01)
            {
                PerformActionUnit01();
            }

            if (ToggleAU02)
            {
                PerformActionUnit02();
            }

            if (ToggleAU03NotFacs)
            {
                PerformActionUnit03();
            }

            if (ToggleAU04)
            {
                PerformActionUnit04();
            }

            if (ToggleAU05)
            {
                PerformActionUnit05();
            }

            if (ToggleAU06)
            {
                PerformActionUnit06();
            }

            if (ToggleAU07)
            {
                PerformActionUnit07();
            }

            if (ToggleAU08)
            {
                PerformActionUnit08();
            }

            if (ToggleAU09)
            {
                PerformActionUnit09();
            }

            if (ToggleAU10)
            {
                PerformActionUnit10();
            }

            if (ToggleAU11)
            {
                PerformActionUnit11();
            }

            if (ToggleAU12)
            {
                PerformActionUnit12();
            }

            if (ToggleAU13)
            {
                PerformActionUnit13();
            }

            if (ToggleAU14)
            {
                PerformActionUnit14();
            }

            if (ToggleAU15)
            {
                PerformActionUnit15();
            }

            if (ToggleAU16)
            {
                PerformActionUnit16();
            }

            if (ToggleAU17)
            {
                PerformActionUnit17();
            }

            if (ToggleAU18)
            {
                PerformActionUnit18();
            }

            if (ToggleAU19)
            {
                PerformActionUnit19();
            }

            if (ToggleAU20)
            {
                PerformActionUnit20();
            }

            if (ToggleAU21)
            {
                PerformActionUnit21();
            }

            if (ToggleAU22)
            {
                PerformActionUnit22();
            }

            if (ToggleAU23)
            {
                PerformActionUnit23();
            }

            if (ToggleAU24)
            {
                PerformActionUnit24();
            }

            if (ToggleAU25)
            {
                PerformActionUnit25();
            }

            if (ToggleAU26)
            {
                PerformActionUnit26();
            }

            if (ToggleAU27)
            {
                PerformActionUnit27();
            }

            if (ToggleAU28)
            {
                PerformActionUnit28();
            }

            if (ToggleAU29)
            {
                PerformActionUnit29();
            }

            if (ToggleAU30)
            {
                PerformActionUnit30();
            }

            if (ToggleAU31)
            {
                PerformActionUnit31();
            }

            if (ToggleAU32)
            {
                PerformActionUnit32();
            }

            if (ToggleAU33)
            {
                PerformActionUnit33();
            }

            if (ToggleAU34)
            {
                PerformActionUnit34();
            }

            if (ToggleAU35)
            {
                PerformActionUnit35();
            }

            if (ToggleAU36)
            {
                PerformActionUnit36();
            }

            if (ToggleAU37)
            {
                PerformActionUnit37();
            }

            if (ToggleAU38)
            {
                PerformActionUnit38();
            }

            if (ToggleAU39)
            {
                PerformActionUnit39();
            }

            if (ToggleAU43)
            {
                PerformActionUnit43();
            }

            if (ToggleAU45)
            {
                PerformActionUnit45();
            }

            if (ToggleAU46)
            {
                PerformActionUnit46();
            }
        }


        private void PerformActionUnit01()
        {
            if (ToggleAU04 || ToggleAU03NotFacs)
            {
                Debug.LogWarning("ActionUnit01 not compatible with ActionUnit04");
                ToggleAU04 = false;
                ToggleAU03NotFacs = false;
            }

            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Inner_L"), AU_01);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Inner_R"), AU_01);

            Brow_Raise_Inner_L_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Raise_Inner_L"));
            Brow_Raise_Inner_R_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Raise_Inner_R"));
        }

        private void PerformActionUnit02()
        {
            if (ToggleAU04 || ToggleAU06 || ToggleAU03NotFacs)
            {
                ToggleAU03NotFacs = false;
                ToggleAU04 = false;
                ToggleAU06 = false;
                Debug.LogWarning("ActionUnit02 not compatible with ActionUnit04 and ActionUnit06");
            }

            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_R"),
                ConvertNumberMaintainingRange(0f, 95f, AU_02));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_L"),
                ConvertNumberMaintainingRange(0f, 95f, AU_02));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Compress_L"), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Compress_R"), NoValue);

            Brow_Raise_Outer_R_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_R"));
            Brow_Raise_Outer_L_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_L"));
        }

        private void PerformActionUnit03()
        {
            if (ToggleAU01 || ToggleAU02 || ToggleAU04 || ToggleAU06)
            {
                ToggleAU01 = false;
                ToggleAU02 = false;
                ToggleAU04 = false;
                ToggleAU06 = false;
                Debug.LogWarning("ActionUnit03 not compatible with ActionUnit01, ActionUnit02, ActionUnit04 and ActionUnit06.");
            }

            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Inner_L"), AU_03);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Inner_R"), AU_03);

            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_L"), AU_03);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_R"), AU_03);

            Brow_Raise_Outer_R_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_R"));
            Brow_Raise_Outer_L_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_L"));

            Brow_Raise_Inner_L_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Raise_Inner_L"));
            Brow_Raise_Inner_R_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Raise_Inner_R"));
        }

        private void PerformActionUnit04()
        {
            if (ToggleAU01 || ToggleAU02 || ToggleAU03NotFacs)
            {
                ToggleAU01 = false;
                ToggleAU02 = false;
                ToggleAU03NotFacs = false;
                Debug.LogWarning("ActionUnit04 not compatible with ActionUnit01, ActionUnit02 and ActionUnit03");
            }

            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Drop_L"),
                ConvertNumberMaintainingRange(0f, 20f, AU_04));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Drop_R"),
                ConvertNumberMaintainingRange(0f, 20f, AU_04));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Compress_L"), AU_04);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Compress_R"), AU_04);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Inner_L"), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Inner_R"), NoValue);

            Brow_Drop_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Drop_L"));
            Brow_Drop_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Drop_R"));

            Brow_Compress_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Compress_L"));
            Brow_Compress_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Compress_R"));
        }

        private void PerformActionUnit05()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Wide_L"), AU_05);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Wide_R"), AU_05);

            Eye_Wide_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Wide_L"));
            Eye_Wide_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Wide_R"));
        }

        private void PerformActionUnit06()
        {
            if (ToggleAU02 || ToggleAU12)
            {
                ToggleAU02 = false;
                ToggleAU12 = false;
                Debug.LogWarning("ActionUnit06 not compatible with ActionUnit02 and ActionUnit12");
            }

            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Cheek_Raise_L"),
                ConvertNumberMaintainingRange(0f, 70f, AU_06));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Cheek_Raise_R"),
                ConvertNumberMaintainingRange(0f, 70f, AU_06));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Smile_L"),
                ConvertNumberMaintainingRange(0f, 65f, AU_06));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Smile_R"),
                ConvertNumberMaintainingRange(0f, 65f, AU_06));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_L"),
                ConvertNumberMaintainingRange(0f, 35f, AU_06));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_R"),
                ConvertNumberMaintainingRange(0f, 35f, AU_06));

            Cheek_Raise_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Cheek_Raise_L"));
            Cheek_Raise_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Cheek_Raise_R"));

            Mouth_Smile_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Smile_L"));
            Mouth_Smile_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Smile_R"));

            Brow_Raise_Outer_L_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_L"));
            Brow_Raise_Outer_R_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_R"));
        }

        private void PerformActionUnit07()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Squint_L"), AU_07);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Squint_R"), AU_07);

            Eye_Squint_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Squint_L"));
            Eye_Squint_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Squint_R"));
        }

        private void PerformActionUnit08()
        {
            if (ToggleAU19 || ToggleAU27)
            {
                ToggleAU19 = false;
                ToggleAU27 = false;
                Debug.LogWarning("ActionUnit08 not compatible with ActionUnit19 and ActionUnit27");
            }

            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Close"), ConvertNumberMaintainingRange(0f, 10f, AU_08));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Contract"), ConvertNumberMaintainingRange(0f, 40f, AU_08));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Jaw_Open"), ConvertNumberMaintainingRange(0f, 10f, AU_08));

            Mouth_Close_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Close"));
            Mouth_Contract_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Contract"));
            Jaw_Open_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Jaw_Open"));
        }

        private void PerformActionUnit09()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Nose_Sneer_L"), AU_09);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Nose_Sneer_R"), AU_09);

            Nose_Sneer_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Nose_Sneer_L"));
            Nose_Sneer_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Nose_Sneer_R"));
        }

        private void PerformActionUnit10()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Up_Upper_L"), AU_10);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Up_Upper_R"), AU_10);

            Mouth_Up_Upper_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Up_Upper_L"));
            Mouth_Up_Upper_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Up_Upper_R"));
        }

        private void PerformActionUnit11()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Nose_Crease_L"), AU_11);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Nose_Crease_R"), AU_11);

            Nose_Crease_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Nose_Crease_L"));
            Nose_Crease_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Nose_Crease_R"));
        }

        private void PerformActionUnit12()
        {
            if (ToggleAU06)
            {
                ToggleAU06 = false;
                Debug.LogWarning("ActionUnit12 not compatible with ActionUnit06");
            }

            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Smile_L"), AU_12);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Smile_R"), AU_12);

            Mouth_Smile_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Smile_L"));
            Mouth_Smile_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Smile_R"));
        }

        private void PerformActionUnit13()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Smile_Sharp_L"), AU_13);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Smile_Sharp_R"), AU_13);

            Mouth_Smile_Sharp_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Smile_Sharp_L"));
            Mouth_Smile_Sharp_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Smile_Sharp_R"));
        }

        private void PerformActionUnit14()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Dimple_L"), AU_14);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Dimple_R"), AU_14);

            Mouth_Dimple_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Dimple_L"));
            Mouth_Dimple_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Dimple_R"));
        }

        private void PerformActionUnit15()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Frown_L"), AU_15);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Frown_R"), AU_15);

            Mouth_Frown_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Frown_L"));
            Mouth_Frown_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Frown_R"));
        }

        private void PerformActionUnit16()
        {
            if (ToggleAU19)
            {
                ToggleAU19 = false;
                Debug.LogWarning("ActionUnit16 not compatible with ActionUnit19");
            }

            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Down_Lower_L"), AU_16);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Down_Lower_R"), AU_16);

            Mouth_Down_Lower_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Down_Lower_L"));
            Mouth_Down_Lower_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Down_Lower_R"));
        }

        private void PerformActionUnit17()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Chin_Up"), AU_17);

            Mouth_Chin_Up_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Chin_Up"));
        }

        private void PerformActionUnit18()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Pucker_Up_L"), AU_18);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Pucker_Up_R"), AU_18);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Pucker_Down_L"), AU_18);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Pucker_Down_R"), AU_18);

            Mouth_Pucker_Up_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Pucker_Up_L"));
            Mouth_Pucker_Up_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Pucker_Up_R"));
            Mouth_Pucker_Down_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Pucker_Down_L"));
            Mouth_Pucker_Down_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Pucker_Down_R"));
        }


        // TODO: Tongue Blendshapes non existent but under CC_Base_Tongue
        private void PerformActionUnit19()
        {
            if (ToggleAU16 || ToggleAU08 || ToggleAU27 )
            {
                ToggleAU08 = false;
                ToggleAU16 = false;
                ToggleAU27 = false;
                Debug.LogWarning("ActionUnit19 not compatible with ActionUnit16, ActionUnit08 and ActionUnit27");
            }

            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Down_Lower_L"), ConvertNumberMaintainingRange(0f, 20f, AU_19));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Down_Lower_R"), ConvertNumberMaintainingRange(0f, 20f, AU_19));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Tongue_Out"), ConvertNumberMaintainingRange(0f, 60f, AU_19));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Tongue_Up"), ConvertNumberMaintainingRange(0f, 40f, AU_19));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Tongue_Tip_Down"), ConvertNumberMaintainingRange(0f, 20f, AU_19));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Tongue_Narrow"), ConvertNumberMaintainingRange(0f, 20f, AU_19));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Jaw_Open"), ConvertNumberMaintainingRange(0f, 30f, AU_19));

            Mouth_Down_Lower_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Down_Lower_L"));
            Mouth_Down_Lower_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Down_Lower_R"));
            Tongue_Out_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Tongue_Out"));
            Tongue_Up_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Tongue_Up"));
            Tongue_Tip_Down_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Tongue_Tip_Down"));
            Tongue_Narrow_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Tongue_Narrow"));
            Jaw_Open_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Jaw_Open"));
        }

        private void PerformActionUnit20()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Stretch_L"), AU_20);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Stretch_R"), AU_20);

            Mouth_Stretch_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Stretch_L"));
            Mouth_Stretch_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Stretch_R"));
        }

        private void PerformActionUnit21()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Neck_Tighten_L"), AU_21);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Neck_Tighten_R"), AU_21);

            Neck_Tighten_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Neck_Tighten_L"));
            Neck_Tighten_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Neck_Tighten_R"));
        }

        private void PerformActionUnit22()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Funnel_Up_L"), AU_22);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Funnel_Up_R"), AU_22);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Funnel_Down_L"), AU_22);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Funnel_Down_R"), AU_22);

            Mouth_Funnel_Up_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Funnel_Up_L"));
            Mouth_Funnel_Up_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Funnel_Up_R"));
            Mouth_Funnel_Down_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Funnel_Down_L"));
            Mouth_Funnel_Down_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Funnel_Down_R"));
        }

        private void PerformActionUnit23()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Tighten_L"), AU_23);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Tighten_R"), AU_23);

            Mouth_Tighten_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Tighten_L"));
            Mouth_Tighten_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Tighten_R"));
        }

        private void PerformActionUnit24()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Press_L"), AU_24);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Press_R"), AU_24);

            Mouth_Press_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Press_L"));
            Mouth_Press_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Press_R"));
        }

        private void PerformActionUnit25()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Shrug_Upper"), ConvertNumberMaintainingRange(0f, 60f, AU_25));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Shrug_Lower"), ConvertNumberMaintainingRange(0f, 60f, AU_25));

            Mouth_Shrug_Upper_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Shrug_Upper"));
            Mouth_Shrug_Lower_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Shrug_Lower"));
        }

        private void PerformActionUnit26()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Jaw_Down"), AU_26);

            Jaw_Down_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Jaw_Down"));
        }

        private void PerformActionUnit27()
        {
            if (ToggleAU08 || ToggleAU19)
            {
                ToggleAU08 = false;
                ToggleAU19 = false;
                Debug.LogWarning("ActionUnit27 not compatible with ActionUnit08 and ActionUnit19");
            }

            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Jaw_Open"), AU_27);

            Jaw_Open_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Jaw_Open"));
        }

        private void PerformActionUnit28()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Roll_In_Upper_L"), AU_28);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Roll_In_Upper_R"), AU_28);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Roll_In_Lower_L"), AU_28);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Roll_In_Lower_R"), AU_28);

            Mouth_Roll_In_Upper_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Roll_In_Upper_L"));
            Mouth_Roll_In_Upper_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Roll_In_Upper_R"));
            Mouth_Roll_In_Lower_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Roll_In_Lower_L"));
            Mouth_Roll_In_Lower_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Roll_In_Lower_R"));
        }

        private void PerformActionUnit29()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Jaw_Forward"), AU_29);

            Jaw_Forward_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Jaw_Forward"));
        }

        private void PerformActionUnit30()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Jaw_L"), AU_30);

            Jaw_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Jaw_L"));
        }

        private void PerformActionUnit31()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Jaw_Up"), AU_31);

            Jaw_Up_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Jaw_Up"));
        }

        // TODO abhängigkeiten
        private void PerformActionUnit32()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Roll_In_Lower_L"), ConvertNumberMaintainingRange(0f, 50f, AU_32));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Roll_In_Lower_R"), ConvertNumberMaintainingRange(0f, 50f, AU_32));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Shrug_Upper"), ConvertNumberMaintainingRange(0f, 68f, AU_32));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Shrug_Lower"), ConvertNumberMaintainingRange(0f, 20f, AU_32));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Jaw_Open"), ConvertNumberMaintainingRange(0f, 5f, AU_32));

            Mouth_Roll_In_Lower_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Roll_In_Lower_L"));
            Mouth_Roll_In_Lower_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Roll_In_Lower_R"));
            Mouth_Shrug_Upper_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Shrug_Upper"));
            Mouth_Shrug_Lower_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Shrug_Lower"));
            Jaw_Open_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Jaw_Open"));
        }

        private void PerformActionUnit33()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Blow_L"), AU_33);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Blow_R"), AU_33);

            Mouth_Blow_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Blow_L"));
            Mouth_Blow_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Blow_R"));
        }

        private void PerformActionUnit34()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Cheek_Puff_L"), AU_34);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Cheek_Puff_R"), AU_34);

            Cheek_Puff_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Cheek_Puff_L"));
            Cheek_Puff_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Cheek_Puff_R"));
        }

        private void PerformActionUnit35()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Cheek_Suck_L"), AU_35);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Cheek_Suck_R"), AU_35);

            Cheek_Suck_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Cheek_Suck_L"));
            Cheek_Suck_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Cheek_Suck_R"));
        }

        private void PerformActionUnit36()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Tongue_Bulge_L"), AU_36);

            Tongue_Bulge_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Tongue_Bulge_L"));
        }

        private void PerformActionUnit37()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Up_Upper_R"), ConvertNumberMaintainingRange(0f, 20f, AU_37));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Up_Lower_R"), ConvertNumberMaintainingRange(0f, 44f, AU_37));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Tongue_Out"), ConvertNumberMaintainingRange(0f, 30f, AU_37));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Tongue_Up"), ConvertNumberMaintainingRange(0f, 20f, AU_37));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Tongue_Wide"), ConvertNumberMaintainingRange(0f, 30f, AU_37));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Tongue_R"), ConvertNumberMaintainingRange(0f, 56f, AU_37));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Jaw_Open"), ConvertNumberMaintainingRange(0f, 5f, AU_37));

            Mouth_Up_Upper_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Up_Upper_R"));
            Mouth_Up_Lower_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Up_Lower_R"));
            Tongue_Out_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Tongue_Out"));
            Tongue_Up_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Tongue_Up"));
            Tongue_Wide_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Tongue_Wide"));
            Tongue_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Tongue_R"));
            Jaw_Open_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Jaw_Open"));
        }

        private void PerformActionUnit38()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Nose_Nostril_Dilate_L"), AU_38);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Nose_Nostril_Dilate_R"), AU_38);

            Nose_Nostril_Dilate_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Nose_Nostril_Dilate_L"));
            Nose_Nostril_Dilate_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Nose_Nostril_Dilate_R"));
        }

        private void PerformActionUnit39()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Nose_Nostril_In_L"), AU_39);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Nose_Nostril_In_R"), AU_39);

            Nose_Nostril_In_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Nose_Nostril_In_L"));
            Nose_Nostril_In_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Nose_Nostril_In_R"));
        }

        // TODO Negativ values in CC4 - how to present in unity??
        private void PerformActionUnit43()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_L"), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_R"), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Blink_L"), AU_43);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Blink_R"), AU_43);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Squint_L"), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Squint_R"), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Wide_L"), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Wide_R"), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Smile_L"), ConvertNumberMaintainingRange(0f, 3f, AU_43));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Mouth_Smile_R"), ConvertNumberMaintainingRange(0f, 3f, AU_43));

            Brow_Raise_Outer_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_L"));
            Brow_Raise_Outer_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Brow_Raise_Outer_R"));
            Eye_Blink_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Blink_L"));
            Eye_Blink_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Blink_R"));
            Eye_Squint_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Squint_L"));
            Eye_Squint_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Squint_R"));
            Eye_Wide_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Wide_L"));
            Eye_Wide_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Wide_R"));
            Mouth_Smile_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Smile_L"));
            Mouth_Smile_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Mouth_Smile_R"));
        }

        private void PerformActionUnit45()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Blink_L"), AU_45);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Blink_R"), AU_45);

            Eye_Blink_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Blink_L"));
            Eye_Blink_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Blink_R"));
        }

        private void PerformActionUnit46()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Blink_L"), ConvertNumberMaintainingRange(0f, 90f, AU_46));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Eye_Squint_L"), ConvertNumberMaintainingRange(0f, 50f, AU_46));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString("Cheek_Raise_L"), ConvertNumberMaintainingRange(0f, 30f, AU_46));

            Eye_Blink_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Blink_L"));
            Eye_Squint_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Eye_Squint_L"));
            Cheek_Raise_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString("Cheek_Raise_L"));
        }

        /// <summary>
        /// Searches for a blendshape by name and returns the index of it.
        /// </summary>
        /// <param name="blendShapeName"></param>
        /// <returns>The index of Blendshape</returns>
        public int BlendShapeByString(string blendShapeName)
        {
            for (int i = 0; i < SkinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
            {
                if (SkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i) == blendShapeName)
                {
                    return i;
                }
            }
            return -1;
        }

        private float ConvertNumberMaintainingRange(float newMin, float newMax, float value)
        {
            if (value <= 1)
            {
                return 0;
            }
            double old_min = 0f;
            double old_max = 100f;
            double scale = (newMax - newMin) / (old_max - old_min);
            return (float)(newMin + ((value - old_min) * scale));
        }
    }
}