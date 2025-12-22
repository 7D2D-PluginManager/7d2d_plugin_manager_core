using HarmonyLib;
using PluginManager.Api.Capabilities.Implementations.Events.GameEvents;
using PluginManager.Api.Hooks;

namespace PluginManager.Core.Patches;

[HarmonyPatch(typeof(NetPackageDamageEntity), nameof(NetPackageDamageEntity.ProcessPackage))]
public static class DamageEntityPatch
{
    static bool Prefix(NetPackageDamageEntity __instance, World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return false;
        }

        var entityDamageEvent =
            new EntityDamageEvent(__instance.entityId, __instance.attackerEntityId, __instance.strength);
        var result = ModContext.EventRunner.Publish(entityDamageEvent, HookMode.Pre);
        
        if (_world.GetPrimaryPlayer() != null &&
            _world.GetPrimaryPlayer().entityId == entityDamageEvent.VictimEntityId &&
            (__instance.damageTyp == EnumDamageTypes.Falling ||
             (__instance.damageSrc == EnumDamageSource.External &&
              (__instance.damageTyp == EnumDamageTypes.Piercing ||
               __instance.damageTyp == EnumDamageTypes.BarbedWire) &&
              entityDamageEvent.AttackerEntityId == -1)))
            return false;

        Entity entity = _world.GetEntity(entityDamageEvent.VictimEntityId);
        if (entity == null)
            return false;

        DamageSource damageSource = new DamageSourceEntity(
            __instance.damageSrc,
            __instance.damageTyp,
            entityDamageEvent.AttackerEntityId,
            __instance.dirV,
            __instance.hitTransformName,
            __instance.hitTransformPosition,
            __instance.uvHit
        );
        damageSource.SetIgnoreConsecutiveDamages(__instance.bIgnoreConsecutiveDamages);
        damageSource.DamageMultiplier = __instance.damageMultiplier;
        damageSource.BonusDamageType = (EnumDamageBonusType)__instance.bonusDamageType;
        damageSource.AttackingItem = __instance.attackingItem;
        damageSource.BlockPosition = __instance.blockPos;

        DamageResponse damageResponse = new DamageResponse
        {
            Strength = entityDamageEvent.Strength,
            ModStrength = 0,
            MovementState = __instance.movementState,
            HitDirection = (Utils.EnumHitDirection)__instance.hitDirection,
            HitBodyPart = (EnumBodyPartHit)__instance.hitBodyPart,
            PainHit = __instance.bPainHit,
            Fatal = __instance.bFatal,
            Critical = __instance.bCritical,
            Random = __instance.random,
            Source = damageSource,
            CrippleLegs = __instance.bCrippleLegs,
            Dismember = __instance.bDismember,
            TurnIntoCrawler = __instance.bTurnIntoCrawler,
            Stun = (EnumEntityStunType)__instance.StunType,
            StunDuration = __instance.StunDuration,
            ArmorSlot = __instance.ArmorSlot,
            ArmorSlotGroup = __instance.ArmorSlotGroup,
            ArmorDamage = __instance.ArmorDamage
        };

        if (__instance.bFromBuff)
            damageResponse.Source.BuffClass = new BuffClass();
        
        entity.FireAttackedEvents(damageResponse);
        entity.ProcessDamageResponse(damageResponse);

        // Log.Out($"[post] DamageSource: {{ " +
        //         $"damageSrc={__instance.damageSrc}, " +
        //         $"damageTyp={__instance.damageTyp}, " +
        //         $"AttackerEntityId={entityDamageEvent.AttackerEntityId}, " +
        //         $"dirV={__instance.dirV}, " +
        //         $"hitTransformName={__instance.hitTransformName}, " +
        //         $"hitTransformPosition={__instance.hitTransformPosition}, " +
        //         $"uvHit={__instance.uvHit}, " +
        //         $"IgnoreConsecutiveDamages={__instance.bIgnoreConsecutiveDamages}, " +
        //         $"DamageMultiplier={__instance.damageMultiplier}, " +
        //         $"BonusDamageType={(__instance.bonusDamageType)}, " +
        //         $"AttackingItem={__instance.attackingItem}, " +
        //         $"BlockPosition={__instance.blockPos} }}");
        //
        // Log.Out($"[post] DamageResponse: {{ " +
        //         $"Strength={damageResponse.Strength}, " +
        //         $"ModStrength={damageResponse.ModStrength}, " +
        //         $"MovementState={damageResponse.MovementState}, " +
        //         $"HitDirection={damageResponse.HitDirection}, " +
        //         $"HitBodyPart={damageResponse.HitBodyPart}, " +
        //         $"PainHit={damageResponse.PainHit}, " +
        //         $"Fatal={damageResponse.Fatal}, " +
        //         $"Critical={damageResponse.Critical}, " +
        //         $"Random={damageResponse.Random}, " +
        //         $"CrippleLegs={damageResponse.CrippleLegs}, " +
        //         $"Dismember={damageResponse.Dismember}, " +
        //         $"TurnIntoCrawler={damageResponse.TurnIntoCrawler}, " +
        //         $"Stun={damageResponse.Stun}, " +
        //         $"StunDuration={damageResponse.StunDuration}, " +
        //         $"ArmorSlot={damageResponse.ArmorSlot}, " +
        //         $"ArmorSlotGroup={damageResponse.ArmorSlotGroup}, " +
        //         $"ArmorDamage={damageResponse.ArmorDamage}, " +
        //         $"Source={damageResponse.Source} }}");
        
        if (result != HookResult.Stop)
        {
            ModContext.EventRunner.Publish(entityDamageEvent, HookMode.Post);
        }

        return false;
    }
}