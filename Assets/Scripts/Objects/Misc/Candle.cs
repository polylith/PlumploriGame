using System.Collections;
using System.Collections.Generic;
using Action;
using Language;
using UnityEngine;

public class Candle : Interactable
{
    public bool IsBurning { get => isBurning; private set => SetBurning(value); }

    public ParticleSystem flame;
    public AudioSource audioSource;

    private bool isBurning;
    private float factor = 0.005f;
    private float minY = 0.275f;

    public override string GetDescription()
    {
        // TODO
        string s = GetText();
        return s;
    }

    public override List<string> GetAttributes()
    {
        return new List<string>()
        {
            "IsBurning",
            "IsBurnedOut"
        };
    }

    public override void RegisterGoals()
    {
        Formula f = WorldDB.Get(Prefix + "IsBurning");
        WorldDB.RegisterFormula(new Implication(f, null));

        f = WorldDB.Get(Prefix + "IsBurnedOut");
        WorldDB.RegisterFormula(new Implication(f, null));
    }

    protected override void RegisterAtoms()
    {
        RegisterAtoms(GetAttributes());
    }

    public override int IsInteractionEnabled()
    {
        if (!ActionController.GetInstance().IsCurrentAction(typeof(UseAction)))
            return base.IsInteractionEnabled();

        return transform.localScale.y < minY ? -1 : 1;
    }

    public override bool Interact(Interactable interactable = null)
    {
        if(!ActionController.GetInstance().IsCurrentAction(typeof(UseAction)))
            return base.Interact(interactable);

        if (transform.localScale.y < minY)
            return false;

        IsBurning = !isBurning;
        return true;
    }

    private void SetBurning(bool isBurning)
    {
        this.isBurning = isBurning;
        AudioManager audioManager = AudioManager.GetInstance();

        if (isBurning)
            audioManager.PlaySound("candle.burn", gameObject, 1f, audioSource);
        else
            audioSource.Stop();

        flame.gameObject.SetActive(isBurning);
        Fire("IsBurning", IsBurning);
    }

    private void Update()
    {
        if (!IsBurning)
            return;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            new Vector3(1f, 0f, 1f),
            Time.deltaTime * factor
        );

        if (transform.localScale.y < minY)
        {
            IsBurning = false;
            Fire("IsBurnedOut", true);
        }
    }
}
