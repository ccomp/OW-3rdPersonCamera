using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace _3rdPersonCamera
{
    public class MainClass : ModBehaviour
    {
        private bool _isStarted;

        private bool _movingFlightCameraOut;
        private bool _movingFlightCameraIn;

        private bool _movingShipCameraOut;
        private bool _movingShipCameraIn;

        private float startTime1;
        private float startTime2;
        private float startTime3;
        private float startTime4;

        private float flyDuration = 1f;
        private float shipDuration = 2f;

        // +x = 
        // +y = up
        // +z = forward

        private static readonly float groundX = 0f;
        private static readonly float groundY = 0.8f;
        private static readonly float groundZ = -5f;
        private Vector3 _groundPos = new Vector3(groundX, groundY, groundZ);

        private static readonly float shipX = 0f;
        private static readonly float shipY = 0.8f;
        private static readonly float shipZ = -2f;
        private Vector3 _shipPos = new Vector3(shipX, shipY, shipZ);

        private static float flyX = 0f;
        private static float flyY = 6f;
        private static float flyZ = -25f;
        private Vector3 _flyPos = new Vector3(flyX, flyY, flyZ);

        public static GameObject cameraHolder;

        public static GameObject interactObject;

        private void Start()
        {
            base.ModHelper.Console.WriteLine("[3rdPerson] :");
            base.ModHelper.Events.Subscribe<Flashlight>(Events.AfterStart);
            IModEvents events = base.ModHelper.Events;
            events.OnEvent = (Action<MonoBehaviour, Events>)Delegate.Combine(events.OnEvent, new Action<MonoBehaviour, Events>(this.OnEvent));

            // Patch camera controller to allow for fiddling
            base.ModHelper.HarmonyHelper.Transpile<PlayerCameraController>("UpdateCamera", typeof(Patches), "UpdateCameraTranspile");
            base.ModHelper.HarmonyHelper.Transpile<PlayerCameraController>("UpdateRotation", typeof(Patches), "CameraTranspile");

            base.ModHelper.HarmonyHelper.AddPrefix<InteractZone>("UpdateInteractVolume", typeof(Patches), "PatchUpdateInteractVolume");

            base.ModHelper.HarmonyHelper.AddPostfix<PlayerCameraController>("UpdateRotation", typeof(Patches), "CameraPostfix");

            // Add listeners
            GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", new Callback<OWRigidbody>(this.OnEnterFlight));
            GlobalMessenger.AddListener("ExitFlightConsole", new Callback(this.OnExitFlight));

            GlobalMessenger.AddListener("EnterShip", new Callback(this.OnEnterShip));
            GlobalMessenger.AddListener("ExitShip", new Callback(this.OnExitShip));

            GlobalMessenger.AddListener("EnterShipComputer", new Callback(this.OnEnterComputer));
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev)
        {
            bool flag = behaviour.GetType() == typeof(Flashlight) && ev == Events.AfterStart;
            if (flag)
            {
                //GameObject.Find("Traveller_Mesh_v01:PlayerSuit_Helmet").layer = 0;
                GameObject.Find("player_mesh_noSuit:Player_Head").layer = 0;

                // Create dummy GO
                cameraHolder = new GameObject();
                cameraHolder.AddComponent<Transform>();

                // Attach and position dummy on player
                cameraHolder.transform.SetParent(Locator.GetPlayerTransform());
                cameraHolder.transform.localPosition = Vector3.zero;
                cameraHolder.transform.localRotation = Quaternion.Euler(0, 0, 0);

                // Attach and position camera on dummy
                Locator.GetPlayerCamera().transform.SetParent(cameraHolder.transform);
                Locator.GetPlayerCamera().transform.localPosition = _groundPos;

                // Create interact dummy GO
                interactObject = new GameObject();
                interactObject.AddComponent<Transform>();

                interactObject.transform.SetParent(Locator.GetPlayerCamera().transform);
                interactObject.transform.localPosition = new Vector3(0, 0, 5);
                interactObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

                _isStarted = true;
            }
        }

        private void Update()
        {
            if (_isStarted)
            {
                if (_movingFlightCameraOut) // out of ship (flight)
                {
                    float t = (Time.time - startTime1) / flyDuration;
                    Locator.GetPlayerCamera().transform.localPosition = new Vector3(0f, Mathf.SmoothStep(shipY, flyY, t), Mathf.SmoothStep(shipZ, flyZ, t));
                    interactObject.transform.localPosition = new Vector3(0, 0, -flyZ);
                }

                if (_movingFlightCameraIn) // in to ship (flight)
                {
                    float t = (Time.time - startTime2) / flyDuration;
                    Locator.GetPlayerCamera().transform.localPosition = new Vector3(0f, Mathf.SmoothStep(flyY, shipY, t), Mathf.SmoothStep(flyZ, shipZ, t));
                    interactObject.transform.localPosition = new Vector3(0, 0, -shipZ);
                }

                if (_movingShipCameraOut) // out of ship
                {
                    float t = (Time.time - startTime3) / shipDuration;
                    Locator.GetPlayerCamera().transform.localPosition = new Vector3(0f, Mathf.SmoothStep(shipY, groundY, t), Mathf.SmoothStep(shipZ, groundZ, t));
                    interactObject.transform.localPosition = new Vector3(0, 0, -groundZ);
                }

                if (_movingShipCameraIn) // in to ship
                {
                    float t = (Time.time - startTime4) / shipDuration;
                    Locator.GetPlayerCamera().transform.localPosition = new Vector3(0f, Mathf.SmoothStep(groundY, shipY, t), Mathf.SmoothStep(groundZ, shipZ, t));
                    interactObject.transform.localPosition = new Vector3(0, 0, -shipZ);
                }

                if (Input.GetKeyDown(KeyCode.Keypad0))
                {
                    Locator.GetPlayerCamera().transform.localPosition = Vector3.zero;
                }

                if (Input.GetKeyDown(KeyCode.Keypad1))
                {
                    Locator.GetPlayerCamera().transform.localPosition = _groundPos;
                }

                if (Input.GetKeyDown(KeyCode.Keypad2))
                {
                    Locator.GetPlayerCamera().transform.localPosition = _shipPos;
                }

                if (Input.GetKeyDown(KeyCode.Keypad3))
                {
                    Locator.GetPlayerCamera().transform.localPosition = _flyPos;
                }
            }
        }

        private void SetAllToFalse()
        {
            _movingFlightCameraIn = false;
            _movingFlightCameraOut = false;
            _movingShipCameraIn = false;
            _movingShipCameraOut = false;
        }

        private void OnEnterFlight(OWRigidbody shipBody)
        {
            SetAllToFalse();
            startTime1 = Time.time;
            _movingFlightCameraOut = true;
        }

        private void OnExitFlight()
        {
            SetAllToFalse();
            startTime2 = Time.time;
            _movingFlightCameraIn = true;
        }

        private void OnEnterShip()
        {
            SetAllToFalse();
            startTime4 = Time.time;
            _movingShipCameraIn = true;
        }

        private void OnExitShip()
        {
            SetAllToFalse();
            startTime3 = Time.time;
            _movingShipCameraOut = true;
        }

        private void OnEnterComputer()
        {

        }

        private void OnExitComputer()
        {

        }
    }
}
