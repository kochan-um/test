using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;       // �v���C���[��Transform
    public Vector3 offset;        // �v���C���[����J�����̃I�t�Z�b�g�ʒu
    public float smoothSpeed = 0.125f;  // �Ǐ]�̃X���[�Y��

    void LateUpdate()
    {
        // �v���C���[�̈ʒu�ɃI�t�Z�b�g�������ăJ�����̖ڕW�ʒu���v�Z
        Vector3 desiredPosition = player.position + offset;

        // �J�������ڕW�ʒu�Ɍ������ăX���[�Y�Ɉړ�
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // �J�����̈ʒu���X�V
        transform.position = smoothedPosition;

        // �J��������Ƀv���C���[�������悤�ɂ���
        transform.LookAt(player);
    }
}
