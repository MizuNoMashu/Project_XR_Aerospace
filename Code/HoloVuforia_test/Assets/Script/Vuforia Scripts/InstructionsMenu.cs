using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionsMenu : MonoBehaviour {

    [SerializeField] GameObject vuforiaTargetsHandlerObj;
    private VuforiaTargetsHandler vuforiaTargetsHandler;

    [SerializeField] GameObject textToShowObj;
    private TextMeshProUGUI textToShow;

    private const string CURRENT_INSTRUCTION_STRING = "Current instruction: ";
    private const string EMPTY_TARGETS_LIST_STRING = "The instructions list is empty!";
    private const string SERVER_ERROR_STRING = "Download from server failed.";
    private const string INVALID_INDEX_STRING = "Invalid value. Please choose a number between 1 and ";
    private const string DOWNLOADING_STRING = "Downloading target from server...";
    private const string WAIT_FINISH_DOWNLOAD_STRING = "Wait for the previous download to finish!";

    // To avoid sending several requests at once
    private bool downloading = false;

    // Start is called before the first frame update
    void Start() {

        textToShow = textToShowObj.GetComponent<TextMeshProUGUI>();
        vuforiaTargetsHandler = vuforiaTargetsHandlerObj.GetComponent<VuforiaTargetsHandler>();

        // IMPORTANT
        // pass the callback function
        // ( setCallback defined in the other script) to the vuforiaTargetsHandle object
        vuforiaTargetsHandler.setCallback(callback);  

        // Download the first target
        vuforiaTargetsHandler.startTargets();
    }



    /********** HANDLER BUTTONS **********/
    public void nextInstruction() {
        if (downloading) {
            textToShow.text = DOWNLOADING_STRING;
            return;       
            //allows you to exit the function and avoid adding more requests
            // while we are still waiting for the response to the first request
        }
        // if it is not already downloading anything run this function
        // (which is a VuforiaTargets Handler function)
        vuforiaTargetsHandler.nextTarget();  

    }

    public void previousInstruction() {
        if (downloading) {
            textToShow.text = DOWNLOADING_STRING;
            return;
        }
        vuforiaTargetsHandler.previousTarget();
    }



    public void skipToInstructionNumber(int instruction) {
        if (downloading) {
            textToShow.text = DOWNLOADING_STRING;
            return;
        }
        vuforiaTargetsHandler.jumpToTarget(instruction - 1); 
    }

    /**************************************/
    
    private void callback(int res) {
        if (res == VuforiaTargetsHandler.DOWNLOADING) {
            textToShow.text = DOWNLOADING_STRING;
            downloading = true;
        }
        else {
            downloading = false;

            if (res == VuforiaTargetsHandler.DOWNLOAD_SUCCEEDED) {
                textToShow.text = CURRENT_INSTRUCTION_STRING + (vuforiaTargetsHandler.getCurrentTarget() + 1);
            }
            else if (res == VuforiaTargetsHandler.EMPTY_TARGETS_LIST) {
                textToShow.text = EMPTY_TARGETS_LIST_STRING;
            }
            else if (res == VuforiaTargetsHandler.DOWNLOAD_FAILED) {
                textToShow.text = SERVER_ERROR_STRING;
            }
            else if (res == VuforiaTargetsHandler.INVALID_INDEX) {
                invalidInputValue();
            }
        }
    }

    private void invalidInputValue() {
        textToShow.text = INVALID_INDEX_STRING + (vuforiaTargetsHandler.getInstructionsCount()) + ".";
    }
}
