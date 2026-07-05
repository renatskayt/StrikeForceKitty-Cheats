using System;
using System.Reflection;
using System.Collections;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace SFKMod
{
    [BepInPlugin("com.sfk.mod", "SFK Cheat Panel", "1.0.0")]
    public class SFKMod : BaseUnityPlugin
    {
        private bool show = true;
        private Rect win = new Rect(10, 10, 360, 570);
        private float timer;

        public static bool god;
        public static bool egod;
        public static bool trapImmune;
        private bool heal;
        private bool autofish;
        private bool speedhack;
        private float speedMul = 2f;
        private string speedText = "2";

        void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(P), "com.sfk.mod");
            Logger.LogInfo("SFKMod loaded");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) show = !show;

            timer += Time.deltaTime;
            if (timer < 0.3f) return;
            timer = 0f;

            DataGame dg = FindObjectOfType<DataGame>();
            Bucket bk = FindObjectOfType<Bucket>();

            if ((heal || god) && dg != null)
                DoHeal(dg);

            if (autofish)
            {
                if (dg != null) dg.overallFish = 99999f;
                if (bk != null)
                {
                    bk.overallFish = 99999f;
                    SetUIText(bk, "fishTx", "99999");
                }
            }

            Time.timeScale = speedhack ? speedMul : 1f;
        }

        void SetUIText(Bucket bk, string field, string val)
        {
            var f = typeof(Bucket).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            var tx = f?.GetValue(bk) as UnityEngine.UI.Text;
            if (tx != null) tx.text = val;
        }

        IList GetCats(DataGame dg)
        {
            var f = typeof(DataGame).GetField("arrCatsScript",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return f?.GetValue(dg) as IList;
        }

        IList GetEnemies(DataGame dg)
        {
            var f = typeof(Battle).GetField("arrEnemyInBattle",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return f?.GetValue(dg) as IList;
        }

        void DoHeal(DataGame dg)
        {
            var c = GetCats(dg);
            if (c == null) return;
            for (int i = 0; i < c.Count; i++)
            {
                var cat = c[i] as CatBase;
                if (cat == null) continue;
                cat.hp = cat.hp2;
            }
            var slots = FindObjectsOfType<BattleSlot>();
            if (slots == null) return;
            int ci = 0;
            foreach (var s in slots)
            {
                if (!s.cat || s.hp_tx == null) continue;
                if (ci < c.Count)
                {
                    var cat = c[ci] as CatBase;
                    if (cat != null) s.hp_tx.text = cat.hp2.ToString();
                    ci++;
                }
            }
        }

        void DoKill(DataGame dg)
        {
            var e = GetEnemies(dg);
            if (e != null)
            {
                var m = typeof(Battle).GetMethod("removeEnemyHp",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (m != null)
                {
                    for (int i = 0; i < e.Count; i++)
                    {
                        var cat = e[i] as CatBase;
                        if (cat == null || cat.hp <= 0) continue;
                        m.Invoke(dg, new object[] { i, 99999, 0 });
                    }
                }
            }
            // Kill bosses too
            var m2 = typeof(Battle).GetMethod("removeBossHp",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            m2?.Invoke(dg, new object[] { 99999, 0 });
        }

        void UnlockAllLevels()
        {
            for (int i = 1; i <= 30; i++)
            {
                PlayerPrefs.SetInt("level_" + i, 1);
                PlayerPrefs.SetInt("levelShow_" + i, 1);
            }
            for (int i = 1; i <= 5; i++)
                PlayerPrefs.SetInt("boss_" + i, 1);
            for (int i = 21; i <= 35; i++)
                PlayerPrefs.SetInt("BonuslevelShow_" + i, 1);
            PlayerPrefs.Save();
            Logger.LogInfo("All levels + bosses unlocked!");
        }

        void UnlockAllCostumes()
        {
            for (int tid = 1; tid <= 350; tid++)
            {
                PlayerPrefs.SetInt("dress1_" + tid, 1);
                PlayerPrefs.SetInt("dress2_" + tid, 1);
                PlayerPrefs.SetInt("dress3_" + tid, 1);
                PlayerPrefs.SetInt("abySetFirstTime_" + tid, 2);
            }
            PlayerPrefs.Save();
            Logger.LogInfo("All costumes unlocked!");
        }

        void UnlockAllArtifacts()
        {
            for (int id = 1; id <= 29; id++)
            {
                PlayerPrefs.SetInt("artifact_" + id, 1);
                PlayerPrefs.SetInt("artifactWear_" + id, 1);
                PlayerPrefs.SetInt("artifacts2_" + id, 1);
                PlayerPrefs.SetInt("openArtSet_" + id, 1);
            }
            PlayerPrefs.SetInt("artefactGot", 1);
            // Open all chests (includes artifact chests on levels)
            OpenAllChests();
            Logger.LogInfo("All artifacts + chests unlocked!");
        }

        void UnlockAllAbilities()
        {
            for (int id = 1; id <= 12; id++)
                PlayerPrefs.SetInt("abyLevel_" + id, 5);
            for (int id = 1; id <= 15; id++)
                PlayerPrefs.SetInt("skill" + id, 1);
            PlayerPrefs.Save();
            Logger.LogInfo("All abilities + skills unlocked!");
        }

        void UnlockAllTeams()
        {
            for (int id = 1; id <= 105; id++)
                PlayerPrefs.SetInt("team_" + id, 1);
            PlayerPrefs.SetInt("foundTeam", 105);
            PlayerPrefs.SetInt("openTeamSet1_6", 76);
            PlayerPrefs.SetInt("openTeamSet2_6", 246);
            PlayerPrefs.SetInt("openTeamSet3_6", 252);
            PlayerPrefs.SetInt("openTeamSet4_6", 253);
            PlayerPrefs.SetInt("openTeamSet1_7", 7);
            PlayerPrefs.SetInt("openTeamSet2_7", 78);
            PlayerPrefs.SetInt("openTeamSet3_7", 165);
            PlayerPrefs.SetInt("openTeamSet4_7", 166);
            PlayerPrefs.SetInt("openTeamSet1_8", 111);
            PlayerPrefs.SetInt("openTeamSet2_8", 112);
            PlayerPrefs.SetInt("openTeamSet3_8", 113);
            PlayerPrefs.SetInt("openTeamSet4_8", 114);
            PlayerPrefs.SetInt("openTeamSet1_9", 6);
            PlayerPrefs.SetInt("openTeamSet2_9", 67);
            PlayerPrefs.SetInt("openTeamSet3_9", 81);
            PlayerPrefs.SetInt("openTeamSet4_9", 82);
            PlayerPrefs.SetInt("openTeamSet1_10", 50);
            PlayerPrefs.SetInt("openTeamSet2_10", 16);
            PlayerPrefs.SetInt("openTeamSet3_10", 56);
            PlayerPrefs.SetInt("openTeamSet4_10", 185);
            PlayerPrefs.SetInt("openTeamSet1_11", 98);
            PlayerPrefs.SetInt("openTeamSet2_11", 154);
            PlayerPrefs.SetInt("openTeamSet3_11", 155);
            PlayerPrefs.SetInt("openTeamSet4_11", 156);
            PlayerPrefs.SetInt("openTeamSet1_12", 86);
            PlayerPrefs.SetInt("openTeamSet2_12", 99);
            PlayerPrefs.SetInt("openTeamSet3_12", 100);
            PlayerPrefs.SetInt("openTeamSet4_12", 238);
            PlayerPrefs.SetInt("openTeamSet1_13", 27);
            PlayerPrefs.SetInt("openTeamSet2_13", 28);
            PlayerPrefs.SetInt("openTeamSet3_13", 29);
            PlayerPrefs.SetInt("openTeamSet4_13", 30);
            PlayerPrefs.SetInt("openTeamSet1_14", 21);
            PlayerPrefs.SetInt("openTeamSet2_14", 22);
            PlayerPrefs.SetInt("openTeamSet3_14", 243);
            PlayerPrefs.SetInt("openTeamSet4_14", 247);
            PlayerPrefs.SetInt("openTeamSet1_15", 259);
            PlayerPrefs.SetInt("openTeamSet2_15", 261);
            PlayerPrefs.SetInt("openTeamSet3_15", 265);
            PlayerPrefs.SetInt("openTeamSet4_15", 266);
            PlayerPrefs.SetInt("openTeamSet1_16", 240);
            PlayerPrefs.SetInt("openTeamSet2_16", 241);
            PlayerPrefs.SetInt("openTeamSet3_16", 245);
            PlayerPrefs.SetInt("openTeamSet4_16", 319);
            PlayerPrefs.SetInt("openTeamSet1_17", 14);
            PlayerPrefs.SetInt("openTeamSet2_17", 54);
            PlayerPrefs.SetInt("openTeamSet3_17", 62);
            PlayerPrefs.SetInt("openTeamSet4_17", 64);
            PlayerPrefs.SetInt("openTeamSet1_18", 198);
            PlayerPrefs.SetInt("openTeamSet2_18", 276);
            PlayerPrefs.SetInt("openTeamSet3_18", 280);
            PlayerPrefs.SetInt("openTeamSet4_18", 281);
            PlayerPrefs.SetInt("openTeamSet1_19", 167);
            PlayerPrefs.SetInt("openTeamSet2_19", 168);
            PlayerPrefs.SetInt("openTeamSet3_19", 169);
            PlayerPrefs.SetInt("openTeamSet4_19", 170);
            PlayerPrefs.SetInt("openTeamSet1_20", 149);
            PlayerPrefs.SetInt("openTeamSet2_20", 150);
            PlayerPrefs.SetInt("openTeamSet3_20", 151);
            PlayerPrefs.SetInt("openTeamSet4_20", 152);
            PlayerPrefs.SetInt("openTeamSet1_21", 94);
            PlayerPrefs.SetInt("openTeamSet2_21", 95);
            PlayerPrefs.SetInt("openTeamSet3_21", 96);
            PlayerPrefs.SetInt("openTeamSet4_21", 97);
            PlayerPrefs.SetInt("openTeamSet1_22", 90);
            PlayerPrefs.SetInt("openTeamSet2_22", 91);
            PlayerPrefs.SetInt("openTeamSet3_22", 92);
            PlayerPrefs.SetInt("openTeamSet4_22", 93);
            PlayerPrefs.SetInt("openTeamSet1_23", 295);
            PlayerPrefs.SetInt("openTeamSet2_23", 296);
            PlayerPrefs.SetInt("openTeamSet3_23", 297);
            PlayerPrefs.SetInt("openTeamSet4_23", 298);
            PlayerPrefs.SetInt("openTeamSet1_24", 262);
            PlayerPrefs.SetInt("openTeamSet2_24", 269);
            PlayerPrefs.SetInt("openTeamSet3_24", 289);
            PlayerPrefs.SetInt("openTeamSet4_24", 323);
            PlayerPrefs.SetInt("openTeamSet1_25", 73);
            PlayerPrefs.SetInt("openTeamSet2_25", 290);
            PlayerPrefs.SetInt("openTeamSet3_25", 291);
            PlayerPrefs.SetInt("openTeamSet4_25", 292);
            PlayerPrefs.SetInt("openTeamSet1_26", 8);
            PlayerPrefs.SetInt("openTeamSet2_26", 13);
            PlayerPrefs.SetInt("openTeamSet3_26", 61);
            PlayerPrefs.SetInt("openTeamSet4_26", 68);
            PlayerPrefs.SetInt("openTeamSet1_27", 52);
            PlayerPrefs.SetInt("openTeamSet2_27", 196);
            PlayerPrefs.SetInt("openTeamSet3_27", 197);
            PlayerPrefs.SetInt("openTeamSet4_27", 198);
            PlayerPrefs.SetInt("openTeamSet1_28", 46);
            PlayerPrefs.SetInt("openTeamSet2_28", 325);
            PlayerPrefs.SetInt("openTeamSet3_28", 347);
            PlayerPrefs.SetInt("openTeamSet4_28", 348);
            PlayerPrefs.SetInt("openTeamSet1_29", 76);
            PlayerPrefs.SetInt("openTeamSet2_29", 244);
            PlayerPrefs.SetInt("openTeamSet3_29", 254);
            PlayerPrefs.SetInt("openTeamSet4_29", 246);
            PlayerPrefs.SetInt("openTeamSet1_30", 116);
            PlayerPrefs.SetInt("openTeamSet2_30", 233);
            PlayerPrefs.SetInt("openTeamSet3_30", 234);
            PlayerPrefs.SetInt("openTeamSet4_30", 235);
            PlayerPrefs.SetInt("openTeamSet1_31", 10);
            PlayerPrefs.SetInt("openTeamSet2_31", 18);
            PlayerPrefs.SetInt("openTeamSet3_31", 230);
            PlayerPrefs.SetInt("openTeamSet4_31", 232);
            PlayerPrefs.SetInt("openTeamSet1_32", 72);
            PlayerPrefs.SetInt("openTeamSet2_32", 133);
            PlayerPrefs.SetInt("openTeamSet3_32", 189);
            PlayerPrefs.SetInt("openTeamSet4_32", 190);
            PlayerPrefs.SetInt("openTeamSet1_33", 288);
            PlayerPrefs.SetInt("openTeamSet2_33", 112);
            PlayerPrefs.SetInt("openTeamSet3_33", 113);
            PlayerPrefs.SetInt("openTeamSet4_33", 114);
            PlayerPrefs.SetInt("openTeamSet1_34", 244);
            PlayerPrefs.SetInt("openTeamSet2_34", 251);
            PlayerPrefs.SetInt("openTeamSet3_34", 257);
            PlayerPrefs.SetInt("openTeamSet4_34", 258);
            PlayerPrefs.SetInt("openTeamSet1_35", 100);
            PlayerPrefs.SetInt("openTeamSet2_35", 101);
            PlayerPrefs.SetInt("openTeamSet3_35", 114);
            PlayerPrefs.SetInt("openTeamSet4_35", 235);
            PlayerPrefs.SetInt("openTeamSet1_36", 176);
            PlayerPrefs.SetInt("openTeamSet2_36", 177);
            PlayerPrefs.SetInt("openTeamSet3_36", 178);
            PlayerPrefs.SetInt("openTeamSet4_36", 179);
            PlayerPrefs.SetInt("openTeamSet1_37", 37);
            PlayerPrefs.SetInt("openTeamSet2_37", 38);
            PlayerPrefs.SetInt("openTeamSet3_37", 39);
            PlayerPrefs.SetInt("openTeamSet4_37", 40);
            PlayerPrefs.SetInt("openTeamSet1_38", 141);
            PlayerPrefs.SetInt("openTeamSet2_38", 142);
            PlayerPrefs.SetInt("openTeamSet3_38", 143);
            PlayerPrefs.SetInt("openTeamSet4_38", 147);
            PlayerPrefs.SetInt("openTeamSet1_39", 163);
            PlayerPrefs.SetInt("openTeamSet2_39", 164);
            PlayerPrefs.SetInt("openTeamSet3_39", 170);
            PlayerPrefs.SetInt("openTeamSet4_39", 200);
            PlayerPrefs.SetInt("openTeamSet1_40", 86);
            PlayerPrefs.SetInt("openTeamSet2_40", 88);
            PlayerPrefs.SetInt("openTeamSet3_40", 102);
            PlayerPrefs.SetInt("openTeamSet4_40", 109);
            PlayerPrefs.SetInt("openTeamSet1_41", 183);
            PlayerPrefs.SetInt("openTeamSet2_41", 230);
            PlayerPrefs.SetInt("openTeamSet3_41", 232);
            PlayerPrefs.SetInt("openTeamSet4_41", 315);
            PlayerPrefs.SetInt("openTeamSet1_42", 52);
            PlayerPrefs.SetInt("openTeamSet2_42", 53);
            PlayerPrefs.SetInt("openTeamSet3_42", 195);
            PlayerPrefs.SetInt("openTeamSet4_42", 196);
            PlayerPrefs.SetInt("openTeamSet1_43", 21);
            PlayerPrefs.SetInt("openTeamSet2_43", 22);
            PlayerPrefs.SetInt("openTeamSet3_43", 241);
            PlayerPrefs.SetInt("openTeamSet4_43", 286);
            PlayerPrefs.SetInt("openTeamSet1_44", 137);
            PlayerPrefs.SetInt("openTeamSet2_44", 138);
            PlayerPrefs.SetInt("openTeamSet3_44", 139);
            PlayerPrefs.SetInt("openTeamSet4_44", 140);
            PlayerPrefs.SetInt("openTeamSet1_45", 5);
            PlayerPrefs.SetInt("openTeamSet2_45", 158);
            PlayerPrefs.SetInt("openTeamSet3_45", 159);
            PlayerPrefs.SetInt("openTeamSet4_45", 194);
            PlayerPrefs.SetInt("openTeamSet1_46", 260);
            PlayerPrefs.SetInt("openTeamSet2_46", 263);
            PlayerPrefs.SetInt("openTeamSet3_46", 264);
            PlayerPrefs.SetInt("openTeamSet4_46", 267);
            PlayerPrefs.SetInt("openTeamSet1_47", 285);
            PlayerPrefs.SetInt("openTeamSet2_47", 259);
            PlayerPrefs.SetInt("openTeamSet3_47", 261);
            PlayerPrefs.SetInt("openTeamSet4_47", 265);
            PlayerPrefs.SetInt("openTeamSet1_48", 84);
            PlayerPrefs.SetInt("openTeamSet2_48", 305);
            PlayerPrefs.SetInt("openTeamSet3_48", 320);
            PlayerPrefs.SetInt("openTeamSet4_48", 321);
            PlayerPrefs.SetInt("openTeamSet1_49", 40);
            PlayerPrefs.SetInt("openTeamSet2_49", 179);
            PlayerPrefs.SetInt("openTeamSet3_49", 40);
            PlayerPrefs.SetInt("openTeamSet4_49", 42);
            PlayerPrefs.SetInt("openTeamSet1_50", 197);
            PlayerPrefs.SetInt("openTeamSet2_50", 274);
            PlayerPrefs.SetInt("openTeamSet3_50", 275);
            PlayerPrefs.SetInt("openTeamSet4_50", 278);
            PlayerPrefs.SetInt("openTeamSet1_51", 46);
            PlayerPrefs.SetInt("openTeamSet2_51", 58);
            PlayerPrefs.SetInt("openTeamSet3_51", 347);
            PlayerPrefs.SetInt("openTeamSet4_51", 348);
            PlayerPrefs.SetInt("openTeamSet1_52", 17);
            PlayerPrefs.SetInt("openTeamSet2_52", 23);
            PlayerPrefs.SetInt("openTeamSet3_52", 26);
            PlayerPrefs.SetInt("openTeamSet4_52", 31);
            PlayerPrefs.SetInt("openTeamSet1_53", 25);
            PlayerPrefs.SetInt("openTeamSet2_53", 32);
            PlayerPrefs.SetInt("openTeamSet3_53", 188);
            PlayerPrefs.SetInt("openTeamSet4_53", 189);
            PlayerPrefs.SetInt("openTeamSet1_54", 15);
            PlayerPrefs.SetInt("openTeamSet2_54", 16);
            PlayerPrefs.SetInt("openTeamSet3_54", 17);
            PlayerPrefs.SetInt("openTeamSet4_54", 56);
            PlayerPrefs.SetInt("openTeamSet1_55", 87);
            PlayerPrefs.SetInt("openTeamSet2_55", 91);
            PlayerPrefs.SetInt("openTeamSet3_55", 104);
            PlayerPrefs.SetInt("openTeamSet4_55", 119);
            PlayerPrefs.SetInt("openTeamSet1_56", 19);
            PlayerPrefs.SetInt("openTeamSet2_56", 63);
            PlayerPrefs.SetInt("openTeamSet3_56", 65);
            PlayerPrefs.SetInt("openTeamSet4_56", 79);
            PlayerPrefs.SetInt("openTeamSet1_57", 46);
            PlayerPrefs.SetInt("openTeamSet2_57", 129);
            PlayerPrefs.SetInt("openTeamSet3_57", 175);
            PlayerPrefs.SetInt("openTeamSet4_57", 187);
            PlayerPrefs.SetInt("openTeamSet1_58", 86);
            PlayerPrefs.SetInt("openTeamSet2_58", 88);
            PlayerPrefs.SetInt("openTeamSet3_58", 91);
            PlayerPrefs.SetInt("openTeamSet4_58", 102);
            PlayerPrefs.SetInt("openTeamSet1_59", 35);
            PlayerPrefs.SetInt("openTeamSet2_59", 314);
            PlayerPrefs.SetInt("openTeamSet3_59", 313);
            PlayerPrefs.SetInt("openTeamSet4_59", 312);
            PlayerPrefs.SetInt("openTeamSet1_60", 195);
            PlayerPrefs.SetInt("openTeamSet2_60", 222);
            PlayerPrefs.SetInt("openTeamSet3_60", 274);
            PlayerPrefs.SetInt("openTeamSet4_60", 278);
            PlayerPrefs.SetInt("openTeamSet1_61", 18);
            PlayerPrefs.SetInt("openTeamSet2_61", 63);
            PlayerPrefs.SetInt("openTeamSet3_61", 107);
            PlayerPrefs.SetInt("openTeamSet4_61", 144);
            PlayerPrefs.SetInt("openTeamSet1_62", 5);
            PlayerPrefs.SetInt("openTeamSet2_62", 51);
            PlayerPrefs.SetInt("openTeamSet3_62", 151);
            PlayerPrefs.SetInt("openTeamSet4_62", 182);
            PlayerPrefs.SetInt("openTeamSet1_63", 102);
            PlayerPrefs.SetInt("openTeamSet2_63", 103);
            PlayerPrefs.SetInt("openTeamSet3_63", 117);
            PlayerPrefs.SetInt("openTeamSet4_63", 173);
            PlayerPrefs.SetInt("openTeamSet1_64", 84);
            PlayerPrefs.SetInt("openTeamSet2_64", 87);
            PlayerPrefs.SetInt("openTeamSet3_64", 104);
            PlayerPrefs.SetInt("openTeamSet4_64", 106);
            PlayerPrefs.SetInt("openTeamSet1_65", 43);
            PlayerPrefs.SetInt("openTeamSet2_65", 227);
            PlayerPrefs.SetInt("openTeamSet3_65", 344);
            PlayerPrefs.SetInt("openTeamSet4_65", 345);
            PlayerPrefs.SetInt("openTeamSet1_66", 86);
            PlayerPrefs.SetInt("openTeamSet2_66", 99);
            PlayerPrefs.SetInt("openTeamSet3_66", 120);
            PlayerPrefs.SetInt("openTeamSet4_66", 121);
            PlayerPrefs.SetInt("openTeamSet1_67", 6);
            PlayerPrefs.SetInt("openTeamSet2_67", 7);
            PlayerPrefs.SetInt("openTeamSet3_67", 135);
            PlayerPrefs.SetInt("openTeamSet4_67", 141);
            PlayerPrefs.SetInt("openTeamSet1_68", 253);
            PlayerPrefs.SetInt("openTeamSet2_68", 256);
            PlayerPrefs.SetInt("openTeamSet3_68", 258);
            PlayerPrefs.SetInt("openTeamSet4_68", 264);
            PlayerPrefs.SetInt("openTeamSet1_69", 52);
            PlayerPrefs.SetInt("openTeamSet2_69", 53);
            PlayerPrefs.SetInt("openTeamSet3_69", 195);
            PlayerPrefs.SetInt("openTeamSet4_69", 198);
            PlayerPrefs.SetInt("openTeamSet1_70", 34);
            PlayerPrefs.SetInt("openTeamSet2_70", 71);
            PlayerPrefs.SetInt("openTeamSet3_70", 129);
            PlayerPrefs.SetInt("openTeamSet4_70", 347);
            PlayerPrefs.SetInt("openTeamSet1_71", 3);
            PlayerPrefs.SetInt("openTeamSet2_71", 44);
            PlayerPrefs.SetInt("openTeamSet3_71", 45);
            PlayerPrefs.SetInt("openTeamSet4_71", 132);
            PlayerPrefs.SetInt("openTeamSet1_72", 1);
            PlayerPrefs.SetInt("openTeamSet2_72", 5);
            PlayerPrefs.SetInt("openTeamSet3_72", 20);
            PlayerPrefs.SetInt("openTeamSet4_72", 24);
            PlayerPrefs.SetInt("openTeamSet1_73", 248);
            PlayerPrefs.SetInt("openTeamSet2_73", 250);
            PlayerPrefs.SetInt("openTeamSet3_73", 255);
            PlayerPrefs.SetInt("openTeamSet4_73", 318);
            PlayerPrefs.SetInt("openTeamSet1_74", 35);
            PlayerPrefs.SetInt("openTeamSet2_74", 314);
            PlayerPrefs.SetInt("openTeamSet3_74", 313);
            PlayerPrefs.SetInt("openTeamSet4_74", 33);
            PlayerPrefs.SetInt("openTeamSet1_75", 51);
            PlayerPrefs.SetInt("openTeamSet2_75", 79);
            PlayerPrefs.SetInt("openTeamSet3_75", 122);
            PlayerPrefs.SetInt("openTeamSet4_75", 148);
            PlayerPrefs.SetInt("openTeamSet1_76", 88);
            PlayerPrefs.SetInt("openTeamSet2_76", 89);
            PlayerPrefs.SetInt("openTeamSet3_76", 308);
            PlayerPrefs.SetInt("openTeamSet4_76", 109);
            PlayerPrefs.SetInt("openTeamSet1_77", 1);
            PlayerPrefs.SetInt("openTeamSet2_77", 20);
            PlayerPrefs.SetInt("openTeamSet3_77", 130);
            PlayerPrefs.SetInt("openTeamSet4_77", 137);
            PlayerPrefs.SetInt("openTeamSet1_78", 12);
            PlayerPrefs.SetInt("openTeamSet2_78", 49);
            PlayerPrefs.SetInt("openTeamSet3_78", 50);
            PlayerPrefs.SetInt("openTeamSet4_78", 322);
            PlayerPrefs.SetInt("openTeamSet1_79", 9);
            PlayerPrefs.SetInt("openTeamSet2_79", 11);
            PlayerPrefs.SetInt("openTeamSet3_79", 32);
            PlayerPrefs.SetInt("openTeamSet4_79", 57);
            PlayerPrefs.SetInt("openTeamSet1_80", 14);
            PlayerPrefs.SetInt("openTeamSet2_80", 15);
            PlayerPrefs.SetInt("openTeamSet3_80", 31);
            PlayerPrefs.SetInt("openTeamSet4_80", 54);
            PlayerPrefs.SetInt("openTeamSet1_81", 98);
            PlayerPrefs.SetInt("openTeamSet2_81", 138);
            PlayerPrefs.SetInt("openTeamSet3_81", 139);
            PlayerPrefs.SetInt("openTeamSet4_81", 140);
            PlayerPrefs.SetInt("openTeamSet1_82", 322);
            PlayerPrefs.SetInt("openTeamSet2_82", 16);
            PlayerPrefs.SetInt("openTeamSet3_82", 56);
            PlayerPrefs.SetInt("openTeamSet4_82", 185);
            PlayerPrefs.SetInt("openTeamSet1_83", 61);
            PlayerPrefs.SetInt("openTeamSet2_83", 106);
            PlayerPrefs.SetInt("openTeamSet3_83", 120);
            PlayerPrefs.SetInt("openTeamSet4_83", 310);
            PlayerPrefs.SetInt("openTeamSet1_84", 36);
            PlayerPrefs.SetInt("openTeamSet2_84", 41);
            PlayerPrefs.SetInt("openTeamSet3_84", 146);
            PlayerPrefs.SetInt("openTeamSet4_84", 293);
            PlayerPrefs.SetInt("openTeamSet1_85", 339);
            PlayerPrefs.SetInt("openTeamSet2_85", 260);
            PlayerPrefs.SetInt("openTeamSet3_85", 263);
            PlayerPrefs.SetInt("openTeamSet4_85", 264);
            PlayerPrefs.SetInt("openTeamSet1_86", 34);
            PlayerPrefs.SetInt("openTeamSet2_86", 58);
            PlayerPrefs.SetInt("openTeamSet3_86", 71);
            PlayerPrefs.SetInt("openTeamSet4_86", 74);
            PlayerPrefs.SetInt("openTeamSet1_87", 34);
            PlayerPrefs.SetInt("openTeamSet2_87", 36);
            PlayerPrefs.SetInt("openTeamSet3_87", 59);
            PlayerPrefs.SetInt("openTeamSet4_87", 60);
            PlayerPrefs.SetInt("openTeamSet1_88", 83);
            PlayerPrefs.SetInt("openTeamSet2_88", 97);
            PlayerPrefs.SetInt("openTeamSet3_88", 105);
            PlayerPrefs.SetInt("openTeamSet4_88", 107);
            PlayerPrefs.SetInt("openTeamSet1_89", 10);
            PlayerPrefs.SetInt("openTeamSet2_89", 19);
            PlayerPrefs.SetInt("openTeamSet3_89", 98);
            PlayerPrefs.SetInt("openTeamSet4_89", 181);
            PlayerPrefs.SetInt("openTeamSet1_90", 9);
            PlayerPrefs.SetInt("openTeamSet2_90", 23);
            PlayerPrefs.SetInt("openTeamSet3_90", 25);
            PlayerPrefs.SetInt("openTeamSet4_90", 32);
            PlayerPrefs.SetInt("openTeamSet1_91", 87);
            PlayerPrefs.SetInt("openTeamSet2_91", 102);
            PlayerPrefs.SetInt("openTeamSet3_91", 103);
            PlayerPrefs.SetInt("openTeamSet4_91", 104);
            PlayerPrefs.SetInt("openTeamSet1_92", 299);
            PlayerPrefs.SetInt("openTeamSet2_92", 300);
            PlayerPrefs.SetInt("openTeamSet3_92", 301);
            PlayerPrefs.SetInt("openTeamSet4_92", 302);
            PlayerPrefs.SetInt("openTeamSet1_93", 74);
            PlayerPrefs.SetInt("openTeamSet2_93", 58);
            PlayerPrefs.SetInt("openTeamSet3_93", 129);
            PlayerPrefs.SetInt("openTeamSet4_93", 348);
            PlayerPrefs.SetInt("openTeamSet1_94", 34);
            PlayerPrefs.SetInt("openTeamSet2_94", 58);
            PlayerPrefs.SetInt("openTeamSet3_94", 223);
            PlayerPrefs.SetInt("openTeamSet4_94", 74);
            PlayerPrefs.SetInt("openTeamSet1_95", 259);
            PlayerPrefs.SetInt("openTeamSet2_95", 267);
            PlayerPrefs.SetInt("openTeamSet3_95", 339);
            PlayerPrefs.SetInt("openTeamSet4_95", 340);
            PlayerPrefs.SetInt("openTeamSet1_96", 36);
            PlayerPrefs.SetInt("openTeamSet2_96", 59);
            PlayerPrefs.SetInt("openTeamSet3_96", 60);
            PlayerPrefs.SetInt("openTeamSet4_96", 145);
            PlayerPrefs.SetInt("openTeamSet1_97", 1);
            PlayerPrefs.SetInt("openTeamSet2_97", 5);
            PlayerPrefs.SetInt("openTeamSet3_97", 137);
            PlayerPrefs.SetInt("openTeamSet4_97", 315);
            PlayerPrefs.SetInt("openTeamSet1_98", 33);
            PlayerPrefs.SetInt("openTeamSet2_98", 236);
            PlayerPrefs.SetInt("openTeamSet3_98", 311);
            PlayerPrefs.SetInt("openTeamSet4_98", 312);
            PlayerPrefs.SetInt("openTeamSet1_99", 98);
            PlayerPrefs.SetInt("openTeamSet2_99", 154);
            PlayerPrefs.SetInt("openTeamSet3_99", 155);
            PlayerPrefs.SetInt("openTeamSet4_99", 156);
            PlayerPrefs.SetInt("openTeamSet1_100", 87);
            PlayerPrefs.SetInt("openTeamSet2_100", 203);
            PlayerPrefs.SetInt("openTeamSet3_100", 103);
            PlayerPrefs.SetInt("openTeamSet4_100", 104);
            PlayerPrefs.SetInt("openTeamSet1_101", 34);
            PlayerPrefs.SetInt("openTeamSet2_101", 58);
            PlayerPrefs.SetInt("openTeamSet3_101", 223);
            PlayerPrefs.SetInt("openTeamSet4_101", 74);
            PlayerPrefs.SetInt("openTeamSet1_102", 1);
            PlayerPrefs.SetInt("openTeamSet2_102", 220);
            PlayerPrefs.SetInt("openTeamSet3_102", 4);
            PlayerPrefs.SetInt("openTeamSet4_102", 5);
            PlayerPrefs.SetInt("openTeamSet1_103", 9);
            PlayerPrefs.SetInt("openTeamSet2_103", 23);
            PlayerPrefs.SetInt("openTeamSet3_103", 80);
            PlayerPrefs.SetInt("openTeamSet4_103", 191);
            PlayerPrefs.SetInt("openTeamSet1_104", 219);
            PlayerPrefs.SetInt("openTeamSet2_104", 207);
            PlayerPrefs.SetInt("openTeamSet3_104", 294);
            PlayerPrefs.SetInt("openTeamSet4_104", 313);
            PlayerPrefs.SetInt("openTeamSet1_105", 124);
            PlayerPrefs.SetInt("openTeamSet2_105", 125);
            PlayerPrefs.SetInt("openTeamSet3_105", 126);
            PlayerPrefs.SetInt("openTeamSet4_105", 127);
            PlayerPrefs.Save();
            Logger.LogInfo("All teams unlocked!");
        }

        void OpenAllChests()
        {
            for (int id = 0; id <= 100; id++)
                PlayerPrefs.SetInt("chest_" + id, 1);
            PlayerPrefs.Save();
            Logger.LogInfo("All chests opened!");
        }

        void OnGUI()
        {
            if (!show) return;
            win = GUI.Window(0, win, DoWin, "Cheats");
        }

        void DoWin(int id)
        {
            GUI.DragWindow(new Rect(0, 0, win.width, 20));

            DataGame dg = FindObjectOfType<DataGame>();
            Bucket bk = FindObjectOfType<Bucket>();

            var fv = dg != null ? dg.overallFish.ToString()
                : bk != null ? bk.overallFish.ToString() : "?";
            GUILayout.Label("Fish: " + fv);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+1000")) AddFish(dg, bk, 1000);
            if (GUILayout.Button("+10000")) AddFish(dg, bk, 10000);
            GUILayout.EndHorizontal();
            autofish = GUILayout.Toggle(autofish, "Auto Fish 99999");

            GUILayout.Space(5);
            GUILayout.Label("--- Battle ---");
            god = GUILayout.Toggle(god, "God Mode");
            trapImmune = GUILayout.Toggle(trapImmune, "Trap Immune");
            egod = GUILayout.Toggle(egod, "God Mode (enemies+bosses)");
            heal = GUILayout.Toggle(heal, "Auto Heal");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Heal Now") && dg != null) DoHeal(dg);
            if (GUILayout.Button("Kill All") && dg != null) DoKill(dg);
            GUILayout.EndHorizontal();

            if (bk != null)
            {
                GUILayout.Label("Gold: " + bk.collectedGold);
                if (GUILayout.Button("+1000 Gold"))
                {
                    bk.collectedGold += 1000;
                    var f2 = typeof(Bucket).GetField("goldTx",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    var tx = f2?.GetValue(bk) as UnityEngine.UI.Text;
                    if (tx != null) tx.text = bk.collectedGold.ToString();
                    var keepM = typeof(DataF).GetMethod("keep",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    int total = bk.collectedGold + PlayerPrefs.GetInt("goldOverall", 0);
                    keepM?.Invoke(bk, new object[] { "gold", bk.collectedGold });
                    keepM?.Invoke(bk, new object[] { "goldOverall", total });
                    Logger.LogInfo("Gold added");
                }
            }

            if (dg != null)
            {
                var c = GetCats(dg);
                var e = GetEnemies(dg);
                GUILayout.Label("Cats: " + (c?.Count ?? 0) + " Enemies: " + (e?.Count ?? 0));
            }

            GUILayout.Space(5);
            speedhack = GUILayout.Toggle(speedhack, "Speedhack x" + speedText);
            if (speedhack)
            {
                speedMul = GUILayout.HorizontalSlider(speedMul, 1f, 10f);
                speedText = speedMul.ToString("F1");
            }

            GUILayout.Space(5);
            GUILayout.Label("--- Global ---");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock All Levels")) UnlockAllLevels();
            if (GUILayout.Button("Unlock Costumes")) UnlockAllCostumes();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock Artifacts")) UnlockAllArtifacts();
            if (GUILayout.Button("Unlock Abilities")) UnlockAllAbilities();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock Teams")) UnlockAllTeams();
            if (GUILayout.Button("Open Chests")) OpenAllChests();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("UNLOCK ALL")) { UnlockAllLevels(); UnlockAllCostumes(); UnlockAllArtifacts(); UnlockAllAbilities(); UnlockAllTeams(); OpenAllChests(); }
            GUILayout.EndHorizontal();
        }

        void AddFish(DataGame dg, Bucket bk, float n)
        {
            if (dg != null) dg.overallFish += n;
            if (bk != null)
            {
                bk.overallFish += n;
                SetUIText(bk, "fishTx", bk.overallFish.ToString());
            }
            int ni = (int)n;
            int cur = PlayerPrefs.GetInt("fishOverall", 0);
            PlayerPrefs.SetInt("fishOverall", cur + ni);
            // Per-cat fish (used in CatGym / training room)
            for (int cat = 1; cat <= 4; cat++)
            {
                int pc = PlayerPrefs.GetInt("cat_" + cat + "_fish", 0);
                PlayerPrefs.SetInt("cat_" + cat + "_fish", pc + ni);
            }
            PlayerPrefs.Save();
        }
    }

    class P
    {
        [HarmonyPatch(typeof(Battle), "removeCatHp")]
        [HarmonyPrefix]
        static void PreCat(int id, ref int d, int crit)
        {
            if (SFKMod.god) d = 0;
        }

        [HarmonyPatch(typeof(Battle), "removeEnemyHp")]
        [HarmonyPrefix]
        static void PreEnemy(int id, ref int d, int crit)
        {
            if (SFKMod.egod) d = 0;
        }

        [HarmonyPatch(typeof(Battle), "removeBossHp")]
        [HarmonyPrefix]
        static void PreBoss(ref int d, int crit)
        {
            if (SFKMod.egod) d = 0;
        }

        // Trap immunity: skip all trap collision logic when enabled
        [HarmonyPatch(typeof(Game), "objTrap")]
        [HarmonyPrefix]
        static bool PreTrap()
        {
            return !SFKMod.trapImmune;
        }

        // Automatically heal cats after trap damage when god mode is on
        [HarmonyPatch(typeof(Game), "objTrap")]
        [HarmonyPostfix]
        static void PostTrap()
        {
            if (!SFKMod.god) return;
            DataGame dg = UnityEngine.Object.FindObjectOfType(typeof(DataGame)) as DataGame;
            if (dg == null) return;
            var f = typeof(DataGame).GetField("arrCatsScript",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var c = f?.GetValue(dg) as IList;
            if (c == null) return;
            for (int i = 0; i < c.Count; i++)
            {
                var obj = c[i];
                if (obj == null) continue;
                var ty = obj.GetType();
                var hpF = ty.GetField("hp");
                var hp2F = ty.GetField("hp2");
                var exF = ty.GetField("exist");
                if (hpF == null || hp2F == null || exF == null) continue;
                int hp = (int)hpF.GetValue(obj);
                if (hp <= 0)
                {
                    hpF.SetValue(obj, (int)hp2F.GetValue(obj));
                    exF.SetValue(obj, true);
                }
            }
        }
    }
}
