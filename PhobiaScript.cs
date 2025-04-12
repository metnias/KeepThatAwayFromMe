using UnityEngine;

namespace KeepThatAwayFromMe
{
    internal static class PhobiaScript
    {
        public static void Patch()
        {
            // Don't let banned creature escape
            // If they're somehow realized,
            // force them 'out of the way' w/o
            // killing them (to keep the save unaffected)
            On.AbstractCreature.ctor += StayInDen;
            On.AbstractCreature.InDenUpdate += StayInDenUpdate;
            On.AbstractCreature.Realize += CreatureNoRealize;
            On.Creature.Update += CreatureFreeze;
            On.AbstractPhysicalObject.Realize += ObjectNoRealize;
            //On.GraphicsModule.DrawSprites += CreatureHide;
            On.RoomCamera.SpriteLeaser.Update += ObjectHide;
            On.PhysicalObject.Update += ObjectFreeze;
            On.PlayerCarryableItem.Update += PCObjectFreeze;
            On.Spear.ChangeMode += StuckSpearFix;

            On.FliesRoomAI.Update += FliesRoomAIPatch;
            On.Room.PlaceQuantifiedCreaturesInRoom += PlaceQuantifiedCreaturesInRoomPatch;
        }

        private static void StayInDen(On.AbstractCreature.orig_ctor orig, AbstractCreature self,
            World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);
            if (PhobiaPlugin.IsCritBanned(creatureTemplate)) { self.remainInDenCounter = -1; }
        }

        private static void StayInDenUpdate(On.AbstractCreature.orig_InDenUpdate orig, AbstractCreature self, int time)
        {
            if (PhobiaPlugin.IsCritBanned(self.creatureTemplate)) { self.remainInDenCounter = -1; }
            orig(self, time);
        }

        private static void CreatureFreeze(On.Creature.orig_Update orig, Creature self, bool eu)
        {
            if (PhobiaPlugin.IsCritBanned(self.abstractCreature.creatureTemplate))
            { foreach (BodyChunk bc in self.bodyChunks) { bc.HardSetPosition(new Vector2(0f, -1000f)); } return; }
            orig(self, eu);
        }

        /*
        private static void CreatureHide(On.GraphicsModule.orig_DrawSprites orig, GraphicsModule self,
            RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            void Hide()
            { foreach (FSprite fs in sLeaser.sprites) { fs.isVisible = false; } }

            if (self.owner is Creature c && PhobiaPlugin.IsCritBanned(c.Template)) { Hide(); return; }
            else if (PhobiaPlugin.IsObjBanned(self.owner.abstractPhysicalObject)) { Hide(); return; }
            orig(self, sLeaser, rCam, timeStacker, camPos);
        } */

        private static void ObjectHide(On.RoomCamera.SpriteLeaser.orig_Update orig, RoomCamera.SpriteLeaser self,
            float timeStacker, RoomCamera rCam, Vector2 camPos)
        {
            void Hide()
            { foreach (FSprite fs in self.sprites) { fs.isVisible = false; } }

            if (self.drawableObject is GraphicsModule gm)
            {
                if (gm.owner is Creature cr && PhobiaPlugin.IsCritBanned(cr.Template))
                {
                    //foreach (FSprite fs in self.sprites) { if (fs.isVisible) { Debug.Log(cr.Template.name); break; } }
                    Hide(); return;
                }
                if (PhobiaPlugin.IsObjBanned(gm.owner.abstractPhysicalObject))
                {
                    //foreach (FSprite fs in self.sprites) { if (fs.isVisible) { Debug.Log(gm.owner.abstractPhysicalObject.type); break; } }
                    Hide(); return;
                }
            }
            if (self.drawableObject is Creature c && PhobiaPlugin.IsCritBanned(c.Template)) { Hide(); return; }
            if (self.drawableObject is PhysicalObject po && PhobiaPlugin.IsObjBanned(po.abstractPhysicalObject)) { Hide(); return; }
            orig(self, timeStacker, rCam, camPos);
        }

