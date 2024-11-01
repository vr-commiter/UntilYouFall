using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MyTrueGear;
using SG.Claymore.Armaments.Abilities;
using SG.Claymore.Armaments;
using SG.Claymore.Armory;
using SG.Claymore.Combat.Blocking;
using SG.Claymore.Combat.EnemyAttacks;
using SG.Claymore.Entities;
using SG.Claymore.Interaction;
using SG.Claymore.Movement.Dash;
using SG.Claymore.UI.Popup;
using SG.Claymore.UI;
using System.Threading;

namespace UntilYouFall_TrueGear
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log;

        private static TrueGearMod _TrueGear = null;

        private static bool isLeftCrush = false;
        private static bool isBulwark = false;
        private static bool isCrush = false;
        private static bool isHeartBeat = false;

        private static bool canRiseAgain = true;
        private static bool canRightAttack = true;
        private static bool canLeftAttack = true;
        private static bool canTwoHandAttack = true;

        private static bool isPause = false;

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;

            HarmonyLib.Harmony.CreateAndPatchAll(typeof(Plugin));
            _TrueGear = new TrueGearMod();
            _TrueGear.Play("HeartBeat");

            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }


        [HarmonyPostfix, HarmonyPatch(typeof(PlayerDash), "OnDashForward")]
        private static void PlayerDash_OnDashForward_Postfix()
        {
            if (isPause)
            {
                return;
            }
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo("FlashForward");
            _TrueGear.Play("FlashForward");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerDash), "OnDashBack")]
        private static void PlayerDash_OnDashBack_Postfix()
        {
            if (isPause)
            {
                return;
            }
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo("FlashBack");
            _TrueGear.Play("FlashBack");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Armament), "InitSummon")]
        private static void Armament_InitSummon_Postfix(Armament __instance, PlayerHand summoningHand)
        {
            if (summoningHand.name == "PlayerHandLeft")
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("LeftHandSummonWeapon");
                _TrueGear.Play("LeftHandSummonWeapon");
            }
            else
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("RightHandSummonWeapon");
                _TrueGear.Play("RightHandSummonWeapon");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ArmamentAbilityUser), "OnSuperActivated")]
        private static void ArmamentAbilityUser_OnSuperActivated_Postfix(ArmamentAbilityUser __instance)
        {
            if (__instance.armament.BoundHand == __instance.HoldingPlayer.LeftHand)
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("LeftHandWeaponSpuerActivated");
                _TrueGear.Play("LeftHandMeleeBombHit");
            }
            else
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("RightHandWeaponSpuerActivated");
                _TrueGear.Play("RightHandMeleeBombHit");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerHealth), "OnHealthChanged")]
        private static void PlayerHealth_OnHealthChanged_Postfix(PlayerHealth __instance, float hp)
        {
            isBulwark = false;
            if (hp > 0 && isHeartBeat)
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("StopHeartBeat");
                _TrueGear.StopHeartBeat();
                isHeartBeat = false;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerDefense), "OnAttackHit")]
        private static void PlayerDefense_OnAttackHit_Postfix(PlayerDefense __instance, BlockTimingData blockTiming)
        {
            if (__instance.health.IsDead)
            {
                return;
            }

            Log.LogInfo("----------------------------------------------");
            float attackAngle = blockTiming.AttackMessage.AttackAngle;
            if (blockTiming.AttackMessage.AttackAngle < -90f)
            {
                attackAngle = 360 + attackAngle;
            }

            if (__instance.health.IsOnDeathsDoor)
            {
                isHeartBeat = true;
                Log.LogInfo("StartHeartBeat");
                _TrueGear.StartHeartBeat();
                return;
            }
            if (isBulwark)
            {
                Log.LogInfo("BulwarkDamage");
                _TrueGear.Play("PoisonDamage");
                return;
            }
            if (blockTiming.IsDodgePremonition)
            {
                Log.LogInfo($"HammerDamage,{attackAngle}");
                _TrueGear.PlayDir("LineDamage", attackAngle);
                return;
            }
            Log.LogInfo($"DefaultDamage,{attackAngle}");
            _TrueGear.PlayDir("LineDamage", attackAngle);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerDefense), "OnAttackBlocked")]
        private static void PlayerDefense_OnAttackBlocked_Postfix(PlayerDefense __instance, AttackBlocker blocker)
        {
            if (blocker.armament.isHeldInTwoHands)
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("TwoHandAttackBlock");
                _TrueGear.Play("LeftHandMeleeHit");
                _TrueGear.Play("RightHandMeleeHit");
            }
            else if (blocker == blocker.HoldingPlayer.leftBlocker)
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("LeftHandAttackBlock");
                _TrueGear.Play("LeftHandMeleeHit");
            }
            else
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("RightHandAttackBlock");
                _TrueGear.Play("RightHandMeleeHit");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerDefense), "KnockbackPlayer")]
        private static void PlayerDefense_KnockbackPlayer_Postfix(PlayerDefense __instance)
        {
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo("PlayerDeath");
            Log.LogInfo("StopHeartBeat");
            _TrueGear.Play("PlayerDeath");
            _TrueGear.StopHeartBeat();
            isHeartBeat = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CrushInteractable), "OnCrushStart")]
        private static void CrushInteractable_OnCrushStart_Postfix(CrushInteractable __instance)
        {
            if (__instance.crushingHand.name == "PlayerHandLeft")
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("StartLeftCrushCrystal");
                _TrueGear.StartLeftCrushCrystal();
                isLeftCrush = true;
            }
            if (__instance.crushingHand.name == "PlayerHandRight")
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("StartRightCrushCrystal");
                _TrueGear.StartRightCrushCrystal();
                isLeftCrush = false;
            }
            Log.LogInfo(__instance.crushingHand.name);
            //if (!isCrush)
            //{
            //    isCrush = true;

            //}            
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CrushInteractable), "OnCancelCrush")]
        private static void CrushInteractable_OnCancelCrush_Postfix(CrushInteractable __instance)
        {
            if (__instance.crushingHand.name == "PlayerHandLeft")
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("StopLeftHandCrushCrystal");
                _TrueGear.StopLeftCrushCrystal();
            }
            if (__instance.crushingHand.name == "PlayerHandRight")
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("StopRightHandCrushCrystal");
                _TrueGear.StopRightCrushCrystal();
            }
            Log.LogInfo(__instance.crushingHand.name);

            //if (isCrush)
            //{ 
            //    isCrush = false;

            //}            
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CrushInteractable), "FinishCrush")]
        private static void CrushInteractable_FinishCrush_Postfix(CrushInteractable __instance)
        {
            if (isLeftCrush)
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("LeftHandFinishCrush");
                _TrueGear.Play("LeftHandFinishCrush");
            }
            else
            {
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("RightHandFinishCrush");
                _TrueGear.Play("RightHandFinishCrush");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GroundAttackPlayable), "Raise")]
        private static void GroundAttackPlayable_Raise_Postfix(GroundAttackPlayable __instance)
        {
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo("GroundAttack");
            _TrueGear.Play("FallDamage");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerHealth), "Restore")]
        private static void PlayerHealth_Restore_Postfix(PlayerHealth __instance)
        {
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo("Healing");
            _TrueGear.Play("Healing");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(WeaponRackSlot), "OnEnable")]
        private static void WeaponRackSlot_OnEnable_Postfix(WeaponRackSlot __instance)
        {
            if (canRiseAgain)
            {
                canRiseAgain = false;
                Log.LogInfo("----------------------------------------------");
                Log.LogInfo("RiseAgain");
                _TrueGear.Play("LevelStarted");
                Timer riseAgainTimer = new Timer(RiseAgainTimerCallBack, null, 200, Timeout.Infinite);
            }
        }

        private static void RiseAgainTimerCallBack(System.Object o)
        {
            canRiseAgain = true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MeleeWeapon), "GetForceRating")]
        private static void MeleeWeapon_GetForceRating_Postfix(MeleeWeapon __instance, float __result)
        {
            if (__result > 1f)
            {
                if (__instance.armament.isHeldInTwoHands)
                {
                    if (canTwoHandAttack)
                    {
                        canTwoHandAttack = false;
                        Log.LogInfo("----------------------------------------------");
                        Log.LogInfo("TwoHandMeleeAttack");
                        _TrueGear.Play("LeftHandMeleeMajorHit");
                        _TrueGear.Play("RightHandMeleeMajorHit");
                        Timer twoHandAttackTimer = new Timer(TwoHandAttackTimerCallBack, null, 80, Timeout.Infinite);
                    }
                }
                if (__instance.armament.BoundHand == __instance.HoldingPlayer.LeftHand)
                {
                    if (canLeftAttack)
                    {
                        canLeftAttack = false;
                        Log.LogInfo("----------------------------------------------");
                        Log.LogInfo("LeftHandMeleeAttack");
                        _TrueGear.Play("LeftHandMeleeMajorHit");
                        Timer leftAttackTimer = new Timer(LeftAttackTimerCallBack, null, 80, Timeout.Infinite);
                    }
                }
                else
                {
                    if (canRightAttack)
                    {
                        canRightAttack = false;
                        Log.LogInfo("----------------------------------------------");
                        Log.LogInfo("RightHandMeleeAttack");
                        _TrueGear.Play("RightHandMeleeMajorHit");
                        Timer rightAttackTimer = new Timer(RightAttackTimerCallBack, null, 30, Timeout.Infinite);
                    }
                }
            }
        }

        private static void TwoHandAttackTimerCallBack(System.Object o)
        {
            canTwoHandAttack = true;
        }
        private static void LeftAttackTimerCallBack(System.Object o)
        {
            canLeftAttack = true;
        }
        private static void RightAttackTimerCallBack(System.Object o)
        {
            canRightAttack = true;
        }


        [HarmonyPostfix, HarmonyPatch(typeof(BulwarkAbility), "OnActivationSuccess")]
        private static void BulwarkAbility_OnActivationSuccess_Postfix(BulwarkAbility __instance)
        {
            isBulwark = true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PopupUIController), "OnDisable")]
        private static void PopupUIController_OnDisable_Postfix(PopupUIController __instance)
        {
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo("StopHeartBeat");
            _TrueGear.StopHeartBeat();
            isHeartBeat = false;
        }


        [HarmonyPostfix, HarmonyPatch(typeof(PauseScreen), "BeginOpening")]
        private static void PauseScreen_BeginOpening_Postfix(PauseScreen __instance)
        {
            isPause = true;
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo("StopHeartBeat");
            _TrueGear.StopHeartBeat();

        }

        [HarmonyPostfix, HarmonyPatch(typeof(PauseScreen), "BeginClosing")]
        private static void PauseScreen_BeginClosing_Postfix(PauseScreen __instance)
        {
            isPause = false;
            if (!isHeartBeat)
            {
                return;
            }
            Log.LogInfo("----------------------------------------------");
            Log.LogInfo("StartHeartBeat");
            _TrueGear.StartHeartBeat();
        }
    }
}
