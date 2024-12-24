using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;       // プレイヤーのTransform
    public Vector3 offset;        // プレイヤーからカメラのオフセット位置
    public float smoothSpeed = 0.125f;  // 追従のスムーズさ

    void LateUpdate()
    {
        // プレイヤーの位置にオフセットを加えてカメラの目標位置を計算
        Vector3 desiredPosition = player.position + offset;

        // カメラが目標位置に向かってスムーズに移動
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // カメラの位置を更新
        transform.position = smoothedPosition;

        // カメラが常にプレイヤーを向くようにする
        transform.LookAt(player);
    }
}
