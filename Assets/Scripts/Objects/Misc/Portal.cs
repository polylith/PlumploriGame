using System.Collections;
using System.Collections.Generic;
using Action;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// This portal can teleport a character to another location.
/// </summary>
public class Portal : Interactable
{
    public ParticleSystem particles;
    public Transform transitPos;
    public AudioSource audioSource;
    public bool isActive = true;
    public PostProcessVolume ppvolume;

    private Character character;

    public override List<string> GetAttributes()
    {
        List<string> attributes = new List<string>() {
            "IsActive"
        };

        return attributes;
    }

    public override string GetDescription()
    {
        throw new System.NotImplementedException();
    }

    public override void RegisterGoals()
    {
        Formula f = WorldDB.Get(Prefix + "IsActive");
        WorldDB.RegisterFormula(new Implication(null, f));
    }

    protected override void RegisterAtoms()
    {
        RegisterAtoms();
        SetDelegate("IsActive", SetActive);
    }

    private void SetActive(bool isActive)
    {
        if (this.isActive == isActive)
            return;

        this.isActive = isActive;
        // TODO animationgedöns
    }

    public override int IsInteractionEnabled()
    {
        /*
         * TODO optimize action handling
         * only valid action is pointer, but any other make the player
         * walk to the portal, but nothing is happening. feels strange.
         */
        ActionController actionController = ActionController.GetInstance();

        return actionController.IsCurrentAction(typeof(PointerAction))
            ? 1 : base.IsInteractionEnabled();
    }

    public override bool Interact(Interactable interactable)
    {
        ActionController actionController = ActionController.GetInstance();

        if (!actionController.IsCurrentAction(typeof(PointerAction)))
        {
            return base.Interact(interactable);
        }

        StartCoroutine(Teleport(character));
        return true;
    }

    private IEnumerator Teleport(Character character)
    {
        AudioManager audioManager = AudioManager.GetInstance();
        audioManager.PlaySound("portal.activate", gameObject, 1f, audioSource);
        particles.Play();

        float f = 0f;
        ppvolume.enabled = true;
        ppvolume.weight = f;

        while (f <= 1f)
        {
            ppvolume.weight = f;

            yield return null;

            f += Time.deltaTime;
        }

        if (!character.IsNPC)
        {
            UIGame uiGame = UIGame.GetInstance();
            uiGame.SetUIExclusive(gameObject, true);
            uiGame.SetCursorVisible(false);
            uiGame.ShowShade();
            UIDropPoint.GetInstance().HidePointer();
        }

        while (audioSource.isPlaying)
            yield return null;

        float pitch = 0.875f + 0.075f * Random.value;
        audioManager.PlaySound("slip", gameObject, pitch, audioSource);

        yield return new WaitForSecondsRealtime(2f);

        RaycastHit hit = Calc.GetPointOnGround(transitPos.position);
        character.SetPosition(hit.point);

        if (!character.IsNPC)
        {
            yield return new WaitForSecondsRealtime(0.5f);

            ppvolume.weight = 0f;
            ppvolume.enabled = false;
            particles.Stop();

            // close all visible interactable uis
            InteractableUI.CloseActiveUIs();

            GameManager.GetInstance().CurrentRoom.ForceUpdateCameraPosition();

            yield return new WaitForSecondsRealtime(0.5f);

            character.InstantLookAt(Camera.main.transform.position);

            UIGame uiGame = UIGame.GetInstance();
            uiGame.HideShade();

            yield return new WaitForSecondsRealtime(1f);
            
            uiGame.SetCursorVisible(true);

            yield return new WaitForSecondsRealtime(0.5f);

            uiGame.SetUIExclusive(gameObject, true);
        }
        else
        {
            f = 1f;

            while (f >= 0f)
            {
                ppvolume.weight = f;

                yield return null;

                f -= Time.deltaTime;
            }

            ppvolume.weight = 0f;
            ppvolume.enabled = false;
            particles.Stop();
        }
    }

    protected override bool ShouldBeEnabled()
    {
        return isActive;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Character character = collision.transform.GetComponent<Character>();

        if (null == character)
            return;

        this.character = character;
    }
}
