using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Action;

namespace Television
{
    /// <summary>
    /// <para>
    /// By default, the TV displays one channel with
    /// white noise and a second channel with an
    /// animated texture (CATexture).
    /// </para>
    /// <para>
    /// There is also a statically managed list of
    /// TVs where the TVs register themselves.Cameras
    /// can be dynamically added and removed as channels.
    /// </para>
    /// </summary>
    public class TV : Interactable
    {
        #region static TV
        private static List<TV> tvs = new List<TV>();
        private static List<CameraPosition> positions = InitPositions();
        private static int inputLength = 1;
        private static float zRot;
        
        /// <summary>
        /// Registers a TV in a global list.
        /// </summary>
        /// <param name="tv">TV to register</param>
        private static void SignIn(TV tv)
        {
            if (tvs.Contains(tv))
                return;

            tvs.Add(tv);

            foreach (CameraPosition cameraPosition in positions)
                cameraPosition.OnChange += tv.OnChannelsUpdate;
        }

        /// <summary>
        /// Unregisters a TV in a global list.
        /// </summary>
        /// <param name="tv">TV to unregister</param>
        private static void SignOut(TV tv)
        {
            if (!tvs.Contains(tv))
                return;

            tvs.Remove(tv);

            foreach (CameraPosition cameraPosition in positions)
                cameraPosition.OnChange -= tv.OnChannelsUpdate;
        }

        /// <summary>
        /// Initializes all existing camera positions (tv channels)
        /// </summary>
        /// <returns>list of all camera positions (tv channels)</returns>
        private static List<CameraPosition> InitPositions()
        {
            CameraPosition campos1 = new CameraPosition(null, null);
            campos1.IsEnabled = false;

            List<CameraPosition> positions = new List<CameraPosition>();
            positions.Add(new CameraPosition(null, null));
            positions.Add(campos1);
            return positions;
        }

        /// <summary>
        /// Registers a new camera as another channel for
        /// all TVs in the global list.
        /// </summary>
        /// <param name="trans">Transform to place camera GameObject</param>
        /// <param name="focus">optional Transform for the camera to look at</param>
        /// <param name="name">optional custom name of the channel</param>
        /// <param name="isEnabled">is this channel enabled on register</param>
        /// <returns>data object of this channel</returns>
        public static CameraPosition AddCameraPosition(Transform trans,
            Transform focus = null, string name = null, bool isEnabled = true)
        {
            if (null == trans)
                return null;

            CameraPosition newPosition = new CameraPosition(trans, focus, name, isEnabled);
            positions.Add(newPosition);
            float divisions = Mathf.Max(5, positions.Count + 1);
            zRot = 360f / divisions;
            inputLength = positions.Count.ToString().Length;

            foreach (TV tv in tvs)
                if (tv.gameObject.activeInHierarchy)
                    newPosition.OnChange += tv.OnChannelsUpdate;

            return newPosition;
        }
        #endregion static TV

        #region TV
        public delegate void OnChannelChangeEvent();
        public event OnChannelChangeEvent OnChannelChange;

        public AudioSource audioSource;
        public ProceduralAudio proceduralAudio;
        public GameObject cameraObj;
        private CATex caTexture;

        public Canvas canvas;
        public Text channelDisplay;
        public Text channelInput;
        public Text channelName;
        public Text volumePercentage;
        public Image volumeBar;
        public GameObject screen;
        public GameObject switcher;
        public Material[] materials = new Material[4];
        public bool State { get => state; }
        public Camera tvCam;
        public AudioSource switcherAudioSource;

        public float Volume { get => volume; set => SetVolume(value); }

        private Renderer rend;
        private int channel = 0;
        private bool state = true;
        private float volume = 0.5f;
        private float volumeDamp = 1f;
        private bool isEnabled = true;
        private string tmpInput;
        private int tmpChannel;
        private IEnumerator ieType;
        private IEnumerator ieAudio;
        private bool isInited = false;
        private Texture renderTexture;

        public override List<string> GetAttributes()
        {
            string[] attributes = new string[]
            {
                "IsEnabled",
                "State"
            };

            List<string> list = new List<string>();

            foreach (string attribute in attributes)
                list.Add(attribute);

            return list;
        }

        protected override void RegisterAtoms()
        {
            RegisterAtoms(GetAttributes());
            SetDelegate("IsEnabled", SetEnabled);
            SetDelegate("State", SetState);
        }

        public override void RegisterGoals()
        {
            // TODO
            Formula f = WorldDB.Get(Prefix + "IsEnabled");
            WorldDB.RegisterFormula(new Implication(null, f));

            f = WorldDB.Get(Prefix + "State");
            WorldDB.RegisterFormula(new Implication(f, null));
        }

        public override string GetDescription()
        {
            throw new System.NotImplementedException();
        }

        private void SetEnabled(bool isEnabled)
        {
            if (this.isEnabled == isEnabled)
                return;

            this.isEnabled = isEnabled;

            if (!isEnabled && state)
                SetState(false);            
        }

