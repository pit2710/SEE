using Sirenix.OdinInspector;
using UnityEngine;

namespace SEE.Game.Avatars
{
    /// <summary>
    /// This script is used to visualize ActionUnits on a CharacterCreator4 avatar.
    /// </summary>
    public class ActionUnits : MonoBehaviour
    {
        // Skinned Mesh Renderer.
        private SkinnedMeshRenderer SkinnedMeshRenderer;

        // Empty Value.
        private const int NoValue = 0;

        // Brow strings.
        private const string BrowRaiseInnerL = "Brow_Raise_Inner_L";
        private const string BrowRaiseInnerR = "Brow_Raise_Inner_R";

        private const string BrowRaiseOuterL = "Brow_Raise_Outer_L";
        private const string BrowRaiseOuterR = "Brow_Raise_Outer_R";

        private const string BrowCompressL = "Brow_Compress_L";
        private const string BrowCompressR = "Brow_Compress_R";

        private const string BrowDropL = "Brow_Drop_L";
        private const string BrowDropR = "Brow_Drop_R";

        // Eye strings.
        private const string EyeWideL = "Eye_Wide_L";
        private const string EyeWideR = "Eye_Wide_R";

        private const string EyeSquintL = "Eye_Squint_L";
        private const string EyeSquintR = "Eye_Squint_R";

        private const string EyeBlinkL = "Eye_Blink_L";
        private const string EyeBlinkR = "Eye_Blink_R";

        // Cheek strings.
        private const string CheekRaiseL = "Cheek_Raise_L";
        private const string CheekRaiseR = "Cheek_Raise_R";

        private const string CheekPuffL = "Cheek_Puff_L";
        private const string CheekPuffR = "Cheek_Puff_R";

        private const string CheekSuckL = "Cheek_Suck_L";
        private const string CheekSuckR = "Cheek_Suck_R";

        // Mouth strings.
        private const string MouthSmileL = "Mouth_Smile_L";
        private const string MouthSmileR = "Mouth_Smile_R";

        private const string MouthClose = "Mouth_Close";
        private const string Mouth_Contract = "Mouth_Contract";

        private const string MouthUpUpperL = "Mouth_Up_Upper_L";
        private const string MouthUpUpperR = "Mouth_Up_Upper_R";

        private const string MouthSmileSharpL = "Mouth_Smile_Sharp_L";
        private const string MouthSmileSharpR = "Mouth_Smile_Sharp_R";

        private const string MouthDimpleL = "Mouth_Dimple_L";
        private const string MouthDimpleR = "Mouth_Dimple_R";

        private const string MouthFrownL = "Mouth_Frown_L";
        private const string MouthFrownR = "Mouth_Frown_R";

        private const string MouthDownLowerL = "Mouth_Down_Lower_L";
        private const string MouthDownLowerR = "Mouth_Down_Lower_R";

        private const string MouthChinUp = "Mouth_Chin_Up";

        private const string MouthPuckerUpL = "Mouth_Pucker_Up_L";
        private const string MouthPuckerUpR = "Mouth_Pucker_Up_R";

        private const string MouthPuckerDownL = "Mouth_Pucker_Down_L";
        private const string MouthPuckerDownR = "Mouth_Pucker_Down_R";

        private const string MouthStretchL = "Mouth_Stretch_L";
        private const string MouthStretchR = "Mouth_Stretch_R";

        private const string MouthFunnelUpL = "Mouth_Funnel_Up_L";
        private const string MouthFunnelUpR = "Mouth_Funnel_Up_R";

        private const string MouthFunnelDownL = "Mouth_Funnel_Down_L";
        private const string MouthFunnelDownR = "Mouth_Funnel_Down_R";

        private const string MouthTightenL = "Mouth_Tighten_L";
        private const string MouthTightenR = "Mouth_Tighten_R";

        private const string MouthPressL = "Mouth_Press_L";
        private const string MouthPressR = "Mouth_Press_R";

        private const string MouthShrugUpper = "Mouth_Shrug_Upper";
        private const string MouthShrugLower = "Mouth_Shrug_Lower";

        private const string MouthRollInUpperL = "Mouth_Roll_In_Upper_L";
        private const string MouthRollInUpperR = "Mouth_Roll_In_Upper_R";

        private const string MouthRollInLowerL = "Mouth_Roll_In_Lower_L";
        private const string MouthRollInLowerR = "Mouth_Roll_In_Lower_R";

