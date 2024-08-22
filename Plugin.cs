using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BepInEx;
using GorillaNetworking;
using Photon.Pun;
using Photon.Voice.PUN;
using UnityEngine;
using UnityEngine.InputSystem;
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
        public float raycastDistance = 1f;
        private bool ui = false;
        public static string roomCode = "";
        private static string emptycodecheck = "Join Room";
        private bool thumbsup = true;
        private bool pushtotalk;
        private bool cooldown = false;
        private bool midfinger = true;
        private Vector3 prevpos;
        private bool boxing = true;
        private bool HandStatus = true;
        private float speed = 10f;
        private bool fc = false;
        private bool UiClick = true;
        private float rotationX = 0f;
        private float rotationY = 0f;
        private bool vrheadset;
        private bool keyPressed = false;
        bool toggled = true;
        bool inRoom;

        void Start()
        {
            Debug.LogWarning("checking for XR rendering activity");
            /* A lot of Gorilla Tag systems will not be set up when start is called
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
                Debug.Log("\r\n██╗░░██╗░█████╗░███╗░░██╗██████╗░\r\n██║░░██║██╔══██╗████╗░██║██╔══██╗\r\n███████║███████║██╔██╗██║██║░░██║\r\n██╔══██║██╔══██║██║╚████║██║░░██║\r\n██║░░██║██║░░██║██║░╚███║██████╔╝\r\n╚═╝░░╚═╝╚═╝░░╚═╝╚═╝░░╚══╝╚═════╝░");
                Debug.Log("\r\n░█████╗░░█████╗░███╗░░██╗████████╗██████╗░░█████╗░██╗░░░░░██╗░░░░░███████╗██████╗░\r\n██╔══██╗██╔══██╗████╗░██║╚══██╔══╝██╔══██╗██╔══██╗██║░░░░░██║░░░░░██╔════╝██╔══██╗\r\n██║░░╚═╝██║░░██║██╔██╗██║░░░██║░░░██████╔╝██║░░██║██║░░░░░██║░░░░░█████╗░░██████╔╝\r\n██║░░██╗██║░░██║██║╚████║░░░██║░░░██╔══██╗██║░░██║██║░░░░░██║░░░░░██╔══╝░░██╔══██╗\r\n╚█████╔╝╚█████╔╝██║░╚███║░░░██║░░░██║░░██║╚█████╔╝███████╗███████╗███████╗██║░░██║\r\n░╚════╝░░╚════╝░╚═╝░░╚══╝░░░╚═╝░░░╚═╝░░╚═╝░╚════╝░╚══════╝╚══════╝╚══════╝╚═╝░░╚═╝");
            }
        }

        void Update()
        {
            if (!vrheadset)
            {
                if (toggled == true)
                {
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
                    if (HandRFollower == null && HandLFollower == null)
                    {
                        return;
                    }
                    if (HandStatus)
                    {
                        if (inRoom != true)
                        {

                            GorillaTagger.Instance.thirdPersonCamera.SetActive(true);
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
                                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
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
                                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
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
                        if (fc)
                        {

                            if (Keyboard.current.wKey.isPressed) GorillaLocomotion.Player.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * speed * GorillaLocomotion.Player.Instance.scale;

                            if (Keyboard.current.sKey.isPressed) GorillaLocomotion.Player.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * -speed * GorillaLocomotion.Player.Instance.scale;

                            if (Keyboard.current.dKey.isPressed) GorillaLocomotion.Player.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.right * Time.deltaTime * speed * GorillaLocomotion.Player.Instance.scale;

                            if (Keyboard.current.aKey.isPressed) GorillaLocomotion.Player.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.right * Time.deltaTime * -speed * GorillaLocomotion.Player.Instance.scale;

                            if (Keyboard.current.spaceKey.isPressed) GorillaLocomotion.Player.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.up * Time.deltaTime * speed * GorillaLocomotion.Player.Instance.scale;

                            if (Keyboard.current.ctrlKey.isPressed) GorillaLocomotion.Player.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.up * Time.deltaTime * -speed * GorillaLocomotion.Player.Instance.scale;

                            if (Mouse.current.rightButton.isPressed)
                            {
                                float mouseX = Mouse.current.delta.x.ReadValue() * 50f * Time.deltaTime;
                                float mouseY = Mouse.current.delta.y.ReadValue() * 50f * Time.deltaTime;

                                rotationX -= mouseY;
                                rotationY += mouseX;

                                rotationX = Mathf.Clamp(rotationX, -90f, 90f);

                                GorillaTagger.Instance.mainCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
                            }
                            GorillaLocomotion.Player.Instance.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;

                            GorillaLocomotion.Player.Instance.transform.GetComponent<Rigidbody>().velocity = (GorillaTagger.Instance.offlineVRRig.transform.up * 0.073f) * GorillaLocomotion.Player.Instance.scale;

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

                }
            }
        }
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
                            LayerMask layerMask = LayerMask.GetMask(new string[]
                            {
                                "Gorilla Trigger",
                                "Zone",
                                "Gorilla Body"
                            });

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
                            if (CustomRaycast(ray, ref raycastHit, 5f, ~layerMask))
                            {
                                GorillaTagger.Instance.leftHandTriggerCollider.transform.position = raycastHit.point;

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

        void OnGUI()
        {
            if (!vrheadset)
            {
                if (ui)
                {
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
                            }

                        }
                        else
                        {
                            fc = false;
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
                    return;
                }
                else
                {
                    if (!ui)
                    {
                        GUI.Label(new Rect(0f, Screen.height - 20, 170, 20), "Press Tab to open UI");
                    }
                }
                return;
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
            prevpos = GorillaTagger.Instance.mainCamera.transform.position;
            cooldown = true;
                        GorillaTagger.Instance.mainCamera.transform.position = new Vector3(-66.706f, 11.8304f, -78.8302f);
            await Task.Delay(500);
            obeject.gameObject.SetActive(true);
            GorillaTagger.Instance.mainCamera.transform.position = prevpos;
            GorillaTagger.Instance.bodyCollider.transform.GetComponent<CapsuleCollider>().enabled = true;
            await Task.Delay(5000);
            cooldown = false;
        }
        private Color GetCyclingColor()
        {
            float t = Time.time;
            float r = Mathf.Sin(t * 2f) * 0.5f + 0.5f;
            float g = Mathf.Sin(t * 2f + Mathf.PI / 3f) * 0.5f + 0.5f;
            float b = Mathf.Sin(t * 2f + 2f * Mathf.PI / 3f) * 0.5f + 0.5f;
            return new Color(r, g, b);
        }
        /* This attribute tells Utilla to call this method when a modded room is joined */
        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            if (!vrheadset)
            {
                inRoom = true;
                GorillaTagger.Instance.thirdPersonCamera.SetActive(false);
                if (toggled == true)
                {
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
                inRoom = false;
                if (HandRFollower != null && HandLFollower != null && GorillaTagger.Instance.thirdPersonCamera != null)
                {
                    HandRFollower.SetActive(inRoom);
                    HandLFollower.SetActive(inRoom);
                    GorillaTagger.Instance.thirdPersonCamera.SetActive(true);


                }
            }
        }
    }
}

