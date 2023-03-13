using System.Linq;
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
            On.Creature.Update += CreatureFreeze;
            //On.GraphicsModule.DrawSprites += CreatureHide;
            On.RoomCamera.SpriteLeaser.Update += ObjectHide;
            On.PhysicalObject.Update += ObjectFreeze;
            On.Spear.ChangeMode += StuckSpearFix;
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

            if (self.drawableObject is Creature c && PhobiaPlugin.IsCritBanned(c.Template)) { Hide(); return; }
            else if (self.drawableObject is PhysicalObject po && PhobiaPlugin.IsObjBanned(po.abstractPhysicalObject)) { Hide(); return; }
            orig(self, timeStacker, rCam, camPos);
        }

        private static void ObjectFreeze(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
        {
            if (PhobiaPlugin.IsObjBanned(self.abstractPhysicalObject))
            {
                if (self.room.abstractRoom.shelter)
                {
                    self.CollideWithObjects = false;
                    if (self is PlayerCarryableItem item) { item.Forbid(); }
                }
                else { self.Destroy(); }
            }
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