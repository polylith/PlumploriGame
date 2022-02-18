using UnityEngine;

namespace Creation
{
    public class KeyData : AbstractData
    {

        public int Id { get => id; }
        public int Mask { get => mask; }
        public bool Collected { get => collected; }
        public string CollectedBy { get => collectedBy; }
        public Vector3 position;
        public Vector3 rotation;

        private int id;
        private int mask;
        private bool collected;
        private string collectedBy;
        
        public KeyData(int id, int mask)
        {
            this.id = id;
            this.mask = mask;
        }

        public void SetCollected(bool collected, string characterName)
        {
            this.collected = collected;
            collectedBy = collected ? characterName : null;
        }

        public void Load()
        {

        }

        public void Save()
        {

        }
    }
}