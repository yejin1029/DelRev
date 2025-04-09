using UnityEngine;
using System.Collections;

public class GarageDoor : MonoBehaviour {

	public AudioClip garageDoorSound;

	public void ToggleDoor(){
		Animator anim = transform.parent.GetComponent<Animator> ();
		anim.SetBool ("openDoor", !anim.GetBool ("openDoor"));
		if (garageDoorSound != null)
			AudioSource.PlayClipAtPoint (garageDoorSound, transform.position);
	}
}
