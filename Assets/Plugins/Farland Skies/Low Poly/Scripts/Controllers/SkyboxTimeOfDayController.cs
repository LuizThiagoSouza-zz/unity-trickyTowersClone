using Borodar.FarlandSkies.Core.Helpers;
using UnityEngine;

namespace Borodar.FarlandSkies.LowPoly
{
    [HelpURL("http://www.borodar.com/stuff/farlandskies/lowpoly/docs/QuickStart_v2.4.3.pdf")]

    [ExecuteInEditMode]
    public class SkyboxTimeOfDayController : MonoBehaviour
    {
        [Tooltip("Current time of day (in percents)")]
        public float TimeOfDay;

        private SkyboxDayNightCycle _dayNightCycle;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Start()
        {
            _dayNightCycle = SkyboxDayNightCycle.Instance;
        }

        protected void Update()
        {
            Debug.Log("Update");
            if (_dayNightCycle != null)
                _dayNightCycle.TimeOfDay = TimeOfDay;
        }
    }
}