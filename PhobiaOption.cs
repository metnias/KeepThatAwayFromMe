using System;
using System.Collections.Generic;
using System.Linq;
using Menu.Remix;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;
using static IconSymbol;

namespace KeepThatAwayFromMe
{
    public class PhobiaOption : OptionInterface
    {
        public PhobiaOption() : base()
        {
            OnConfigChanged += ConfigOnChange;
            OnDeactivate += DeactivateCheck;
        }

        public override void Initialize()
        {
            base.Initialize();

            if (critPresets == null)
            {
                critPresets = new string[9, 3];
                critPresets[0, 0] = "All Creatures"; critPresets[0, 1] = "Toggle All Creatures"; critPresets[0, 2] = "all Creatures";
                critPresets[1, 0] = "Arachnophobia"; critPresets[1, 1] = "Toggle All Spiders (Big and Small)"; critPresets[1, 2] = "all Spiders";
                critPresets[2, 0] = "Scolopendrphobia"; critPresets[2, 1] = "Toggle All Centipedes"; critPresets[2, 2] = "all Centipedes and Centiwings";
                critPresets[3, 0] = "Entomophobia"; critPresets[3, 1] = "Toggle All Bugs (Excluding Spiders and Centipedes)"; critPresets[3, 2] = "all Bugs (except Spiders and Centipedes) and Noodleflies";
                critPresets[4, 0] = "Herpetophobia"; critPresets[4, 1] = "Toggle All Lizards"; critPresets[4, 2] = "all Lizards and Salamanders";
                critPresets[5, 0] = "Ornithophobia"; critPresets[5, 1] = "Toggle All Birds"; critPresets[5, 2] = "all Vultures and Miros";
                critPresets[6, 0] = "Bdellophobia"; critPresets[6, 1] = "Toggle All Leeches"; critPresets[6, 2] = "all Leeches";
                critPresets[7, 0] = "Pithecophobia"; critPresets[7, 1] = "Toggle All Scavengers"; critPresets[7, 2] = "all Scavengers";
                critPresets[8, 0] = "Seplophobia"; critPresets[8, 1] = "Toggle All Daddies"; critPresets[8, 2] = "all Daddies";

                objPresets = new string[5, 3];
                objPresets[0, 0] = "All Items"; objPresets[0, 1] = "Toggle All Items"; objPresets[0, 2] = "all Items";
                objPresets[1, 0] = "Weapons"; objPresets[1, 1] = "Toggle All Damaging Weapons"; objPresets[1, 2] = "Rock, Spear and Bomb";
                objPresets[2, 0] = "Pearls"; objPresets[2, 1] = "Toggle All Pearls"; objPresets[2, 2] = "Data Pearls and Pebbles' Pearls";
                objPresets[3, 0] = "Plants"; objPresets[3, 1] = "Toggle All Unedible Plants"; objPresets[3, 2] = "all Unedible Plants";
                objPresets[4, 0] = "Crops"; objPresets[4, 1] = "Toggle All Edible Crops"; objPresets[4, 2] = "all Edible Crops";
            }

            Tabs = new OpTab[] { new OpTab(this, Translate("Creatures")), new OpTab(this, Translate("Objects")) };

            const float ITEM_INTERVAL = 40f, ITEM_HEIGHT = 30f;
            Vector2 ICON_ANCHOR = new Vector2(1.0f, 0.5f);

            #region Creatures

            // Title
            // AddBasicProfile(Tabs[0], this.mod);
            // Tabs[0].AddItems(new OpLabel(new Vector2(50f, 525f), new Vector2(500f, 20f), Translate(modDescription), alignment: FLabelAlignment.Center));

            // Decorations
            lblCritAlert = new OpLabel(new Vector2(260f, 575f), new Vector2(300f, 20f), "", FLabelAlignment.Right);
            alertAlpha = new float[] { 0f, 0f };
            alertSin = new float[] { 0f, 0f };
            Tabs[0].AddItems(new OpRect(new Vector2(30f, 0f), new Vector2(540f, 470f)), lblCritAlert,
                new OpLabel(new Vector2(50f, 520f), new Vector2(240f, 40f), Translate("Creature List"), FLabelAlignment.Left, true),
                new OpLabel(new Vector2(100f, 475f), new Vector2(64f, 20f), Translate("BAN"), FLabelAlignment.Center),
                new OpRect(new Vector2(320f, 495f), new Vector2(240f, 80f), 0.2f));

            // Main List
            float GetCritOffset(int idx) => (PhobiaPlugin.allCritTypes.Length - idx) * ITEM_INTERVAL - 15.01f;
            ckCrits = new OpCheckBox[PhobiaPlugin.allCritTypes.Length];
            idxCrits = new Dictionary<string, int>();
            sbCrits = new OpScrollBox(new Vector2(30f, 0f), new Vector2(540f, 470f), PhobiaPlugin.allCritTypes.Length * ITEM_INTERVAL + 40f, false, false, true);
            Tabs[0].AddItems(sbCrits);
            for (int c = 0; c < PhobiaPlugin.allCritTypes.Length; c++)
            {
                idxCrits.Add(GenerateCritKey(PhobiaPlugin.allCritTypes[c]), c);
                ckCrits[c] = new OpCheckBox(PhobiaPlugin.critTypesBan[c], new Vector2(90f, GetCritOffset(c) + 3f));
                sbCrits.AddItems(ckCrits[c]);
                IconSymbolData iconData = new IconSymbolData(PhobiaPlugin.allCritTypes[c], AbstractPhysicalObject.AbstractObjectType.Creature, 0);
                string iconName = CreatureSymbol.SpriteNameOfCreature(iconData);
                if (iconName != "Futile_White")
                {
                    sbCrits.AddItems(new OpImage(new Vector2(80f, GetCritOffset(c) + ITEM_HEIGHT / 2f), iconName)
                    { color = CreatureSymbol.ColorOfCreature(iconData), anchor = ICON_ANCHOR });
                }
                string text = PhobiaPlugin.allCritTypes[c].ToString(), key = "creaturetype-" + text, id = text.ToString();
                text = Translate(key);
                if (text == key) text = id;
                else text += $" ({Translate("MenuModStat_ModID").Replace("<ModID>", id)})";
                sbCrits.AddItems(new OpLabel(new Vector2(124f, GetCritOffset(c)), new Vector2(160f, ITEM_HEIGHT), text, FLabelAlignment.Left) { bumpBehav = ckCrits[c].bumpBehav });
                if (c > 0) UIfocusable.MutualVerticalFocusableBind(ckCrits[c], ckCrits[c - 1]);
            }

            // Presets
            List<ListItem> prs = new List<ListItem>();
            for (int p = 0; p < critPresets.GetLength(0); p++)
            { prs.Add(new ListItem(critPresets[p, 0], p + 1) { displayName = Translate(critPresets[p, 0]), desc = Translate(critPresets[p, 1]) }); }
            cbCritPresets = new OpComboBox(config.Bind("_critPreset", ""), new Vector2(330f, 505f), 220f, prs)
            { description = Translate("Toggle many types at once with Presets"), listHeight = (ushort)critPresets.GetLength(0) };
            cbCritPresets.OnValueChanged += OnCritPresetChanged;
            Tabs[0].AddItems(cbCritPresets,
                new OpLabel(new Vector2(349f, 540f), new Vector2(200f, 30f), Translate("Presets"), FLabelAlignment.Left)
                { bumpBehav = cbCritPresets.bumpBehav, description = Translate("Toggle many types at once with Presets") });
            UIfocusable.MutualVerticalFocusableBind(ckCrits[0], cbCritPresets);
            UIfocusable.MutualHorizontalFocusableBind(ckCrits[0], cbCritPresets);
            cbCritPresets.SetNextFocusable(UIfocusable.NextDirection.Up, cbCritPresets);
            cbCritPresets.SetNextFocusable(UIfocusable.NextDirection.Right, cbCritPresets);
            foreach (OpCheckBox c in ckCrits)
            {
                c.SetNextFocusable(UIfocusable.NextDirection.Right, cbCritPresets);
                c.SetNextFocusable(UIfocusable.NextDirection.Back, cbCritPresets);
            }

            #endregion Creatures

            #region Objects

            // Title
            //AddBasicProfile(Tabs[1], this.mod);
            // Tabs[1].AddItems(new OpLabel(new Vector2(50f, 525f), new Vector2(500f, 20f), Translate(modDescription), alignment: FLabelAlignment.Center));

            // Decorations
            lblObjAlert = new OpLabel(new Vector2(260f, 575f), new Vector2(300f, 20f), "", FLabelAlignment.Right);

            Tabs[1].AddItems(new OpRect(new Vector2(30f, 0f), new Vector2(540f, 470f)), lblObjAlert,
                new OpLabel(new Vector2(50f, 520f), new Vector2(240f, 40f), Translate("Object List"), FLabelAlignment.Left, true),
                new OpLabel(new Vector2(100f, 475f), new Vector2(64f, 20f), Translate("BAN"), FLabelAlignment.Center),
                new OpRect(new Vector2(320f, 495f), new Vector2(240f, 80f), 0.2f));

            // Main List
            float GetObjOffset(int idx) => (PhobiaPlugin.allObjTypes.Length - idx) * ITEM_INTERVAL - 15.01f;
            ckObjs = new OpCheckBox[PhobiaPlugin.allObjTypes.Length];
            idxObjs = new Dictionary<string, int>();
            sbObjs = new OpScrollBox(new Vector2(30f, 0f), new Vector2(540f, 470f), PhobiaPlugin.allObjTypes.Length * ITEM_INTERVAL + 40f, false, false, true);
            Tabs[1].AddItems(sbObjs);
            for (int c = 0; c < PhobiaPlugin.allObjTypes.Length; c++)
            {
                idxObjs.Add(GenerateObjKey(PhobiaPlugin.allObjTypes[c]), c);
                ckObjs[c] = new OpCheckBox(PhobiaPlugin.objTypesBan[c], new Vector2(90f, GetObjOffset(c) + 3f));
                sbObjs.AddItems(ckObjs[c]);
                string iconName = ItemSymbol.SpriteNameForItem(PhobiaPlugin.allObjTypes[c], 0);
                if (iconName != "Futile_White")
                {
                    sbObjs.AddItems(new OpImage(new Vector2(80f, GetObjOffset(c) + ITEM_HEIGHT / 2f), iconName)
                    { color = ItemSymbol.ColorForItem(PhobiaPlugin.allObjTypes[c], 0), anchor = ICON_ANCHOR });
                }
                string text = PhobiaPlugin.allObjTypes[c].ToString(), key = "objecttype-" + text, id = text.ToString();
                text = Translate(key);
                if (text == key) text = id;
                else text += $" ({Translate("MenuModStat_ModID").Replace("<ModID>", id)})";
                sbObjs.AddItems(new OpLabel(new Vector2(124f, GetObjOffset(c)), new Vector2(160f, ITEM_HEIGHT), text, FLabelAlignment.Left) { bumpBehav = ckObjs[c].bumpBehav });
                if (c > 0) UIfocusable.MutualVerticalFocusableBind(ckObjs[c], ckObjs[c - 1]);
            }

            // Presets
            prs = new List<ListItem>();
            for (int p = 0; p < objPresets.GetLength(0); p++)
            { prs.Add(new ListItem(objPresets[p, 0], p + 1) { displayName = Translate(objPresets[p, 0]), desc = Translate(objPresets[p, 1]) }); }
            cbObjPresets = new OpComboBox(config.Bind("_objPreset", ""), new Vector2(330f, 505f), 220f, prs)
            { description = Translate("Toggle many types at once with Presets"), listHeight = (ushort)objPresets.GetLength(0) };
            cbObjPresets.OnValueChanged += OnObjPresetChanged;
            Tabs[1].AddItems(cbObjPresets,
                new OpLabel(new Vector2(349f, 540f), new Vector2(200f, 30f), Translate("Presets"), FLabelAlignment.Left)
                { bumpBehav = cbObjPresets.bumpBehav, description = Translate("Toggle many types at once with Presets") });
            UIfocusable.MutualVerticalFocusableBind(ckObjs[0], cbObjPresets);
            UIfocusable.MutualHorizontalFocusableBind(ckObjs[0], cbObjPresets);
            cbObjPresets.SetNextFocusable(UIfocusable.NextDirection.Up, cbObjPresets);
            cbObjPresets.SetNextFocusable(UIfocusable.NextDirection.Right, cbObjPresets);
            foreach (OpCheckBox c in ckObjs)
            {
                c.SetNextFocusable(UIfocusable.NextDirection.Right, cbObjPresets);
                c.SetNextFocusable(UIfocusable.NextDirection.Back, cbObjPresets);
            }

            #endregion Objects

            Tabs[0].OnPostUpdate += PostUpdateTab0;
            Tabs[1].OnPostUpdate += PostUpdateTab1;
        }

