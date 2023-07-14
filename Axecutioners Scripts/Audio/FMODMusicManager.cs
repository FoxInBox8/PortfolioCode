//Built using the help of the following article:
//https://colinvandervort.medium.com/fmod-unity-beat-mapping-3e294bb9b288

using UnityEngine;
using FMODUnity;
using System;
using System.Runtime.InteropServices;

public class FMODMusicManager : MonoBehaviour
{
    public static FMODMusicManager instance;

    [SerializeField]
    private EventReference music;


    //Beat Detection
    public TimelineInfo timelineInfo = null;

    private GCHandle timelineHandle;


    private FMOD.Studio.EVENT_CALLBACK beatCallback;
    private FMOD.Studio.EventDescription descriptionCallback;


    public FMOD.Studio.EventInstance musicPlayEvent;




    public delegate void BeatEventDelegate();
    public static event BeatEventDelegate beatUpdated;

    public delegate void MarkerListenerDelegate();
    public static event MarkerListenerDelegate markerUpdated;

    public static int lastBeat = 0;
    public static string lastMarkerString = null;


    //Music Parameters

    //Song to play
    public float Intensity;
    public enum IntensityLevels
    {
        Title,
        Low,
        Medium,
        High
    }


    //Player 1 Health
    public int P1Health = 3;

    //Player 2 Health
    public int P2Health = 3;

    //Player has crown
    public int PlayerHasCrown = 0;

    //Player gets hit
    public float PlayerHit = 0;

    //Player entered hot zone
    public float PlayerEnterHotZone = 0;

    //Player exited hot zone
    public float PlayerMovedOutOfHotZone = 0;

    //Seconds remainint in match
    public int SecondsRemaining = 99;


    //HotZoneTimer
    bool hotZoneIncreasing = true;

    float currentHotZoneTimer = 1f;


    [StructLayout(LayoutKind.Sequential)]
    public class TimelineInfo
    {
        public int currentBeat = 0;
        public int currentBar = 0;
        public float currentTempo = 0;
        public int currentPosition = 0;
        public float songLength = 0;
        public FMOD.StringWrapper lastMarker = new FMOD.StringWrapper();
    }


    private void Awake()
    {
        musicPlayEvent = RuntimeManager.CreateInstance(music);

        InitFMODSPHooks();

        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }


        DontDestroyOnLoad(this);
        musicPlayEvent.start();

        musicPlayEvent.setParameterByName("HotZoneTimer", currentHotZoneTimer);
    }

    private void OnDestroy()
    {
        musicPlayEvent.setUserData(IntPtr.Zero);
        musicPlayEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); //Change stop mode if needed
        musicPlayEvent.release();
        timelineHandle.Free();
    }

    private void InitFMODSPHooks()
    {
        timelineInfo = new TimelineInfo();
        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);

        timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);
        musicPlayEvent.setUserData(GCHandle.ToIntPtr(timelineHandle));
        musicPlayEvent.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        musicPlayEvent.getDescription(out descriptionCallback);
        descriptionCallback.getLength(out int length);

        timelineInfo.songLength = length;
    }



    private void Update()
    {
        //Beat Detection
        musicPlayEvent.getTimelinePosition(out timelineInfo.currentPosition);

        if(lastMarkerString != timelineInfo.lastMarker)
        {
            lastMarkerString = timelineInfo.lastMarker;

            if(markerUpdated != null)
            {
                markerUpdated();
            }
        }

        if(lastBeat != timelineInfo.currentBeat)
        {
            lastBeat = timelineInfo.currentBeat;

            if(beatUpdated != null)
            {
                beatUpdated();
            }
        }


        //Parameter Updates
        UpdateParameters();
    }


    //play player hit sfx to beat of music
    public void DoPlayerHit(float hitType)
    {
        musicPlayEvent.setParameterByName("PlayerHit", hitType);
    }

    //play player crown sfx to beat of music
    public void DoPlayerCrown()
    {
        musicPlayEvent.setParameterByName("PlayerHasCrown", 1);
        musicPlayEvent.setParameterByName("PlayerEnterHotZone", 0);
    }

    public void DoPlayerEnterHotZone()
    {
        musicPlayEvent.setParameterByName("PlayerEnterHotZone", 1);
    }

    void UpdateHotZoneVariable()
    {
        if(hotZoneIncreasing)
        {
            currentHotZoneTimer += 0.05f;
        }
        else
        {
            currentHotZoneTimer -= 0.05f;
        }



        if (currentHotZoneTimer >= 1f)
        {
            currentHotZoneTimer = 1f;
        }
        else if (currentHotZoneTimer <= 0f)
        {
            currentHotZoneTimer = 0f;
        }

        musicPlayEvent.setParameterByName("HotZoneTimer", currentHotZoneTimer);
    }



    public void DoHotZoneFadeOut()
    {
        hotZoneIncreasing = true;
    }



    public void DoHotZoneFadeIn()
    {
        float hotZoneValue = 0f;

        musicPlayEvent.getParameterByName("PlayerEnterHotZone", out hotZoneValue);

        if (hotZoneValue == 0f)
        {
            hotZoneIncreasing = false;
        }
    }



    public void DoPlayerExitHotZone()
    {
        musicPlayEvent.setParameterByName("PlayerMovedOutOfHotZone", 1);
        musicPlayEvent.setParameterByName("PlayerEnterHotZone", 0);
    }

    //Update FMOD Parameters
    void UpdateParameters()
    {
        musicPlayEvent.setParameterByName("Intensity", Intensity);
        musicPlayEvent.setParameterByName("P1Health", P1Health);
        musicPlayEvent.setParameterByName("P2Health", P2Health);
        musicPlayEvent.setParameterByName("SecondsRemaining", SecondsRemaining);

        UpdateHotZoneVariable();
    }



    /*void OnGUI()
    {
        Debug.LogWarning("COMMENT ALL OnGUI CODE OUT BEFORE BUILDING, THIS IS FOR TESTING BUT WILL SHOW IN BUILD");
        GUILayout.Box($"Current Beat = {timelineInfo.currentBeat}, Current Bar = {timelineInfo.currentBar}, Current Tempo = {timelineInfo.currentTempo}, Last Marker = {(string)timelineInfo.lastMarker}");
        GUILayout.Box($"Intensity = {Intensity}, P1 Health = {P1Health}, P2 Health = {P2Health}, SecondsRemaining = {SecondsRemaining}, HotZoneTimer = {currentHotZoneTimer}");
    }*/



    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT BeatEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr paremeterPtr)
    {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);

        IntPtr timelineInfoPtr;
        FMOD.RESULT result = instance.getUserData(out timelineInfoPtr);


        if(result != FMOD.RESULT.OK)
        {
            Debug.LogError("Timeline Callback error: " + result);
        }
        else if(timelineInfoPtr != IntPtr.Zero)
        {
            GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr);
            TimelineInfo timelineInfo = (TimelineInfo)timelineHandle.Target;

            switch(type)
            {
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
                    {
                        var parameter = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(paremeterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
                        timelineInfo.currentBeat = parameter.beat;
                        timelineInfo.currentBar = parameter.bar;
                        timelineInfo.currentTempo = parameter.tempo;
                    }
                    break;
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
                    {
                        var parameter = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(paremeterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
                        timelineInfo.lastMarker = parameter.name;
                    }
                    break;
            }
        }
        return FMOD.RESULT.OK;
    }
}
