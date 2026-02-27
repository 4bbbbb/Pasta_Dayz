using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServeMessageData : MonoBehaviour
{
    [System.Serializable]

    public class ServeMessage
    {
        public string success;
        public string fail;
        public string nothing;
    }
   
}
