using UnityEngine;

namespace Creation
{
    /// <summary>
    /// <para>
    /// This class is designed to put a key in a lock.
    /// It is like a proxy for interacting with the key.
    /// </para>
    /// <para>
    /// Rather not used anymore, because keys always stay
    /// in the active character's inventory until the player
    /// drops the key to some place.
    /// It also no longer fits into the updated interaction
    /// system, which works exclusively with interactables.
    /// </para>
    /// </summary>
    public class KeyPosition : MonoBehaviour
    {
        public Collider col;

        private Key key;

        private void Awake()
        {
            SetActive(false);
        }

        private void SetActive(bool isActive)
        {
            col.enabled = isActive;
        }

        private void OnMouseEnter()
        {
            if (null == key)
                return;

            key.MouseOver();
        }

        private void OnMouseExit()
        {
            if (null == key)
                return;

            key.MouseExit();
        }

        private void OnMouseDown()
        {
            if (null == key)
                return;

            key.MouseClick();
        }

        /// <summary>
        /// Put the key in position.
        /// </summary>
        /// <param name="key">the key to place</param>
        public void Place(Key key)
        {
            this.key = key;
            key.transform.SetParent(transform, false);
            key.transform.localPosition = Vector3.zero;
            key.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            key.transform.localScale = Vector3.one;
            key.transform.gameObject.layer = transform.gameObject.layer;
            key.Place();
            SetActive(true);
        }

        /// <summary>
        /// Remove the key from position.
        /// </summary>
        /// <param name="key">the key to unplace</param>
        public void Unplace(Key key)
        {
            if (this.key != key)
                return;

            key.Unplace();
            SetActive(false);
        }
    }
}
