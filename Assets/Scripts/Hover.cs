using UnityEngine;

public class Hover : MonoBehaviour {
	private void Update ()
    {
        transform.position = new Vector3(transform.position.x, 1.25f + Mathf.Sin(Time.time) * 0.1f,transform.position.z);
	}
}