        private static void ObjectFreeze(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
        {
            if (PhobiaPlugin.IsObjBanned(self.abstractPhysicalObject))
            {
                if (self.room == null || !self.room.readyForAI) goto SKIP;
                if (self.room.abstractRoom.shelter)
                {
                    self.CollideWithObjects = false;
                    //if (self is PlayerCarryableItem item) { item.Forbid(); }
                }
                else
                {
                    // Copied from https://github.com/woutkolkman/mousedrag/blob/2330657c266c9fdda5624da700e86039345ce1bf/MouseDrag/Tools/Destroy.cs#L12
                    ReleaseAllGrasps(self);

                    if (self is SporePlant && (self as SporePlant).stalk != null)
                    {
                        (self as SporePlant).stalk.sporePlant = null;
                        (self as SporePlant).stalk = null;
                    }

                    if (self is Spear) //prevent spear leaving invisible beams behind
                        (self as Spear).resetHorizontalBeamState();

                    self?.RemoveFromRoom();
                    self?.abstractPhysicalObject?.Room?.RemoveEntity(self.abstractPhysicalObject); //prevent realizing after hibernation
                    self?.Destroy();

                    return;

                    void ReleaseAllGrasps(PhysicalObject obj)
                    {
                        if (obj?.grabbedBy != null)
                            for (int i = obj.grabbedBy.Count - 1; i >= 0; i--)
                                obj.grabbedBy[i]?.Release();

                        if (obj is Creature)
                        {
                            if (obj is Player)
                            {
                                //drop slugcats
                                (obj as Player).slugOnBack?.DropSlug();
                                (obj as Player).onBack?.slugOnBack?.DropSlug();
                                (obj as Player).slugOnBack = null;
                                (obj as Player).onBack = null;
                                (obj as Player).spearOnBack?.DropSpear();
                            }

                            (obj as Creature).LoseAllGrasps();
                        }
                    }
                }
            }
        SKIP:
            orig(self, eu);
        }

        private static void ObjectNoRealize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
        {
            if (PhobiaPlugin.IsObjBanned(self))
            {
                /*
                if (!self.Room.shelter)
                {
                    self?.Room?.RemoveEntity(self);
                    self?.Destroy();
                }
                */

                return;
            }
            orig(self);
        }

        private static void CreatureNoRealize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
        {
            if (PhobiaPlugin.IsCritBanned(self.creatureTemplate))
            {
                if (!self.Room.shelter)
                {
                    self?.Room?.RemoveEntity(self);
                    self?.Destroy();
                }

                return;
            }
            orig(self);
        }

        private static void FliesRoomAIPatch(On.FliesRoomAI.orig_Update orig, FliesRoomAI self, bool eu)
        {
            if (PhobiaPlugin.bannedCritTypes.Contains(CreatureTemplate.Type.Fly))
            {
                foreach (var f in self.flies) f?.Destroy();
                self.flies.Clear();
                foreach (var f in self.inHive) f?.Destroy();
                self.inHive.Clear();
            }
            orig(self, eu);
        }

        private static void PlaceQuantifiedCreaturesInRoomPatch(On.Room.orig_PlaceQuantifiedCreaturesInRoom orig, Room self, CreatureTemplate.Type critType)
        {
            if (PhobiaPlugin.bannedCritTypes.Contains(critType)) return;
            orig(self, critType);
        }

        private static void PCObjectFreeze(On.PlayerCarryableItem.orig_Update orig, PlayerCarryableItem self, bool eu)
        {
            if (PhobiaPlugin.IsObjBanned(self.abstractPhysicalObject))
            {
                if (self.room == null || !self.room.readyForAI) goto SKIP;
                if (self.room.abstractRoom.shelter)
                {
                    self.CollideWithObjects = false;
                    self.Forbid();
                }
                else
                {
                    // Copied from https://github.com/woutkolkman/mousedrag/blob/2330657c266c9fdda5624da700e86039345ce1bf/MouseDrag/Tools/Destroy.cs#L12
                    ReleaseAllGrasps(self);

                    if (self is SporePlant && (self as SporePlant).stalk != null)
                    {
                        (self as SporePlant).stalk.sporePlant = null;
                        (self as SporePlant).stalk = null;
                    }

                    if (self is Spear) //prevent spear leaving invisible beams behind
                        (self as Spear).resetHorizontalBeamState();

                    self?.RemoveFromRoom();
                    self?.abstractPhysicalObject?.Room?.RemoveEntity(self.abstractPhysicalObject); //prevent realizing after hibernation
                    self?.Destroy();

                    return;

                    void ReleaseAllGrasps(PhysicalObject obj)
                    {
                        if (obj?.grabbedBy != null)
                            for (int i = obj.grabbedBy.Count - 1; i >= 0; i--)
                                obj.grabbedBy[i]?.Release();

                        if (obj is Creature)
                        {
                            if (obj is Player)
                            {
                                //drop slugcats
                                (obj as Player).slugOnBack?.DropSlug();
                                (obj as Player).onBack?.slugOnBack?.DropSlug();
                                (obj as Player).slugOnBack = null;
                                (obj as Player).onBack = null;
                                (obj as Player).spearOnBack?.DropSpear();
                            }

                            (obj as Creature).LoseAllGrasps();
                        }
                    }
                }
            }
        SKIP:
            orig(self, eu);
        }

        private static bool SpearBanned() => PhobiaPlugin.bannedObjTypes.Contains(AbstractPhysicalObject.AbstractObjectType.Spear);

        private static void StuckSpearFix(On.Spear.orig_ChangeMode orig, Spear self, Weapon.Mode newMode)
        {
            if (SpearBanned() && newMode == Weapon.Mode.StuckInWall) { self.mode = Weapon.Mode.StuckInWall; return; }
            orig(self, newMode);
        }
    }
}