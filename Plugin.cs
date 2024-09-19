using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using GorillaNetworking;
using Photon.Pun;
using Photon.Voice.PUN;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR;
using Utilla;

namespace GorillaTagModTemplateProject
{
    /// <summary>
    /// This is your mod's main class.
    /// </summary>

    /* This attribute tells Utilla to look for [ModdedGameJoin] and [ModdedGameLeave] */
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public GameObject HandRFollower;
        public GameObject HandLFollower;
        /* public Transform MetaR;
         public Transform MetaL; */ // adding for a future update to implement Meta Software VR hand controls.
        public float raycastDistance = 1.02f;
        private bool ui = false;
        public static string roomCode = "";
        int airLayer = LayerMask.NameToLayer("AirLayer");
        private static string emptycodecheck = "Join Room";
        private bool thumbsup = true;
        private float minwalldistance = 0.475f;
        private float maxwalldistance = 0.9f;
        private bool pushtotalk;
        private float speedMultiplier = 1f;
        private bool cooldown = false;
        private bool midfinger = true;
        public GameObject air;
        public Camera cubcam;
        public Camera cubmirrorcam;
        public GameObject cub;
        public string screenshotFilename = "HandCapture";
        private bool toggleair = false;
        private Vector3 prevpos;
        private bool boxing = true;
        private bool HandStatus = true;
        private float minspeed = 0f;
        private bool speedmode = true;
        private float maxspeed = 1000f;
        private bool f5 = false;
        private bool fc = false;
        private bool UiClick = true;
        private bool tp = false;
        private bool gravity = false;
        private float rotationX = 0f;
        private float rotationY = 0f;
        private bool vrheadset;
        public float sensitivity = 0.000100f;
        float scrollValue = Mouse.current.scroll.y.ReadValue();
        private bool Xray = false;
        private bool pausecolor = false;
        private bool keyPressed = false;
        private GUIStyle lblstyle;
        bool toggled = true;
        bool inRoom;