        private OpComboBox cbCritPresets, cbObjPresets;
        private string[,] critPresets, objPresets;
        private Dictionary<string, int> idxCrits, idxObjs;
        private OpCheckBox[] ckCrits, ckObjs;
        private OpScrollBox sbCrits, sbObjs;
        private OpLabel lblCritAlert, lblObjAlert;
        private float[] alertAlpha, alertSin;

        private void PostUpdateTab0()
        {
            alertAlpha[1] = 0f;
            // lblAlert Management
            if (alertAlpha[0] > 0f)
            {
                lblCritAlert.alpha = Mathf.Clamp01(alertAlpha[0]);
                alertAlpha[0] -= 0.02f;
                alertSin[0] += Mathf.Clamp01(alertAlpha[0]) * 0.5f;
                lblCritAlert.color = Color.Lerp(Color.white, Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey), Mathf.Sin(alertSin[0]));
            }
            else { lblCritAlert.alpha = 0f; }
        }

        private void PostUpdateTab1()
        {
            alertAlpha[0] = 0f;
            // lblAlert Management
            if (alertAlpha[1] > 0f)
            {
                lblObjAlert.alpha = Mathf.Clamp01(alertAlpha[1]);
                alertAlpha[1] -= 0.02f;
                alertSin[1] += Mathf.Clamp01(alertAlpha[1]) * 0.5f;
                lblObjAlert.color = Color.Lerp(Color.white, Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey), Mathf.Sin(alertSin[1]));
            }
            else { lblObjAlert.alpha = 0f; }
        }

        public override void Update()
        {
            base.Update();
            pendingCheck = false;
        }

        private void OnCritPresetChanged(UIconfig config, string value, string oldValue)
        {
            if (string.IsNullOrEmpty(value)) return;
            //Debug.Log($"{value}({cbCritPresets.GetIndex(value)}) <-- {oldValue}");
            ConfigConnector.MuteMenu(true);
            string[] trig = new string[0];
            switch (cbCritPresets.GetIndex(value))
            {
                case 0: // All
                    trig = new string[] { };
                    break;

                case 1: // Spiders
                    trig = new string[] { "spider" };
                    break;

                case 2: // Centipedes
                    trig = new string[] { "centipede", "centiwing", "aquacenti" };
                    break;

                case 3: // Etc Insects
                    trig = new string[] { "bug", "needleworm", "grub", "moth" };
                    break;

                case 4: // Lizards
                    trig = new string[] { "lizard", "salamander" };
                    break;

                case 5: // Birds
                    trig = new string[] { "vulture", "bird" };
                    break;

                case 6: // Leeches
                    trig = new string[] { "leech" };
                    break;

                case 7: // Scavs
                    trig = new string[] { "scavenger" };
                    break;

                case 8: // Daddies
                    trig = new string[] { "daddy", "longlegs", "rot", "rattler" };
                    break;
            }
            List<CreatureTemplate.Type> toggles = new List<CreatureTemplate.Type>();
            if (cbCritPresets.GetIndex(cbCritPresets.value) == 0) toggles.AddRange(PhobiaPlugin.allCritTypes);
            else foreach (CreatureTemplate.Type t in PhobiaPlugin.allCritTypes)
                { foreach (string tr in trig) { if (t.ToString().ToLower().Contains(tr)) { toggles.Add(t); break; } } }

            int enabled = 0;
            foreach (CreatureTemplate.Type t in toggles)
            { if (ckCrits[idxCrits[GenerateCritKey(t)]].GetValueBool()) { enabled++; } }
            bool newToggle = (float)enabled / toggles.Count < 0.5f;
            foreach (CreatureTemplate.Type t in toggles)
            { ckCrits[idxCrits[GenerateCritKey(t)]].SetValueBool(newToggle); }
            lblCritAlert.text = Translate(newToggle ? "Banned <Preset>" : "Reenabled <Preset>").Replace("<Preset>", Translate(critPresets[cbCritPresets.GetIndex(cbCritPresets.value), 2]));
            alertAlpha[0] = 1.5f; alertSin[0] = 0f;
            cbCritPresets.value = "";

            ConfigConnector.MuteMenu(false);
            ConfigContainer.PlaySound(newToggle ? SoundID.MENU_Player_Unjoin_Game : SoundID.MENU_Player_Join_Game);
        }

        private void OnObjPresetChanged(UIconfig config, string value, string oldValue)
        {
            if (string.IsNullOrEmpty(value)) return;
            // Apply Presets
            ConfigConnector.MuteMenu(true);
            string[] trig = new string[0];
            switch (cbObjPresets.GetIndex(value))
            {
                case 0: // All
                    trig = new string[] { };
                    break;

                case 1: // Weapons
                    trig = new string[] { "rock", "spear", "scavengerbomb" };
                    break;

                case 2: // Pearls
                    trig = new string[] { "pearl" };
                    break;

                case 3: // Plants
                    trig = new string[] { "flarebomb", "puffball", "firecrackerplant", "flylure", "grass" };
                    break;

                case 4: // Crops
                    trig = new string[] { "fruit", "nut", "seedcob", "mushroom", "slimemold",
                        "gooieduck", "lillypuck", "glowweed", "peach", "seed", "pomegranate" };
                    break;
            }
            List<AbstractPhysicalObject.AbstractObjectType> toggles = new List<AbstractPhysicalObject.AbstractObjectType>();
            if (cbObjPresets.GetIndex(cbObjPresets.value) == 0) toggles.AddRange(PhobiaPlugin.allObjTypes);
            else foreach (AbstractPhysicalObject.AbstractObjectType t in PhobiaPlugin.allObjTypes)
                { foreach (string tr in trig) { if (t.ToString().ToLower().Contains(tr)) { toggles.Add(t); break; } } }
            if (trig.Length == 3) { toggles.Remove(AbstractPhysicalObject.AbstractObjectType.PebblesPearl); } // debug

            int enabled = 0;
            foreach (AbstractPhysicalObject.AbstractObjectType t in toggles)
            { if (ckObjs[idxObjs[GenerateObjKey(t)]].GetValueBool()) { enabled++; } }
            bool newToggle = (float)enabled / toggles.Count < 0.5f;
            foreach (AbstractPhysicalObject.AbstractObjectType t in toggles)
            { ckObjs[idxObjs[GenerateObjKey(t)]].SetValueBool(newToggle); }
            lblObjAlert.text = Translate(newToggle ? "Banned <Preset>" : "Reenabled <Preset>").Replace("<Preset>", Translate(objPresets[cbObjPresets.GetIndex(cbObjPresets.value), 2]));
            alertAlpha[1] = 1.5f; alertSin[1] = 0f;
            cbObjPresets.value = "";

            ConfigConnector.MuteMenu(false);
            ConfigContainer.PlaySound(newToggle ? SoundID.MENU_Player_Unjoin_Game : SoundID.MENU_Player_Join_Game);
        }

        internal static string GenerateCritKey(CreatureTemplate.Type type) => $"BanCrit{type.value}";

        internal static string GenerateObjKey(AbstractPhysicalObject.AbstractObjectType type) => $"BanObj{type.value}";

        private void ConfigOnChange()
        {
            // Grab Creatures
            var bannedCrit = new HashSet<CreatureTemplate.Type>();
            for (int i = 0; i < PhobiaPlugin.critTypesBan.Length; i++)
                if (PhobiaPlugin.critTypesBan[i].Value) bannedCrit.Add(PhobiaPlugin.allCritTypes[i]);
            PhobiaPlugin.bannedCritTypes = bannedCrit;

            // Grab Objects
            var bannedObj = new HashSet<AbstractPhysicalObject.AbstractObjectType>();
            for (int i = 0; i < PhobiaPlugin.objTypesBan.Length; i++)
                if (PhobiaPlugin.objTypesBan[i].Value) bannedObj.Add(PhobiaPlugin.allObjTypes[i]);
            PhobiaPlugin.bannedObjTypes = bannedObj;
            pendingCheck = true;

            Debug.Log($"bannedCrits: {bannedCrit.Count}, bannedObjs: {bannedObj.Count}");
        }

        private bool pendingCheck = false;

        private void DeactivateCheck()
        {
            if (!pendingCheck) return;
            if (ModManager.MSC)
            {
                // Artificer Check
                if (PhobiaPlugin.bannedCritTypes.Contains(CreatureTemplate.Type.Scavenger))
                {
                    string o = "ban-artificer-softlock-warning", t = Translate(o);
                    if (o == t) t = "Warning!<LINE><LINE>Banning Scavenger will softlock Artificer Campaign.";
                    ConfigConnector.CreateDialogBoxNotify(t);
                    return;
                }
                // Spearmaster Check
                if (PhobiaPlugin.bannedObjTypes.Contains(AbstractPhysicalObject.AbstractObjectType.Spear))
                {
                    string o = "ban-spearmaster-softlock-warning", t = Translate(o);
                    if (o == t) t = "Warning!<LINE><LINE>Banning Spear will softlock Spearmaster runs.";
                    ConfigConnector.CreateDialogBoxNotify(t);
                    return;
                }
                // Gourmand Check
                foreach (var tracker in WinState.GourmandPassageTracker)
                {
                    if (tracker.type == AbstractPhysicalObject.AbstractObjectType.Creature)
                    {
                        foreach (var crit in tracker.crits)
                            if (!PhobiaPlugin.bannedCritTypes.Contains(crit)) goto Okay;
                        string o = "ban-gourmand-endinglock-warning", t = Translate(o);
                        if (o == t) t = "Warning!<LINE><LINE>You cannot complete Gourmand's Food Quest with this configuration.";
                        ConfigConnector.CreateDialogBoxNotify(t);
                        return;
                    }
                    else
                    {
                        if (PhobiaPlugin.bannedObjTypes.Contains(tracker.type))
                        {
                            string o = "ban-gourmand-endinglock-warning", t = Translate(o);
                            if (o == t) t = "Warning!<LINE><LINE>You cannot complete Gourmand's Food Quest with this configuration.";
                            ConfigConnector.CreateDialogBoxNotify(t);
                            return;
                        }
                    }
                Okay: continue;
                }
            }
        }
    }
}