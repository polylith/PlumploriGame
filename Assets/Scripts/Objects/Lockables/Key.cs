using System.Collections.Generic;
using Action;
using UnityEngine;

namespace Creation
{
    /// <summary>
    /// <para>
    /// A key is a useable object that can be used
    /// in combination with lockable objects to unlock
    /// or lock them.
    /// </para>
    /// <para>
    /// As a useable, it is also a collectable that can
    /// be placed in specified places in the current room.
    /// </para>
    /// <para>
    /// Each key has an Id and a mask. The mesh for a 3D key
    /// object must have several blendshapes that are used
    /// to visualize the Id. Therefore, the GameObject
    /// requires a SkinnedMeshRenderer component.
    /// </para>
    /// <para>
    /// The idea of keys matching multiple lockables is
    /// derived from IP and subnetting.
    /// If the Id of the lockable matches the Id and mask of
    /// the key, then they are in the same lock domain and may
    /// interact, otherwise the interaction is disabled.
    /// A key with mask 0 is a general key.
    /// </para>
    /// </summary>
    public class Key : Useable
    {
        public int InitId = -1;
        public int InitMask;

        public GameObject keyObj;
        public int maxKeys = 8;
        public int maxSteps = 5;

        public int Id { get => id; }
        public int Mask { get => mask; }        

        private int id;
        private int mask;

        private void Awake()
        {
            if (InitId > 0)
            {
                KeyData keyData = new KeyData(InitId, InitMask);
                keyData.position = transform.position;
                keyData.rotation = transform.rotation.eulerAngles;
                Init(keyData);
            }
        }

        public override List<string> GetAttributes()
        {
            return new List<string>();
        }

        protected override void RegisterAtoms()
        {
            // None
        }

        public override string GetDescription()
        {
            throw new System.NotImplementedException();
        }

        public override bool RequiresType()
        {
            return true;
        }

        public override bool CheckRequiredType(Interactable interactable)
        {
            return null != interactable && interactable is Lockable;
        }

        public override int IsUseable(Interactable interactable)
        {
            if (null == interactable)
                return 0;

            if (!(interactable is Lockable lockable))
                return -1;

            return CheckAccess(lockable) ? 1 : -1;
        }

        public override bool Interact(Interactable interactable)
        {
            if (!ActionController.GetInstance().IsCurrentAction(typeof(UseAction)))
                return base.Interact(interactable);

            if (null == interactable || !(interactable is Lockable lockable))
                return false;

            return lockable.Interact(this);
        }

        public void Init(KeyData keyData)
        {
            SetIdAndMask(keyData.Id, keyData.Mask);
            transform.name = "Key " + id;
            transform.position = keyData.position;
            transform.rotation = Quaternion.Euler(keyData.rotation);

            if (keyData.Collected)
            {
                Player player = GameManager.GetInstance().GetPlayer(keyData.CollectedBy);
                player.AddInventoryItem(this);
            }
        }

        public void SetIdAndMask(int id, int mask)
        {
            this.id = id;
            this.mask = mask;
            ApplyKey();
        }

        private void ApplyKey()
        {
            SkinnedMeshRenderer renderer = keyObj.GetComponent<SkinnedMeshRenderer>();
            int k = id;
            float d = 100f / maxSteps;

            for (int i = 0; i < maxKeys; i++)
            {
                renderer.SetBlendShapeWeight(i, (k % maxSteps) * d);
                k /= maxSteps;
            }
        }

        public bool CheckAccess(Lockable lockable)
        {
            if (null == lockable)
                return false;

            int id = lockable.Id;

            return Compare(id, this.id, mask);
        }

        public static bool Compare(int id, int key, int mask)
        {
            //Debug.Log("Compare\n\t" + KeyManager.Int2BinStr(id) + "\n\t" + KeyManager.Int2BinStr(key) + "\n\t" + KeyManager.Int2BinStr(mask));

            if (id == int.MinValue)
                return false;

            if (key == id)
                return true;

            int a = id;
            int b = key;
            int c = mask;

            while (c > 0)
            {
                if (c % 2 == 1 && (a % 2) != (b % 2))
                    return false;

                a >>= 1;
                b >>= 1;
                c >>= 1;
            }

            return true;
        }

        public override string ToString()
        {
            return "id: " + id + " mask " + mask;
        }
    }
}