        void Start()
        {
            /* A lot of Gorilla Tag systems will not be set up when start is called /*
			/* Put code in OnGameInitialized to avoid null references */

            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        void OnEnable()
        {
            toggled = true;
            /* Set up your mod here */
            /* Code here runs at the start and whenever your mod is enabled */

            HarmonyPatches.ApplyHarmonyPatches();
        }

        void OnDisable()
        {

            toggled = false;
            /* Undo mod setup here */
            /* This provides support for toggling mods with ComputerInterface, please implement it :) */
            /* Code here runs whenever your mod is disabled (including if it disabled on startup) */

            HarmonyPatches.RemoveHarmonyPatches();
        }
        void OnGameInitialized(object sender, EventArgs e)
        {
            var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(xrDisplaySubsystems);

            if (xrDisplaySubsystems.Count > 0 && xrDisplaySubsystems[0].running)
            {
                vrheadset = true;
                Debug.LogWarning("[HandController] XR rendering activity found deactivating HandController.");
            }
            else
            {
                vrheadset = false;
                Debug.LogWarning("[HandController] XR rendering activity not found activating HandController");
            }
        }
        GameObject cylindercam;
        RenderTexture rendertexture;
        GameObject cubmirror;
        /* to do: meta software implemantion and change camera perspective to monitor view */
        void Update()
        {

            if (!vrheadset)
            {
                if (toggled)
                {
                    if (Keyboard.current.f5Key.wasPressedThisFrame)
                    {
                        if (f5)
                        {
                            f5 = false;
                            GorillaTagger.Instance.thirdPersonCamera.SetActive(f5);
                        }
                        else
                        {
                            f5 = true;
                            GorillaTagger.Instance.thirdPersonCamera.SetActive(f5);
                        }
                    }
                    if (Keyboard.current.tabKey.isPressed)
                    {
                        if (!keyPressed)
                        {
                            ui = !ui;
                        }
                        keyPressed = true;
                    }
                    else
                    {
                        keyPressed = false;
                    }
                    if (!inRoom && PhotonNetwork.InRoom)
                    {
                        NetworkSystem.Instance.ReturnToSinglePlayer();
                    }
                    if (cub == null && cylindercam == null && cubmirror == null)
                    {
                        print("CREATED");
                        cub = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cubmirror = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cylindercam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        Renderer renderer = cub.GetComponent<Renderer>();
                        Renderer renderer1 = cylindercam.GetComponent<Renderer>();
                        if (renderer == null && renderer1 == null && cubmirror.transform.GetComponent<Renderer>() == null)
                        {
                            renderer = cub.AddComponent<Renderer>();
                            renderer1 = cylindercam.AddComponent<Renderer>();
                            cubmirror.AddComponent<Renderer>();
                        }
                        renderer.material = new Material(Shader.Find("Sprites/Default"));
                        renderer1.material = new Material(Shader.Find("Sprites/Default"));
                        cubmirror.transform.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Texture"));
                        Color color = new Color(12f / 255f, 12f / 255f, 12f / 255f);
                        renderer.material.color = color;
                        renderer1.material.color = Color.white;
                        cub.GetComponent<Collider>().enabled = false;
                        cylindercam.GetComponent<Collider>().enabled = false;
                        cubmirror.GetComponent<Collider>().enabled = false;
                        cub.transform.localScale = new Vector3(0.075f, 0.075f, 0.075f);
                        cubmirror.transform.localScale = new Vector3(1.25f, 1.25f, 0f);

                        cylindercam.transform.localScale = new Vector3(0.2f, 0.05f, 0.2f);
                        cub.transform.name = "cubcameera";
                        cylindercam.transform.name = "cylinderforcam";
                        cub.transform.SetParent(GorillaTagger.Instance.leftHandTriggerCollider.transform, false);
                        cubmirror.transform.SetParent(cub.transform, false);
                        cylindercam.transform.SetParent(cub.transform, false);
                        cub.transform.localPosition = Vector3.zero;
                        cylindercam.transform.localPosition = Vector3.zero;
                        cubmirror.transform.localPosition = Vector3.zero;
                        cub.transform.localRotation = Quaternion.identity;
                        cylindercam.transform.localRotation = Quaternion.identity;
                        cubmirror.transform.localRotation = Quaternion.identity;

                        if (cubcam == null)
                        {
                            rendertexture = new RenderTexture(256, 256, 16);
                            cubmirrorcam = cubmirror.AddComponent<Camera>();
                            cubmirrorcam.targetTexture = rendertexture;
                            cubmirror.GetComponent<Renderer>().material.mainTexture = rendertexture;
                            cubcam = cub.AddComponent<Camera>();
                            cubcam.fieldOfView = 75f;
                            cubcam.nearClipPlane = 0.1f;
                            cubcam.enabled = false;
                            cubcam.tag = "Untagged";
                            cubcam.clearFlags = CameraClearFlags.Skybox;
                            cubcam.backgroundColor = Color.black;
                            cubcam.Render();
                        }
                    }
                    else
                    {
                        cylindercam.transform.rotation = cub.transform.rotation * Quaternion.Euler(90, 0, 0);
                        cylindercam.transform.position = cub.transform.position + cub.transform.forward * 0.04f;
                        cubmirror.transform.position = cub.transform.position + cub.transform.up * 0.10f;
                        cubmirror.transform.rotation = cub.transform.rotation;
                        cub.transform.position = GorillaTagger.Instance.leftHandTriggerCollider.transform.position;
                        cub.transform.rotation = GorillaTagger.Instance.leftHandTriggerCollider.transform.rotation * Quaternion.Euler(0f, 90f, -32f);

                        bool captureInputDetected = Keyboard.current.cKey.wasPressedThisFrame ||
                            (ControllerInputPoller.instance.leftControllerSecondaryButton && ControllerInputPoller.instance.rightControllerIndexFloat > 0.9f);

                        if (captureInputDetected && !hasCapturedThisFrame)
                        {
                            Capture();
                            hasCapturedThisFrame = true;
                        }

                        if (!captureInputDetected)
                        {
                            hasCapturedThisFrame = false;
                        }
                    }
                    if (HandRFollower == null && HandLFollower == null)
                    {
                        return;
                    }
                    if (HandStatus)
                    {
                        if (inRoom != true)
                        {

                            GorillaTagger.Instance.thirdPersonCamera.SetActive(f5);
                            HandRFollower.SetActive(inRoom);
                            HandLFollower.SetActive(inRoom);
                        }
                        else
                        {
                            HandRFollower.SetActive(inRoom);
                            HandLFollower.SetActive(inRoom);
                            if (UnityInput.Current.GetMouseButton(1))
                            {
                                Vector3 mousePosition = UnityInput.Current.mousePosition;
                                Ray ray;
                                bool activeInHierarchy = GorillaTagger.Instance.thirdPersonCamera.activeInHierarchy;
                                if (activeInHierarchy)
                                {
                                    ray = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
                                }
                                else
                                {
                                    ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                                }
                                RaycastHit hit;
                                if (Physics.Raycast(ray, out hit, raycastDistance))
                                {
                                    HandRFollower.transform.position = hit.point;
                                    GorillaTagger.Instance.rightHandTransform.position = HandRFollower.transform.position;
                                }
                            }
                            if (UnityInput.Current.GetMouseButton(0))
                            {
                                Vector3 mousePosition = UnityInput.Current.mousePosition;
                                Ray ray;
                                bool activeInHierarchy = GorillaTagger.Instance.thirdPersonCamera.activeInHierarchy;
                                if (activeInHierarchy)
                                {
                                    ray = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
                                }
                                else
                                {
                                    ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                                }
                                RaycastHit hit;
                                if (Physics.Raycast(ray, out hit, raycastDistance))
                                {
                                    HandLFollower.transform.position = hit.point;
                                    GorillaTagger.Instance.leftHandTransform.position = HandLFollower.transform.position;
                                }
                            }
                        }

                    }
                    else
                    {
                        if (HandLFollower != null && HandRFollower != null)
                        {
                            HandLFollower.SetActive(HandStatus);
                            HandRFollower.SetActive(HandStatus);
                        }
                    }
                    if (inRoom)
                    {
                        if (tp)
                        {
                            if (Mouse.current.leftButton.wasReleasedThisFrame)
                            {
                                Vector3 mousePosition = UnityInput.Current.mousePosition;
                                Ray ray;
                                bool activeInHierarchy = GorillaTagger.Instance.thirdPersonCamera.activeInHierarchy;
                                if (activeInHierarchy)
                                {
                                    ray = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
                                }
                                else
                                {
                                    ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                                }
                                RaycastHit hit;
                                if (Physics.Raycast(ray, out hit, 100f))
                                {
                                    GorillaLocomotion.Player.Instance.transform.position = hit.point + new Vector3(0f, 1f, 0f);
                                }
                            }
                        }
                        if (fc)
                        {


                            if (Keyboard.current.wKey.isPressed) GorillaLocomotion.Player.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * minspeed * GorillaLocomotion.Player.Instance.scale * speedMultiplier;

                            if (Keyboard.current.sKey.isPressed) GorillaLocomotion.Player.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * -minspeed * GorillaLocomotion.Player.Instance.scale * speedMultiplier;

                            if (Keyboard.current.dKey.isPressed) GorillaLocomotion.Player.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.right * Time.deltaTime * minspeed * GorillaLocomotion.Player.Instance.scale * speedMultiplier;

                            if (Keyboard.current.aKey.isPressed) GorillaLocomotion.Player.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.right * Time.deltaTime * -minspeed * GorillaLocomotion.Player.Instance.scale * speedMultiplier;

                            if (Keyboard.current.spaceKey.isPressed) GorillaLocomotion.Player.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.up * Time.deltaTime * minspeed * GorillaLocomotion.Player.Instance.scale * speedMultiplier;

                            if (Keyboard.current.ctrlKey.isPressed) GorillaLocomotion.Player.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.up * Time.deltaTime * -minspeed * GorillaLocomotion.Player.Instance.scale * speedMultiplier;

                            if (toggleair)
                            {
                                if (air == null)
                                {
                                    UnityEngine.Color custom = new UnityEngine.Color(215f / 255f, 206f / 255f, 255f / 255f);
                                    air = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    air.transform.rotation = GorillaTagger.Instance.mainCamera.transform.rotation;
                                    air.transform.localPosition = GorillaTagger.Instance.mainCamera.transform.position + GorillaTagger.Instance.mainCamera.transform.forward * minwalldistance;
                                    air.transform.localScale = new Vector3(200, 200, 0.05f);
                                    air.transform.GetComponent<Renderer>().material = new Material(Shader.Find("Sprites/Default")); //debug
                                    air.transform.GetComponent<BoxCollider>().enabled = true;
                                    air.layer = airLayer;
                                    if (air.transform.GetComponent<Renderer>().enabled)
                                    {
                                        air.transform.GetComponent<Renderer>().enabled = false;
                                    }
                                    air.transform.name = "airwall";
                                    air.transform.GetComponent<Renderer>().material.color = custom; //debug
                                    toggleair = true;

                                }
                                if (air != null & toggleair)
                                {
                                    if (Keyboard.current.f5Key.wasPressedThisFrame && Keyboard.current.f6Key.wasPressedThisFrame)
                                    {
                                        if (!air.transform.GetComponent<Renderer>().enabled)
                                        {
                                            air.transform.GetComponent<Renderer>().enabled = true;
                                        }
                                        else
                                        {
                                            air.transform.GetComponent<Renderer>().enabled = false;
                                        }
                                    }
                                    air.transform.rotation = GorillaTagger.Instance.mainCamera.transform.rotation;
                                    air.transform.localPosition = GorillaTagger.Instance.mainCamera.transform.position + GorillaTagger.Instance.mainCamera.transform.forward * minwalldistance;
                                }
                            }
                            else
                            {
                                if (air != null || toggleair)
                                {
                                    GameObject.Destroy(air);
                                    toggleair = false;
                                    speedmode = true;
                                    print("desotryed");
                                }
                            }

                            if (Keyboard.current.shiftKey.isPressed)
                            {
                                speedMultiplier = 3.5f;
                            }
                            else
                            {
                                speedMultiplier = 1f;
                            }
                            if (Mouse.current.rightButton.isPressed)
                            {
                                float mouseX = Mouse.current.delta.x.ReadValue() * 50f * Time.deltaTime;
                                float mouseY = Mouse.current.delta.y.ReadValue() * 50f * Time.deltaTime;

                                rotationX -= mouseY;
                                rotationY += mouseX;

                                rotationX = Mathf.Clamp(rotationX, -90f, 90f);

                                GorillaTagger.Instance.offlineVRRig.headConstraint.transform.localEulerAngles = new Vector3(rotationX, 0, 0);
                                GorillaTagger.Instance.mainCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
                            }
                            if (!gravity)
                            {
                                GorillaLocomotion.Player.Instance.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;

                                GorillaLocomotion.Player.Instance.transform.GetComponent<Rigidbody>().velocity = (GorillaTagger.Instance.offlineVRRig.transform.up * 0.0715f) * GorillaLocomotion.Player.Instance.scale;
                            }
                            if (speedmode)
                            {
                                float scrollValue = Mouse.current.scroll.y.ReadValue();

                                if (Mouse.current.scroll.y.ReadValue() > 0f)
                                {
                                    minspeed++;
                                }
                                else if (Mouse.current.scroll.y.ReadValue() < 0f)
                                {
                                    minspeed--;
                                }

                                minspeed = Mathf.Clamp(minspeed, 0f, maxspeed);
                                if (minspeed < 0f)
                                {
                                    minspeed = 0f;
                                }

                            }
                            else
                            {
                                if (Mouse.current != null)
                                {
                                    float scrollValue = Mouse.current.scroll.y.ReadValue();
                                    float scaledScrollValue = scrollValue * sensitivity;
                                    minwalldistance += scaledScrollValue;
                                    minwalldistance = Mathf.Clamp(minwalldistance, 0.475f, maxwalldistance);
                                }
                            }
                        }
                        else
                        {
                            if (air != null)
                            {
                                print("desotryed");
                                GameObject.Destroy(air);
                            }
                            if (toggleair)
                            {
                                print("turned off");
                                toggleair = false;
                            }
                        }
                        if (HandStatus)
                        {
                            if (Keyboard.current.lKey.wasPressedThisFrame)
                            {
                                if (thumbsup)
                                {
                                    midfinger = true;
                                    thumbsup = false;
                                    boxing = true;
                                    FingerPatch.forceLeftGrip = true;
                                    FingerPatch.forceLeftTrigger = true;
                                    FingerPatch.forceRightGrip = true;
                                    FingerPatch.forceRightTrigger = true;
                                    if (pushtotalk)
                                    {
                                        FingerPatch.forceLeftPrimary = true;
                                        FingerPatch.forceRightPrimary = true;
                                    }
                                    else
                                    {
                                        FingerPatch.forceLeftPrimary = false;
                                        FingerPatch.forceRightPrimary = false;
                                    }
                                }
                                else
                                {
                                    boxing = true;
                                    thumbsup = true;
                                    midfinger = true;
                                    FingerPatch.forceLeftGrip = false;
                                    FingerPatch.forceLeftTrigger = false;
                                    FingerPatch.forceRightGrip = false;
                                    FingerPatch.forceRightTrigger = false;
                                    if (pushtotalk)
                                    {
                                        FingerPatch.forceLeftPrimary = true;
                                        FingerPatch.forceRightPrimary = true;
                                    }
                                    else
                                    {
                                        FingerPatch.forceLeftPrimary = false;
                                        FingerPatch.forceRightPrimary = false;
                                    }
                                }
                            }
                            if (Keyboard.current.kKey.wasPressedThisFrame)
                            {
                                if (midfinger)
                                {
                                    boxing = true;
                                    thumbsup = true;
                                    midfinger = false;
                                    FingerPatch.forceLeftGrip = false;
                                    FingerPatch.forceLeftTrigger = true;
                                    FingerPatch.forceRightGrip = false;
                                    FingerPatch.forceRightTrigger = true;
                                    if (pushtotalk)
                                    {
                                        FingerPatch.forceLeftPrimary = true;
                                        FingerPatch.forceRightPrimary = true;
                                    }
                                    else
                                    {
                                        FingerPatch.forceLeftPrimary = false;
                                        FingerPatch.forceRightPrimary = false;
                                    }
                                }
                                else
                                {
                                    boxing = true;
                                    thumbsup = true;
                                    midfinger = true;
                                    FingerPatch.forceLeftGrip = false;
                                    FingerPatch.forceLeftTrigger = false;
                                    FingerPatch.forceRightGrip = false;
                                    FingerPatch.forceRightTrigger = false;
                                    if (pushtotalk)
                                    {
                                        FingerPatch.forceLeftPrimary = true;
                                        FingerPatch.forceRightPrimary = true;
                                    }
                                    else
                                    {
                                        FingerPatch.forceLeftPrimary = false;
                                        FingerPatch.forceRightPrimary = false;
                                    }
                                }
                            }
                            if (Keyboard.current.jKey.wasPressedThisFrame)
                            {
                                if (boxing)
                                {
                                    thumbsup = true; midfinger = true;
                                    boxing = false;
                                    FingerPatch.forceLeftGrip = true;
                                    FingerPatch.forceLeftTrigger = true;
                                    FingerPatch.forceRightGrip = true;
                                    FingerPatch.forceRightTrigger = true;
                                    FingerPatch.forceLeftPrimary = true;
                                    FingerPatch.forceRightPrimary = true;
                                }
                                else
                                {
                                    thumbsup = true; midfinger = true;
                                    boxing = true;
                                    FingerPatch.forceLeftGrip = false;
                                    FingerPatch.forceLeftTrigger = false;
                                    FingerPatch.forceRightGrip = false;
                                    FingerPatch.forceRightTrigger = false;
                                    if (pushtotalk)
                                    {
                                        FingerPatch.forceLeftPrimary = true;
                                        FingerPatch.forceRightPrimary = true;
                                    }
                                    else
                                    {
                                        FingerPatch.forceLeftPrimary = false;
                                        FingerPatch.forceRightPrimary = false;
                                    }
                                }
                            }
                            if (Keyboard.current.pKey.wasPressedThisFrame)
                            {
                                if (!pushtotalk && PhotonVoiceNetwork.Instance.PrimaryRecorder.TransmitEnabled == false)
                                {
                                    pushtotalk = true;
                                    FingerPatch.forceLeftPrimary = true;
                                    FingerPatch.forceRightPrimary = true;
                                }
                            }
                            if (Keyboard.current.pKey.wasReleasedThisFrame && PhotonVoiceNetwork.Instance.PrimaryRecorder.TransmitEnabled == true)
                            {
                                if (pushtotalk)
                                {
                                    pushtotalk = false;
                                    FingerPatch.forceLeftPrimary = false;
                                    FingerPatch.forceRightPrimary = false;
                                }
                            }
                        }
                        else
                        {
                            thumbsup = false; midfinger = false;
                            boxing = true;
                            FingerPatch.forceLeftGrip = false;
                            FingerPatch.forceLeftTrigger = false;
                            FingerPatch.forceRightGrip = false;
                            FingerPatch.forceRightTrigger = false;
                            if (Keyboard.current.pKey.wasPressedThisFrame)
                            {
                                if (!pushtotalk && PhotonVoiceNetwork.Instance.PrimaryRecorder.TransmitEnabled == false)
                                {
                                    pushtotalk = true;
                                    FingerPatch.forceLeftPrimary = true;
                                    FingerPatch.forceRightPrimary = true;
                                }
                            }
                            if (Keyboard.current.pKey.wasReleasedThisFrame && PhotonVoiceNetwork.Instance.PrimaryRecorder.TransmitEnabled == true)
                            {
                                if (pushtotalk)
                                {
                                    pushtotalk = false;
                                    FingerPatch.forceLeftPrimary = false;
                                    FingerPatch.forceRightPrimary = false;
                                }
                            }
                        }
                        if (Xray && PhotonNetwork.InRoom)
                        {
                            Transform main = GameObject.Find("Environment Objects/LocalObjects_Prefab/").transform;
                            Transform main2 = main.Find("Forest").transform;
                            Transform treeroom = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/").transform;
                            MeshRenderer[] visibilitycomponents = main2.GetComponentsInChildren<MeshRenderer>();
                            MeshRenderer[] treeroomcompo = treeroom.GetComponentsInChildren<MeshRenderer>();
                            foreach (MeshRenderer visible in visibilitycomponents)
                            {
                                visible.enabled = false;
                            }
                            foreach (MeshRenderer trvis in treeroomcompo)
                            {
                                trvis.enabled = false;
                            }
                        }
                        else
                        {
                            Xray = false;
                            Transform main = GameObject.Find("Environment Objects/LocalObjects_Prefab/").transform;
                            Transform main2 = main.Find("Forest").transform;
                            Transform treeroom = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/").transform;
                            MeshRenderer[] visibilitycomponents = main2.GetComponentsInChildren<MeshRenderer>();
                            treeroom.Find("sky jungle entrance 2/").GetComponent<MeshRenderer>().SafeDestroy();
                            MeshRenderer[] treeroomcompo = treeroom.GetComponentsInChildren<MeshRenderer>();
                            foreach (MeshRenderer visible in visibilitycomponents)
                            {
                                visible.enabled = true;
                            }
                            foreach (MeshRenderer trvis in treeroomcompo)
                            {
                                trvis.enabled = true;
                            }
                        }
                    }
                    else
                    {
                        tp = false;
                        fc = false;
                        if (!inRoom && PhotonNetwork.InRoom)
                        {
                            gravity = true;
                        }
                        GorillaTagger.Instance.mainCamera.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                }
                else
                {
                    if (HandLFollower != null && HandRFollower != null)
                    {
                        HandLFollower.SetActive(toggled);
                        HandRFollower.SetActive(toggled);
                    }
                    else
                    {
                        return;
                    }
                    tp = false;
                    Xray = false;
                    midfinger = true;
                    boxing = false;
                    thumbsup = true;
                    fc = false;
                    HandStatus = false;
                    ui = false;
                    gravity = true;

                }
            }
            else if (vrheadset)
            {
                if (toggled)
                {
                    if (cub == null && cylindercam == null && cubmirror == null)
                    {
                        print("CREATED");
                        cub = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cubmirror = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cylindercam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        Renderer renderer = cub.GetComponent<Renderer>();
                        Renderer renderer1 = cylindercam.GetComponent<Renderer>();
                        if (renderer == null && renderer1 == null && cubmirror.transform.GetComponent<Renderer>() == null)
                        {
                            renderer = cub.AddComponent<Renderer>();
                            renderer1 = cylindercam.AddComponent<Renderer>();
                            cubmirror.AddComponent<Renderer>();
                        }
                        renderer.material = new Material(Shader.Find("Sprites/Default"));
                        renderer1.material = new Material(Shader.Find("Sprites/Default"));
                        cubmirror.transform.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Texture"));
                        Color color = new Color(12f / 255f, 12f / 255f, 12f / 255f);
                        renderer.material.color = color;
                        renderer1.material.color = Color.white;
                        cub.GetComponent<Collider>().enabled = false;
                        cylindercam.GetComponent<Collider>().enabled = false;
                        cubmirror.GetComponent<Collider>().enabled = false;
                        cub.transform.localScale = new Vector3(0.075f, 0.075f, 0.075f);
                        cubmirror.transform.localScale = new Vector3(1.5f, 1.5f, 0f);

                        cylindercam.transform.localScale = new Vector3(0.2f, 0.05f, 0.2f);
                        cub.transform.name = "cubcameera";
                        cylindercam.transform.name = "cylinderforcam";
                        cub.transform.SetParent(GorillaTagger.Instance.leftHandTriggerCollider.transform, false);
                        cubmirror.transform.SetParent(cub.transform, false);
                        cylindercam.transform.SetParent(cub.transform, false);
                        cub.transform.localPosition = Vector3.zero;
                        cylindercam.transform.localPosition = Vector3.zero;
                        cubmirror.transform.localPosition = Vector3.zero;
                        cub.transform.localRotation = Quaternion.identity;
                        cylindercam.transform.localRotation = Quaternion.identity;
                        cubmirror.transform.localRotation = Quaternion.identity;

                        if (cubcam == null)
                        {
                            rendertexture = new RenderTexture(256, 256, 16);
                            cubmirrorcam = cubmirror.AddComponent<Camera>();
                            cubmirrorcam.targetTexture = rendertexture;
                            cubmirror.GetComponent<Renderer>().material.mainTexture = rendertexture;
                            cubcam = cub.AddComponent<Camera>();
                            cubcam.fieldOfView = 70f;
                            cubcam.enabled = false;
                            cubcam.tag = "Untagged";
                            cubcam.clearFlags = CameraClearFlags.Skybox;
                            cubcam.backgroundColor = Color.black;
                            cubcam.Render();
                        }
                    }
                    else
                    {
                        cylindercam.transform.rotation = cub.transform.rotation * Quaternion.Euler(90, 0, 0);
                        cylindercam.transform.position = cub.transform.position + cub.transform.forward * 0.04f;
                        cubmirror.transform.position = cub.transform.position + cub.transform.up * 0.10f;
                        cubmirror.transform.rotation = cub.transform.rotation;
                        cub.transform.position = GorillaTagger.Instance.leftHandTriggerCollider.transform.position;
                        cub.transform.rotation = GorillaTagger.Instance.leftHandTriggerCollider.transform.rotation * Quaternion.Euler(0f, 90f, -32f);

                        bool captureInputDetected = Keyboard.current.cKey.wasPressedThisFrame ||
                            (ControllerInputPoller.instance.leftControllerSecondaryButton && ControllerInputPoller.instance.rightControllerIndexFloat > 0.9f);

                        if (captureInputDetected && !hasCapturedThisFrame)
                        {
                            Capture();
                            hasCapturedThisFrame = true;
                        }

                        if (!captureInputDetected)
                        {
                            hasCapturedThisFrame = false;
                        }
                    }
                }
            }
        }
        private bool hasCapturedThisFrame = false;
        private void FixedUpdate()
        {
            if (!vrheadset)
            {
                if (inRoom)
                {

                    if (UiClick)
                    {
                        if (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
                        {

                            bool activeInHierarchy = GorillaTagger.Instance.thirdPersonCamera.activeInHierarchy;
                            Ray ray;
                            if (activeInHierarchy)
                            {
                                ray = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
                            }
                            else
                            {
                                ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                            }

                            RaycastHit raycastHit = new RaycastHit();

                            if (air != null)
                            {
                                if (CustomRaycast(ray, ref raycastHit, 5f, ~(1 << air.layer)))
                                {
                                    VRMap hand = GorillaTagger.Instance.offlineVRRig.leftHand;
                                    GorillaTagger.Instance.leftHandTriggerCollider.transform.position = raycastHit.point;
                                }
                            }
                            else
                            {
                                LayerMask layerMask = LayerMask.GetMask(new string[]
                                {
                                "Gorilla Trigger",
                                "Zone",
                                "Gorilla Body"
                                });
                                if (CustomRaycast(ray, ref raycastHit, 5f, ~layerMask))
                                {
                                    VRMap hand = GorillaTagger.Instance.offlineVRRig.leftHand;
                                    GorillaTagger.Instance.leftHandTriggerCollider.transform.position = raycastHit.point;
                                }
                            }
                        }
                    }
                }
            }
        }
        private bool CustomRaycast(Ray ray, ref RaycastHit hitInfo, float maxDistance, int layerMask)
        {
            bool hit = Physics.Raycast(ray, out RaycastHit tempHitInfo, maxDistance, layerMask);

            if (hit)
            {
                // Assign the out parameter value to the ref parameter
                hitInfo = tempHitInfo;
            }

            return hit;
        }
        private async Task Rejoin()
        {
            string code = NetworkSystem.Instance.RoomName;

            if (NetworkSystem.Instance.InRoom)
            {
                code = NetworkSystem.Instance.RoomName;
                NetworkSystem.Instance.ReturnToSinglePlayer();
            }


            await Task.Delay(3000);
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(code, GorillaNetworking.JoinType.Solo);
        }
        private async Task Generate()
        {
            string code = null;

            if (NetworkSystem.Instance.InRoom)
            {
                NetworkSystem.Instance.ReturnToSinglePlayer();
            }

            await Task.Delay(1000);
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(code, GorillaNetworking.JoinType.Solo);
        }
        public Font font;
        GUIStyle boxstyle;
        private Texture2D gradientTexture;
        Texture2D MakeGradientTex(int width, int height, Color startColor, Color endColor)
        {
            Texture2D tex = new Texture2D(width, height);
            Color[] pix = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                Color gradientColor = Color.Lerp(startColor, endColor, t);

                for (int x = 0; x < width; x++)
                {
                    pix[y * width + x] = gradientColor;
                }
            }

            tex.SetPixels(pix);
            tex.Apply();

            return tex;
        }
        void InitializeBoxStyle()
        {
            boxstyle = new GUIStyle(GUI.skin.box);
            gradientTexture = MakeGradientTex(160, 290, new Color(0, 0, 0, 0), GetCyclingColor());
            boxstyle.normal.background = gradientTexture;
        }

        void UpdateBoxTexture()
        {
            if (gradientTexture != null)
            {
                DestroyImmediate(gradientTexture);
            }
            if (!pausecolor)
            {
                gradientTexture = MakeGradientTex(160, 290, new Color(0, 0, 0, 0), GetCyclingColor());
                boxstyle.normal.background = gradientTexture;
            }
        }
        void Capture()
        {
            if (cubcam == null)
            {
                Debug.LogError("No camera assigned for taking screenshots!");
                return;
            }
            cubcam.enabled = true;

            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
            cubcam.targetTexture = rt;
            RenderTexture.active = rt;

            cubcam.Render();

            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            cubcam.targetTexture = null;
            RenderTexture.active = null;

            byte[] bytes = screenshot.EncodeToPNG();
            string gamePath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"));
            string bepInExPath = Path.Combine(gamePath, "BepInEx");
            string filePath = Path.Combine(bepInExPath, $"{screenshotFilename}_{System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png");
            System.IO.File.WriteAllBytes(filePath, bytes);

            Debug.Log($"Screenshot saved to: {filePath}");
           /* if (vrheadset)
            {
                cubcam.enabled = true;
            }
            else
            {
                cubcam.enabled = false;
            } */
            cubcam.enabled = false;
        }
        void OnGUI()
        {
            if (!vrheadset)
            {
                if (ui)
                {
                    if (toggled)
                    {
                        if (inRoom)
                        {

                            if (lblstyle == null)
                            {
                                lblstyle = new GUIStyle(GUI.skin.label);
                                lblstyle.fontSize = 20;
                                lblstyle.fontStyle = FontStyle.Bold;
                            }
                            if (boxstyle == null)
                            {
                                boxstyle = new GUIStyle(GUI.skin.box);
                                InitializeBoxStyle();
                            }
                            else
                            {
                                if (!pausecolor)
                                {
                                    UpdateBoxTexture();
                                }
                            }
                            GUI.Box(new Rect(1760, 0, 160, 290), "", style: boxstyle);
                            GUILayout.BeginArea(new Rect(1780, 10, 120, 450));
                            GUILayout.Label("Scoreboard", style: lblstyle);
                            if (PhotonNetwork.InRoom)
                            {
                                foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
                                {
                                    GUILayout.Label(player.NickName);
                                }
                            }
                            GUILayout.EndArea();
                        }
                    }
                    float buttonWidth = 180f;
                    float buttonHeight = 30f;
                    float buttonX = 30f;
                    float buttonY = Screen.height - 210f;

                    Color originalColor = GUI.color;
                    GUI.color = GetCyclingColor();

                    GUI.Label(new Rect(215f, Screen.height - 170f, buttonWidth, buttonHeight), "Code to join");
                    // GUI.Label(new Rect(485f, Screen.height - 170f, buttonWidth, buttonHeight), "Change your name");
                    /* Username = GUI.TextArea(new Rect(300f, Screen.height - 170f, buttonWidth, buttonHeight), Username, maxLength: 12);
                     if (GUI.Button(new Rect(300f, Screen.height - 200f, buttonWidth, buttonHeight), "Change Name"))
                     {
                         PhotonNetwork.LocalPlayer.NickName = Username;
                     }*/
                    if (toggled)
                    {
                        if (inRoom)
                        {
                            if (HandStatus)
                            {
                                if (GUI.Button(new Rect(300f, Screen.height - 170f, buttonWidth, buttonHeight), "Turn HandMovement off"))
                                {
                                    HandStatus = false;
                                }
                            }
                            else
                            {
                                if (GUI.Button(new Rect(300f, Screen.height - 170f, buttonWidth, buttonHeight), "Turn HandMovement on"))
                                {
                                    HandStatus = true;
                                }
                            }
                            tp = GUI.Toggle(new Rect(480f, Screen.height - 210f, 120f, 15f), tp, "TP(modded)");
                            if (!fc)
                            {
                                if (GUI.Button(new Rect(300f, Screen.height - 230f, buttonWidth, buttonHeight), "Turn Freecam on"))
                                {
                                    fc = true;
                                }
                            }
                            else
                            {
                                if (GUI.Button(new Rect(300f, Screen.height - 230f, buttonWidth, buttonHeight), "Turn Freecam off"))
                                {
                                    fc = false;
                                }
                                gravity = GUI.Toggle(new Rect(480f, Screen.height - 190f, 90f, 20f), gravity, "Gravity");
                            }
                            Xray = GUI.Toggle(new Rect(480f, Screen.height - 230f, 120f, 20f), Xray, "Xray(modded)");
                        }
                        if (!inRoom)
                        {
                            pausecolor = GUI.Toggle(new Rect(480f, Screen.height - 190f, 150f, 20f), pausecolor, "Stop Color Cycle");
                        }
                        else
                        {
                            if (fc)
                            {
                                pausecolor = GUI.Toggle(new Rect(480f, Screen.height - 250f, 150f, 20f), pausecolor, "Stop Color Cycle");
                            }
                            else
                            {
                                pausecolor = GUI.Toggle(new Rect(480f, Screen.height - 190f, 150f, 20f), pausecolor, "Stop Color Cycle");
                            }
                        }
                        if (!fc)
                        {
                            if (inRoom)
                            {
                                UiClick = GUI.Toggle(new Rect(300f, Screen.height - 260f, 120f, 30), UiClick, "Toggle UI Click");
                            }
                            else
                            {

                                UiClick = GUI.Toggle(new Rect(300f, Screen.height - 230f, 120f, 30), UiClick, "Toggle UI Click");
                            }
                        }
                        else
                        {
                            if (speedmode)
                            {
                                if (fc)
                                {
                                    if (toggleair)
                                    {
                                        if (GUI.Button(new Rect(1650f, Screen.height - 80f, 250, buttonHeight), "Airwall distance mode"))
                                        {
                                            speedmode = false;
                                        }
                                    }
                                }
                            }
                            else
                            {

                                if (GUI.Button(new Rect(1650f, Screen.height - 80f, 250, buttonHeight), "Flying speed mode"))
                                {
                                    speedmode = true;
                                }
                            }
                            toggleair = GUI.Toggle(new Rect(480f, Screen.height - 170, 150f, 20f), toggleair, "Air HandMovement");
                            UiClick = GUI.Toggle(new Rect(300f, Screen.height - 260f, 150f, 30), UiClick, "Toggle UI Click");
                        }
                    }
                    if (GorillaComputer.instance.currentQueue == "DEFAULT")
                    {
                        if (GUI.Button(new Rect(300f, Screen.height - 200f, buttonWidth, buttonHeight), "Go Comp"))
                        {
                            GorillaComputer.instance.currentQueue = "COMPETITIVE";
                        }
                    }
                    else
                    {
                        if (GUI.Button(new Rect(300f, Screen.height - 200f, buttonWidth, buttonHeight), "Go Default"))
                        {
                            GorillaComputer.instance.currentQueue = "DEFAULT";
                        }
                    }
                    if (GorillaComputer.instance.currentGameMode.Value != "MODDED_CASUAL")
                    {
                        if (inRoom)
                        {
                            if (GUI.Button(new Rect(480f, Screen.height - 170f, buttonWidth, buttonHeight), "Set mode to Modded casual"))
                            {
                                GorillaComputer.instance.currentGameMode.Value = "MODDED_CASUAL";
                            }
                        }
                        else
                        {
                            if (GUI.Button(new Rect(300f, Screen.height - 170f, buttonWidth, buttonHeight), "Set mode to Modded casual"))
                            {
                                GorillaComputer.instance.currentGameMode.Value = "MODDED_CASUAL";
                            }
                        }

                    }
                    if (!PhotonNetwork.InRoom && !cooldown)
                    {
                        if (GorillaComputer.instance.currentGameMode.Value != "MODDED_CASUAL")
                        {
                            if (GUI.Button(new Rect(480f, Screen.height - 200f, buttonWidth, buttonHeight), "Join Random"))
                            {
                                _ = joinrandom();

                            }
                        }
                        else
                        {
                            if (GUI.Button(new Rect(300f, Screen.height - 170f, buttonWidth, buttonHeight), "Join Random"))
                            {
                                _ = joinrandom();
                            }
                        }
                    }
                    if (NetworkSystem.Instance.InRoom)
                    {
                        if (GUI.Button(new Rect(buttonX, Screen.height - 260f, buttonWidth, buttonHeight), "Rejoin"))
                        {
                            _ = Rejoin();
                        }
                    }
                    if (GUI.Button(new Rect(buttonX, Screen.height - 230f, buttonWidth, buttonHeight), "Generate Room"))
                    {
                        _ = Generate();
                    }
                    roomCode = GUI.TextArea(new Rect(buttonX, Screen.height - 170f, buttonWidth, buttonHeight), roomCode, 10);
                    if (GUI.Button(new Rect(buttonX, Screen.height - 200f, buttonWidth, buttonHeight), emptycodecheck))
                    {
                        if (roomCode != "")
                        {
                            emptycodecheck = "Join Room";
                            if (PhotonNetwork.InRoom)
                            {
                                NetworkSystem.Instance.ReturnToSinglePlayer();
                            }
                            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomCode, GorillaNetworking.JoinType.Solo);
                        }
                        else
                        {
                            emptycodecheck = "empty room!";
                            _ = wait3000();
                        }

                    }
                    if (PhotonNetwork.InRoom)
                    {
                        if (GUI.Button(new Rect(buttonX, Screen.height - 290f, buttonWidth, buttonHeight), "Leave Room"))
                        {
                            NetworkSystem.Instance.ReturnToSinglePlayer();
                        }

                    }

                    if (PhotonNetwork.CurrentRoom != null)
                    {
                        GUI.Label(new Rect(buttonX, Screen.height - 140, 170, 20), "Room: " + PhotonNetwork.CurrentRoom.Name);
                        GUI.Label(new Rect(buttonX, Screen.height - 120, 170, 20), "Player count: " + PhotonNetwork.CurrentRoom.PlayerCount);
                    }
                    GUI.Label(new Rect(buttonX, Screen.height - 100, 300, 20), "Live regional player count: " + PhotonNetwork.CountOfPlayers);
                    GUI.Label(new Rect(buttonX, Screen.height - 80, 300, 20), "Live regional Room count: " + PhotonNetwork.CountOfRooms);
                }
                else
                {
                    if (!ui)
                    {
                        GUI.Label(new Rect(0f, Screen.height - 20, 170, 20), "Press Tab to open UI");
                    }
                }
            }
        }
        private async Task wait3000()
        {
            await Task.Delay(3000);
            emptycodecheck = "Join Room";
        }
        private async Task joinrandom()
        {
            Transform root = GameObject.Find("Environment Objects").transform;
            Transform obeject = root.Find("LocalObjects_Prefab").transform;
            obeject.gameObject.SetActive(false);
            GorillaComputer.instance.currentGameMode.Value = "MODDED_CASUAL";
            GorillaTagger.Instance.bodyCollider.transform.GetComponent<CapsuleCollider>().enabled = false;
            prevpos = GorillaLocomotion.Player.Instance.transform.position;
            cooldown = true;
            GorillaLocomotion.Player.Instance.transform.position = new Vector3(-66.706f, 11.8304f, -78.8302f);
            await Task.Delay(500);
            obeject.gameObject.SetActive(true);
            GorillaLocomotion.Player.Instance.transform.position = prevpos;
            GorillaTagger.Instance.bodyCollider.transform.GetComponent<CapsuleCollider>().enabled = true;
            await Task.Delay(5000);
            cooldown = false;
        }
        private Color currentColor = Color.white;
        private Color GetCyclingColor()
        {
            if (!pausecolor)
            {
                float t = Time.time;
                float r = Mathf.Sin(t * 2f) * 0.5f + 0.5f;
                float g = Mathf.Sin(t * 2f + Mathf.PI / 3f) * 0.5f + 0.5f;
                float b = Mathf.Sin(t * 2f + 2f * Mathf.PI / 3f) * 0.5f + 0.5f;
                currentColor = new Color(r, g, b);
            }
            return currentColor;
        }
        /* This attribute tells Utilla to call this method when a modded room is joined */
        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            if (!vrheadset)
            {
                if (toggled)
                {
                    if (minspeed == 0)
                    {
                        minspeed = 10f;
                    }
                    inRoom = true;
                    GorillaTagger.Instance.thirdPersonCamera.SetActive(f5);
                    if (HandRFollower)
                    {
                        HandRFollower.SetActive(inRoom);
                    }
                    else
                    {
                        HandRFollower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        HandRFollower.name = "HandRayTracker";
                        if (HandRFollower != null && GorillaTagger.Instance != null)
                        {
                            HandRFollower.transform.SetParent(GorillaTagger.Instance.rightHandTransform, false);
                        }
                        HandRFollower.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);

                        if (HandRFollower.GetComponent<SphereCollider>().enabled == true && HandRFollower.GetComponent<MeshRenderer>().enabled == true)
                        {
                            HandRFollower.GetComponent<SphereCollider>().enabled = false;
                            HandRFollower.GetComponent<MeshRenderer>().enabled = false;
                        }
                        if (HandLFollower)
                        {
                            HandLFollower.SetActive(inRoom);
                        }
                        else
                        {
                            HandLFollower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            HandLFollower.name = "HandRayLTracker";
                            if (HandLFollower != null && GorillaTagger.Instance != null)
                            {
                                HandLFollower.transform.SetParent(GorillaTagger.Instance.leftHandTransform, false);
                            }
                            HandLFollower.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);

                            if (HandLFollower.GetComponent<SphereCollider>().enabled == true && HandLFollower.GetComponent<MeshRenderer>().enabled == true)
                            {
                                HandLFollower.GetComponent<SphereCollider>().enabled = false;
                                HandLFollower.GetComponent<MeshRenderer>().enabled = false;
                            }
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }

        /* This attribute tells Utilla to call this method when a modded room is left */
        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            if (!vrheadset)
            {
                if (toggled)
                {
                    inRoom = false;
                    if (HandRFollower != null && HandLFollower != null && GorillaTagger.Instance.thirdPersonCamera != null)
                    {
                        HandRFollower.SetActive(inRoom);
                        HandLFollower.SetActive(inRoom);
                        GorillaTagger.Instance.thirdPersonCamera.SetActive(f5);


                    }
                }
            }
        }
    }
}

