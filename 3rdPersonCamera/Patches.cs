using Harmony;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace _3rdPersonCamera
{
    public static class Patches
    {
        // Token: 0x06000009 RID: 9 RVA: 0x00002568 File Offset: 0x00000768
        public static IEnumerable<CodeInstruction> UpdateCameraTranspile(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            list.RemoveRange(76, 10);
            return list.AsEnumerable<CodeInstruction>();
        }

        public static IEnumerable<CodeInstruction> CameraTranspile(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            list.RemoveRange(79, 6);
            return list.AsEnumerable<CodeInstruction>();
        }

        public static void CameraPostfix(ref Quaternion ____rotationX, ref Quaternion ____rotationY)
        {
            Quaternion localRotation = ____rotationX * ____rotationY * Quaternion.identity;
            Locator.GetPlayerCamera().transform.parent.localRotation = localRotation;
        }

        static bool PatchUpdateInteractVolume(
                InteractZone __instance,
                OWCamera ____playerCam,
                float ____viewingWindow,
                ref bool ____focused
            )
        {
            float num = 2f * Vector3.Angle(-MainClass.interactObject.transform.forward, __instance.transform.forward);
            ____focused = (num <= ____viewingWindow);
            var Base = __instance as SingleInteractionVolume;

            var method = typeof(SingleInteractionVolume).GetMethod("UpdateInteractVolume");
            var ftn = method.MethodHandle.GetFunctionPointer();
            var func = (Action)Activator.CreateInstance(typeof(Action), __instance, ftn);

            func();

            return false;
        }
    }
}
