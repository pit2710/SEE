using UnityChan;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace CVVTuber.UnityChan
{
    // Unity-Chan! Model >= ver1.2.2
    public class UnityChanCVVTuberExampleMenuItem : MonoBehaviour
    {
        [MenuItem("Tools/CVVTuberExample/Setup UnityChanCVVTuberExample", false, 1)]
        public static void SetUnityChanCVVTuberSettings()
        {
            GameObject unitychan = GameObject.Find("unitychan");
            if (unitychan != null)
            {
                //Undo.RecordObject(unitychan.transform.localEulerAngles, "Change unitychan.transform.localEulerAngles");
                //unitychan.transform.localEulerAngles = new Vector3 (0, 180, 0);

                bool allComplete = true;

                Animator animator = unitychan.GetComponent<Animator>();

                AnimatorController animCon = animator.runtimeAnimatorController as AnimatorController;
                if (animCon != null)
                {
                    Undo.RecordObject(animCon, "Set true to layer.ikPass");
                    var layers = animCon.layers;
                    bool success = false;
                    foreach (var layer in layers)
                    {
                        if (layer.stateMachine.name == "Base Layer")
                        {
                            layer.iKPass = true;
                            success = true;
                        }
                    }
                    EditorUtility.SetDirty(animCon);

                    if (success)
                    {
                        Debug.Log("Set true to layer.ikPass");
                    }
                    else
                    {
                        Debug.LogError("success == false");
                        allComplete = false;
                    }
                }
                else
                {
                    Debug.LogError("animCon == null");
                    allComplete = false;
                }

                IdleChanger idleChanger = unitychan.GetComponent<IdleChanger>();
                if (idleChanger != null)
                {
                    Undo.RecordObject(idleChanger, "Set false to idleChanger.enabled");
                    idleChanger.enabled = false;
                    EditorUtility.SetDirty(idleChanger);

                    Debug.Log("Set false to idleChanger.enabled");
                }
                else
                {
                    Debug.LogError("idleChanger == null");
                    allComplete = false;
                }

                FaceUpdate faceUpdate = unitychan.GetComponent<FaceUpdate>();
                if (faceUpdate != null)
                {
                    Undo.RecordObject(faceUpdate, "Set false to faceUpdate.enabled");
                    faceUpdate.enabled = false;
                    EditorUtility.SetDirty(faceUpdate);

                    Debug.Log("Set false to faceUpdate.enabled");
                }
                else
                {
                    Debug.LogError("faceUpdate == null");
                    allComplete = false;
                }

                MonoBehaviour autoBlink = (MonoBehaviour)unitychan.GetComponent("AutoBlink");
                if (autoBlink != null)
                {
                    Undo.RecordObject(autoBlink, "Set false to autoBlink.enabled");
                    autoBlink.enabled = false;
                    EditorUtility.SetDirty(autoBlink);

                    Debug.Log("Set false to autoBlink.enabled");
                }
                else
                {
                    Debug.LogError("autoBlink == null");
                    allComplete = false;
                }

                HeadLookAtIKController headLookAtIKController = FindObjectOfType<HeadLookAtIKController>();
                if (headLookAtIKController != null)
                {
                    Undo.RecordObject(headLookAtIKController, "Set animator to headLookAtIKController.target");
                    headLookAtIKController.target = animator;

                    var lookAtLoot = GameObject.Find("LookAtRoot").transform;
                    if (lookAtLoot != null)
                    {
                        headLookAtIKController.lookAtRoot = lookAtLoot;
                        var lookAtTarget = lookAtLoot.transform.Find("LookAtTarget").transform;
                        if (lookAtTarget != null)
                        {
                            headLookAtIKController.lookAtTarget = lookAtTarget;
                        }
                    }
                    EditorUtility.SetDirty(headLookAtIKController);

                    if (headLookAtIKController.lookAtRoot != null && headLookAtIKController.lookAtTarget != null)
                    {
                        Debug.Log("Set animator to headLookAtIKController.target");
                    }
                    else
                    {
                        Debug.LogError("headLookAtIKController.lookAtRoot == null || headLookAtIKController.lookAtTarget == null");
                        allComplete = false;
                    }
                }
                else
                {
                    Debug.LogError("headLookAtIKController == null");
                    allComplete = false;
                }

                HeadRotationController headRotationController = FindObjectOfType<HeadRotationController>();
                if (headRotationController != null)
                {
                    Undo.RecordObject(headRotationController, "Set Character1_Head.transform to headRotationController.target");
                    headRotationController.target = unitychan.transform.Find("Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_Neck/Character1_Head").transform;
                    EditorUtility.SetDirty(headRotationController);

                    if (headRotationController.target != null)
                    {
                        Debug.Log("Set Character1_Head.transform to headRotationController.target");
                    }
                    else
                    {
                        Debug.LogError("headRotationController.target == null");
                        allComplete = false;
                    }
                }
                else
                {
                    Debug.LogError("headRotationController == null");
                    allComplete = false;
                }

                UnityChanFaceBlendShapeController faceBlendShapeController = FindObjectOfType<UnityChanFaceBlendShapeController>();
                if (faceBlendShapeController != null)
                {
                    Undo.RecordObject(faceBlendShapeController, "Set SkinnedMeshRenderer to dlibFaceBlendShapeController");
                    faceBlendShapeController.EYE_DEF = unitychan.transform.Find("Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_Neck/Character1_Head/EYE_DEF").GetComponent<SkinnedMeshRenderer>();
                    faceBlendShapeController.EL_DEF = unitychan.transform.Find("Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_Neck/Character1_Head/EYE_DEF/EL_DEF").GetComponent<SkinnedMeshRenderer>();
                    faceBlendShapeController.BLW_DEF = unitychan.transform.Find("Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_Neck/Character1_Head/BLW_DEF").GetComponent<SkinnedMeshRenderer>();
                    faceBlendShapeController.MTH_DEF = unitychan.transform.Find("Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_Neck/Character1_Head/MTH_DEF").GetComponent<SkinnedMeshRenderer>();
                    EditorUtility.SetDirty(faceBlendShapeController);

                    if (faceBlendShapeController.EYE_DEF != null && faceBlendShapeController.EL_DEF != null && faceBlendShapeController.BLW_DEF != null && faceBlendShapeController.MTH_DEF != null)
                    {
                        Debug.Log("Set SkinnedMeshRenderer to dlibFaceBlendShapeController");
                    }
                    else
                    {
                        Debug.LogError("faceBlendShapeController.EYE_DEF == null");
                        allComplete = false;
                    }
                }
                else
                {
                    Debug.LogError("faceBlendShapeController == null");
                    allComplete = false;
                }

                UnityChanFaceAnimationClipController faceAnimationClipController = FindObjectOfType<UnityChanFaceAnimationClipController>();
                if (faceAnimationClipController != null)
                {
                    Undo.RecordObject(faceAnimationClipController, "Set animator to faceAnimationClipController.target");
                    faceAnimationClipController.target = animator;
                    EditorUtility.SetDirty(faceAnimationClipController);

                    if (faceAnimationClipController.target != null)
                    {
                        Debug.Log("Set animator to faceAnimationClipController.target");
                    }
                    else
                    {
                        Debug.LogError("faceAnimationClipController.target == null");
                        allComplete = false;
                    }
                }
                else
                {
                    Debug.LogError("faceAnimationClipController == null");
                    allComplete = false;
                }

                if (allComplete)
                    Debug.Log("UnityChanVTuberExample setup is all complete!");

            }
            else
            {
                Debug.LogError("There is no \"unitychan\" prefab in the scene. Please add \"unitychan\" prefab to the scene.");
            }
        }
    }
}