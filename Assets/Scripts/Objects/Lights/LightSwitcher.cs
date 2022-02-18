using System.Collections.Generic;
using Action;
using UnityEngine;

namespace Creation
{
    /// <summary>
    /// <para>
    /// This class is used to switch lights on and off.
    /// An object of class SwitchLight registers to the
    /// delegate OnSwitch and is controlled by this switch.
    /// </para>
    /// <para>
    /// The GameObject has a mesh that is put into position
    /// as a toggle switch. The material indicates whether
    /// the light is off.
    /// </para>
    /// </summary>
    public class LightSwitcher : Interactable
    {
        public delegate void OnSwitchEvent(bool isOn);
        public event OnSwitchEvent OnSwitch;
        public bool IsOn { get => isOn; }

        public GameObject switchButton;
        public bool isEnabled;

        private bool isOn;
        private Material mat;
        private bool inited;
        private bool switchPosition;

        private void Awake()
        {
            Renderer renderer = GetComponent<Renderer>();

            if (null != renderer && null != renderer.materials && renderer.materials.Length > 1)
                mat = renderer.materials[1];

            SetEnabled(true);
            SetSwitch();
            inited = true;

            if (null == langKey || "".Equals(langKey))
               langKey = Language.LangKey.Switch.ToString();
        }
        
        public override List<string> GetAttributes()
        {
            string[] attributes = new string[]
            {
                "IsEnabled",
                "IsOn"
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
            SetDelegate("IsOn", SwitchedOther);
        }

        public override void RegisterGoals()
        {
            Formula f = WorldDB.Get(Prefix + "IsOn");
            WorldDB.RegisterFormula(new Implication(f, null));
        }

        public override string GetDescription()
        {
            return "";
        }
        
        public override bool Interact(Interactable interactable)
        {
            if (!isEnabled
                || !ActionController.GetInstance().IsCurrentAction(typeof(UseAction)))
                return false;

            Switch();
            Fire("IsOn", IsOn);
            return true;
        }

        private void SetEnabled(bool isEnabled)
        {
            if (this.isEnabled == isEnabled)
                return;

            this.isEnabled = isEnabled;

            if (!isEnabled && isOn)
                SetSwitch(true);
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

        public void SwitchedOther(bool isOn)
        {
            this.isOn = isOn;
            SetSwitch(false);
        }

        private void Switch()
        {
            isOn = !isOn;

            if (inited)
            {
                AudioManager.GetInstance()?.PlaySound("light.switch", gameObject);
            }

            SetSwitch();
            OnSwitch?.Invoke(isOn);
        }

        private void SetSwitch(bool self = true)
        {
            if (self)
            {
                switchButton.transform.localRotation = Quaternion.Euler(switchPosition ? 14f : 0f, 0f, 0f);
                switchPosition = !switchPosition;
            }

            if (null == mat)
                return;

            if (isOn || !isEnabled)
            {
                mat.DisableKeyword("_EMISSION");
                mat.color = Color.black;
                mat.SetColor("_EmissionColor", Color.black);
            }
            else
            {
                mat.EnableKeyword("_EMISSION");
                mat.color = Color.red;
                mat.SetColor("_EmissionColor", Color.red);
            }
        }
    }
}