using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Globalization;
using System;

public class ContinuityTest : MonoBehaviour {

    [SerializeField] GameObject textObj;
    private TextMeshProUGUI textComponent;

    [SerializeField] GameObject messageObj;
    private TextMeshProUGUI messageComponent;

    private string URL = "http://10.2.150.7:5000/data";

    // Start is called before the first frame update
    void Start() {
        textComponent = textObj.GetComponent<TextMeshProUGUI>();
        messageComponent = messageObj.GetComponent<TextMeshProUGUI>();

        startRequestingValues();
    }

    //Start coroutine that starts the parallel function getValuesCoroutine
    public void startRequestingValues() {
        StartCoroutine(getValuesCoroutine(URL));
    }

    // Ask for a new value every second
    IEnumerator getValuesCoroutine(string url) {
        while (true) {

            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest(); // Send request and wait for reply
            // The word yield is used to suspend execution until the next frame

            if (request.result == UnityWebRequest.Result.ConnectionError) {
                messageComponent.text = "Server error";
                Debug.LogError(request.error);
            }
            else {
                string data = request.downloadHandler.text;
                textComponent.text = data;

                String[] fields = data.Split(' ');
                string value = fields[0];

                if (value == "OL") {
                    messageComponent.text = "Open circuit";
                }
                else {
                    messageComponent.text = "";

                    String[] parts = value.Split('.');
                    string int_part_string = parts[0];
                    string dec_part_string = parts[1];

                    int int_part = 0;
                    int dec_part = 0;

                    if (Int32.TryParse(int_part_string, out int_part)) {
                        if (Int32.TryParse(dec_part_string, out dec_part)) {
                            // If parsing is successful
                            if (int_part == 0 && dec_part == 0) {
                                messageComponent.text = "There is electrical continuity";
                            }
                            
                        }
                    }

                }
            }

            // Wait and then repeat (0.3s)
            yield return new WaitForSeconds(0.3f); 
        }
    }
}
