using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class NotionAPI : MonoBehaviour
{
    // Notion APIトークンとデータベースID
    private string notionToken = "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"; // 新しいAPIトークンを入力
    private string databaseId = "15dfb7cb7502802295d9ccf4d65d3e67"; // データベースID
    private string notionUrl = "https://api.notion.com/v1/databases/";

    void Start()
    {
        // データ取得を開始
        StartCoroutine(GetNotionData());
    }

    IEnumerator GetNotionData()
    {
        // クエリリクエストの設定
        string url = notionUrl + databaseId + "/query"; // クエリエンドポイントを使用
        string requestBody = "{}";  // 例えば、フィルターなしの全データを取得するための空のJSON

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        // AuthorizationヘッダーにBearerトークンを設定
        request.SetRequestHeader("Authorization", "Bearer " + notionToken);
        request.SetRequestHeader("Notion-Version", "2022-06-28"); // 修正：サポートされているバージョンに変更
        request.SetRequestHeader("Content-Type", "application/json"); // Content-TypeをJSONに設定

        // リクエストを送信
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // レスポンスを処理
            Debug.Log("Response: " + request.downloadHandler.text);
        }
        else
        {
            // エラーハンドリング
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Response Code: " + request.responseCode); // レスポンスコードを表示
            Debug.LogError("Response Text: " + request.downloadHandler.text); // エラーレスポンスを表示
        }
    }
}
