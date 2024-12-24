using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;  // プレイヤーの移動速度
    private Vector3 moveDirection;

    void Update()
    {
        // プレイヤーの移動方向を取得
        float moveX = Input.GetAxis("Horizontal"); // 左右移動 (A,Dキー)
        float moveZ = Input.GetAxis("Vertical");   // 前後移動 (W,Sキー)

        moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        // プレイヤーを移動
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }
}
