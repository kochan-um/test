using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class NotionAPI : MonoBehaviour
{
    // Notion API�g�[�N���ƃf�[�^�x�[�XID
    private string notionToken = "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"; // �V����API�g�[�N�������
    private string databaseId = "15dfb7cb7502802295d9ccf4d65d3e67"; // �f�[�^�x�[�XID
    private string notionUrl = "https://api.notion.com/v1/databases/";

    void Start()
    {
        // �f�[�^�擾���J�n
        StartCoroutine(GetNotionData());
    }

    IEnumerator GetNotionData()
    {
        // �N�G�����N�G�X�g�̐ݒ�
        string url = notionUrl + databaseId + "/query"; // �N�G���G���h�|�C���g���g�p
        string requestBody = "{}";  // �Ⴆ�΁A�t�B���^�[�Ȃ��̑S�f�[�^���擾���邽�߂̋��JSON

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        // Authorization�w�b�_�[��Bearer�g�[�N����ݒ�
        request.SetRequestHeader("Authorization", "Bearer " + notionToken);
        request.SetRequestHeader("Notion-Version", "2022-06-28"); // �C���F�T�|�[�g����Ă���o�[�W�����ɕύX
        request.SetRequestHeader("Content-Type", "application/json"); // Content-Type��JSON�ɐݒ�

        // ���N�G�X�g�𑗐M
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // ���X�|���X������
            Debug.Log("Response: " + request.downloadHandler.text);
        }
        else
        {
            // �G���[�n���h�����O
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Response Code: " + request.responseCode); // ���X�|���X�R�[�h��\��
            Debug.LogError("Response Text: " + request.downloadHandler.text); // �G���[���X�|���X��\��
        }
    }
}
