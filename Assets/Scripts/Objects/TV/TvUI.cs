using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Television
{
    /// <summary>
    /// This is the interactive UI for a TV. It is the
    /// remote control to switch the channel on the TV
    /// and adjust the volume. Also, a larger version
    /// of the TV screen can be displayed and closed.
    /// </summary>
    public class TvUI : InteractableUI
    {
        public UIButton[] numsButtons = new UIButton[10];
        public UIButton minusButton;
        public UIButton plusButton;
        public UIButton volumeMinusButton;
        public UIButton volumePlusButton;
        public UIButton enterButton;
        public UIIconButton toggleScreenButton;

        public RectTransform uiScreen;
        public RawImage renderImage;
        public Text channelDisplay;
        public Text channelInput;
        public Text channelName;
        public Text volumePercentage;
        public Image volumeBar;

        private bool screenState = false;

        protected override void Initialize()
        {
            numsButtons[0].SetAction(Input0);
            numsButtons[1].SetAction(Input1);
            numsButtons[2].SetAction(Input2);
            numsButtons[3].SetAction(Input3);
            numsButtons[4].SetAction(Input4);
            numsButtons[5].SetAction(Input5);
            numsButtons[6].SetAction(Input6);
            numsButtons[7].SetAction(Input7);
            numsButtons[8].SetAction(Input8);
            numsButtons[9].SetAction(Input9);

            minusButton.SetAction(ChannelDown);
            plusButton.SetAction(ChannelUp);

            volumeMinusButton.SetAction(InputVolumeDown);
            volumePlusButton.SetAction(InputVolumeUp);

            enterButton.SetAction(InputEnter);
            toggleScreenButton.SetAction(ToggleScreenState);

            screenState = true;
            SetScreenState(false, true);

            if (interactable is TV tv)
            {
                tv.OnChannelChange += UpdateRenderImage;
            }
        }

        protected override void BeforeHide()
        {
            if (!(interactable is TV tv))
                return;

            tv.OnChannelChange -= UpdateRenderImage;
            tv.SetState(false);
        }

        private void ChannelUp()
        {
            if (!(interactable is TV tv))
                return;

            tv.ChannelUp();
        }

        private void ChannelDown()
        {
            if (!(interactable is TV tv))
                return;

            tv.ChannelDown();
        }

        private void Input0()
        {
            InputNum(0);
        }

        private void Input1()
        {
            InputNum(1);
        }

        private void Input2()
        {
            InputNum(2);
        }

        private void Input3()
        {
            InputNum(3);
        }

        private void Input4()
        {
            InputNum(4);
        }

        private void Input5()
        {
            InputNum(5);
        }

        private void Input6()
        {
            InputNum(6);
        }

        private void Input7()
        {
            InputNum(7);
        }

        private void Input8()
        {
            InputNum(8);
        }

        private void Input9()
        {
            InputNum(9);
        }

        private void InputNum(int num)
        {
            if (!(interactable is TV tv))
                return;

            tv.SetChannelInput(num);
            UpdateChannelInput(tv);
        }

        private void InputVolumeDown()
        {
            InputVolume(-1);
        }

        private void InputVolumeUp()
        {
            InputVolume(1);
        }

        private void InputVolume(int delta)
        {
            if (!(interactable is TV tv))
                return;

            tv.ChangeVolume(delta);
            UpdateVolumeDisplay(tv);
        }

        private void UpdateDisplays(TV tv)
        {
            UpdateChannelDisplay(tv);
            UpdateChannelInput(tv);
            UpdateVolumeDisplay(tv);
        }

        private void UpdateVolumeDisplay(TV tv)
        {
            volumeBar.fillAmount = tv.Volume;
            volumePercentage.text = tv.volumePercentage.text;
        }

        private void UpdateChannelDisplay(TV tv)
        {
            channelDisplay.text = tv.channelDisplay.text;
            channelName.text = tv.channelName.text;
        }

        private void UpdateChannelInput(TV tv)
        {
            channelInput.text = tv.channelInput.text;
        }

        private void InputEnter()
        {
            if (!(interactable is TV tv))
                return;

            tv.CheckInput();
            UpdateChannelInput(tv);
        }

        private void ToggleScreenState()
        {
            SetScreenState(!screenState);
        }

        private void SetScreenState(bool state, bool instant = false)
        {
            if (screenState == state)
                return;

            screenState = state;
            UpdateRenderImage();
            Vector3 scale = state ? Vector3.one : Vector3.zero;
                        
            if (instant)
            {
                uiScreen.localScale = scale;
                return;
            }

            DOTween.Sequence()
                .SetAutoKill(true)
                .Append(uiScreen.DOScale(scale, 0.5f))
                .Play();
        }

        private void UpdateRenderImage()
        {
            if (screenState && interactable is TV tv)
            {
                Texture renderTexture = tv.GetCurrentRenderTexture();
                renderImage.texture = renderTexture;
                renderImage.color = Color.white;
                UpdateDisplays(tv);
            }
            else
            {
                renderImage.texture = null;
                renderImage.color = Color.black;
            }
        }
    }
}
