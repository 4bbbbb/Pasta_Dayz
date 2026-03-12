using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ServeMessageData;

public class ServeMessageDatabase : MonoBehaviour
{
    public List<ServeMessage> messageList = new List<ServeMessage>();
    private List<string> nothingMessages = new List<string>();

    private void Awake()
    {
        LoadMessageData();
    }

    void LoadMessageData()
    {
        var data = CSVReader.Read("Data/ServeMessageData");

        foreach (var row in data)
        {
            ServeMessage msg = new ServeMessage();
            msg.success = row["Success"].ToString();
            msg.fail = row["Fail"].ToString();
            msg.nothing = row["Nothing"].ToString();

            messageList.Add(msg);

            if (!string.IsNullOrEmpty(msg.nothing))
            {
                nothingMessages.Add(msg.nothing);
            }

        }
    }

    public string GetRandomMessage(bool isSuccess)
    {
        if (messageList.Count == 0)
        {
            return "";
        }

        int rand = Random.Range(0, messageList.Count);

        return isSuccess ? messageList[rand].success : messageList[rand].fail;
    }

    public string GetRandomMessageNothing()
    {
        if (messageList.Count == 0)
            return "";

        int rand = Random.Range(0, nothingMessages.Count);
        return nothingMessages[rand];
    }
}
