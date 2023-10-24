using DlibFaceLandmarkDetector;
using FaceMaskExample;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils.Helper;
using SEE.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Rect = UnityEngine.Rect;

namespace SEE.Tools.FaceCam
{
    public class WebCam //: MonoBehaviour
    {
        /// <summary>
        /// A frame of the webcam video as texture.
        /// </summary>
        private Texture2D texture;

        /// <summary>
        /// Texture2D of the cropped webcam frame, containing the face.
        /// </summary>
        private Texture2D croppedTexture;

        /// <summary>
        /// X position of the cropped texture.
        /// </summary>
        private int croppedTextureX;

        /// <summary>
        /// Y position of the cropped texture.
        /// </summary>
        private int croppedTextureY;

        /// <summary>
        /// Width of the cropped texture.
        /// </summary>
        private int croppedTextureWidth = 480; // 480 is a reasonable size to
                                               // display the 'webcam not found' image.

        /// <summary>
        /// Height of the cropped texture.
        /// </summary>
        private int croppedTextureHeight = 480; // 480 is a reasonable size to display
                                                // the 'webcam not found' image.

        /// <summary>
        /// X position of the cropped texture of the last frame.
        /// </summary>
        private int lastFrameCutoutTextureX;

        /// <summary>
        /// Y position of the cropped texture of the last frame.
        /// </summary>
        private int lastFrameCutoutTextureY;

        /// <summary>
        /// Width of the cropped texture of the last frame.
        /// </summary>
        private int lastFrameCutoutTextureWidth;

        /// <summary>
        /// Height of the cropped texture of the last frame.
        /// </summary>
        private int lastFrameCutoutTextureHeight;

        /// <summary>
        /// The webcam texture to mat helper from the WebCamTextureToMatHelperExample.
        /// </summary>
        public WebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The face landmark detector from the WebCamTextureToMatHelperExample.
        /// </summary>
        public FaceLandmarkDetector faceLandmarkDetector;

        /// <summary>
        /// The dlib shape predictor file name from the WebCamTextureToMatHelperExample.
        /// </summary>
        private string dlibShapePredictorFileName = "DlibFaceLandmarkDetector/sp_human_face_6.dat";

        /// <summary>
        /// The dlib shape predictor complete file path from the WebCamTextureToMatHelperExample.
        /// </summary>
        private string dlibShapePredictorFilePath;

        /// <summary>
        /// The speed which the face tracking will use to follow the face if it detects one.
        /// </summary>
        [SerializeField, FormerlySerializedAs("Face tracking speed"),
            Tooltip("Set the speed which the face tracking will use to follow the face if it detects one.")]
        private float moveStartSpeed;
        public float MoveStartSpeed
        {
            get => moveStartSpeed;
            set => moveStartSpeed = Mathf.Abs(value);
        }

        /// <summary>
        /// The acceleration which occurs after the face tracking found a face.
        /// </summary>
        [SerializeField, FormerlySerializedAs("Face tracking acceleration"),
            Tooltip("Set the acceleration which occurs after the face tracking found a face.")]
        private float moveAcceleration;
        public float MoveAcceleration
        {
            get => moveAcceleration;
            set => moveAcceleration = Mathf.Abs(value);
        }

        /// <summary>
        /// The speed which the face tracking will use to follow the face.
        /// </summary>
        public float faceTrackingSpeed;

        /// <summary>
        /// An interpolation factor, determining how close our position (cropped texture)
        /// is to the detected face.
        /// If it is 0 it is just our position on the webcam frame.
        /// If it is 1 our position is exactly the same as the detected face.
        /// </summary>
        private float interpolationFactor;

        /// <summary>
        /// The startup Code from the WebCamTextureToMatHelperExample.
        /// </summary>
        public void StartupCodeFromWebCamTextureToMatHelperExample()
        {
            dlibShapePredictorFileName = DlibFaceLandmarkDetectorExample.DlibFaceLandmarkDetectorExample.dlibShapePredictorFileName;
#if UNITY_WEBGL
            getFilePath_Coroutine = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePathAsync(dlibShapePredictorFileName, (result) =>
            {
                getFilePath_Coroutine = null;

                dlibShapePredictorFilePath = result;
                Run();
            });
            StartCoroutine(getFilePath_Coroutine);
#else
            dlibShapePredictorFilePath = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePath(dlibShapePredictorFileName);
            Run();
#endif

            /// <summary>
            /// The 'run' code from the WebCamTextureToMatHelperExample.
            /// </summary>
            void Run()
            {
                if (string.IsNullOrEmpty(dlibShapePredictorFilePath))
                {
                    throw new InvalidOperationException
                        ("shape predictor file does not exist. Please copy from “DlibFaceLandmarkDetector/StreamingAssets/DlibFaceLandmarkDetector/” to “Assets/StreamingAssets/DlibFaceLandmarkDetector/” folder. ");
                }

                faceLandmarkDetector = new FaceLandmarkDetector(dlibShapePredictorFilePath);

                webCamTextureToMatHelper.Initialize();
            }
        }

