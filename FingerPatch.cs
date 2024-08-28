using System;
using HarmonyLib;

namespace GorillaTagModTemplateProject
{
	// Token: 0x0200000E RID: 14
	[HarmonyPatch(typeof(ControllerInputPoller), "Update")]
	public class FingerPatch
	{
		// Token: 0x0600005D RID: 93 RVA: 0x00003DE8 File Offset: 0x00001FE8
		private static void Postfix(ControllerInputPoller __instance)
		{
			bool flag2 = FingerPatch.forceLeftGrip;
			if (flag2)
			{
				__instance.leftControllerGripFloat = 1f;
				__instance.leftGrab = true;
				__instance.leftGrabRelease = false;
			}
			bool flag3 = FingerPatch.forceRightGrip;
			if (flag3)
			{
				__instance.rightControllerGripFloat = 1f;
				__instance.rightGrab = true;
				__instance.rightGrabRelease = false;
			}
			bool flag4 = FingerPatch.forceLeftTrigger;
			if (flag4)
			{
				__instance.leftControllerIndexFloat = 1f;
			}
			bool flag5 = FingerPatch.forceRightTrigger;
			if (flag5)
			{
				__instance.rightControllerIndexFloat = 1f;
			}
			bool flag6 = FingerPatch.forceLeftPrimary;
			if (flag6)
			{
				__instance.leftControllerPrimaryButton = true;
			}
			bool flag7 = FingerPatch.forceRightPrimary;
			if (flag7)
			{
				__instance.rightControllerPrimaryButton = true;
			}
			bool flag8 = FingerPatch.forceLeftSecondary;
			if (flag8)
			{
				__instance.leftControllerSecondaryButton = true;
			}
			bool flag9 = FingerPatch.forceRightSecondary;
			if (flag9)
			{
				__instance.rightControllerSecondaryButton = true;
			}
		}

		// Token: 0x04000050 RID: 80
		public static bool forceLeftGrip;

		// Token: 0x04000051 RID: 81
		public static bool forceRightGrip;

		// Token: 0x04000052 RID: 82
		public static bool forceLeftTrigger;

		// Token: 0x04000053 RID: 83
		public static bool forceRightTrigger;

		// Token: 0x04000054 RID: 84
		public static bool forceLeftPrimary;

		// Token: 0x04000055 RID: 85
		public static bool forceRightPrimary;

		// Token: 0x04000056 RID: 86
		public static bool forceLeftSecondary;

		// Token: 0x04000057 RID: 87
		public static bool forceRightSecondary;
	}
}