        private const string MouthBlowL = "Mouth_Blow_L";
        private const string MouthBlowR = "Mouth_Blow_R";

        private const string MouthUpLowerR = "Mouth_Up_Lower_R";

        // Jaw strings.
        private const string JawOpen = "Jaw_Open";
        private const string JawDown = "Jaw_Down";
        private const string JawForward = "Jaw_Forward";

        private const string JawL = "Jaw_L";
        private const string JawUp = "Jaw_Up";

        // Nose strings.
        private const string NoseSneerL = "Nose_Sneer_L";
        private const string NoseSneerR = "Nose_Sneer_R";

        private const string NoseCreaseL = "Nose_Crease_L";
        private const string NoseCreaseR = "Nose_Crease_R";

        private const string NoseNostrilDilateL = "Nose_Nostril_Dilate_L";
        private const string NoseNostrilDilateR = "Nose_Nostril_Dilate_R";

        private const string NoseNostrilInL = "Nose_Nostril_In_L";
        private const string NoseNostrilInR = "Nose_Nostril_In_R";

        // Tongue strings.
        private const string TongueOut = "Tongue_Out";
        private const string TongueUp = "Tongue_Up";

        private const string TongueWide = "Tongue_Wide";

        private const string TongueR = "Tongue_R";

        private const string TongueTipDown = "Tongue_Tip_Down";
        private const string TongueNarrow = "Tongue_Narrow";

        private const string TongueBulgeL = "Tongue_Bulge_L";

        // Neck strings.
        private const string NeckTightenL = "Neck_Tighten_L";
        private const string NeckTightenR = "Neck_Tighten_R";

        // Enum for different ActionUnits.
        public enum ActionUnit
        {
            ActionUnit01,
            ActionUnit02,
            ActionUnit03,
            ActionUnit04,
            ActionUnit05,
            ActionUnit06,
            ActionUnit07,
            ActionUnit08,
            ActionUnit09,
            ActionUnit10,
            ActionUnit11,
            ActionUnit12,
            ActionUnit13,
            ActionUnit14,
            ActionUnit15,
            ActionUnit16,
            ActionUnit17,
            ActionUnit18,
            ActionUnit19,
            ActionUnit20,
            ActionUnit21,
            ActionUnit22,
            ActionUnit23,
            ActionUnit24,
            ActionUnit25,
            ActionUnit26,
            ActionUnit27,
            ActionUnit28,
            ActionUnit29,
            ActionUnit30,
            ActionUnit31,
            ActionUnit32,
            ActionUnit33,
            ActionUnit34,
            ActionUnit35,
            ActionUnit36,
            ActionUnit37,
            ActionUnit38,
            ActionUnit39,
            ActionUnit43,
            ActionUnit45,
            ActionUnit46
        }

        [SerializeField]
        public ActionUnit ActionUnitList = new ActionUnit();

