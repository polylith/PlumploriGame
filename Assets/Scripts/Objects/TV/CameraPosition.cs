using UnityEngine;

namespace Television
{
    /// <summary>
    /// This class holds the data for a TV channel.
    /// </summary>
    public class CameraPosition
    {
        /*
         * TODO 
         * enable setup of the used camera, 
         * e.g. physical camera, field of view 
         * and other attributes of a camera.
         * 
         * additionally, a flag should allow 
         * this channel to have audio. 
         * one problem is there can only be 
         * one active audio listener. the audio 
         * from the camera needs to be redirected 
         * to the audio source on the tv instead.
         */

        public delegate void OnChangeEvent();
        public event OnChangeEvent OnChange;

        public Transform Position { get => cameraPosition; }
        public Transform Focus { get => cameraFocus; }
        public string Name { get => name; }
        public bool IsEnabled { get => isEnabled; set => SetEnabled(value); }
        public bool ShowCamera { get => showCamera; }

        private Transform cameraPosition;
        private Transform cameraFocus;
        private string name;
        private bool isEnabled;
        private bool showCamera;

        public CameraPosition(Transform position, Transform focus, string name = null,
            bool isEnabled = true, bool showCamera = false)
        {
            this.cameraPosition = position;
            this.cameraFocus = focus;
            this.name = name;
            this.isEnabled = isEnabled;
            this.showCamera = showCamera;
        }

        private void SetEnabled(bool isEnabled)
        {
            if (this.isEnabled == isEnabled)
                return;

            this.isEnabled = isEnabled;
            OnChange?.Invoke();
        }
    }
}