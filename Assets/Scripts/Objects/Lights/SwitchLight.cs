using System.Collections.Generic;
using UnityEngine;

namespace Creation
{
    public class SwitchLight : Lamp
    {
        public GameObject shade;
        public LightSwitcher lightSwitcher;

        private List<Material> matList;

        private void Awake()
        {
            Init();
        }

        protected override void Init()
        {
            base.Init();

            if (null != lightSwitcher)
                lightSwitcher.OnSwitch += Switch;

            if (null != shade)
            {
                Renderer renderer = shade.GetComponent<Renderer>();
                matList = new List<Material>();

                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    if (renderer.materials[i].IsKeywordEnabled("_EMISSION"))
                        matList.Add(renderer.materials[i]);
                }

                if (matList.Count == 0)
                    matList = null;
            }

            SetLight(false);
        }

        public void Switch(bool isOn)
        {
            Switch(isOn, Random.value > 0.25f);
        }

        public void Switch(bool isOn, bool instant = false)
        {
            if (IsOn == isOn)
                return;

            if (!isOn)
            {
                StopFlicker(false);
                return;
            }

            if (instant)
                SetLight(true);
            else
                StartFlicker(SwitchOn);
        }

        private void SwitchOn()
        {
            SetLight(true);
        }

        public override void SetLight(bool isOn)
        {
            base.SetLight(isOn);

            if (null != matList)
            {
                foreach (Material mat in matList)
                {
                    if (IsOn)
                        mat.EnableKeyword("_EMISSION");
                    else
                        mat.DisableKeyword("_EMISSION");
                }
            }
        }
    }
}
