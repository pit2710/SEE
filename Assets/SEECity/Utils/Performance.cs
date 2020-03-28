﻿using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace SEE
{
    /// <summary>
    /// Allows us to measure and emit the elapsed time for long-running actions.
    /// 
    /// Example use:
    /// 
    ///   Performance p = Performance.Begin("loading graph data");
    ///   ... do something
    ///   p.End();
    /// </summary>
    public class Performance
    {
        private Performance() { }

        private Performance(string action, Stopwatch sw)
        {
            this.action = action;
            this.stopWatch = sw;
        }

        private Stopwatch stopWatch;

        private readonly string action;

        private string elapsedTime = "00:00:00:00";

        /// <summary>
        /// Returns a new performance time stamp and emits given action.
        /// </summary>
        /// <param name="action">name of action started to be printed</param>
        /// <returns></returns>
        public static Performance Begin(string action)
        {
            Stopwatch sw = new Stopwatch();
            Performance result = new Performance(action, sw);
            Debug.LogFormat("Begin of {0}.\n", action);
            sw.Start();
            return result;
        }

        /// <summary>
        /// Emits the elapsed time from the start of the performance time span
        /// until now. Reports it to Debug.Log along with the action name.
        /// </summary>
        public void End()
        {
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            this.elapsedTime = elapsedTime;
            Debug.Log("End of " + action + " (" + elapsedTime + ").\n");
        }

        public string GetElapsedTime()
        {
            return elapsedTime;
        }
    }
}