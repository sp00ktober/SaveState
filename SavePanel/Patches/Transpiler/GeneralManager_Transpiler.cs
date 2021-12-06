using HarmonyLib;
using SavePanel.Managers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SavePanel.Patches.Transpiler
{
    [HarmonyPatch(typeof(GeneralManager))]
    class GeneralManager_Transpiler
    {
        // remove days survived text while in save selection
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GeneralManager), "Update")]
        public static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "GetComponentInChildren"),
                    new CodeMatch(OpCodes.Ldstr),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldflda),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "ToString"),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "PushLocalizedMsg")
                )
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<GeneralManager, int>>((__instance =>
                {
                    if (GameObjectManager.loadedIntoSavegame)
                    {
                        int daySurvived = (int)AccessTools.Field(typeof(GeneralManager), "daySurvived").GetValue(__instance);
                        __instance.GetComponentInChildren<OverMsgManager>().PushLocalizedMsg("INV_DAY_SURVIVED", daySurvived.ToString());
                    }
                    return 0;
                })))
                .Insert(new CodeInstruction(OpCodes.Pop));
            return matcher.InstructionEnumeration();
        }
    }
}
