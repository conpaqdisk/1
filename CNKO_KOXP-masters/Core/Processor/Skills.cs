using KOXP.Constants.Addresses;
using System.Security.Cryptography.Xml;
using static KOXP.Constants.Id;
using static KOXP.Core.Helper;
using static KOXP.Core.Processor.CharFunctions;
using static KOXP.Core.Processor.Functions;
using static KOXP.Core.Processor.InventoryFunctions;
using static KOXP.Core.Processor.SkillFunctions;

namespace KOXP.Core.Processor
{
    public class Skills : AddressHandler
    {
        public static bool UseHealSkill(int SkillId)
        {
            SendPacket("3101" + AlignDWORD(SkillId)[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000F00");
            SendPacket("3103" + AlignDWORD(SkillId)[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "0000000000000000000000000000000000000000000000000000");
            return true;
        }

        public static bool UseTimedSkill(string Skill)
        {
            if (Skill == null)
                return false;

            switch (Skill)
            {
                case "Wolf":
                    {
                        if (GetAction() == EAction.Attack && GetItemsInInventory(Wolf))
                        {
                            SendPacket("3101" + AlignDWORD(int.Parse(GetClass().ToString() + "030"))[..8] + AlignDWORD(GetId())[..8] + "FFFFFFFF" + AlignDWORD(GetX())[..8] + AlignDWORD(GetZ())[..8] + AlignDWORD(GetY())[..8] + "000000000000000000000000000000001100");
                            SendPacket("3103" + AlignDWORD(int.Parse(GetClass().ToString() + "030"))[..8] + AlignDWORD(GetId())[..8] + "FFFFFFFF" + AlignDWORD(GetX())[..8] + AlignDWORD(GetZ())[..8] + AlignDWORD(GetY())[..8] + "0000000000000000000000000000");
                        }
                        break;
                    }
                case "Sprint":
                    {
                        SendPacket("3103" + AlignDWORD(int.Parse(GetClass().ToString() + "002"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000000");
                        break;
                    }
                case "Defence":
                    {
                        SendPacket("3103" + AlignDWORD(int.Parse(GetClass().ToString() + "007"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000000");
                        break;
                    }
                case "Swift":
                    {
                        SendPacket("3101" + AlignDWORD(int.Parse(GetClass().ToString() + "010"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000F00");
                        SendPacket("3103" + AlignDWORD(int.Parse(GetClass().ToString() + "010"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "0000000000000000000000000000000000000000000000000000");
                        break;
                    }
                case "Light Feet":
                    {
                        SendPacket("3103" + AlignDWORD(int.Parse(GetClass().ToString() + "725"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000000");
                        break;
                    }
                case "Evade":
                    {
                        SendPacket("3103" + AlignDWORD(int.Parse(GetClass().ToString() + "710"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000000");
                        break;
                    }
                case "Safely":
                    {
                        SendPacket("3103" + AlignDWORD(int.Parse(GetClass().ToString() + "730"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000000");
                        break;
                    }
                case "Scaled Skin":
                    {
                        SendPacket("3103" + AlignDWORD(int.Parse(GetClass().ToString() + "760"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "0000000000000000000000000000000000000000000000000000000000000");
                        break;
                    }
                case "Lupin Eyes":
                    {
                        SendPacket("3101" + AlignDWORD(int.Parse(GetClass().ToString() + "735"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000001400");
                        SendPacket("3103" + AlignDWORD(int.Parse(GetClass().ToString() + "735"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000");
                        break;
                    }
                case "Hide":
                    {
                        SendPacket("3101" + AlignDWORD(int.Parse(GetClass().ToString() + "700"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000F00");
                        SendPacket("3103" + AlignDWORD(int.Parse(GetClass().ToString() + "700"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000");
                        break;
                    }
                case "Prayer of God's Power":
                    {
                        SendPacket("3103" + AlignDWORD(int.Parse(GetClass().ToString() + "020"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "0000000000000000000000000000000000000000000000000000000000000");
                        break;
                    }
            }

            return false;
        }

        public static void BuffSC(string skillName)
        {
            int skillID = -1;

            switch (skillName)
            {
                case "Hyper Noah": skillID = 500094; break;
                case "AP 120 min": skillID = 500096; break;
                case "Def SC": skillID = 500097; break;
                case "AP 30 min": skillID = 501139; break;
                case "Pink": skillID = 500342; break;
                case "Gray": skillID = 500343; break;
                case "Green": skillID = 500344; break;
                case "Rich Merchant": skillID = 500125; break;
            }

            if (!IsSkillInUsed(skillID) && skillID != -1)
                SendPacket("3103" + AlignDWORD(skillID)[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000000");
        }

        public static void MinorHeal()
        {
            SendPacket("3103" + AlignDWORD(int.Parse(GetClass().ToString() + "705"))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(GetId())[..8] + "0000000000000000000000000000000000000000000000000000");
        }

        public static bool MageAttack(string Skill, int TargetId = 0)
        {
            if (GetHp() == 0)
                return false;

            int Base = GetTargetBase(TargetId);

            int TargetX = GetTargetX(Base); int TargetY = GetTargetY(Base); int TargetZ = GetTargetZ(Base);

            string SkillSelected = "";
            int type = -1;

            switch (Skill)
            {
                case "Burn": SkillSelected = "503"; type = 1; break;
                case "Ignition": SkillSelected = "518"; type = 1; break;
                case "Specter Of Fire": SkillSelected = "543"; type = 1; break;
                case "Manes Of Fire": SkillSelected = "556"; type = 1; break;
                case "Incineration": SkillSelected = "570"; type = 1; break;

                case "Blaze": SkillSelected = "509"; type = 2; break;
                case "Hell Fire": SkillSelected = "539"; type = 2; break;
                case "Fire Thorn": SkillSelected = "554"; type = 2; break;

                case "Flame Staff": SkillSelected = "542"; type = 4; break;
                case "Glacier Staff": SkillSelected = "642"; type = 4; break;
                case "Lightining Staff": SkillSelected = "742"; type = 4; break;

                case "Flash": SkillSelected = "002"; type = 5; break;
                case "Shiver": SkillSelected = "003"; type = 5; break;
                case "Flame": SkillSelected = "005"; type = 5; break;
                case "Cold Wave": SkillSelected = "007"; type = 5; break;
                case "Spark": SkillSelected = "009"; type = 5; break;

                case "Freeze": SkillSelected = "603"; type = 5; break;
                case "Chill": SkillSelected = "609"; type = 5; break;
                case "Solid": SkillSelected = "618"; type = 5; break;
                case "Frostbite": SkillSelected = "639"; type = 5; break;
                case "Frozen Blade": SkillSelected = "642"; type = 5; break;
                case "Specter Of Ice": SkillSelected = "643"; type = 5; break;
                case "Ice Comet": SkillSelected = "651"; type = 5; break;

                case "Fire Blast": SkillSelected = "535"; type = 6; break;
                case "Fire Spear": SkillSelected = "527"; type = 6; break;
                case "Fire Ball": SkillSelected = "515"; type = 6; break;
                case "Ice Orb": SkillSelected = "627"; type = 6; break;

                case "Pillar Of Fire": SkillSelected = "551"; type = 7; break;

                case "Fire Impact": SkillSelected = "557"; type = 8; break;

                case "Inferno": SkillSelected = "545"; type = 9; break;
                case "Super Nova": SkillSelected = "560"; type = 9; break;

                case "Fire Burst": SkillSelected = "533"; type = 10; break;

                case "Fire Blade": SkillSelected = "542"; type = 11; break;
                case "Stroke": SkillSelected = "001"; type = 11; break;
            }

            string SkillID = AlignDWORD(int.Parse(GetClass().ToString() + SkillSelected))[..8];

            if (!IsAttackableTargetWithBase(Base))
                return false;

            switch (type)
            {
                case 1:
                    SendPacket("3101" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000A00");
                    Thread.Sleep(20);
                    SendPacket("3103" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000");

                    return true;

                case 2:
                    SendPacket("3101" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000F00");
                    Thread.Sleep(20);
                    SendPacket("3103" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "000000000000000000000000");

                    return true;

                case 4:
                    SendPacket("3101" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000D00");
                    Thread.Sleep(20);
                    SendPacket("3102" + SkillID + "FFFFFFFF" + AlignDWORD(GetId())[..8] + "000000000000000000000000000000000000000000000000000000000F00");
                    SendPacket("3103" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000F00");

                    return true;

                case 5:
                    SendPacket("3101" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000F00");
                    Thread.Sleep(20);
                    SendPacket("3103" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000");

                    return true;

                case 6:

                    SendPacket("3101" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000F00");
                    Thread.Sleep(20);
                    SendPacket("3102" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000");
                    Thread.Sleep(20);
                    SendPacket("3103" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000000");
                    SendPacket("3104" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "9BFFFFFF0000000000000000000000000000");

                    return true;

                case 7:

                    SendPacket("3101" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "000000000000000000000000000000000F00");
                    Thread.Sleep(20);
                    SendPacket("3103" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "000000000000000000000000");

                    return true;

                case 8:
                    SendPacket("3101" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "000000000000000000000000000000000F00");
                    Thread.Sleep(20);
                    SendPacket("3102" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "000000000000000000000000");
                    Thread.Sleep(20);
                    SendPacket("3103" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "000000000000000000000000000000000000");
                    SendPacket("3104" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "9BFFFFFF0000000000000000000000000000");

                    return true;

                case 9:
                    SendPacket("3101" + SkillID + AlignDWORD(GetId())[..8] + "FFFFFFFF" + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "000000000000000000000000000000000F00");
                    Thread.Sleep(20);
                    SendPacket("3103" + SkillID + AlignDWORD(GetId())[..8] + "FFFFFFFF" + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "000000000000000000000000");

                    return true;

                case 10:
                    SendPacket("3101" + SkillID + AlignDWORD(GetId())[..8] + "FFFFFFFF" + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "000000000000000000000000000000000F00");
                    Thread.Sleep(20);
                    SendPacket("3102" + SkillID + AlignDWORD(GetId())[..8] + "FFFFFFFF" + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "000000000000000000000000");
                    Thread.Sleep(20);
                    SendPacket("3103" + SkillID + AlignDWORD(GetId())[..8] + "FFFFFFFF" + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "000000000000000000000000000000000000");
                    SendPacket("3104" + SkillID + AlignDWORD(GetId())[..8] + "FFFFFFFF" + AlignDWORD(TargetX)[..8] + AlignDWORD(TargetZ)[..8] + AlignDWORD(TargetY)[..8] + "9BFFFFFF0000000000000000000000000000");

                    return true;

                case 11:
                    SendPacket("3103" + SkillID + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "010000000100000000000000000000000000000000000000000000000000");

                    return true;
            }

            return false;
        }

        public static bool RogueAttack(string Skill, int TargetId)
        {
            if (GetHp() == 0 || Skill == "")
                return false;

            string Class = GetClass().ToString();
            string SkillSelected = "";

            int Base = GetTargetBase(TargetId);

            int TargetX = GetTargetX(Base); int TargetY = GetTargetY(Base); int TargetZ = GetTargetZ(Base);

            int Id = GetId();

            switch (Skill)
            {
                case "Archery": SkillSelected = "003"; break;
                case "Through Shot": SkillSelected = "500"; break;
                case "Fire Arrow": SkillSelected = "505"; break;
                case "Poison Arrow": SkillSelected = "510"; break;
                case "Guided Arrow": SkillSelected = "520"; break;
                case "Perfect Shot": SkillSelected = "525"; break;
                case "Fire Shot": SkillSelected = "530"; break;
                case "Poison Shot": SkillSelected = "535"; break;
                case "Arc Shot": SkillSelected = "540"; break;
                case "Explosive Shot": SkillSelected = "545"; break;
                case "Viper": SkillSelected = "550"; break;
                case "Shadow Shot": SkillSelected = "557"; break;
                case "Shadow Hunter": SkillSelected = "560"; break;
                case "Ice Shot": SkillSelected = "562"; break;
                case "Lightning Shot": SkillSelected = "566"; break;
                case "Dark Pursuer": SkillSelected = "570"; break;
                case "Blow Arrow": SkillSelected = "572"; break;
                case "Blinding Strafe": SkillSelected = "580"; break;
                case "Multiple Shot":
                    {
                        if (!IsAttackableTargetWithBase(Base))
                        {
                            return false;
                        }

                        SendPacket("3101" + AlignDWORD(int.Parse(Class + "515"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000D00");
                        Thread.Sleep(20);
                        SendPacket("3102" + AlignDWORD(int.Parse(Class + "515"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000010000000000000000000000");
                        SendPacket("3103" + AlignDWORD(int.Parse(Class + "515"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000010000000000000000000000000000000000");
                        SendPacket("3103" + AlignDWORD(int.Parse(Class + "515"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000020000000000000000000000000000000000");
                        SendPacket("3103" + AlignDWORD(int.Parse(Class + "515"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000030000000000000000000000000000000000");
                        return true;
                    }
                case "Counter Strike":
                    {
                        if (!IsAttackableTargetWithBase(Base))
                        {
                            return false;
                        }

                        SendPacket("3101" + AlignDWORD(int.Parse(Class + "552"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000A00");
                        Thread.Sleep(20);
                        SendPacket("3102" + AlignDWORD(int.Parse(Class + "552"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000");
                        SendPacket("3103" + AlignDWORD(int.Parse(Class + "552"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000000");

                        return true;
                    }
                case "Arrow Shower":
                    {
                        if (!IsAttackableTargetWithBase(Base))
                        {
                            return false;
                        }

                        SendPacket("3101" + AlignDWORD(int.Parse(Class + "555"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000F00");
                        Thread.Sleep(20);
                        SendPacket("3102" + AlignDWORD(int.Parse(Class + "555"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000010000000000000000000000");
                        SendPacket("3103" + AlignDWORD(int.Parse(Class + "555"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000010000000000000000000000000000000000");

                        if (Distance(GetX(), GetY(), TargetX, TargetY) <= 4 && GetTargetMoveType(Base) != 4) // second arrow
                            SendPacket("3103" + AlignDWORD(int.Parse(Class + "555"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000020000000000000000000000000000000000");

                        if (Distance(GetX(), GetY(), TargetX, TargetY) <= 3 && GetTargetMoveType(Base) != 4) // third arrow
                            SendPacket("3103" + AlignDWORD(int.Parse(Class + "555"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000030000000000000000000000000000000000");

                        if (Distance(GetX(), GetY(), TargetX, TargetY) <= 2 && GetTargetMoveType(Base) != 4) // fourth arrow
                            SendPacket("3103" + AlignDWORD(int.Parse(Class + "555"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000050000000000000000000000000000000000");

                        if (Distance(GetX(), GetY(), TargetX, TargetY) <= 1 && GetTargetMoveType(Base) != 4) // fifth arrow
                            SendPacket("3103" + AlignDWORD(int.Parse(Class + "555"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000040000000000000000000000000000000000");

                        return true;
                    }
                case "Super Archer":
                    {
                        if (!IsAttackableTargetWithBase(Base))
                        {
                            return false;
                        }

                        SendPacket("3101" + AlignDWORD(int.Parse(Class + "515"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000D00");
                        Thread.Sleep(20);
                        SendPacket("3102" + AlignDWORD(int.Parse(Class + "515"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000010000000000000000000000");

                        byte multipleShotArrowCount = 3;

                        if (Distance(GetX(), GetY(), TargetX, TargetY) >= 16)
                            multipleShotArrowCount = 2;

                        for (int i = 0; i < multipleShotArrowCount; i++)
                            SendPacket("3103" + AlignDWORD(int.Parse(Class + "515"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000010000000000000000000000000000000000");

                        if (GetTargetMoveType(Base) == 4)
                            return false;

                        byte arrowShowerCount = 3;

                        if (Distance(GetX(), GetY(), TargetX, TargetY) >= 16)
                            arrowShowerCount = 2;
                        else if (Distance(GetX(), GetY(), TargetX, TargetY) <= 1)
                            arrowShowerCount = 5;

                        SendPacket("3101" + AlignDWORD(int.Parse(Class + "555"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000F00");
                        Thread.Sleep(20);
                        SendPacket("3102" + AlignDWORD(int.Parse(Class + "555"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000010000000000000000000000");

                        for (int i = 0; i < arrowShowerCount; i++)
                            SendPacket("3103" + AlignDWORD(int.Parse(Class + "555"))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000010000000000000000000000000000000000");
                        
                        return true;
                    }
            }

            if (!SkillSelected.Equals(""))
            {
                if (!IsAttackableTargetWithBase(Base))
                {
                    return false;
                }

                SendPacket("3101" + AlignDWORD(int.Parse(Class + SkillSelected))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000D00");
                Thread.Sleep(20);
                SendPacket("3102" + AlignDWORD(int.Parse(Class + SkillSelected))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000");
                SendPacket("3103" + AlignDWORD(int.Parse(Class + SkillSelected))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "000000000000000000000000000000000000000000000000000000000000");

                return true;
            }

            else
            {
                switch (Skill)
                {
                    case "Stroke": SkillSelected = "001"; break;
                    case "Stab": SkillSelected = "005"; break;
                    case "Stab2": SkillSelected = "006"; break;
                    case "Jab": SkillSelected = "600"; break;
                    case "Pierce": SkillSelected = "615"; break;
                    case "Shock": SkillSelected = "620"; break;
                    case "Thrust": SkillSelected = "635"; break;
                    case "Cut": SkillSelected = "640"; break;
                    case "Spike": SkillSelected = "655"; break;
                    case "Blody Beast": SkillSelected = "670"; break;
                    case "Blinding": SkillSelected = "675"; break;
                }

                if (!IsAttackableTargetWithBase(Base))
                {
                    return false;
                }

                SendPacket("080101" + AlignDWORD(TargetId)[..8] + "FF0000000000"); // R Attack
                SendPacket("3103" + AlignDWORD(int.Parse(Class + SkillSelected))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "010000000100000000000000000000000000000000000000000000000000");

                return true;
            }
        }

        public static bool WarriorAttack(string Skill, int TargetId)
        {
            if (GetHp() == 0 || Skill == "")
                return false;

            string Class = GetClass().ToString();
            string SkillSelected = "";

            int Base = GetTargetBase(TargetId);

            int Id = GetId();

            switch (Skill)
            {
                case "Stroke": SkillSelected = "001"; break;
                case "Slash": SkillSelected = "003"; break;
                case "Crash": SkillSelected = "005"; break;
                case "Piercing": SkillSelected = "009"; break;
                case "Hash": SkillSelected = "500"; break;
                case "Hoodwink": SkillSelected = "505"; break;
                case "Shear": SkillSelected = "510"; break;
                case "Pierce": SkillSelected = "515"; break;
                case "Leg Cutting": SkillSelected = "520"; break;
                case "Carving": SkillSelected = "525"; break;
                case "Sever": SkillSelected = "530"; break;
                case "Prick": SkillSelected = "535"; break;
                case "Multiple Shock": SkillSelected = "540"; break;
                case "Cleave": SkillSelected = "545"; break;
                case "Mangling": SkillSelected = "550"; break;
                case "Thrust": SkillSelected = "555"; break;
                case "Sword Aura": SkillSelected = "557"; break;
                case "Sword Dancing": SkillSelected = "560"; break;
                case "Howling Sword": SkillSelected = ""; break;
                case "Blooding": SkillSelected = ""; break;
                case "Hell Blade": SkillSelected = ""; break;
                case "Provoke": SkillSelected = "630"; break;
            }

            if (!IsAttackableTargetWithBase(Base))
                return false;

            SendPacket("3103" + AlignDWORD(int.Parse(Class + SkillSelected))[..8] + AlignDWORD(Id)[..8] + AlignDWORD(TargetId)[..8] + "010000000100000000000000000000000000000000000000000000000000");

            return true;
        }

        public static bool PriestAttack(string Skill, int TargetId)
        {
            if (GetHp() == 0 || Skill == "")
                return false;

            string Class = GetClass().ToString();
            string SkillSelected = "";
            
            int Base = GetTargetBase(TargetId);

            switch (Skill)
            {
                case "Stroke": SkillSelected = "001"; break;
                case "Holy Attack": SkillSelected = "006"; break;
                case "Collision": SkillSelected = "511"; break;
                case "Wrath": SkillSelected = "611"; break;
                case "Tilt": SkillSelected = "712"; break;
                case "Shuddering": SkillSelected = "520"; break;
                case "Wield": SkillSelected = "620"; break;
                case "Bloody": SkillSelected = "721"; break;
                case "Ruin": SkillSelected = "542"; break;
                case "Harsh": SkillSelected = "641"; break;
                case "Raving Edge": SkillSelected = "739"; break;
                case "Hellish": SkillSelected = "551"; break;
                case "Collapse": SkillSelected = "650"; break;
                case "Hades": SkillSelected = "750"; break;
                case "Mangling": SkillSelected = "550"; break;
            }

            if (!IsAttackableTargetWithBase(Base))
                return false;

            SendPacket("3103" + AlignDWORD(int.Parse(Class + SkillSelected))[..8] + AlignDWORD(GetId())[..8] + AlignDWORD(TargetId)[..8] + "010000000100000000000000000000000000000000000000000000000000");

            return true;
        }
    }
}
