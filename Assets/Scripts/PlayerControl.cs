using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Valve.VR;

[NetworkSettings(channel = 1, sendInterval = 0.1f)]
public class PlayerControl : NetworkBehaviour {

	[SyncVar] private float hp = 100f;
	[SyncVar (hook = "DisplayScore")] public int score;
	private int ammunition = 0, maxAmmunition = 30;
	private Color damageColor;
	private bool takeDamage = false;
	private string transparentLayerName = "TransparentFX", defaultLayerName = "Default";
	private Material damageScreenMat;
	private float time = 0.5f, timer = 0.5f;
	private Color originalDamageColor;
	[SyncVar]private Vector3 headInterPos, leftHandInterPos, rightHandInterPos, bodyInterPos;
	private Quaternion headInterRot, leftHandInterRot, rightHandInterRot;
	private Text scoreTextMain, scoreTextHand;
	private GameManager gameManager;

	public GameObject head, eyeCamera, headObject, leftHand, rightHand, body, damageScreen;
	//public Text ammunitionNo;
	//public GameObject damageScreen;

	[SerializeField]
	private GameObject LeftHand, RightHand;

	[HideInInspector]public string id;
	[HideInInspector]public bool localPlayerOrNot, serverOrNot;

	// Use this for initialization
	void Start () {

		scoreTextMain = body.GetComponentInChildren<Text> ();
		scoreTextHand = leftHand.GetComponentInChildren<Text> ();
		if (!isLocalPlayer) 
		{
			
			damageScreen.SetActive (false);
			//headObject.GetComponentsInChildren<GameObject> (true).ToList ().ForEach (x => x.layer = LayerMask.NameToLayer (defaultLayerName));
			SetScore();
		} 
		else 
		{
			
			headObject.GetComponentsInChildren<Transform> (true).ToList ().ForEach (x => x.gameObject.layer = LayerMask.NameToLayer (transparentLayerName));
			scoreTextMain.text = "0";
			scoreTextHand.text = "0";
		}

		head.SetActive (false);
		head.GetComponent<Camera> ().enabled = false;

		localPlayerOrNot = isLocalPlayer;
		serverOrNot = isServer;
		id = GetComponent<NetworkIdentity> ().netId.ToString();

		ammunition = maxAmmunition;
		damageScreenMat = damageScreen.GetComponent<Renderer> ().material;
		damageScreenMat.color = Color.red;
		originalDamageColor = damageScreenMat.color;
		gameManager = GameObject.FindObjectOfType<GameManager> ();
		gameManager.RegisterPlayer (id);

		CheckControllerManager ();
		if (!GetComponent<SteamVR_ControllerManager> ().enabled)
		{
			LeftHand.SetActive (true);
			RightHand.SetActive (true);
		}
	}

	
	// Update is called once per frame
	void Update () 
	{
		//ammunitionNo.text = ammunition.ToString();

		headObject.transform.position = new Vector3(eyeCamera.transform.position.x, eyeCamera.transform.position.y , eyeCamera.transform.position.z);
		headObject.transform.rotation = eyeCamera.transform.rotation;

		float yPos = (float)-0.224 + eyeCamera.transform.position.y;
		body.transform.position = new Vector3(eyeCamera.transform.position.x, yPos, eyeCamera.transform.position.z);

		if (isLocalPlayer) 
		{
			headInterPos = head.transform.position;
			headInterRot = head.transform.rotation;
			leftHandInterPos = leftHand.transform.position;
			leftHandInterRot = leftHand.transform.rotation;
			rightHandInterPos = rightHand.transform.position;
			rightHandInterRot = rightHand.transform.rotation;
			bodyInterPos = body.transform.position;

			Vector3[] positions = new Vector3[4];
			Quaternion[] rotations = new Quaternion[3];

			positions [0] = head.transform.position;
			positions [1] = leftHand.transform.position;
			positions [2] = rightHand.transform.position;
			positions [3] = body.transform.position;
			rotations [0] = head.transform.rotation;
			rotations [1] = leftHand.transform.rotation;
			rotations [2] =  rightHand.transform.rotation;


			if (!isServer)
			{
				CmdSyncMovement (positions, rotations);
			}
		}
		else
		{	
			head.transform.position = Vector3.Lerp (head.transform.position, headInterPos, Time.deltaTime);
			leftHand.transform.position = Vector3.Lerp (leftHand.transform.position, leftHandInterPos, Time.deltaTime);
			rightHand.transform.position = Vector3.Lerp (rightHand.transform.position, rightHandInterPos, Time.deltaTime);
			body.transform.position = Vector3.Lerp (body.transform.position, bodyInterPos, Time.deltaTime);

			head.transform.rotation = Quaternion.Lerp (head.transform.rotation, headInterRot, Time.deltaTime);
			leftHand.transform.rotation = Quaternion.Lerp (leftHand.transform.rotation, leftHandInterRot, Time.deltaTime);
			rightHand.transform.rotation = Quaternion.Lerp (rightHand.transform.rotation, rightHandInterRot, Time.deltaTime);

		}

		if (isLocalPlayer && takeDamage) 
		{
			damageScreen.SetActive (true);
			damageScreenMat.color = Color.Lerp (damageScreenMat.color, Color.clear, 0.1f);
			time -= Time.deltaTime;
			if (time < 0) 
			{
				takeDamage = false;
				damageScreenMat.color = originalDamageColor;
				damageScreen.SetActive (false);
				time = timer;

			}
		}

		scoreTextMain.transform.parent.transform.Rotate (0f, 45f * Time.deltaTime, 0f);

		CheckControllerManager ();

		if (rightHand.transform.parent == transform)
		{
//			rightHand.transform.parent	= transform.parent;
		}
	}