        #region WebCamTextureToMatHelper event handlers

        /// <summary>
        /// Initializes <see cref="texture"/>.
        /// </summary>
        /// <remarks>Code from the WebCamTextureToMatHelperExample<./remarks>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
            OpenCVForUnity.UnityUtils.Utils.fastMatToTexture2D(webCamTextureMat, texture);
        }

        /// <summary>
        /// Code from the WebCamTextureToMatHelperExample.
        /// Disposes <see cref="texture"/> and <see cref="croppedTexture"/>.
        /// </summary>
        /// <remarks>Code from the WebCamTextureToMatHelperExample.</remarks>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            if (texture != null)
            {
                Destroyer.Destroy(texture);
                texture = null;
            }
            if (croppedTexture != null)
            {
                Destroyer.Destroy(croppedTexture);
                croppedTexture = null;
            }
        }

        /// <summary>
        /// Logs the given <paramref name="errorCode"/> as an error.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        /// <remarks>Code from the WebCamTextureToMatHelperExample.</remarks>
        public static void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.LogError($"OnWebCamTextureToMatHelperErrorOccurred {errorCode}\n");
        }

        #endregion

        /// <summary>
        /// This is the final maximum height of the FaceCam.
        /// </summary>
        private const float maxHeight = 0.24f;

        /// <summary>
        /// Extracts the face from the web cam. The resulting <paramref name="croppedTextureOut"/>
        /// contains only the video frame of the web cam that contains the face of the player
        /// sitting in front of it (that is, the video frame is cropped to the face).
        /// </summary>
        /// <param name="croppedTextureOut">The resulting face texture; may be <c>null</c></param>
        /// <param name="localScale">the size of the texture reduced such that it fits into the
        /// face cam's video tile; may be null</param>
        public void GetFace(out Texture2D croppedTextureOut, out Vector3? localScale)
        {
            localScale = null;
            croppedTextureOut = null;

            // Code from the WebCamTextureToMatHelperExample.
            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                // Code from the WebCamTextureToMatHelperExample.
                Mat rgbaMat = webCamTextureToMatHelper.GetMat();

                // Code from the WebCamTextureToMatHelperExample.
                OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);

                // Code from the WebCamTextureToMatHelperExample.
                // Detect all face rectangles
                List<Rect> detectResult = faceLandmarkDetector.Detect();

                // This is the rectangle which is selected to be the face we want to zoom in.
                Rect mainRect = new(0, 0, 0, 0);

                // bool, true if there is there any rectangle found.
                bool rectFound = false;

                // Find the biggest, resp. closest Face
                foreach (Rect rect in detectResult)
                {
                    if (mainRect.height * mainRect.width <= rect.height * rect.width)
                    {
                        mainRect = rect;
                        rectFound = true;
                    }
                }

                // Code from the WebCamTextureToMatHelperExample.
                // Convert the material to a 2D texture.
                OpenCVForUnity.UnityUtils.Utils.fastMatToTexture2D(rgbaMat, texture);

                // If a face is found, calculate the area of the texture which should be displayed.
                // This should be the face with a little bit extra space.
                if (rectFound)
                {
                    // Dimensions of the new found face.
                    int nextRectX = Mathf.RoundToInt(mainRect.x);
                    int nextRectY = Mathf.RoundToInt(mainRect.y);
                    int nextRectWidth = Mathf.RoundToInt(mainRect.width);
                    int nextRectHeight = Mathf.RoundToInt(mainRect.height);

                    // calculate the space over and under the detected head to make it fully visible.
                    int spaceAbove = nextRectHeight / 2;
                    int spaceBelow = nextRectHeight / 6;

                    // Add the Space above and below to the dimension of the cropped texture.
                    int nextCutoutTextureX = nextRectX;
                    // Because texture and rect do not both use y the same way, it needs to be converted.
                    int nextCutoutTextureY = Math.Max(0, texture.height - nextRectY - nextRectHeight - spaceBelow);
                    int nextCutoutTextureWidth = nextRectWidth;
                    int nextCutoutTextureHeight = nextRectHeight + spaceAbove + spaceBelow;

                    // If the new texture is outside of the original webcam texture, remove the extra space.
                    if (nextCutoutTextureY + nextCutoutTextureHeight > texture.height)
                    {
                        nextCutoutTextureHeight = texture.height - nextCutoutTextureY;
                    }
                    if (nextCutoutTextureX + nextCutoutTextureWidth > texture.width)
                    {
                        nextCutoutTextureWidth = texture.width - nextCutoutTextureX;
                    }

                    // This is the distance which will be ignored, if a face moves.
                    int rectMoveOffset = nextRectWidth / 11;
                    // This is the distance which means the face is at a completely new position.
                    int rectPositionOffset = nextRectWidth;

                    // Reset the interpolation factor if the cropped texture already is at the face,
                    // or otherwise if the face moves a significant amount.
                    if (// Reset the interpolation factor if the rectangle of the face is already at the cropped texture.
                        Math.Abs(nextCutoutTextureX - croppedTextureX) <= rectMoveOffset &&
                        Math.Abs(nextCutoutTextureY - croppedTextureY) <= rectMoveOffset &&
                        Math.Abs(nextCutoutTextureWidth - croppedTextureWidth) <= rectMoveOffset &&
                        Math.Abs(nextCutoutTextureHeight - croppedTextureHeight) <= rectMoveOffset ||
                        // Or reset the interpolation factor if the rectangle of the face gets a new position.
                        Math.Abs(nextCutoutTextureX - lastFrameCutoutTextureX) > rectPositionOffset ||
                        Math.Abs(nextCutoutTextureY - lastFrameCutoutTextureY) > rectPositionOffset ||
                        Math.Abs(nextCutoutTextureWidth - lastFrameCutoutTextureWidth) > rectPositionOffset ||
                        Math.Abs(nextCutoutTextureHeight - lastFrameCutoutTextureHeight) > rectPositionOffset)
                    {
                        interpolationFactor = 0;
                    }

                    // Remember the position of the cropped texture of the last frame, resp. the position right now for the next frame.
                    lastFrameCutoutTextureX = nextCutoutTextureX;
                    lastFrameCutoutTextureY = nextCutoutTextureY;
                    lastFrameCutoutTextureWidth = nextCutoutTextureWidth;
                    lastFrameCutoutTextureHeight = nextCutoutTextureHeight;

                    // Calculate the position, if necessary moving towards the new found face with the interpolation factor.
                    croppedTextureX = Mathf.RoundToInt(Mathf.Lerp(croppedTextureX, nextCutoutTextureX, interpolationFactor));
                    croppedTextureY = Mathf.RoundToInt(Mathf.Lerp(croppedTextureY, nextCutoutTextureY, interpolationFactor));
                    croppedTextureWidth = Mathf.RoundToInt(Mathf.Lerp(croppedTextureWidth, nextCutoutTextureWidth, interpolationFactor));
                    croppedTextureHeight = Mathf.RoundToInt(Mathf.Lerp(croppedTextureHeight, nextCutoutTextureHeight, interpolationFactor));

                    // Calculate the distance and size difference from the new cropped texture towards the actual
                    // rectangle of the face. (There will always be some distance, but more if the face is further away)
                    float distancePosition = Vector2.Distance(new Vector2(croppedTextureX, croppedTextureY), mainRect.position);
                    float distanceSize = Vector2.Distance(new Vector2(croppedTextureWidth, croppedTextureHeight), mainRect.size);

                    // Calculate the interpolation factor for the next frame.
                    // If the new rectangle is further away than the actual cropped texture plus half the size of the rectangle,
                    // move faster towards the rectangle.
                    if (distancePosition >= nextRectWidth / 2.0 || distanceSize >= nextRectWidth / 2.0)
                    {
                        faceTrackingSpeed += MoveAcceleration * Time.deltaTime;
                    }
                    // Otherwise reset the acceleration.
                    else
                    {
                        faceTrackingSpeed = MoveStartSpeed;
                    }

                    // Move towards the rectangle of the face.
                    // Resp. update the interpolation factor which might be reset to 0.
                    interpolationFactor += faceTrackingSpeed * Time.deltaTime;

                    // Apply the cutout texture size to the FacCam prefab.
                    // The size is way too big, so it needs to be reduced. A maximum height is used.
                    float divisor = croppedTextureHeight / maxHeight;
                    localScale = new Vector3(croppedTextureWidth / divisor, croppedTextureHeight / divisor, -1);
                }

                // Copy the pixels from the original texture to the cutout texture.
                Color[] pixels = texture.GetPixels(croppedTextureX, croppedTextureY, croppedTextureWidth, croppedTextureHeight);
                croppedTexture = new Texture2D(croppedTextureWidth, croppedTextureHeight);
                croppedTexture.SetPixels(pixels);
                croppedTexture.Apply();
                croppedTextureOut = croppedTexture;
            }
        }

        internal void Initialize()
        {
            // The startup code from the WebCamTextureToMatHelperExample.
            StartupCodeFromWebCamTextureToMatHelperExample();

            // New texture for the cropped texture only displaying the face, resp. the final texture.
            croppedTexture = new Texture2D(0, 0, TextureFormat.RGBA32, false);

            // Set the speed of the face tracking.
            faceTrackingSpeed = MoveStartSpeed;
        }
    }
}