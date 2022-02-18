using System.Collections;
using UnityEngine;

namespace Creation
{
    public class Lamp : MonoBehaviour
    {
        private static Vector4[] colors = new Vector4[] {
            new Vector4(1.00f, 1.00f, 1.00f), // white
            new Vector4(0.125f, 0.823f, 0.957f), // neon blue
            new Vector4(1.000f, 0.027f, 0.227f), // neon red
            new Vector4(0.996f, 0.003f, 0.603f), // neon pink
            new Vector4(0.811f, 1.000f, 0.015f), // neon yellow
            new Vector4(0.305f, 0.992f, 0.329f), // neon green
            new Vector4(0.125f, 0.823f, 0.957f) // neon purple
        };

        public enum NeonColors
        {
            White,
            NeonBlue,
            NeonRed,
            NeonPink,
            NeonYellow,
            NeonGreen,
            NeonPurple
        }

        public bool bFlicker = true;
        public string soundId = "light.flicker";
        public GameObject bulp;
        public Light lighter;
        public NeonColors lightColorType = NeonColors.White;

        public bool IsOn {  get => isOn; } 

        private IEnumerator ieFlicker;
        private float flickerProb = 1f;
        private Material bulpMat;
        private bool hasEmission;
        private bool inited;
        private bool isOn;
        private Vector4 lightColor = new Vector4(1f, 1f, 1f);

        private void Awake()
        {
            Init();
        }

        protected virtual void Init()
        {
            if (inited)
            {
                return;
            }

            if (null != bulp)
            {
                Renderer renderer = bulp.GetComponent<Renderer>();
                bulpMat = renderer.material;
                hasEmission = bulpMat.IsKeywordEnabled("_EMISSION");
                SetLightColor(lightColorType);
            }

            bool b = isOn;
            isOn = !b;
            SetLight(b);
            inited = true;
        }

        public void SetLightColor(NeonColors c)
        {
            Vector4 lightColor = colors[(int)c];
            lighter.color = new Color(lightColor.x, lightColor.y, lightColor.z);
            lightColorType = c;

            if (hasEmission)
                bulpMat.SetColor("_EmissionColor", lightColor * 7.5f);
        }

        public void SetFlickerProbability(float flickerProb)
        {
            this.flickerProb = Mathf.Clamp01(flickerProb);
        }

        public void MaybeFlicker()
        {
            if (!bFlicker)
                return;

            float zz = Random.value;

            if (zz < flickerProb || null != ieFlicker)
                return;

            ieFlicker = IEFlicker();
            StartCoroutine(ieFlicker);
        }

        public void StartFlicker(System.Action action)
        {
            if (null != ieFlicker)
                StopCoroutine(ieFlicker);

            if (!bFlicker)
            {
                action?.Invoke();
                return;
            }

            ieFlicker = null;

            ieFlicker = IEFlicker(action);
            StartCoroutine(ieFlicker);
        }

        public void StopFlicker(bool isOn = true)
        {
            if (null != ieFlicker)
                StopCoroutine(ieFlicker);

            ieFlicker = null;
            SetLight(isOn);
        }

        public virtual void SetLight(bool isOn)
        {
            if (this.isOn == isOn)
                return;

            if (inited && !"".Equals(soundId))
            {
                AudioManager.GetInstance()?.PlaySound(soundId, gameObject, 1f + Random.value * 0.075f);
            }

            this.isOn = isOn;

            if (lighter.gameObject.activeSelf)
                lighter.enabled = isOn;

            if (hasEmission)
            {
                if (isOn)
                    bulpMat.EnableKeyword("_EMISSION");
                else
                    bulpMat.DisableKeyword("_EMISSION");
            }
        }

        private IEnumerator IEFlicker(System.Action action = null)
        {
            int n = Random.Range(5, 10);
            float t = Random.value * 0.5f + (null == action ? Random.Range(0.5f, 0.75f) : 0.5f);
            int i = 0;

            if (null == action)
                yield return new WaitForSeconds(Random.Range(5f, 15f));

            while (i < n)
            {
                yield return new WaitForSeconds(t);

                SetLight(!lighter.enabled);

                t *= Random.Range(0.5f, 0.9f);
                i++;
            }

            yield return new WaitForSeconds(0.25f + Random.value * 0.5f);

            if (null != action)
            {
                action.Invoke();
            }
            else
            {
                SetLight(true);
                ieFlicker = IEFlicker();

                yield return new WaitForSeconds(Random.Range(15f, 30f));

                if (null != ieFlicker)
                    StartCoroutine(ieFlicker);
            }
        }
    }
}