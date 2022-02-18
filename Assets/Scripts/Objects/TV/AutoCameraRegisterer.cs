using UnityEngine;

namespace Television
{
    /// <summary>
    /// This script automatically registers a camera position
    /// as a TV channel on Start.
    /// </summary>
    public class AutoCameraRegisterer : MonoBehaviour
    {
        public CameraPosition CameraPosition { get; private set; }

        public Transform cameraPosition;
        public Transform cameraFocus;
        public string channelName;
        public bool isEnabled;

        protected bool isInited;

        /// <summary>
        /// Start is called by Unity Engine before the first frame update
        /// </summary>
        private void Start()
        {
            if (!isInited)
            {
                // register TV channel
                CameraPosition = TV.AddCameraPosition(cameraPosition, cameraFocus, channelName, isEnabled);
                isInited = true;
                OnEnabled();
            }
        }

        /// <summary>
        /// Is called by Unity Engine when object is disabled
        /// </summary>
        private void OnDisable()
        {
            if (isInited)
            {
                isEnabled = enabled;
                CameraPosition.IsEnabled = isEnabled;
                OnDisabled();
            }
        }

        /// <summary>
        /// Is called by Unity Engine when object is enabled
        /// </summary>
        private void OnEnable()
        {
            if (isInited)
            {
                isEnabled = enabled;
                CameraPosition.IsEnabled = isEnabled;
                OnEnabled();
            }
        }

        protected virtual void OnEnabled()
        {
            // Nothing to do here
        }

        protected virtual void OnDisabled()
        {
            // Nothing to do here
        }
    }
}
