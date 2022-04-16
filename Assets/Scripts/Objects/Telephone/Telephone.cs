using Action;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Telephone : Interactable, ITelephoneDevice
{
    private void Start()
    {
        PhoneDirectory.Init();
        Init();
        // GameEvent.GetInstance().Execute<bool>(SetCallIn, true, 10f);
    }

    private readonly float[][][] audioPositions = new float[][][] {
        new float[][]
        {
            new float[] { 0.00f, 1.00f },
            new float[] { 1.00f, 1.95f },
            new float[] { 1.95f, 2.75f },
            new float[] { 2.75f, 3.90f },
            new float[] { 3.90f, 4.20f },
            new float[] { 4.20f, 4.90f }
        },
        new float[][]
        {
            new float[] { 0.0f, 1.475f },
            new float[] { 1.475f, 2.8f },
            new float[] { 3.3f, 7.71f },
            new float[] { 7.9f, 9.2f },
            new float[] { 9.3f, 10.5f }
        },
        new float[][]
        {
            new float[] { 0.0f, 1.383f },
            new float[] { 1.383f, 2.15f },
            new float[] { 2.15f, 3.181f },
            new float[] { 3.181f, 4.362f },
            new float[] { 4.362f, 5.712f },
            new float[] { 5.712f, 6.709f },
            new float[] { 6.709f, 8f },
            new float[] { 8f, 8.232f },
            new float[] { 8.232f, 8.778f }
        }
    };

    public bool InUse { get => inUse; }
    public string Number { get => number; }
    public bool HasNumber { get => null != number; }

    public Color color = Color.clear;
    public string number;
    public GameObject handset;
    public AudioSource audioSource;
    public AudioDistortionFilter audioFilter;

    private IEnumerator ieVoice;

    private bool isEnabled;
    private bool isCallIn;
    private bool inUse;
    private bool isTalking;
    private bool isRinging;
    private bool isHandsetOff;
    private Sequence seq;
    private string currentNumber;
    private ITelephoneDevice currentConnection;
    private Material[] materials;
    private Vector3 handsetLocalPosition;
    private bool inited;
        
    private void OnDestroy()
    {
        PhoneDirectory.Remove(number);
    }

    private void Init()
    {
        if (inited)
            return;

        isEnabled = true;
        number = PhoneDirectory.Register(this);
        transform.name = "Phone " + number;
        handsetLocalPosition = handset.transform.localPosition;

        MaterialMap matMap = new MaterialMap();
        matMap.Init(transform);
        materials = matMap.RemapMaterials(transform);

        if (color.Equals(Color.clear))
            color = CATex.RandomColor();

        SetColor(0, color);
        InitInteractableUI(true);
        inited = true;
    }

    private void SetColor(int index, Color color)
    {
        materials[index].color = color;
    }

    private void KillSeq()
    {
        if (null == seq)
            return;

        seq.Pause();
        seq.Kill(false);
        seq = null;
    }

    public override int IsInteractionEnabled()
    {
        if (inUse)
            return -1;

        return base.IsInteractionEnabled();
    }

    public override bool Interact(Interactable interactable)
    {
        if (!isEnabled || inUse)
            return false;

        ActionController actionController = ActionController.GetInstance();

        if (!actionController.IsCurrentAction(typeof(UseAction)))
            return base.Interact(interactable);

        inUse = true;
        Fire("InUse", true);
        HandleCall();
        return true;
    }

    public void SetEnabled(bool isEnabled)
    {
        if (this.isEnabled == isEnabled)
            return;

        this.isEnabled = isEnabled;
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public override List<string> GetAttributes()
    {
        string[] attributes = new string[]
        {
                "IsEnabled",
                "InUse",
                "IsCallIn",
                "CallMissed",
                "OutCall"
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
        SetDelegate("IsCallIn", SetCallIn);
    }

    public override void RegisterGoals()
    {
        Formula f = WorldDB.Get(Prefix + "IsEnabled");
        WorldDB.RegisterFormula(new Implication(null, f));

        f = WorldDB.Get(Prefix + "IsCallIn");
        WorldDB.RegisterFormula(new Implication(null, f));

        f = WorldDB.Get(Prefix + "InUse");
        WorldDB.RegisterFormula(new Implication(null, f));

        f = WorldDB.Get(Prefix + "OutCall");
        WorldDB.RegisterFormula(new Implication(f, null));

        f = WorldDB.Get(Prefix + "CallMissed");
        WorldDB.RegisterFormula(new Implication(f, null));
    }

    private void SetCallIn(bool isCallIn)
    {
        if (inUse || this.isCallIn == isCallIn)
            return;

        this.isCallIn = isCallIn;

        if (isCallIn)
            HandleCall();
    }

    private void HandleCall()
    {
        if (isCallIn)
        {
            if (!inUse)
            {
                if (!isRinging)
                    Ring();
            }
            else
            {
                if (!isTalking)
                {
                    isTalking = true;
                    PutHandset(true);
                    GameEvent.GetInstance().Execute(Talk, 1f);
                    float duration = Random.Range(7f, 15f);
                    GameEvent.GetInstance().Execute(FinishCall, duration);
                }
            }
        }
        else if (inUse && null != InteractableUI)
        {
            InteractableUI.Show();
        }
    }

    public void ClearCurrentNumber()
    {
        currentNumber = null;
        currentConnection = null;
    }

    public void Call(string number)
    {
        currentNumber = number;
        currentConnection = PhoneDirectory.GetPhone(currentNumber);
        audioFilter.distortionLevel = 0.65f;
        AudioManager.GetInstance().PlaySound("signal", handset, 1f, audioSource);
        audioSource.mute = false;
        StartCoroutine(Dial());
    }

    private IEnumerator Dial()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        AudioManager.GetInstance().StopSound("signal", handset, true);

        if (!inUse)
            yield break;

        int i = 0;
        audioSource.time = 0f;
        audioSource.mute = true;
        audioSource.Pause();

        while (i < currentNumber.Length)
        {
            audioSource.mute = true;
            audioSource.Pause();
            string s = currentNumber.Substring(i, 1);
            AudioManager.GetInstance().PlaySound("dtmf" + s, handset, 1f, audioSource);
            audioSource.mute = false;

            yield return new WaitForSecondsRealtime(0.25f);

            if (!inUse)
                yield break;

            i++;
        }    

        Connect();
    }

    private void Connect()
    {
        if (!inUse)
            return;

        int signalType = null == currentConnection ? 2 : (currentConnection.InUse || Random.value < 0.25f ? 1 : 0);

        //if (signalType > 0)
          //  currentConnection = null;

        int successLimit = signalType == 0 ? Random.Range(3, 5) : 4;
        int maxSignals = signalType == 0 ? Random.Range(2, successLimit) : successLimit;
        successLimit *= 2;
        maxSignals *= 2;
        StartCoroutine(PlaySignal(signalType, maxSignals, successLimit));
    }

    private static float[][] signalTimes = new float[][] {
        new float[] { 1f, 4f },
        new float[] { 0.48f, 0.48f },
        new float[] { 0.24f, 0.24f }
    };

    private IEnumerator PlaySignal(int signalType, int maxSignals, int successLimit)
    {
        yield return new WaitForSecondsRealtime(1f);

        AudioManager.GetInstance().PlaySound("signal", handset, 1f, audioSource);
        audioSource.mute = true;
        audioSource.loop = true;
        audioSource.Pause();

        int i = 0;
        
        while (i < maxSignals)
        {
            if (i % 2 == 0)
            {
                audioSource.time = 0f;
                audioSource.Play();
                audioSource.mute = false;

                try
                {
                    currentConnection?.RingOnce();
                }
                catch (System.Exception) { }
            }
            else
            {
                audioSource.Pause();
                audioSource.mute = true;                
            }

            yield return new WaitForSecondsRealtime(signalTimes[signalType][i % 2]);

            if (!inUse)
                break;

            i++;
        }

        audioSource.Pause();
        audioSource.mute = false;
        audioSource.loop = false;

        if (inUse)
        {
            if (signalType > 0 || maxSignals > successLimit)
                ((TelephoneUI)InteractableUI).CancelCall();
            else
            {
                Fire("OutCall", true);
                currentConnection?.AnswerCall(this);
            }
        }
    }

    public void AnswerCall(ITelephoneDevice phoneDevice)
    {
        currentConnection = phoneDevice;
        inUse = true;
        isTalking = true;
        StartTalk(audioSource);
        float duration = Random.Range(7f, 15f);
        GameEvent.GetInstance().Execute(FinishInCall, duration);
    }

    public void FinishInCall()
    {
        ((TelephoneUI)InteractableUI).CancelCall();
        isCallIn = false;
        isTalking = false;
        audioSource.mute = true;
        inUse = false;
        currentConnection?.FinishOutCall();
        ClearCurrentNumber();
    }

    public void FinishOutCall()
    {
        currentConnection?.FinishInCall();
        TelephoneUI telephoneUI = ((TelephoneUI)InteractableUI);
        telephoneUI.CancelCall();

        isCallIn = false;
        isTalking = false;
        audioSource.mute = true;
        inUse = false;
        ClearCurrentNumber();
    }

    public void FinishCall()
    {
        isCallIn = false;
        isTalking = false;
        audioSource.mute = true;
        PutHandset(false);
        inUse = false;
        Fire("InUse", false);
        ClearCurrentNumber();
    }

    private void PutHandset(bool mode)
    {
        // TODO solve that in player
        Player player = GameManager.GetInstance().CurrentPlayer;

        if (mode)
        {
            UIGame.GetInstance().ShowObject(this);

            if (isHandsetOff)
                return;

            isHandsetOff = true;
            handset.gameObject.SetActive(false);
            /* TODO
            player.SetAnimationTrigger("phone.l");
            handset.transform.SetParent(player.handL, false);
            handset.transform.localPosition = new Vector3(-0.025f, -0.155f, 0f);
            handset.transform.localRotation = Quaternion.Euler(21.622f, 78.911f, -1.645f);
            */
        }
        else
        {
            UIGame.GetInstance().HideObject();

            if (!isHandsetOff)
                return;

            isHandsetOff = false;
            player.Interact(null);
            handset.gameObject.SetActive(true);
            /* TODO
            player.SetAnimationTrigger("phone.l");
            player.SetAnimationTrigger("idle");
            handset.transform.SetParent(handsetParent, false);
            handset.transform.localPosition = Vector3.zero;
            handset.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            */
        }

        AudioManager.GetInstance().PlaySound("phone." + (mode ? "answer" : "hangup"), gameObject);
    }

    private void Ring()
    {
        if (isRinging)
            return;

        isRinging = true;
        Ring(0);
    }

    private void Ring(int count)
    {
        if (!isRinging || inUse || count > 6)
        {
            if (isCallIn && count > 6)
            {
                KillSeq();
                Fire("CallMissed", true);
            }

            return;
        }

        RingOnce();
        float duration = AudioManager.GetInstance().GetSoundLength("phone.ring");
        GameEvent.GetInstance().Execute<int>(Ring, count + 1, duration);
    }

    public void RingOnce()
    {
        float duration = AudioManager.GetInstance().GetSoundLength("phone.ring");
        AudioManager.GetInstance().PlaySound("phone.ring", gameObject);

        if (null == seq)
        {
            Vector3 pos1 = handsetLocalPosition;
            Vector3 pos2 = handsetLocalPosition + new Vector3(5f, 0f, 20f);
            seq = DOTween.Sequence()
                .SetAutoKill(false)
                .Append(handset.transform.DOLocalJump(pos1, 0.125f, 1, 0.75f))
                .Join(handset.transform.DOPunchRotation(pos2, duration * 0.5f, 10, 1f))
                .Play();
        }
        else
            seq.Restart();
    }

    private void Talk()
    {
        if (isRinging)
        {
            isRinging = false;
            KillSeq();
            //AudioManager.GetInstance().PlaySound("phone.answer", gameObject);
        }

        StartTalk();
    }

    private void StartTalk(AudioSource source = null)
    {
        if (null == source)
            source = audioSource;

        if (AudioManager.GetInstance().IsAudioOn)
        {
            ieVoice = IEVoice(source);
            StartCoroutine(ieVoice);
        }
    }

    private IEnumerator IEVoice(AudioSource source)
    {
        int v = Random.Range(0, 100) % audioPositions.Length;
        int[] arr = ArrayHelper.GetArray(0, audioPositions[v].Length);
        ArrayHelper.ShuffleArray(arr);
        audioFilter.distortionLevel = 0.6f;
        source.mute = true;
        AudioManager audioManager = AudioManager.GetInstance();
        audioManager.PlaySound("phone.voice." + v, handset, 1f, source);
        source.Pause();
        float volume = source.volume * 0.175f;
        int i = 0;
        int pauseIndex = Random.Range(0, 100) % 6 + 2;
        float speed = Random.Range(0.75f, 1.1f);

        while (isTalking)
        {
            int j = arr[i];
            float[] t = audioPositions[v][j];
            float duration = t[1] - t[0];
            source.time = t[0];
            source.mute = false;
            source.pitch = speed;
            duration /= speed;
            source.volume = volume;
            source.Play();

            yield return new WaitForSecondsRealtime(duration);
            source.mute = true;
            source.Pause();

            if (!isTalking)
                break;

            i++;
            i %= arr.Length;

            if (i % pauseIndex == 0)
            {
                pauseIndex = Random.Range(0, 100) % 6 + 2;
                i %= arr.Length;
                ArrayHelper.ShuffleArray(arr);

                yield return new WaitForSecondsRealtime(Random.value * 0.75f + 0.25f);
            }
        }

        source.mute = true;
        source.time = 0f;
        source.mute = false;
        ieVoice = null;
    }

    protected override bool ShouldBeEnabled()
    {
        return isEnabled;
    }
}