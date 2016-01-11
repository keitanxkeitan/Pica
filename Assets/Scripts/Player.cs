using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	//----------------------------------
	// 定数
	//----------------------------------

	// 低感度バイアス
	public float	cStickBias;

	// 移動
	public float	cMove_Vel;
	public float	cMove_Acc;

	// 旋回
	public float	cRot_Speed;

	//----------------------------------
	// 変数
	//----------------------------------

	// Rigidbody
	private Rigidbody	mRigidbody;

	// カメラ
	public GameObject	mCamera;

	// スティック入力
	private Vector2	mStick;

	// 移動速度
	private Vector3	mMoveVel_Req;
	private Vector3	mMoveVel;

	void Start () {
		// Rigidbody
		mRigidbody = GetComponent<Rigidbody> ();

		// 移動速度
		mMoveVel_Req = Vector3.zero;
		mMoveVel     = Vector3.zero;
	}

	void Update () {
		mStick = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical")).normalized * 10;
	}

	void FixedUpdate() {
		// 移動計算
		CalcMove();
	}

	void CalcMove() {
		// 移動
		{
			Vector3 moveDir = Vector3.zero;
			{
				Vector3 stickFwd = mCamera.GetComponent<CameraController> ().getFwdDirXZ ();
				Vector3 left = Vector3.Cross (Vector3.up, stickFwd);
				moveDir += left * mStick.x;
				moveDir += stickFwd * mStick.y;
			}

			float moveRt = moveDir.magnitude;
			moveDir.Normalize ();

			// 低感度バイアス
			moveRt = Mathf.Pow(moveRt, cStickBias);

			// 目標速度
			mMoveVel_Req = moveDir * cMove_Vel * moveRt;

			// 加速度
			Vector3 acc;
			{
				acc = mMoveVel_Req - mMoveVel;
				acc = Vector3.ClampMagnitude (acc, cMove_Acc);
			}

			// 速度更新
			mMoveVel += acc;

			// 速度クランプ
			mMoveVel = Vector3.ClampMagnitude(mMoveVel, cMove_Vel);

			// 移動
			mRigidbody.MovePosition(mRigidbody.position + mMoveVel * Time.fixedDeltaTime);
		}

		// 姿勢
		if ( mMoveVel_Req.magnitude > 0.01f ) // テキトウ
		{
			Quaternion to = Quaternion.LookRotation (mMoveVel_Req);
			mRigidbody.rotation = Quaternion.Slerp (mRigidbody.rotation, to, cRot_Speed * Time.fixedDeltaTime);
		}
	}
}