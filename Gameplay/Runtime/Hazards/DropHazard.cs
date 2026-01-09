using UnityEngine;

namespace Gameplay.Runtime {
    public class DropHazard : MonoBehaviour {
        public void DropObject(GameObject obj) {
            if (obj == null || obj.GetComponent<Rigidbody>() == null)
                Debug.LogError("Missing Object or Rigidbody on object!");
            else
                Debug.Log("Dropping object " + obj.name);

            obj.GetComponent<Rigidbody>().isKinematic = false;
        }
    }
}