	void OnDestroy()
	{
		gameManager.UnregisterPlayer (id);
	}

	void SetScore()
	{
		scoreTextMain.text = score.ToString ();
		scoreTextHand.text = score.ToString ();
	}

	public void Damage()
	{
		hp--;
		takeDamage = true;
	}

	public void minusAmmunition()
	{
		ammunition--;
	}

	void DisplayScore(int sc)
	{
		score = sc;
		scoreTextMain.text = score.ToString();
		scoreTextHand.text = score.ToString ();
		gameManager.AmendScore (id, score);
	}

	public static void CheckControllerManager()
	{
		int leftID = 0;
		leftID = SteamVR_Controller.GetDeviceIndex (SteamVR_Controller.DeviceRelation.Leftmost);
		int rightID = 0;
		rightID = SteamVR_Controller.GetDeviceIndex (SteamVR_Controller.DeviceRelation.Rightmost);

		if (leftID < 0 && rightID < 0)
		{
			GameObject.FindObjectOfType<SteamVR_ControllerManager> ().enabled = false;
		}
		else
		{
			GameObject.FindObjectOfType<SteamVR_ControllerManager> ().enabled = true;
		}
	}

	public void UpdateScore(string playerID)
	{
		PlayerControl[] players = GameObject.FindObjectsOfType<PlayerControl> ();
		foreach (PlayerControl player in players)
		{
			if (player.id == playerID && isServer) 
			{
				player.CmdUpdateScore (playerID);
			}
		}
	}

	[Command]
	void CmdSyncMovement(Vector3[] positions, Quaternion[] rotations)
	{
		headInterPos = positions [0]; 
		leftHandInterPos = positions [1];
		rightHandInterPos = positions [2];
		bodyInterPos = positions [3];
		headInterRot = rotations [0];
		leftHandInterRot = rotations [1];
		rightHandInterRot = rotations [2];

		RpcSyncMovement (positions, rotations);
	}

	[ClientRpc]
	void RpcSyncMovement(Vector3[] positions, Quaternion[] rotations)
	{
		headInterPos = positions [0]; 
		leftHandInterPos = positions [1];
		rightHandInterPos = positions [2];
		bodyInterPos = positions [3];
		headInterRot = rotations [0];
		leftHandInterRot = rotations [1];
		rightHandInterRot = rotations [2];
	}
	
	[Command]
	public void CmdUpdateScore(string playerID)
	{
		score++;
		DisplayScore (score);
	}


}