        private void Start()
        {
            if (isInited)
                return;

            InitInteractableUI(false);

            if (null == materials)
                materials[2] = Instantiate(materials[2]) as Material;

            if (null == rend)
                rend = screen.GetComponent<Renderer>();

            SetAudio(false);
            SetState(false);
            canvas.gameObject.SetActive(false);
            SetChannel(channel);

            if (isEnabled)
                SignIn(this);
            else
                SignOut(this);

            SetVolume(volume);
            SetState(false);
            isInited = true;
        }

        private void OnDestroy()
        {
            SetAudio(false);
            SignOut(this);
        }

        private void OnChannelsUpdate()
        {
            if (!gameObject.activeInHierarchy)
                return;

            int tmpChannel = channel;
            SetChannel(0);
            SetChannel(tmpChannel);
        }

        public override bool Interact(Interactable interactable)
        {
            if (!ActionController.GetInstance().IsCurrentAction(typeof(UseAction)))
                return base.Interact(interactable);

            if (!isEnabled)
                return false;

            SwitchState();
            Fire("State", state);
            return true;
        }

        public void ChangeVolume(int delta)
        {
            SetVolume(volume + delta * 0.1f);
        }

        private void SetVolume(float value)
        {
            volume = Mathf.Clamp(value, 0f, 1f);
            volumeBar.fillAmount = volume;
            volumePercentage.text = Mathf.Round(volume * 100) + " %";
            UpdateActualVolume();
        }

        private void UpdateActualVolume()
        {
            audioSource.volume = volumeDamp * volume * AudioManager.GetInstance().MainVolume;
        }

        public void ChannelUp()
        {
            SetChannel(channel + 1);
        }

        public void ChannelDown()
        {
            SetChannel(channel - 1);
        }

        public void SetAudio(bool isOn)
        {
            isOn &= AudioManager.GetInstance().IsAudioOn;
            audioSource.mute = !isOn;

            if (!isOn)
            {
                StopAudio();
                return;
            }

            proceduralAudio.gameObject.SetActive(isOn);
        }

        private void StopAudio()
        {
            if (null != ieAudio)
                StopCoroutine(ieAudio);

            ieAudio = null;
        }

        public void PlayAudio(bool mode)
        {
            bool isOn = AudioManager.GetInstance().IsAudioOn;
            proceduralAudio.NoiseRatio = mode;
            audioSource.mute = !isOn;
            StopAudio();

            if (!mode)
            {
                proceduralAudio.frequency = 432;
                proceduralAudio.gain = 0.05;
                proceduralAudio.offset = 0f;
                audioSource.pitch = 1f;
            }

            proceduralAudio.enabled = true;
            ieAudio = IEAudio(mode);
            StartCoroutine(ieAudio);
        }

        private IEnumerator IEAudio(bool mode)
        {
            volumeDamp = 0.09f;
            UpdateActualVolume();

            yield return null;

            while (true)
            {
                if (mode)
                {
                    float[] values = new float[] {
                        0f,
                        0f,
                        0f,
                        1f + Random.value * 0.5f,
                        Random.value,
                        0f
                    };

                    Color[] states = caTexture.ColorStates;

                    foreach (Color color in states)
                    {
                        float v = (color.r + color.g + color.b);
                        values[0] += v * 0.333f;
                        values[2] += values[0];
                    }

                    values[0] /= (float)states.Length;
                    values[0] = Mathf.Clamp01(values[0]);
                    values[1] = 1f - values[0];

                    while (values[5] < 1f)
                    {
                        proceduralAudio.noiseRatio = Mathf.Lerp(proceduralAudio.noiseRatio, values[0], Time.deltaTime);
                        proceduralAudio.offset = Mathf.Lerp(proceduralAudio.offset, values[1], Time.deltaTime);
                        proceduralAudio.frequency = Mathf.Lerp((float)proceduralAudio.frequency, values[2], Time.deltaTime);
                        audioSource.pitch = Mathf.Lerp(audioSource.pitch, values[3], Time.deltaTime);
                        proceduralAudio.gain = Mathf.Lerp((float)proceduralAudio.gain, values[4], Time.deltaTime);
                        values[5] += Time.deltaTime;
                        yield return null;
                    }

                    yield return new WaitForSeconds(caTexture.EvolveTime);
                }
                else
                {
                    audioSource.pitch = 0.5f + Random.value * 2.75f;
                    yield return new WaitForSeconds(Random.value * 0.9f + 0.1f);
                }                
            }
        }

