using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	private CharacterController player;
	private Vector3 moveDirection;

	private float moveSpeed = 5f, gravity = 20f, jumpSpeed = 5f;

	// Use this for initialization
	void Start () 
	{
		player = GetComponent<CharacterController> ();
		Camera.main.gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (player.isGrounded) 
		{
			moveDirection = new Vector3 (0f, 0f, Input.GetAxis ("Vertical"));
			moveDirection = transform.TransformDirection (moveDirection);
			moveDirection *= moveSpeed;

			if (Input.GetButton ("Jump")) 
			{
				moveDirection.y = jumpSpeed;
			}
		}
		moveDirection.y -= gravity * Time.deltaTime;
		player.Move (moveDirection * Time.deltaTime);
	
		transform.Rotate(0f, Input.GetAxis("Mouse X") ,0f);
	}
}
