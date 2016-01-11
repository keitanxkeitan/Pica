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
	public float	cMove_Vel_Bird;
	public float	cMove_Acc_Bird;

	// 旋回
	public float	cRot_Speed;

	// 変身
	public float	cTransform_HumanBird_Frm;
	public float	cTransform_BirdHuman_Frm;

	// トリ
	public float	cBird_Height;

	//----------------------------------
	// 変数
	//----------------------------------

	// Rigidbody
	private Rigidbody	mRigidbody;

	// カメラ
	public GameObject	mCamera;

	// スティック入力
	private Vector2	mStick;

	// 入力
	private bool	mbHoldBird;

	// 移動速度
	private Vector3	mMoveVel_Req;
	private Vector3	mMoveVel;

	// トリ
	private float	mBirdRt;

	void Start () {
		// Rigidbody
		mRigidbody = GetComponent<Rigidbody> ();

		// 入力
		mbHoldBird = false;

		// 移動速度
		mMoveVel_Req = Vector3.zero;
		mMoveVel     = Vector3.zero;

		// トリ
		mBirdRt = 0.0f;
	}

	void Update () {
		mStick = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical")).normalized * 10;

		// 入力
		mbHoldBird = Input.GetButton("Bird");
	}

	void FixedUpdate() {
		// 移動計算
		CalcMove();

		// ヒト／トリ計算
		calcHumanBird();
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
			float moveVel = Mathf.Lerp(cMove_Vel, cMove_Vel_Bird, mBirdRt);
			mMoveVel_Req = moveDir * moveVel * moveRt;

			// 加速度
			Vector3 acc;
			{
				acc = mMoveVel_Req - mMoveVel;
				float moveAcc = Mathf.Lerp (cMove_Acc, cMove_Acc_Bird, mBirdRt);
				acc = Vector3.ClampMagnitude (acc, moveAcc);
			}

			// 速度更新
			mMoveVel += acc;

			// 速度クランプ
			mMoveVel = Vector3.ClampMagnitude(mMoveVel, moveVel);

			// 移動
			mRigidbody.MovePosition(mRigidbody.position + mMoveVel * Time.fixedDeltaTime);
		}

		// トリ
		if (mbHoldBird) {
			float velocity = cBird_Height - mRigidbody.position.y;
			velocity *= 0.1f;
			mRigidbody.MovePosition (mRigidbody.position + velocity * Vector3.up);
		}

		// 姿勢
		if ( mMoveVel_Req.magnitude > 0.01f ) // テキトウ
		{
			Quaternion to = Quaternion.LookRotation (mMoveVel_Req);
			mRigidbody.rotation = Quaternion.Slerp (mRigidbody.rotation, to, cRot_Speed * Time.fixedDeltaTime);
		}
	}

	void calcHumanBird() {
		if (mbHoldBird) {
			mBirdRt += 1.0f / cTransform_HumanBird_Frm;
			mBirdRt = Mathf.Clamp01 (mBirdRt);

			mRigidbody.useGravity = false;
		} else {
			mBirdRt -= 1.0f / cTransform_BirdHuman_Frm;
			mBirdRt = Mathf.Clamp01 (mBirdRt);

			mRigidbody.useGravity = true;
		}
	}

	public float getBirdRt() {
		return mBirdRt;
	}
}