        // Only show sliders for the selected ActionUnit using Odin inspector.
        // For reference: https://odininspector.com/attributes/show-if-attribute
        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit01)] [Range(0.0f, 100.0f)]
        public float AU_01;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit02)] [Range(0.0f, 100.0f)]
        public float AU_02;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit03)] [Range(0.0f, 100.0f)]
        public float AU_03;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit04)] [Range(0.0f, 100.0f)]
        public float AU_04;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit05)] [Range(0.0f, 100.0f)]
        public float AU_05;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit06)] [Range(0.0f, 100.0f)]
        public float AU_06;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit07)] [Range(0.0f, 100.0f)]
        public float AU_07;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit08)] [Range(0.0f, 100.0f)]
        public float AU_08;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit09)] [Range(0.0f, 100.0f)]
        public float AU_09;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit10)] [Range(0.0f, 100.0f)]
        public float AU_10;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit11)] [Range(0.0f, 100.0f)]
        public float AU_11;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit12)] [Range(0.0f, 100.0f)]
        public float AU_12;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit13)] [Range(0.0f, 100.0f)]
        public float AU_13;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit14)] [Range(0.0f, 100.0f)]
        public float AU_14;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit15)] [Range(0.0f, 100.0f)]
        public float AU_15;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit16)] [Range(0.0f, 100.0f)]
        public float AU_16;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit17)] [Range(0.0f, 100.0f)]
        public float AU_17;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit18)] [Range(0.0f, 100.0f)]
        public float AU_18;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit19)] [Range(0.0f, 100.0f)]
        public float AU_19;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit20)] [Range(0.0f, 100.0f)]
        public float AU_20;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit21)] [Range(0.0f, 100.0f)]
        public float AU_21;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit22)] [Range(0.0f, 100.0f)]
        public float AU_22;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit23)] [Range(0.0f, 100.0f)]
        public float AU_23;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit24)] [Range(0.0f, 100.0f)]
        public float AU_24;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit25)] [Range(0.0f, 100.0f)]
        public float AU_25;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit26)] [Range(0.0f, 100.0f)]
        public float AU_26;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit27)] [Range(0.0f, 100.0f)]
        public float AU_27;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit28)] [Range(0.0f, 100.0f)]
        public float AU_28;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit29)] [Range(0.0f, 100.0f)]
        public float AU_29;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit30)] [Range(0.0f, 100.0f)]
        public float AU_30;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit31)] [Range(0.0f, 100.0f)]
        public float AU_31;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit32)] [Range(0.0f, 100.0f)]
        public float AU_32;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit33)] [Range(0.0f, 100.0f)]
        public float AU_33;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit34)] [Range(0.0f, 100.0f)]
        public float AU_34;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit35)] [Range(0.0f, 100.0f)]
        public float AU_35;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit36)] [Range(0.0f, 100.0f)]
        public float AU_36;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit37)] [Range(0.0f, 100.0f)]
        public float AU_37;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit38)] [Range(0.0f, 100.0f)]
        public float AU_38;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit39)] [Range(0.0f, 100.0f)]
        public float AU_39;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit43)] [Range(0.0f, 100.0f)]
        public float AU_43;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit45)] [Range(0.0f, 100.0f)]
        public float AU_45;

        [ShowIf("@this.ActionUnitList", ActionUnit.ActionUnit46)] [Range(0.0f, 100.0f)]
        public float AU_46;

        // Show the different blendshapes and their values for the selected action unit. ReadOnly
        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit01 || ActionUnitList == ActionUnit.ActionUnit03")]
        public float Brow_Raise_Inner_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit01 || ActionUnitList == ActionUnit.ActionUnit03")]
        public float Brow_Raise_Inner_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit02 || ActionUnitList == ActionUnit.ActionUnit03 || ActionUnitList == ActionUnit.ActionUnit06 || ActionUnitList == ActionUnit.ActionUnit43")]
        public float Brow_Raise_Outer_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit02 || ActionUnitList == ActionUnit.ActionUnit03 || ActionUnitList == ActionUnit.ActionUnit06 || ActionUnitList == ActionUnit.ActionUnit43")]
        public float Brow_Raise_Outer_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit04")]
        public float Brow_Drop_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit04")]
        public float Brow_Drop_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit04")]
        public float Brow_Compress_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit04")]
        public float Brow_Compress_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit06 || ActionUnitList == ActionUnit.ActionUnit46")]
        public float Cheek_Raise_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit06")]
        public float Cheek_Raise_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit06 || ActionUnitList == ActionUnit.ActionUnit12 || ActionUnitList == ActionUnit.ActionUnit43")]
        public float Mouth_Smile_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit06 || ActionUnitList == ActionUnit.ActionUnit12 || ActionUnitList == ActionUnit.ActionUnit43")]
        public float Mouth_Smile_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit09")]
        public float Nose_Sneer_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit09")]
        public float Nose_Sneer_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit17")]
        public float Mouth_Chin_Up_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit20")]
        public float Mouth_Stretch_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit20")]
        public float Mouth_Stretch_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit24")]
        public float Mouth_Press_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit24")]
        public float Mouth_Press_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit05 || ActionUnitList == ActionUnit.ActionUnit43")]
        public float Eye_Wide_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit05 || ActionUnitList == ActionUnit.ActionUnit43")]
        public float Eye_Wide_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit07 || ActionUnitList == ActionUnit.ActionUnit43 || ActionUnitList == ActionUnit.ActionUnit46")]
        public float Eye_Squint_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit07 || ActionUnitList == ActionUnit.ActionUnit43")]
        public float Eye_Squint_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit08")]
        public float Mouth_Close_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit08")]
        public float Mouth_Contract_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit08 || ActionUnitList == ActionUnit.ActionUnit19 || ActionUnitList == ActionUnit.ActionUnit27 || ActionUnitList == ActionUnit.ActionUnit32 || ActionUnitList == ActionUnit.ActionUnit37")]
        public float Jaw_Open_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit10")]
        public float Mouth_Up_Upper_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit10 || ActionUnitList == ActionUnit.ActionUnit37")]
        public float Mouth_Up_Upper_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit11")]
        public float Nose_Crease_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit11")]
        public float Nose_Crease_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit13")]
        public float Mouth_Smile_Sharp_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit13")]
        public float Mouth_Smile_Sharp_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit14")]
        public float Mouth_Dimple_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit14")]
        public float Mouth_Dimple_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit15")]
        public float Mouth_Frown_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit15")]
        public float Mouth_Frown_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit16 || ActionUnitList == ActionUnit.ActionUnit19")]
        public float Mouth_Down_Lower_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit16 || ActionUnitList == ActionUnit.ActionUnit19")]
        public float Mouth_Down_Lower_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit18")]
        public float Mouth_Pucker_Up_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit18")]
        public float Mouth_Pucker_Up_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit18")]
        public float Mouth_Pucker_Down_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit18")]
        public float Mouth_Pucker_Down_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit19 || ActionUnitList == ActionUnit.ActionUnit37")]
        public float Tongue_Out_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit19 || ActionUnitList == ActionUnit.ActionUnit37")]
        public float Tongue_Up_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit19")]
        public float Tongue_Tip_Down_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit19")]
        public float Tongue_Narrow_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit21")]
        public float Neck_Tighten_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit21")]
        public float Neck_Tighten_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit22")]
        public float Mouth_Funnel_Up_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit22")]
        public float Mouth_Funnel_Up_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit22")]
        public float Mouth_Funnel_Down_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit22")]
        public float Mouth_Funnel_Down_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit23")]
        public float Mouth_Tighten_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit23")]
        public float Mouth_Tighten_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit25 || ActionUnitList == ActionUnit.ActionUnit32")]
        public float Mouth_Shrug_Upper_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit25 || ActionUnitList == ActionUnit.ActionUnit32")]
        public float Mouth_Shrug_Lower_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit26")]
        public float Jaw_Down_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit28")]
        public float Mouth_Roll_In_Upper_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit28")]
        public float Mouth_Roll_In_Upper_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit28 || ActionUnitList == ActionUnit.ActionUnit32")]
        public float Mouth_Roll_In_Lower_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit28 || ActionUnitList == ActionUnit.ActionUnit32")]
        public float Mouth_Roll_In_Lower_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit29")]
        public float Jaw_Forward_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit30")]
        public float Jaw_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit31")]
        public float Jaw_Up_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit33")]
        public float Mouth_Blow_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit33")]
        public float Mouth_Blow_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit34")]
        public float Cheek_Puff_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit34")]
        public float Cheek_Puff_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit35")]
        public float Cheek_Suck_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit35")]
        public float Cheek_Suck_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit36")]
        public float Tongue_Bulge_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit37")]
        public float Mouth_Up_Lower_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit37")]
        public float Tongue_Wide_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit37")]
        public float Tongue_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit38")]
        public float Nose_Nostril_Dilate_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit38")]
        public float Nose_Nostril_Dilate_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit39")]
        public float Nose_Nostril_In_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit39")]
        public float Nose_Nostril_In_R_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit43 || ActionUnitList == ActionUnit.ActionUnit45 || ActionUnitList == ActionUnit.ActionUnit46")]
        public float Eye_Blink_L_Value;

        [ReadOnly] [ShowIf("@this.ActionUnitList == ActionUnit.ActionUnit43 || ActionUnitList == ActionUnit.ActionUnit45")]
        public float Eye_Blink_R_Value;

        /// <summary>
        /// Get and set the SkinnedMeshRenderer at the start.
        /// </summary>
        void Start()
        {
            SkinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
        }

        /// <summary>
        /// Executes the selected action unit.
        /// </summary>
        void Update()
        {
            switch (ActionUnitList)
            {
                case ActionUnit.ActionUnit01:
                    PerformActionUnit01();
                    break;
                case ActionUnit.ActionUnit02:
                    PerformActionUnit02();
                    break;
                case ActionUnit.ActionUnit03:
                    PerformActionUnit03();
                    break;
                case ActionUnit.ActionUnit04:
                    PerformActionUnit04();
                    break;
                case ActionUnit.ActionUnit05:
                    PerformActionUnit05();
                    break;
                case ActionUnit.ActionUnit06:
                    PerformActionUnit06();
                    break;
                case ActionUnit.ActionUnit07:
                    PerformActionUnit07();
                    break;
                case ActionUnit.ActionUnit08:
                    PerformActionUnit08();
                    break;
                case ActionUnit.ActionUnit09:
                    PerformActionUnit09();
                    break;
                case ActionUnit.ActionUnit10:
                    PerformActionUnit10();
                    break;
                case ActionUnit.ActionUnit11:
                    PerformActionUnit11();
                    break;
                case ActionUnit.ActionUnit12:
                    PerformActionUnit12();
                    break;
                case ActionUnit.ActionUnit13:
                    PerformActionUnit13();
                    break;
                case ActionUnit.ActionUnit14:
                    PerformActionUnit14();
                    break;
                case ActionUnit.ActionUnit15:
                    PerformActionUnit15();
                    break;
                case ActionUnit.ActionUnit16:
                    PerformActionUnit16();
                    break;
                case ActionUnit.ActionUnit17:
                    PerformActionUnit17();
                    break;
                case ActionUnit.ActionUnit18:
                    PerformActionUnit18();
                    break;
                case ActionUnit.ActionUnit19:
                    PerformActionUnit19();
                    break;
                case ActionUnit.ActionUnit20:
                    PerformActionUnit20();
                    break;
                case ActionUnit.ActionUnit21:
                    PerformActionUnit21();
                    break;
                case ActionUnit.ActionUnit22:
                    PerformActionUnit22();
                    break;
                case ActionUnit.ActionUnit23:
                    PerformActionUnit23();
                    break;
                case ActionUnit.ActionUnit24:
                    PerformActionUnit24();
                    break;
                case ActionUnit.ActionUnit25:
                    PerformActionUnit25();
                    break;
                case ActionUnit.ActionUnit26:
                    PerformActionUnit26();
                    break;
                case ActionUnit.ActionUnit27:
                    PerformActionUnit27();
                    break;
                case ActionUnit.ActionUnit28:
                    PerformActionUnit28();
                    break;
                case ActionUnit.ActionUnit29:
                    PerformActionUnit29();
                    break;
                case ActionUnit.ActionUnit30:
                    PerformActionUnit30();
                    break;
                case ActionUnit.ActionUnit31:
                    PerformActionUnit31();
                    break;
                case ActionUnit.ActionUnit32:
                    PerformActionUnit32();
                    break;
                case ActionUnit.ActionUnit33:
                    PerformActionUnit33();
                    break;
                case ActionUnit.ActionUnit34:
                    PerformActionUnit34();
                    break;
                case ActionUnit.ActionUnit35:
                    PerformActionUnit35();
                    break;
                case ActionUnit.ActionUnit36:
                    PerformActionUnit36();
                    break;
                case ActionUnit.ActionUnit37:
                    PerformActionUnit37();
                    break;
                case ActionUnit.ActionUnit38:
                    PerformActionUnit38();
                    break;
                case ActionUnit.ActionUnit39:
                    PerformActionUnit39();
                    break;
                case ActionUnit.ActionUnit43:
                    PerformActionUnit43();
                    break;
                case ActionUnit.ActionUnit45:
                    PerformActionUnit45();
                    break;
                case ActionUnit.ActionUnit46:
                    PerformActionUnit46();
                    break;
            }
        }

        /// <summary>
        /// Performs ActionUnit01.
        /// </summary>
        private void PerformActionUnit01()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseInnerL), AU_01);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseInnerR), AU_01);

            Brow_Raise_Inner_L_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowRaiseInnerL));
            Brow_Raise_Inner_R_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowRaiseInnerR));
        }

        /// <summary>
        /// Performs ActionUnit02.
        /// </summary>
        private void PerformActionUnit02()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterR),
                ConvertNumberMaintainingRange(0f, 95f, AU_02));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterL),
                ConvertNumberMaintainingRange(0f, 95f, AU_02));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowCompressL), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowCompressR), NoValue);

            Brow_Raise_Outer_R_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterR));
            Brow_Raise_Outer_L_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterL));
        }

        private void PerformActionUnit03()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseInnerL), AU_03);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseInnerR), AU_03);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterL), AU_03);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterR), AU_03);

            Brow_Raise_Inner_L_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowRaiseInnerL));
            Brow_Raise_Inner_R_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowRaiseInnerR));
            Brow_Raise_Outer_L_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterL));
            Brow_Raise_Outer_R_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterR));
        }

        private void PerformActionUnit04()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowDropL),
                ConvertNumberMaintainingRange(0f, 20f, AU_04));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowDropR),
                ConvertNumberMaintainingRange(0f, 20f, AU_04));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowCompressL), AU_04);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowCompressR), AU_04);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseInnerL), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseInnerR), NoValue);

            Brow_Drop_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowDropL));
            Brow_Drop_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowDropR));

            Brow_Compress_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowCompressL));
            Brow_Compress_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowCompressR));
        }

        private void PerformActionUnit05()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeWideL), AU_05);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeWideR), AU_05);

            Eye_Wide_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeWideL));
            Eye_Wide_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeWideR));
        }

        private void PerformActionUnit06()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(CheekRaiseL),
                ConvertNumberMaintainingRange(0f, 70f, AU_06));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(CheekRaiseR),
                ConvertNumberMaintainingRange(0f, 70f, AU_06));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthSmileL),
                ConvertNumberMaintainingRange(0f, 65f, AU_06));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthSmileR),
                ConvertNumberMaintainingRange(0f, 65f, AU_06));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterL),
                ConvertNumberMaintainingRange(0f, 35f, AU_06));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterR),
                ConvertNumberMaintainingRange(0f, 35f, AU_06));

            Cheek_Raise_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(CheekRaiseL));
            Cheek_Raise_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(CheekRaiseR));

            Mouth_Smile_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthSmileL));
            Mouth_Smile_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthSmileR));

            Brow_Raise_Outer_L_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterL));
            Brow_Raise_Outer_R_Value =
                SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterR));
        }

        private void PerformActionUnit07()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeSquintL), AU_07);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeSquintR), AU_07);

            Eye_Squint_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeSquintL));
            Eye_Squint_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeSquintR));
        }

        private void PerformActionUnit08()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthClose), ConvertNumberMaintainingRange(0f, 10f, AU_08));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(Mouth_Contract), ConvertNumberMaintainingRange(0f, 40f, AU_08));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(JawOpen), ConvertNumberMaintainingRange(0f, 10f, AU_08));

            Mouth_Close_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthClose));
            Mouth_Contract_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(Mouth_Contract));
            Jaw_Open_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(JawOpen));
        }

        private void PerformActionUnit09()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(NoseSneerL), AU_09);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(NoseSneerR), AU_09);

            Nose_Sneer_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(NoseSneerL));
            Nose_Sneer_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(NoseSneerR));
        }

        private void PerformActionUnit10()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthUpUpperL), AU_10);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthUpUpperR), AU_10);

            Mouth_Up_Upper_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthUpUpperL));
            Mouth_Up_Upper_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthUpUpperR));
        }

        private void PerformActionUnit11()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(NoseCreaseL), AU_11);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(NoseCreaseR), AU_11);

            Nose_Crease_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(NoseCreaseL));
            Nose_Crease_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(NoseCreaseR));
        }

        private void PerformActionUnit12()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthSmileL), AU_12);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthSmileR), AU_12);

            Mouth_Smile_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthSmileL));
            Mouth_Smile_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthSmileR));
        }

        private void PerformActionUnit13()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthSmileSharpL), AU_13);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthSmileSharpR), AU_13);

            Mouth_Smile_Sharp_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthSmileSharpL));
            Mouth_Smile_Sharp_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthSmileSharpR));
        }

        private void PerformActionUnit14()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthDimpleL), AU_14);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthDimpleR), AU_14);

            Mouth_Dimple_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthDimpleL));
            Mouth_Dimple_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthDimpleR));
        }

        private void PerformActionUnit15()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthFrownL), AU_15);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthFrownR), AU_15);

            Mouth_Frown_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthFrownL));
            Mouth_Frown_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthFrownR));
        }

        private void PerformActionUnit16()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthDownLowerL), AU_16);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthDownLowerR), AU_16);

            Mouth_Down_Lower_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthDownLowerL));
            Mouth_Down_Lower_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthDownLowerR));
        }

        private void PerformActionUnit17()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthChinUp), AU_17);

            Mouth_Chin_Up_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthChinUp));
        }

        private void PerformActionUnit18()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthPuckerUpL), AU_18);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthPuckerUpR), AU_18);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthPuckerDownL), AU_18);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthPuckerDownR), AU_18);

            Mouth_Pucker_Up_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthPuckerUpL));
            Mouth_Pucker_Up_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthPuckerUpR));
            Mouth_Pucker_Down_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthPuckerDownL));
            Mouth_Pucker_Down_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthPuckerDownR));
        }


        // TODO: Tongue Blendshapes non existent but under CC_Base_Tongue
        private void PerformActionUnit19()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthDownLowerL), ConvertNumberMaintainingRange(0f, 20f, AU_19));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthDownLowerR), ConvertNumberMaintainingRange(0f, 20f, AU_19));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(TongueOut), ConvertNumberMaintainingRange(0f, 60f, AU_19));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(TongueUp), ConvertNumberMaintainingRange(0f, 40f, AU_19));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(TongueTipDown), ConvertNumberMaintainingRange(0f, 20f, AU_19));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(TongueNarrow), ConvertNumberMaintainingRange(0f, 20f, AU_19));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(JawOpen), ConvertNumberMaintainingRange(0f, 30f, AU_19));

            Mouth_Down_Lower_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthDownLowerL));
            Mouth_Down_Lower_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthDownLowerR));
            Tongue_Out_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(TongueOut));
            Tongue_Up_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(TongueUp));
            Tongue_Tip_Down_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(TongueTipDown));
            Tongue_Narrow_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(TongueNarrow));
            Jaw_Open_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(JawOpen));
        }

        private void PerformActionUnit20()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthStretchL), AU_20);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthStretchR), AU_20);

            Mouth_Stretch_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthStretchL));
            Mouth_Stretch_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthStretchR));
        }

        private void PerformActionUnit21()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(NeckTightenL), AU_21);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(NeckTightenR), AU_21);

            Neck_Tighten_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(NeckTightenL));
            Neck_Tighten_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(NeckTightenR));
        }

        private void PerformActionUnit22()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthFunnelUpL), AU_22);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthFunnelUpR), AU_22);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthFunnelDownL), AU_22);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthFunnelDownR), AU_22);

            Mouth_Funnel_Up_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthFunnelUpL));
            Mouth_Funnel_Up_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthFunnelUpR));
            Mouth_Funnel_Down_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthFunnelDownL));
            Mouth_Funnel_Down_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthFunnelDownR));
        }

        private void PerformActionUnit23()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthTightenL), AU_23);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthTightenR), AU_23);

            Mouth_Tighten_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthTightenL));
            Mouth_Tighten_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthTightenR));
        }

        private void PerformActionUnit24()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthPressL), AU_24);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthPressR), AU_24);

            Mouth_Press_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthPressL));
            Mouth_Press_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthPressR));
        }

        private void PerformActionUnit25()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthShrugUpper), ConvertNumberMaintainingRange(0f, 60f, AU_25));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthShrugLower), ConvertNumberMaintainingRange(0f, 60f, AU_25));

            Mouth_Shrug_Upper_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthShrugUpper));
            Mouth_Shrug_Lower_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthShrugLower));
        }

        private void PerformActionUnit26()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(JawDown), AU_26);

            Jaw_Down_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(JawDown));
        }

        private void PerformActionUnit27()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(JawOpen), AU_27);

            Jaw_Open_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(JawOpen));
        }

        private void PerformActionUnit28()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthRollInUpperL), AU_28);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthRollInUpperR), AU_28);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthRollInLowerL), AU_28);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthRollInLowerR), AU_28);

            Mouth_Roll_In_Upper_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthRollInUpperL));
            Mouth_Roll_In_Upper_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthRollInUpperR));
            Mouth_Roll_In_Lower_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthRollInLowerL));
            Mouth_Roll_In_Lower_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthRollInLowerR));
        }

        private void PerformActionUnit29()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(JawForward), AU_29);

            Jaw_Forward_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(JawForward));
        }

        private void PerformActionUnit30()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(JawL), AU_30);

            Jaw_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(JawL));
        }

        private void PerformActionUnit31()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(JawUp), AU_31);

            Jaw_Up_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(JawUp));
        }

        // TODO Abhängigkeiten
        private void PerformActionUnit32()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthRollInLowerL), ConvertNumberMaintainingRange(0f, 50f, AU_32));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthRollInLowerR), ConvertNumberMaintainingRange(0f, 50f, AU_32));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthShrugUpper), ConvertNumberMaintainingRange(0f, 68f, AU_32));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthShrugLower), ConvertNumberMaintainingRange(0f, 20f, AU_32));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(JawOpen), ConvertNumberMaintainingRange(0f, 5f, AU_32));

            Mouth_Roll_In_Lower_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthRollInLowerL));
            Mouth_Roll_In_Lower_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthRollInLowerR));
            Mouth_Shrug_Upper_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthShrugUpper));
            Mouth_Shrug_Lower_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthShrugLower));
            Jaw_Open_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(JawOpen));
        }

        private void PerformActionUnit33()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthBlowL), AU_33);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthBlowR), AU_33);

            Mouth_Blow_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthBlowL));
            Mouth_Blow_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthBlowR));
        }

        private void PerformActionUnit34()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(CheekPuffL), AU_34);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(CheekPuffR), AU_34);

            Cheek_Puff_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(CheekPuffL));
            Cheek_Puff_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(CheekPuffR));
        }

        private void PerformActionUnit35()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(CheekSuckL), AU_35);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(CheekSuckR), AU_35);

            Cheek_Suck_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(CheekSuckL));
            Cheek_Suck_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(CheekSuckR));
        }

        private void PerformActionUnit36()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(TongueBulgeL), AU_36);

            Tongue_Bulge_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(TongueBulgeL));
        }

        private void PerformActionUnit37()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthUpUpperR), ConvertNumberMaintainingRange(0f, 20f, AU_37));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthUpLowerR), ConvertNumberMaintainingRange(0f, 44f, AU_37));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(TongueOut), ConvertNumberMaintainingRange(0f, 30f, AU_37));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(TongueUp), ConvertNumberMaintainingRange(0f, 20f, AU_37));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(TongueWide), ConvertNumberMaintainingRange(0f, 30f, AU_37));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(TongueR), ConvertNumberMaintainingRange(0f, 56f, AU_37));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(JawOpen), ConvertNumberMaintainingRange(0f, 5f, AU_37));

            Mouth_Up_Upper_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthUpUpperR));
            Mouth_Up_Lower_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthUpLowerR));
            Tongue_Out_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(TongueOut));
            Tongue_Up_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(TongueUp));
            Tongue_Wide_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(TongueWide));
            Tongue_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(TongueR));
            Jaw_Open_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(JawOpen));
        }

        private void PerformActionUnit38()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(NoseNostrilDilateL), AU_38);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(NoseNostrilDilateR), AU_38);

            Nose_Nostril_Dilate_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(NoseNostrilDilateL));
            Nose_Nostril_Dilate_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(NoseNostrilDilateR));
        }

        private void PerformActionUnit39()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(NoseNostrilInL), AU_39);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(NoseNostrilInR), AU_39);

            Nose_Nostril_In_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(NoseNostrilInL));
            Nose_Nostril_In_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(NoseNostrilInR));
        }

        // TODO Negativ values in CC4 - how to present in unity??
        private void PerformActionUnit43()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterL), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterR), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeBlinkL), AU_43);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeBlinkR), AU_43);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeSquintL), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeSquintR), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeWideL), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeWideR), NoValue);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthSmileL), ConvertNumberMaintainingRange(0f, 3f, AU_43));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(MouthSmileR), ConvertNumberMaintainingRange(0f, 3f, AU_43));

            Brow_Raise_Outer_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterL));
            Brow_Raise_Outer_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(BrowRaiseOuterR));
            Eye_Blink_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeBlinkL));
            Eye_Blink_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeBlinkR));
            Eye_Squint_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeSquintL));
            Eye_Squint_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeSquintR));
            Eye_Wide_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeWideL));
            Eye_Wide_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeWideR));
            Mouth_Smile_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthSmileL));
            Mouth_Smile_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(MouthSmileR));
        }

        private void PerformActionUnit45()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeBlinkL), AU_45);
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeBlinkR), AU_45);

            Eye_Blink_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeBlinkL));
            Eye_Blink_R_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeBlinkR));
        }

        private void PerformActionUnit46()
        {
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeBlinkL), ConvertNumberMaintainingRange(0f, 90f, AU_46));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(EyeSquintL), ConvertNumberMaintainingRange(0f, 50f, AU_46));
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeByString(CheekRaiseL), ConvertNumberMaintainingRange(0f, 30f, AU_46));

            Eye_Blink_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeBlinkL));
            Eye_Squint_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(EyeSquintL));
            Cheek_Raise_L_Value = SkinnedMeshRenderer.GetBlendShapeWeight(BlendShapeByString(CheekRaiseL));
        }

        /// <summary>
        /// Searches for a blendshape by name and returns the index of it.
        /// </summary>
        /// <param name="blendShapeName"></param>
        /// <returns>The index of Blendshape</returns>
        private int BlendShapeByString(string blendShapeName)
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

        /// <summary>
        /// This function converts the range of one value to another while maintaining the ratio.
        /// For this script, we assume that the old range always ranged from 0 to 100.
        /// </summary>
        /// <param name="newMin">New minimum.</param>
        /// <param name="newMax">New maximum.</param>
        /// <param name="value">Value of the number to be converted into a new range.</param>
        /// <returns></returns>
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