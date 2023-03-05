// associated documentation files (the "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial
// portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO
// EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
// THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SEE.GO.GameObjectExtensions;

namespace SEE.Game.Avatars
{
    /// <summary>
    /// This component has to be attached to a GameObject which also has a <see cref="FACS"/>
    /// script attached. Taking two double values (representing valence and arousal) as input,
    /// this component determines a basic emotion based on a reference point of the dimensional
    /// model from Toisoul et al. (2021). It provides a list of <see cref="FACS.ActionUnit">
    /// which are characteristic for the determined basic emotion.
    /// </summary>
    public class ValenceArousalSlider : MonoBehaviour
    {
        /// <summary>
        /// Inner class for creating instances of two-dimensional cartesian coordinates.
        /// </summary>
        public class Coordinate
        {
            /// <summary>
            /// The provided double value refers to the value on the x-axis.
            /// </summary>
            public double X;

            /// <summary>
            /// The provided double value refers to the value on the y-axis.
            /// </summary>
            public double Y;


            /// <summary>
            /// Constructor of Coordinate-Class for creating instances.
            /// </summary>
            public Coordinate(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        /// <summary>
        /// Inner class for creating objects containing the AUs based on the emotion which
        /// is determined by the given angle.
        /// </summary>
        public class AUsBasedOnEmotionAngle
        {
            /// <summary>
            /// The provided integer value refers to the angle which a base emotion has.
            /// </summary>
            public int Angle;

            /// <summary>
            /// The provided string value displays the name of the base emotion.
            /// </summary>
            public string Emotion;

            /// <summary>
            /// The provided list contains all of the blendshape names of the AUs.
            /// </summary>
            public IList<string> AUStrings;

            /// <summary>
            /// Constructor of AUsBasedOnEmotionAngle-Class for creating instances.
            /// </summary>
            /// <param name="angle">angle of the emotion in the emotion model</param>
            /// <param name="emotion">name of the base emotion</param>
            /// <param name="aUStrings">blendshape names of the action units (AU)</param>
            public AUsBasedOnEmotionAngle(int angle, string emotion, IList<string> aUStrings)
            {
                Angle = angle;
                AUStrings = aUStrings;
                Emotion = emotion;
            }
        }

        /// <summary>
        /// The FACS used for the animations. Will be retrieved from the GameObject.
        /// </summary>
        private FACS facs;

        /// <summary>
        /// This is a list containing all of the base emotions as
        /// <see cref="ValenceArousalSlider.AUsBasedOnEmotionAngle"/> instances.
        /// </summary>
        public IList<AUsBasedOnEmotionAngle> AUsBasedOnEmotionAngles = new List<AUsBasedOnEmotionAngle>();

        /// <summary>
        /// The provided double value describes the degree of valence in dimensional
        /// emotion theories. The value of this variable is the y-value in cartesian
        /// coordinate systems. It ranges from -1 (-100%) to 1 (100%).
        /// </summary>
        [Range(-1, 1)]
        public double Valence;

        /// <summary>
        /// The provided double value describes the degree of arousal in dimensional emotion theories.
        /// The value of this variable is the x-value in cartesian coordinate systems.
        /// It ranges from -1 (-100%) to 1 (100%).
        /// </summary>
        [Range(-1, 1)]
        public double Arousal;

        /// <summary>
        /// This function is called once in the initializing phase.
        /// It defines the base emotions and retrieves the <see cref="FACS"/>
        /// component attached to this GameObject.
        /// </summary>
        void Start()
        {
            AUsBasedOnEmotionAngles.Add(new AUsBasedOnEmotionAngle(13, "Happy", new List<string>
        {
            "au6","au12","au25"
        }));

            AUsBasedOnEmotionAngles.Add(new AUsBasedOnEmotionAngle(70, "Surprised", new List<string>
        {
            "au1","au2","au5","au25" //,"au27"
        }));

            AUsBasedOnEmotionAngles.Add(new AUsBasedOnEmotionAngle(98, "Fear", new List<string>
        {
            "au1","au4","au5","au20","au25","au26"
        }));

            AUsBasedOnEmotionAngles.Add(new AUsBasedOnEmotionAngle(120, "Angry", new List<string>
        {
            "au4","au7","au17","au23","au24","au38"
        }));

            AUsBasedOnEmotionAngles.Add(new AUsBasedOnEmotionAngle(133, "Contempt", new List<string>
        {
            "au14l"
        }));

            AUsBasedOnEmotionAngles.Add(new AUsBasedOnEmotionAngle(154, "Disgust", new List<string>
        {
            "au4","au7","au9","au17"
        }));

            AUsBasedOnEmotionAngles.Add(new AUsBasedOnEmotionAngle(206, "Sad", new List<string>
        {
            "au1","au2","au4","au15","au17"
        }));

            gameObject.TryGetComponentOrLog(out facs);
        }

        /// <summary>
        /// This function returns the radians from a coordinate inside of a
        /// cartesian coordinate system using the <see cref="Math.Atan2(double, double)"/>
        /// function.
        /// </summary>
        /// <param name="x">x co-ordinate</param>
        /// <param name="y">y co-ordinate</param>
        private double GetRadiansFromCoordinates(double x, double y)
        {
            return Math.Atan2(y, x);
        }

        /// <summary>
        /// This function returns the the calculated degree based on a radian value provided.
        /// If y < 0 the result will be counted negatively from 360. In this case, 360 will
        /// be added to the value in order to retrieve a positive degree value from 1 - 360.
        /// </summary>
        private double GetAngleFromRadians(double radians)
        {
            if (radians * (180 / Math.PI) > 0)
            {
                return radians * (180 / Math.PI);
            }
            else
            {
                return 360 + (radians * (180 / Math.PI));
            }
        }

        /// <summary>
        /// This function returns the most distant coordinate, which has the same angle
        /// as the given coordinate using the second intercept theorem.
        /// </summary>
        public Coordinate GetMostDistantCoordinate(Coordinate coordinate)
        {
            Coordinate MostDistantCoordinate = coordinate;

            double max = Math.Max(Math.Abs(coordinate.X), Math.Abs(coordinate.Y));
            MostDistantCoordinate.X = coordinate.X / max;
            MostDistantCoordinate.Y = coordinate.Y / max;

            return MostDistantCoordinate;
        }

        /// <summary>
        /// This function returns the euclidian distance between the zero point of a cartesian
        /// coordinate system by a given x-axis and y-axis value.
        /// </summary>
        private double GetEuclidianDistanceFromZeropoint(Coordinate coordinate)
        {
            double zeropoint_x = 0;
            double zeropoint_y = 0;

            return Math.Sqrt((Math.Pow(zeropoint_x - coordinate.X, 2) + Math.Pow(zeropoint_y - coordinate.Y, 2)));
        }

        /// <summary>
        /// This function is called once per frame. It determines all of the necessary variables
        /// based on the arousal and valence value given, such as the determined emotion based
        /// of an angle inside of dimensional emotiontheories and the resulting
        /// <see cref="FACS.ActionUnit"/> as a list, which will be handed over to the
        /// <see cref="FACS"/> controller. The intensity of the AUs results from the distance
        /// of the selected coordinate in relation to the coordinate which is farthest away
        /// with the same angle.
        /// </summary>
        void Update()
        {
            Coordinate selectedCoordinate = new(Valence, Arousal);

            List<FACS.ActionUnit> DeterminedActionUnits = new();

            // If x=0 or y=0, only an empty list has to be handed to the <see cref="FACS"/> controller.
            if (selectedCoordinate.X != 0 && selectedCoordinate.Y != 0)
            {
                double radiansByValenceAndArousal = GetRadiansFromCoordinates(selectedCoordinate.X, selectedCoordinate.Y);
                double angleByRadians = GetAngleFromRadians(radiansByValenceAndArousal);

                double distanceToSelectedCoordinate = GetEuclidianDistanceFromZeropoint(selectedCoordinate);
                double distanceToMostDistantCoordinate = GetEuclidianDistanceFromZeropoint(GetMostDistantCoordinate(selectedCoordinate));

                // Get the index of the closest emotion by given angle
                int indexOfClosest = AUsBasedOnEmotionAngles
                    .Select((v, i) => new { Angle = v.Angle, Index = i })
                    .OrderBy(p => Math.Abs(p.Angle - angleByRadians))
                    .First().Index;

                // Get the instance of AUsBasedOnEmotionAngle by closest index
                AUsBasedOnEmotionAngle closestEmotionByAngle = AUsBasedOnEmotionAngles[indexOfClosest];

                double intensityRangeOneToFive = Math.Round(5 * (distanceToSelectedCoordinate / distanceToMostDistantCoordinate));

                // Assuming A=1 is the lowest possible value for an AU, the value will be set to 1, if it's below.
                if (intensityRangeOneToFive < 1)
                {
                    intensityRangeOneToFive = 1;
                }

                // Create FACS.ActionUnit instances based on provided strings and determined intensity.
                foreach (string au in closestEmotionByAngle.AUStrings)
                {
                    int intensity = Convert.ToInt32(intensityRangeOneToFive);
                    DeterminedActionUnits.Add(new FACS.ActionUnit(au, intensity));
                }

            }

            // Hand over the newly created list to the FACS controller.
            facs.SelectedActions = DeterminedActionUnits;
        }
    }
}