using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	//----------------------------------
	// 定数
	//----------------------------------

	// 距離
	public float	cDistanceXZ;
	public float	cDistanceXZ_Bird;

	// 注視点オフセット
	public Vector3	cAtOffset;
	public Vector3	cAtOffset_Bird;

	// ヨー回転
	public float	cRotYaw_Kd;
	public float	cRotYaw_Vel;
	public float	cRotYaw_Acc;

	// ピッチ回転
	public float	cRotPit;
	public float	cRotPit_Bird;

	//----------------------------------
	// 変数
	//----------------------------------

	// プレイヤー
	public GameObject	mPlayer;

	// 前方向 XZ
	private Vector3	mFwdDirXZ_Org;
	private Vector3	mFwdDirXZ;

	// ヨー回転
	private float	mRotYaw;
	private float	mRotYaw_Vel;


	void Start() {
		// 前方向 XZ
		mFwdDirXZ_Org   = mPlayer.transform.forward;
		mFwdDirXZ_Org.y = 0.0f;
		mFwdDirXZ_Org.Normalize ();
		mFwdDirXZ       = mFwdDirXZ_Org;

		// ヨー回転
		mRotYaw     = 0.0f;
		mRotYaw_Vel = 0.0f;
	}

	void LateUpdate() {
		// トリ
		float birdRt = mPlayer.GetComponent<Player>().getBirdRt();

		// ヨー回転
		{
			// ダンパ
			mRotYaw_Vel *= cRotYaw_Kd;

			// 加速度
			float rotYaw_Acc; {
				float stickX = Input.GetAxis("Horizontal2");
				rotYaw_Acc = cRotYaw_Acc * stickX;
			}

			// 速度更新
			mRotYaw_Vel += rotYaw_Acc;

			// 速度クランプ
			mRotYaw_Vel = Mathf.Clamp(mRotYaw_Vel, -cRotYaw_Vel, +cRotYaw_Vel);

			// 角度更新
			mRotYaw += mRotYaw_Vel;

			// 周期
			while ( mRotYaw > 360.0f ) mRotYaw -= 360.0f;
			while ( mRotYaw < 0.0f   ) mRotYaw += 360.0f;
		}

		// 前方向 XZ
		{
			mFwdDirXZ = Quaternion.AngleAxis (mRotYaw, Vector3.up) * mFwdDirXZ_Org;
			mFwdDirXZ.Normalize ();
		}

		// 注視位置
		Vector3 atPos; {
			atPos  = mPlayer.transform.position;
			Vector3 atOffset = Vector3.Lerp (cAtOffset, cAtOffset_Bird, birdRt);
			atPos += Vector3.up * atOffset.y;
			atPos += mFwdDirXZ * atOffset.z;
		}

		// 位置
		Vector3 pos; {
			Vector3 left = Vector3.Cross (Vector3.up, mFwdDirXZ);
			Vector3 dir_xz = -mFwdDirXZ;
			float rotPit = Mathf.Lerp (cRotPit, cRotPit_Bird, birdRt);
			Vector3 dir = Quaternion.AngleAxis (rotPit, left) * dir_xz;
			float dot = Vector3.Dot (dir, dir_xz);
			float distanceXZ = Mathf.Lerp (cDistanceXZ, cDistanceXZ_Bird, birdRt);
			float scale = distanceXZ / dot;
			pos = atPos + dir * scale;
		}

		// 更新
		transform.position = pos;
		transform.LookAt (atPos);
	}

	public Vector3 getFwdDirXZ() {
		return mFwdDirXZ;
	}
}