        public void SetChannel(int channel)
        {
            Material mat = materials[0];
            SetChannelInput("");

            if (null != tvCam && null != tvCam.targetTexture)
            {
                tvCam.targetTexture.Release();
                tvCam.gameObject.SetActive(false);
            }

            SetAudio(false);
            caTexture?.Stop();

            if (isInited)
            {
                AudioManager.GetInstance().PlaySound(
                    "tv.switch",
                    switcherAudioSource.gameObject,
                    1.5f,
                    switcherAudioSource
                );
            }

            if (!isEnabled || !state)
            {
                SetSwitcher(0);
                SetDisplayText("");
                SetCameraName("");
                ApplyMaterial(mat);
                return;
            }

            channel += positions.Count;
            channel %= positions.Count;
            this.channel = channel;

            SetSwitcher(channel + 1);
            SetChannelInput(-1);
            SetDisplayText("CH " + (channel + 1) + "/" + positions.Count);

            CameraPosition cameraPosition = positions[channel];
            SetCameraName(null != cameraPosition.Name ? cameraPosition.Name + (cameraPosition.IsEnabled ? "" : " (offline)") : "");

            if (null == cameraPosition.Position || !cameraPosition.IsEnabled)
            {
                mat = materials[2];
                bool mode = cameraPosition.IsEnabled;

                if (null == caTexture)
                {
                    caTexture = new CATex(mat, 100, 100);
                }

                if (mode)
                {
                    caTexture.Restart();
                }
                else
                {
                    caTexture.Noise();
                }

                renderTexture = caTexture.GetRenderTexture();
                PlayAudio(mode);
                tvCam.gameObject.SetActive(false);
            }
            else
            {
                volumeDamp = 1f;
                mat = materials[1];
                renderTexture = new RenderTexture((int)tvCam.pixelRect.width, (int)tvCam.pixelRect.height, 16);

                tvCam.targetTexture = (RenderTexture)renderTexture;
                mat.mainTexture = renderTexture;
                mat.SetTexture("_EmissionMap", renderTexture);
                SetCameraPosition();
            }

            ApplyMaterial(mat);

            if (isInited && null != OnChannelChange)
            {
                OnChannelChange.Invoke();
            }
        }

        private void SetSwitcher(int i)
        {
            if (null == switcher)
                return;

            switcher.transform.localRotation = Quaternion.Euler(0f, 0f, i * zRot);
        }

        public void CheckInput()
        {
            int channel = tmpChannel - 1;

            if (channel < 0 || channel >= positions.Count)
                channel = this.channel;

            SetChannel(channel);
        }

        public void SetChannelInput(int i)
        {
            int channel = -1;

            if (i < 0)
            {
                tmpInput = "";
                tmpChannel = 0;
            }
            else
            {
                tmpInput += i.ToString();
                tmpChannel *= 10;
                tmpChannel += i;

                if (tmpInput.Length == inputLength)
                {
                    tmpChannel--;

                    if (tmpChannel >= 0 && tmpChannel < positions.Count)
                        channel = tmpChannel;
                    else
                    {
                        channel = this.channel;
                    }
                }
            }

            string s = tmpInput;

            while (s.Length < inputLength)
                s = "-" + s;

            if (null != ieType)
                StopCoroutine(ieType);

            ieType = IEType(s, channel);
            StartCoroutine(ieType);
        }

        private IEnumerator IEType(string s, int channel = -1)
        {
            yield return new WaitForSeconds(0.5f);

            SetChannelInput(s);

            yield return new WaitForSeconds(0.5f);

            ieType = null;

            if (channel >= 0)
                SetChannel(channel);
        }

        private void SetChannelInput(string s)
        {
            channelInput.text = s;
        }

        private void SetDisplayText(string s)
        {
            channelDisplay.text = s;
        }

        private void SetCameraName(string s)
        {
            if (null == s)
                s = "";

            channelName.text = s;
        }

        private void SetCameraPosition()
        {
            CameraPosition cameraPosition = positions[channel];
            tvCam.transform.SetParent(cameraPosition.Position, false);
            cameraObj.gameObject.SetActive(cameraPosition.ShowCamera);
            Vector3 focus = tvCam.transform.forward;
            int depth = (int)tvCam.depth;

            if (null != cameraPosition.Focus)
            {
                focus = cameraPosition.Focus.position;
                depth = (int)Vector3.Distance(cameraPosition.Position.position, focus);
            }

            tvCam.transform.LookAt(focus);
            tvCam.gameObject.SetActive(true);
        }

        private void ApplyMaterial(Material mat)
        {
            rend.material = mat;
        }

        public Texture GetCurrentRenderTexture()
        {
            return renderTexture;
        }

        public void SwitchState()
        {
            SetState(!state);

            if (state)
            {
                InteractableUI.SetInteractable(this);
                InteractableUI.Show();
            }
            else
            {
                InteractableUI.Hide();
            }

        }

        public void SetState(bool state)
        {
            if (this.state == state)
                return;

            this.state = state;
            canvas.gameObject.SetActive(state);
            SetChannel(channel);
        }

        public override int IsInteractionEnabled()
        {
            if (!ActionController.GetInstance().IsCurrentAction(typeof(UseAction)))
                return base.IsInteractionEnabled();

            return ShouldBeEnabled() ? 1 : -1;
        }

        protected override bool ShouldBeEnabled()
        {
            return isEnabled;
        }

        /*
        private void LateUpdate()
        {
            if (!state)
                return;

            if (Input.GetKeyDown(KeyCode.KeypadPlus))
                ChannelUp();
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                ChannelDown();
            else if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                CheckInput();
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    if (Input.GetKeyDown((KeyCode)(KeyCode.Keypad0 + i)))
                    {
                        SetChannelInput(i);
                    }
                }
            }
        }
        */
        #endregion TV
    }
}
